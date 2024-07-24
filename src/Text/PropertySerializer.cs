using System.Reflection;
using System.Text;

namespace NuVelocity.Text;

public static class PropertySerializer
{
    private static readonly BindingFlags kSearchFlags =
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Instance |
        BindingFlags.Static;

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
        if (propertyType.IsDefined(typeof(PropertyRootAttribute)))
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
            if (memberInfo.IsDefined(typeof(PropertyAttribute)))
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

        // 4: Write array values.
        else if (propertyType.IsArray)
        {
            Array arrayValue = (Array)propValue;
            // Write property type (Array).
            builder.AppendLine("Array");
            builder.Append('\t', depth + 1);
            builder.AppendLine("{");
            // Check first if array elements have a property root attribute.
            // Otherwise, we can't serialize this object without it.
            Type? elemType = propertyType.GetElementType()
                ?? throw new InvalidDataException();
            PropertyRootAttribute? elemAttr = elemType
                .GetCustomAttribute<PropertyRootAttribute>();
            if (elemAttr != null)
            {
                // Write array element count.
                builder.Append('\t', depth + 2);
                builder.AppendLine($"Item Count={arrayValue.Length}");
                // Write all array elements.
                foreach (object item in arrayValue)
                {
                    builder.Append('\t', depth + 2);
                    builder.Append($"{elemAttr.FriendlyName}=");
                    // Null elements are still represented. They
                    // serialize as empty (CObject=).
                    if (!WritePropertyList(builder, item, depth + 3, source))
                    {
                        builder.AppendLine();
                    }
                }
            }
            // Terminate the array.
            builder.Append('\t', depth + 1);
            builder.AppendLine("}");
        }

        // 5: Write the serialized value from the object.
        else if (propValue is IPropertyListSerializable propSerializable)
        {
            builder.AppendLine(propSerializable.Serialize());
        }

        // 6: Write the string representation (fallback).
        else
        {
            builder.AppendLine(propValue.ToString());
        }

        return true;
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
        // Return early if object does not have a property root attribute.
        PropertyRootAttribute? rootAttr =
            type.GetCustomAttribute<PropertyRootAttribute>();
        if (rootAttr == null)
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
        builder.AppendLine(rootAttr.ClassName);
        builder.Append('\t', depth);
        builder.AppendLine("{");

        StringBuilder builderDynamic = new();
        int dynamicCount = 0;
        int dynamicIndex = builder.Length;

        // Iterate through all properties.
        foreach (var prop in type.GetProperties(kSearchFlags))
        {
            PropertyAttribute? propAttr = prop.GetCustomAttribute<PropertyAttribute>();
            // Ignore properties without the property attribute.
            if (propAttr == null)
            {
                continue;
            }

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
            builder.Insert(dynamicIndex, $"Dynamic Properties={dynamicCount}\n");
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

    public static bool Deserialize(
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

        Type type = target.GetType();
        PropertyRootAttribute? classAttr =
            type.GetCustomAttribute<PropertyRootAttribute>();
        if (classAttr == null)
        {
            return false;
        }

        // Ignore custom property list serialization logic if it is not
        // a child property list.
        if (isChild && target is IPropertyListSerializable classSerializable)
        {
            classSerializable.Deserialize(classNameOrValue);
            return true;
        }

        // Cache property info.
        Dictionary<string, PropertyInfo> properties = new();
        foreach (var prop in type.GetProperties(kSearchFlags))
        {
            var propAttr = prop.GetCustomAttribute<PropertyAttribute>();
            // Ignore properties without the attribute.
            if (propAttr == null)
            {
                continue;
            }
            properties[propAttr.Name] = prop;
        }

        bool skipAhead = false;
        bool skipAheadRoot = false;
        bool checkForUnknownBlock = false;
        bool classFound = false;

        // FIXME: This probably wouldn't work for nested arrays.
        bool inArray = false;
        bool arrayLengthKnown = false;
        int arrayIndex = -1;
        Array? arrayPropValue = null;
        PropertyInfo? arrayPropInfo = null;
        Type? arrayElemType = null;

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
                if (inArray)
                {
                    arrayPropInfo!.SetValue(target, arrayPropValue);
                    inArray = false;
                    arrayLengthKnown = false;
                    arrayPropValue = null;
                    arrayPropInfo = null;
                    arrayElemType = null;
                }
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
                string[] pair = line.Split('=');
                bool isValueEmpty = string.IsNullOrWhiteSpace(pair[1]);

                if (inArray)
                {
                    if (pair[0] == "Item Count")
                    {
                        if (arrayLengthKnown)
                        {
#if NV_LOG
                            Console.WriteLine("Duplicate array length property.");
#endif
                        }
                        if (!int.TryParse(pair[1], out int arrayLength))
                        {
#if NV_LOG
                            Console.WriteLine("Invalid array length.");
#endif
                        }
                        arrayPropValue = Array.CreateInstance(
                            arrayElemType!, arrayLength);
                        arrayLengthKnown = true;
                        continue;
                    }

                    if (arrayPropValue == null)
                    {
                        // XXX: Assume single-element array.
                        arrayPropValue = Array.CreateInstance(
                            arrayElemType!, 1);
                        arrayLengthKnown = true;
                    }

                    object? arrayElem = null;
                    if (!isValueEmpty)
                    {
                        arrayElem = Activator.CreateInstance(arrayElemType!);
                        Deserialize(reader, arrayElem!, true, pair[1]);
                    }

                    arrayPropValue.SetValue(arrayElem, arrayIndex);
                    arrayIndex++;
                    continue;
                }

                if (pair[0] == "Dynamic Properties")
                {
                    continue;
                }

                // Look for the property's info associated with the class.
                properties.TryGetValue(pair[0], out PropertyInfo? propInfo);
                if (propInfo == null)
                {
#if NV_LOG
                    Console.WriteLine($"Found unknown property pair: {line}");
#endif
                    checkForUnknownBlock = true;
                    continue;
                }

                // TODO: special case for ascii-encoded binary
                Type? propType = propInfo.PropertyType;
                // Get the underlying type if it's nullable.
                if (propType.IsGenericType &&
                    propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    propType = Nullable.GetUnderlyingType(propType);
                }
                if (propType == null)
                {
                    throw new InvalidDataException();
                }
                TypeCode typeCode = Type.GetTypeCode(propType);
                object? propValue = null;
                switch (typeCode)
                {
                    case TypeCode.Object:
                        if (propType.IsArray)
                        {
                            arrayPropInfo = propInfo;
                            arrayElemType = propType.GetElementType();
                            arrayIndex = 0;
                            inArray = true;
                            continue;
                        }
                        propValue = Activator.CreateInstance(propType);
                        Deserialize(reader, propValue!, true, pair[1]);
                        break;
                    case TypeCode.Boolean:
                        propValue = pair[1] == "1";
                        break;
                    case TypeCode.Char:
                        if (char.TryParse(pair[1], out char charValue))
                        {
                            propValue = charValue;
                        }
                        break;
                    case TypeCode.SByte:
                        if (sbyte.TryParse(pair[1], out sbyte sbyteValue))
                        {
                            propValue = sbyteValue;
                        }
                        break;
                    case TypeCode.Byte:
                        if (byte.TryParse(pair[1], out byte byteValue))
                        {
                            propValue = byteValue;
                        }
                        break;
                    case TypeCode.Int16:
                        if (short.TryParse(pair[1], out short shortValue))
                        {
                            propValue = shortValue;
                        }
                        break;
                    case TypeCode.UInt16:
                        if (ushort.TryParse(pair[1], out ushort ushortValue))
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
                                if (propAttr.Name == pair[1])
                                {
                                    propValue = Enum.Parse(propType, enumMember.Name);
                                    break;
                                }
                            }
                        }

                        if (int.TryParse(pair[1], out int intValue))
                        {
                            propValue = intValue;
                        }
                        break;
                    case TypeCode.UInt32:
                        if (uint.TryParse(pair[1], out uint uintValue))
                        {
                            propValue = uintValue;
                        }
                        break;
                    case TypeCode.Int64:
                        if (long.TryParse(pair[1], out long longValue))
                        {
                            propValue = longValue;
                        }
                        break;
                    case TypeCode.UInt64:
                        if (ulong.TryParse(pair[1], out ulong ulongValue))
                        {
                            propValue = ulongValue;
                        }
                        break;
                    case TypeCode.Single:
                        if (float.TryParse(pair[1], out float floatValue))
                        {
                            propValue = floatValue;
                        }
                        break;
                    case TypeCode.Double:
                        if (double.TryParse(pair[1], out double doubleValue))
                        {
                            propValue = doubleValue;
                        }
                        break;
                    case TypeCode.Decimal:
                        if (decimal.TryParse(pair[1], out decimal decimalValue))
                        {
                            propValue = decimalValue;
                        }
                        break;
                    case TypeCode.DateTime:
                        if (DateTime.TryParse(pair[1], out DateTime dateTimeValue))
                        {
                            propValue = dateTimeValue;
                        }
                        break;
                    case TypeCode.String:
                        propValue = pair[1];
                        break;
                    default:
                        throw new NotImplementedException();
                }
                propInfo.SetValue(target, propValue);
            }

            // 6: Name of property list.
            else if (line == classAttr.ClassName)
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
