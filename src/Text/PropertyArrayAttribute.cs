using NuVelocity.Graphics;

namespace NuVelocity.Text;

public class PropertyArrayAttribute : PropertyAttribute
{
    public const string ArrayListID = "Array";
    public const string ArrayAsciiEscapedID = "bytes of binary in ASCII esc";
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
        object? defaultValue = null)
        : base(
            name,
            description,
            isEditable,
            isDynamic,
            defaultValue)
    {
        ItemName = itemName
            ?? DefaultItemKey;
        ItemCountName = itemName == null
            ? DefaultItemCountKey
            : NamedItemCountKey;
    }
}
