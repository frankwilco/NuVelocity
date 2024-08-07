using System.Reflection;

namespace NuVelocity.Text;

internal static class PropertyListMetadataCache
{
    private static readonly BindingFlags kSearchFlags =
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Instance |
        BindingFlags.Static;

    public static Dictionary<string, PropertyListMetadata> ByName { get; } = new();

    public static Dictionary<string, PropertyListMetadata> ByFqn { get; } = new();

    private static PropertyListMetadata GetMetadata(
        Type type, PropertyRootAttribute rootAttr)
    {
        PropertyListMetadata classInfo = new(type, rootAttr);
        foreach (PropertyInfo propInfo in type.GetProperties(kSearchFlags))
        {
            var propAttr = propInfo.GetCustomAttribute<PropertyAttribute>();
            // Ignore properties without the attribute.
            if (propAttr == null)
            {
                continue;
            }
            MethodInfo? shouldSerializeMethodInfo =
                type.GetMethod($"ShouldSerialize{propInfo.Name}",
                kSearchFlags);
            if (shouldSerializeMethodInfo != null &&
                shouldSerializeMethodInfo.ReturnType != typeof(bool))
            {
                throw new ArgumentException(
                    "ShouldSerialize method should return a bool.");
            }
            classInfo.Properties[propAttr.Name] = new(
                propAttr, propInfo, shouldSerializeMethodInfo);
        }

        return classInfo;
    }

    public static PropertyListMetadata? Get(Type key)
    {
        string typeFqn = key.FullName ?? key.Name;
        bool hasEntry = ByFqn.TryGetValue(
            typeFqn, out PropertyListMetadata? value);
        if (hasEntry)
        {
            return value;
        }

        PropertyRootAttribute? rootAttr =
            key.GetCustomAttribute<PropertyRootAttribute>();
        if (rootAttr == null)
        {
            return null;
        }
        return GetMetadata(key, rootAttr);
    }

    public static PropertyListMetadata? Get(string key)
    {
        ByName.TryGetValue(key, out PropertyListMetadata? value);
        return value;
    }

    public static void Add(
        Type type,
        PropertyRootAttribute rootAttr,
        bool cacheClassName = true)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }
        if (rootAttr == null)
        {
            throw new ArgumentNullException(nameof(rootAttr));
        }

        string typeFqn = type.FullName ?? type.Name;
        bool hasFqnEntry = ByFqn.ContainsKey(typeFqn);
        if (!cacheClassName && hasFqnEntry)
        {
            return;
        }

        PropertyListMetadata classInfo = GetMetadata(type, rootAttr);

        if (cacheClassName)
        {
            lock (ByName)
            {
                ByName[rootAttr.ClassName] = classInfo;
            }
        }

        if (!hasFqnEntry)
        {
            lock (ByFqn)
            {
                ByFqn[typeFqn] = classInfo;
            }
        }
    }

    public static void Add(Type type, bool cacheClassName = true)
    {
        PropertyRootAttribute? rootAttr =
            type.GetCustomAttribute<PropertyRootAttribute>();
        if (rootAttr == null)
        {
            throw new ArgumentException(
                "Property root attribute is missing from type to be cached.");
        }

        Add(type, rootAttr, cacheClassName);
    }
}
