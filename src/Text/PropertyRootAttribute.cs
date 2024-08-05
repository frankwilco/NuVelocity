namespace NuVelocity.Text;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct,
    Inherited = false, AllowMultiple = false)]
public sealed class PropertyRootAttribute : Attribute
{
    public string ClassName { get; }

    public PropertyRootAttribute(
        string className,
        Type type,
        bool noCache = false)
    {
        ClassName = className;
        if (noCache)
        {
            return;
        }
        PropertyListMetadataCache.Add(type, this, false);
    }
}
