namespace NuVelocity.Text;

public class PropertyArrayAttribute : PropertyAttribute
{
    public const string ClassName = "Array";
    public const string DefaultItemKey = "Array Item";
    public const string DefaultItemCountKey = "Array Count";
    public const string NamedItemCountKey = "Item Count";

    public string ItemName { get; }

    public string ItemCountName { get; }

    public PropertyArrayAttribute(
        string name,
        string? itemName = null,
        string description = "",
        bool isEditable = true,
        bool isDynamic = false,
        bool isTransient = false,
        object? defaultValue = null,
        PropertySerializationFlags includeFlags = default,
        PropertySerializationFlags excludeFlags = default)
        : base(
            name,
            description,
            isEditable,
            isDynamic,
            isTransient,
            defaultValue,
            includeFlags,
            excludeFlags)
    {
        ItemName = itemName
            ?? DefaultItemKey;
        ItemCountName = itemName == null
            ? DefaultItemCountKey
            : NamedItemCountKey;
    }
}
