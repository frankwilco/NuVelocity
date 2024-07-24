using System.Reflection;

namespace NuVelocity.Text;

internal class PropertyListMetadata
{
    public PropertyRootAttribute Root { get; private set; }

    public Dictionary<string, PropertyAttribute> Properties { get; private set; }

    public Dictionary<string, PropertyInfo> PropertyInfoCache { get; private set; }

    public PropertyListMetadata(PropertyRootAttribute rootAttribute)
    {
        Root = rootAttribute;
        Properties = new();
        PropertyInfoCache = new();
    }
}
