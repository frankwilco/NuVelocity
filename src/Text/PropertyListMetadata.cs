namespace NuVelocity.Text;

internal class PropertyListMetadata
{
    public PropertyRootAttribute Root { get; private set; }

    public Dictionary<string, PropertyMetadataInfo> Properties { get; private set; }

    public Type Type { get; private set; }

    public PropertyListMetadata(Type type, PropertyRootAttribute rootAttribute)
    {
        Type = type;
        Root = rootAttribute;
        Properties = new();
    }
}
