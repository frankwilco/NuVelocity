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

    public static PropertyListMetadata? CreateOrGetFor(Type type)
    {
        string typeName = type.FullName ?? type.Name;
        if (All.ContainsKey(typeName))
        {
            return All[typeName];
        }

        PropertyRootAttribute? rootAttr =
            type.GetCustomAttribute<PropertyRootAttribute>();
        if (rootAttr == null)
        {
            return null;
        }

        PropertyListMetadata classInfo = new(rootAttr);

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

        lock (All)
        {
            All[typeName] = classInfo;
        }

        return classInfo;
    }

}
