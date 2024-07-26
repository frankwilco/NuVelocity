using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace NuVelocity.Text;

public static class PropertyListSerializer
{
    private const string kDynamicPropertiesKey = "Dynamic Properties";

    private static readonly Type ListType =
        typeof(List<>);
    private static readonly Type NullableType =
        typeof(Nullable<>);
    private static readonly Type PropertyRootAttributeType =
        typeof(PropertyRootAttribute);
    private static readonly Type PropertyAttributeType =
        typeof(PropertyAttribute);

    private static bool WriteProperty(
        PropertyInfo prop,
        PropertyAttribute propAttr,
        StringBuilder builder,
        object target,
        int depth,
        PropertySerializationFlags source,
        bool ignoreNull = false)
    {
        object? propValue = prop.GetValue(target);
        // If we're supposed to ignore empty properties, return early.
        if (ignoreNull && propValue == null)
        {
            return false;
        }

        // Write property name.
        builder.Append('\t', depth + 1);
        builder.Append($"{propAttr.Name}=");

        // Write property value, if available.
        if (propValue == null)
        {
            // Return early if there's no provided default value.
            if (propAttr.DefaultValue == null)
            {
                builder.AppendLine();
                return true;
            }
            propValue = propAttr.DefaultValue;
        }

        // 1: Write the value's properties if it's marked with the
        // property root attribute.
        Type propertyType = prop.PropertyType;
        Type? underlyingType = Nullable.GetUnderlyingType(propertyType);
        if (underlyingType != null)
        {
            propertyType = underlyingType;
        }
        if (propertyType.IsDefined(PropertyRootAttributeType))
        {
            if (!WritePropertyList(builder, propValue, depth + 1, source))
            {
                builder.AppendLine();
            }
        }

        // 2: Write bool values as 0 (false) or 1 (true).
        else if (propertyType == typeof(bool))
        {
            bool boolValue = (bool)propValue;
            builder.AppendLine(boolValue ? "1" : "0");
        }

        // 3: Write enum values with their friendly name or as an int.
        else if (propertyType.IsEnum)
        {
            string? memberName = propValue.ToString() ??
                throw new InvalidDataException();
            MemberInfo memberInfo = propertyType.GetMember(memberName)[0];
            // Write the enum value's friendly name.
            if (memberInfo.IsDefined(PropertyAttributeType))
            {
                PropertyAttribute? enumAttribute = memberInfo
                    .GetCustomAttribute<PropertyAttribute>()
                    ?? throw new InvalidDataException();
                builder.AppendLine(enumAttribute.Name);
                return true;
            }
            // Otherwise, write the raw int value of the enum.
            string enumValue = ((int)propValue).ToString();
            builder.AppendLine(enumValue);
        }

        // 4: Write list values.
        else if (propertyType.IsGenericType &&
            propertyType.GetGenericTypeDefinition() == ListType)
        {
            ICollection listValue = (ICollection)propValue;
            WritePropertyArrayList(
                propAttr,
                builder,
                depth,
                source,
                listValue,
                listValue.Count);
        }

        // 5: Write array values.
        else if (propertyType.IsArray)
        {
            Array arrayValue = (Array)propValue;
            WritePropertyArrayList(
                propAttr,
                builder,
                depth,
                source,
                arrayValue,
                arrayValue.Length);
        }

        // 6: Write the serialized value from the object.
        else if (propValue is IPropertyListSerializable propSerializable)
        {
            builder.AppendLine(propSerializable.Serialize());
        }

        // 7: Write the string representation (fallback).
        else
        {
            builder.AppendLine(propValue.ToString());
        }

        return true;
    }

    private static void WritePropertyArrayList(
        PropertyAttribute propAttr,
        StringBuilder builder,
        int depth,
        PropertySerializationFlags source,
        IEnumerable collection,
        int collectionLength)
    {
        // Write property type (Array).
        builder.AppendLine(PropertyArrayAttribute.ClassName);
        builder.Append('\t', depth + 1);
        builder.AppendLine("{");

        // Check if this array is marked by an array property attribute.
        if (propAttr is PropertyArrayAttribute propArrayAttr)
        {
            // Write array element count.
            builder.Append('\t', depth + 2);
            builder.AppendLine(
                $"{propArrayAttr.ItemCountName}={collectionLength}");
            // Write all array elements.
            foreach (object item in collection)
            {
                builder.Append('\t', depth + 2);
                builder.Append($"{propArrayAttr.ItemName}=");
                // Null elements are still represented. They
                // serialize as empty (CObject=).
                if (!WritePropertyList(builder, item, depth + 3, source))
                {
                    builder.AppendLine();
                }
            }
        }

#if NV_LOG || DEBUG
        else
        {
            string attrName = nameof(PropertyArrayAttribute);
            string message = $"Array must be marked with {attrName}.";
#if NV_LOG
            Console.WriteLine(message);
#endif
#if DEBUG
            throw new SerializationException(message);
#endif
        }
#endif

        // Terminate the array.
        builder.Append('\t', depth + 1);
        builder.AppendLine("}");
    }

    private static bool WritePropertyList(
        StringBuilder builder,
        object target,
        int depth,
        PropertySerializationFlags flags)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }
        if (depth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(depth));
        }

        Type type = target.GetType();
        PropertyListMetadata? classInfo = PropertyListMetadataCache.CreateOrGetFor(type);
        if (classInfo == null)
        {
            return false;
        }

        // Call the target's custom serialization logic, if present.
        if (target is IPropertyListSerializable targetSerializable)
        {
            builder.AppendLine(targetSerializable.Serialize());
            return true;
        }

        // Write the class name and start the property list.
        builder.AppendLine(classInfo.Root.ClassName);
        builder.Append('\t', depth);
        builder.AppendLine("{");

        StringBuilder builderDynamic = new();
        int dynamicCount = 0;
        int dynamicIndex = builder.Length;

        // Iterate through all properties.
        foreach (var propAttr in classInfo.Properties.Values)
        {
            // Ignore properties without the property attribute.
            if (propAttr == null)
            {
                continue;
            }

            PropertyInfo prop = classInfo.PropertyInfoCache[propAttr.Name];

            bool hasExclusionMatch = (flags & propAttr.ExcludeFlags) > 0;
            if (hasExclusionMatch || propAttr.IsTransient)
            {
#if NV_LOG
                Console.WriteLine(
                    $"Skipping property {prop.Name} " +
                    $"[curr: {flags}, " +
                    $"exclude flags: {propAttr.ExcludeFlags}]");
#endif
                continue;
            }

            bool hasInclusionMatch =
                propAttr.IncludeFlags == PropertySerializationFlags.None ||
                ((flags & propAttr.IncludeFlags) > 0);
            if (!hasInclusionMatch)
            {
#if NV_LOG
                Console.WriteLine(
                    $"Skipping property {prop.Name} " +
                    $"[curr: {flags}, " +
                    $"include starting: {propAttr.IncludeFlags}]");
#endif
                continue;
            }

            if (propAttr.IsDynamic)
            {
                if (WriteProperty(
                    prop, propAttr, builderDynamic, target, depth + 1, flags, true))
                {
                    dynamicCount++;
                }
                continue;
            }

            bool isCompact = (flags & PropertySerializationFlags.Compact) == PropertySerializationFlags.Compact;
            WriteProperty(
                prop, propAttr, builder, target, depth, flags, isCompact);
        }

        if (dynamicCount > 0)
        {
            // The following is inserted in reverse order.
            builder.Insert(dynamicIndex, "}\n");
            builder.Insert(dynamicIndex, "\t", depth + 1);
            builder.Insert(dynamicIndex, builderDynamic.ToString());
            builder.Insert(dynamicIndex, "{\n");
            builder.Insert(dynamicIndex, "\t", depth + 1);
            builder.Insert(dynamicIndex, $"{kDynamicPropertiesKey}={dynamicCount}\n");
            builder.Insert(dynamicIndex, "\t", depth + 1);
        }

        // Terminate the property list.
        builder.Append('\t', depth);
        builder.AppendLine("}");

        return true;
    }

    public static bool Serialize(
        TextWriter writer,
        object target,
        PropertySerializationFlags sourceFilter = PropertySerializationFlags.None)
    {
        if (writer is null)
        {
            throw new ArgumentNullException(nameof(writer));
        }
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        StringBuilder builder = new();
        bool result = WritePropertyList(builder, target, 0, sourceFilter);
        writer.Write(builder.ToString());
        return result;
    }

    public static bool Serialize(
        Stream stream,
        object target,
        PropertySerializationFlags sourceFilter = PropertySerializationFlags.None)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        using StreamWriter writer = new(stream);
        return Serialize(writer, target, sourceFilter);
    }

    private static bool Deserialize(
        StreamReader reader,
        object target,
        bool isChild = false,
        string classNameOrValue = "")
    {
        if (reader is null)
        {
            throw new ArgumentNullException(nameof(reader));
        }
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        // Ignore custom property list serialization logic if it is not
        // a child property list.
        if (isChild && target is IPropertyListSerializable classSerializable)
        {
            classSerializable.Deserialize(classNameOrValue);
            return true;
        }

        Type type = target.GetType();
        PropertyListMetadata? classInfo = PropertyListMetadataCache.CreateOrGetFor(type);
        if (classInfo == null)
        {
            return false;
        }

        bool skipAhead = false;
        bool skipAheadRoot = false;
        bool checkForUnknownBlock = false;
        bool classFound = false;

        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();
            if (line == null)
            {
                break;
            }
            if (skipAheadRoot)
            {
                if (line == "}")
                {
                    skipAheadRoot = false;
                }
                continue;
            }
            line = line.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // 1: An unknown property was previously found.
            // Check if it's an object containing additional properties.
            if (checkForUnknownBlock)
            {
                if (line == "{")
                {
                    skipAhead = true;
                }
                checkForUnknownBlock = false;
            }

            // 2: Skip properties inside unknown object.
            else if (skipAhead && line != "}")
            {
                continue;
            }

            // 3: Start of property list.
            else if (line == "{")
            {
                continue;
            }

            // 4: End of property list.
            else if (line == "}")
            {
                if (isChild)
                {
                    break;
                }
                skipAhead = false;
                continue;
            }

            // 5: Property information.
            else if (line.Contains('='))
            {
                int equalIndex = line.IndexOf('=');
                KeyValuePair<string, string> pair = new(
                    line[..equalIndex],
                    line[(equalIndex + 1)..]);

                if (pair.Key == kDynamicPropertiesKey)
                {
                    continue;
                }

                // Look for the property's info associated with the class.
                classInfo.PropertyInfoCache.TryGetValue(pair.Key, out PropertyInfo? propInfo);
                if (propInfo == null)
                {
#if NV_LOG
                    Console.WriteLine($"Found unknown property pair: {line}");
#endif
                    checkForUnknownBlock = true;
                    continue;
                }

                Type? propType = propInfo.PropertyType;
                // Get the underlying type if it's nullable.
                if (propType.IsGenericType &&
                    propType.GetGenericTypeDefinition() == NullableType)
                {
                    propType = Nullable.GetUnderlyingType(propType);
                }
                if (propType == null)
                {
                    throw new InvalidDataException();
                }

                object? propValue = null;
                if (propType.IsGenericType &&
                    propType.GetGenericTypeDefinition() == ListType)
                {
                    Type? elementType = propType.GetGenericArguments()
                        .FirstOrDefault() ?? throw new InvalidDataException();
                    Array array = ParsePropertyArrayValue(reader, elementType);
                    Type concreteType = ListType.MakeGenericType(elementType);
                    propValue = Activator.CreateInstance(concreteType, array);
                }
                else if (propType.IsArray)
                {
                    Type? elementType = propType.GetElementType()
                        ?? throw new InvalidDataException();
                    propValue = ParsePropertyArrayValue(reader, elementType);
                }
                else
                {
                    propValue = ParsePropertyValue(reader, propType, pair.Value);
                }
                propInfo.SetValue(target, propValue);
            }

            // 6: Name of property list.
            else if (line == classInfo.Root.ClassName)
            {
                if (classFound)
                {
#if NV_LOG
                    Console.WriteLine("Duplicate root property list.");
#endif
                }
                classFound = true;
            }

            // 7: Name of property list (mismatch).
            else
            {
#if NV_LOG
                Console.WriteLine($"Skipping property list: {line}");
#endif
                skipAheadRoot = true;
            }
        }

        return classFound;
    }

    private static Array ParsePropertyArrayValue(StreamReader reader, Type elementType)
    {
        int ignoredElements = 0;
        int index = 0;
        bool lengthKnown = false;
        bool isSingleElement = false;
        Array array = Array.CreateInstance(elementType, 0);

        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // 1: Start of array list.
            else if (line == "{")
            {
                continue;
            }

            // 2: End of array list.
            else if (line == "}")
            {
                break;
            }

            // 3: Array element.
            else if (line.Contains('='))
            {
                int equalIndex = line.IndexOf('=');
                KeyValuePair<string, string> pair = new(
                    line[..equalIndex],
                    line[(equalIndex + 1)..]);

                if (pair.Key == PropertyArrayAttribute.DefaultItemCountKey ||
                    pair.Key == PropertyArrayAttribute.NamedItemCountKey)
                {
                    if (lengthKnown)
                    {
#if NV_LOG
                        Console.WriteLine("Duplicate array length property.");
#endif
                    }
                    if (!int.TryParse(pair.Value, out int arrayLength))
                    {
#if NV_LOG
                        Console.WriteLine("Invalid array length.");
#endif
                    }
                    // Create a new array instance only if it is not empty.
                    if (arrayLength > 0)
                    {
                        array = Array.CreateInstance(
                            elementType, arrayLength);
                    }
                    lengthKnown = true;
                    continue;
                }

                object? element = null;
                if (!string.IsNullOrWhiteSpace(pair.Value))
                {
                    element = Activator.CreateInstance(elementType);
                    Deserialize(reader, element!, true, pair.Value);
                }

                if (lengthKnown)
                {
                    array.SetValue(element, index);
                }
                else if (element == null)
                {
                    ignoredElements++;
                }
                else
                {
#if NV_LOG
                    Console.WriteLine("Creating a single-valued array (assumed).");
#endif
                    if (isSingleElement)
                    {
                        throw new SerializationException(
                            "Array should only have 1 element.");
                    }
                    array = Array.CreateInstance(elementType, 1);
                    array.SetValue(element, 0);
                    isSingleElement = true;
                }

                index++;
                continue;
            }
        }

#if NV_LOG
        if (!lengthKnown)
        {
            Console.WriteLine("Created an empty array.");
        }
        if (ignoredElements > 0)
        {
            Console.WriteLine($"{ignoredElements} element(s) were ignored");
        }
#endif

        return array;
    }

    private static object? ParsePropertyValue(
        StreamReader reader,
        Type propType,
        string propValueText)
    {
        object? propValue = null;

        TypeCode typeCode = Type.GetTypeCode(propType);
        switch (typeCode)
        {
            case TypeCode.Object:
                propValue = Activator.CreateInstance(propType);
                Deserialize(reader, propValue!, true, propValueText);
                break;
            case TypeCode.Boolean:
                propValue = propValueText == "1";
                break;
            case TypeCode.Char:
                if (char.TryParse(propValueText, out char charValue))
                {
                    propValue = charValue;
                }
                break;
            case TypeCode.SByte:
                if (sbyte.TryParse(propValueText, out sbyte sbyteValue))
                {
                    propValue = sbyteValue;
                }
                break;
            case TypeCode.Byte:
                if (byte.TryParse(propValueText, out byte byteValue))
                {
                    propValue = byteValue;
                }
                break;
            case TypeCode.Int16:
                if (short.TryParse(propValueText, out short shortValue))
                {
                    propValue = shortValue;
                }
                break;
            case TypeCode.UInt16:
                if (ushort.TryParse(propValueText, out ushort ushortValue))
                {
                    propValue = ushortValue;
                }
                break;
            case TypeCode.Int32:
                if (propType.IsEnum &&
                    propType.GetCustomAttribute<FlagsAttribute>() == null)
                {
                    foreach (var enumMember in propType.GetMembers())
                    {
                        PropertyAttribute? propAttr =
                            enumMember.GetCustomAttribute<PropertyAttribute>(true);
                        if (propAttr == null)
                        {
                            continue;
                        }
                        if (propAttr.Name == propValueText)
                        {
                            propValue = Enum.Parse(propType, enumMember.Name);
                            break;
                        }
                    }
                }

                if (int.TryParse(propValueText, out int intValue))
                {
                    propValue = intValue;
                }
                break;
            case TypeCode.UInt32:
                if (uint.TryParse(propValueText, out uint uintValue))
                {
                    propValue = uintValue;
                }
                break;
            case TypeCode.Int64:
                if (long.TryParse(propValueText, out long longValue))
                {
                    propValue = longValue;
                }
                break;
            case TypeCode.UInt64:
                if (ulong.TryParse(propValueText, out ulong ulongValue))
                {
                    propValue = ulongValue;
                }
                break;
            case TypeCode.Single:
                if (float.TryParse(propValueText, out float floatValue))
                {
                    propValue = floatValue;
                }
                break;
            case TypeCode.Double:
                if (double.TryParse(propValueText, out double doubleValue))
                {
                    propValue = doubleValue;
                }
                break;
            case TypeCode.Decimal:
                if (decimal.TryParse(propValueText, out decimal decimalValue))
                {
                    propValue = decimalValue;
                }
                break;
            case TypeCode.DateTime:
                if (DateTime.TryParse(propValueText, out DateTime dateTimeValue))
                {
                    propValue = dateTimeValue;
                }
                break;
            case TypeCode.String:
                propValue = propValueText;
                break;
            default:
                throw new NotImplementedException();
        }

        return propValue;
    }

    public static bool Deserialize(Stream stream, object target)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }
        using StreamReader reader = new(stream);
        return Deserialize(reader, target);
    }

    public static bool Deserialize(byte[] buffer, object target)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        using MemoryStream stream = new(buffer);
        return Deserialize(stream, target);
    }
}
