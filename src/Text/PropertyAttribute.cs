using NuVelocity.Graphics;

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

    public object? DefaultValue { get; }

    public PropertyAttribute(
        string name,
        string description = "",
        bool isEditable = true,
        bool isDynamic = false,
        object? defaultValue = default)
    {
        Name = name;
        Description = description;
        IsEditable = isEditable;
        IsDynamic = isDynamic;
        DefaultValue = defaultValue;
    }
}
