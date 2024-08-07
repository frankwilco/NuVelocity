using System.Reflection;

namespace NuVelocity.Text;

internal class PropertyListMetadata
{
    public PropertyRootAttribute Root { get; private set; }

    public Dictionary<string, PropertyAttribute> Properties { get; private set; }

    public Dictionary<string, PropertyInfo> PropertyInfoCache { get; private set; }

    public Dictionary<string, MethodInfo> ShouldSerializeMethods { get; private set; }

    public Type Type { get; private set; }

    public PropertyListMetadata(Type type, PropertyRootAttribute rootAttribute)
    {
        Type = type;
        Root = rootAttribute;
        Properties = new();
        PropertyInfoCache = new();
        ShouldSerializeMethods = new();
    }
}
