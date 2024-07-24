namespace NuVelocity.Text;

[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field,
    Inherited = false,
    AllowMultiple = false)]
public class PropertyAttribute : Attribute
{
    public string Name { get; }

    public string Description { get; }

    public bool IsEditable { get; }

    public bool IsDynamic { get; }

    public bool IsTransient { get; }

    public object? DefaultValue { get; }

    public PropertySerializationFlags IncludeFlags { get; }

    public PropertySerializationFlags ExcludeFlags { get; }

    public PropertyAttribute(
        string name,
        string description = "",
        bool isEditable = true,
        bool isDynamic = false,
        bool isTransient = false,
        object? defaultValue = default,
        PropertySerializationFlags includeFlags = default,
        PropertySerializationFlags excludeFlags = default)
    {
        Name = name;
        Description = description;
        IsEditable = isEditable;
        IsDynamic = isDynamic;
        IsTransient = isTransient;
        DefaultValue = defaultValue;
        IncludeFlags = includeFlags;
        ExcludeFlags = excludeFlags;
    }
}
