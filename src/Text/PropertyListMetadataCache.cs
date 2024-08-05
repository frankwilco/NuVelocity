using System.Reflection;

namespace NuVelocity.Text;

internal static class PropertyListMetadataCache
{
    private static readonly BindingFlags kSearchFlags =
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Instance |
        BindingFlags.Static;

    public static Dictionary<string, PropertyListMetadata> All { get; } = new();

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
            classInfo.Properties[propAttr.Name] = propAttr;
            classInfo.PropertyInfoCache[propAttr.Name] = propInfo;
        }

        return classInfo;
    }

    public static PropertyListMetadata? Get(Type key)
    {
        PropertyRootAttribute? rootAttr =
            key.GetCustomAttribute<PropertyRootAttribute>();
        if (rootAttr == null)
        {
            return null;
        }

        bool hasEntry = All.TryGetValue(
            rootAttr.ClassName, out PropertyListMetadata? value);
        if (hasEntry)
        {
            return value;
        }
        return GetMetadata(key, rootAttr);
    }

    public static PropertyListMetadata? Get(string key)
    {
        All.TryGetValue(key, out PropertyListMetadata? value);
        return value;
    }

    public static void Add(
        Type type,
        PropertyRootAttribute rootAttr,
        bool replaceExisting = false)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }
        if (rootAttr == null)
        {
            throw new ArgumentNullException(nameof(rootAttr));
        }

        // Don't do anything if we shouldn't replace existing entries.
        if (All.ContainsKey(rootAttr.ClassName) && !replaceExisting)
        {
            return;
        }

        PropertyListMetadata classInfo = GetMetadata(type, rootAttr);

        lock (All)
        {
            All[rootAttr.ClassName] = classInfo;
        }
    }

    public static void Add(Type type, bool replaceExisting = false)
    {
        PropertyRootAttribute? rootAttr =
            type.GetCustomAttribute<PropertyRootAttribute>();
        if (rootAttr == null)
        {
            throw new ArgumentException(
                "Property root attribute is missing from type to be cached.");
        }

        Add(type, rootAttr, replaceExisting);
    }
}
