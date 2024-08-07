namespace NuVelocity.Text;

[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field,
    Inherited = false,
    AllowMultiple = false)]
public class PropertyArrayAttribute : Attribute
{
    public const string ArrayListID = "Array";
    public const string ArrayAsciiEscapedID = "bytes of binary in ASCII esc";
    public const string DefaultItemKey = "Array Item";
    public const string DefaultItemCountKey = "Array Count";
    public const string NamedItemCountKey = "Item Count";

    public string ItemName { get; }

    public string ItemCountName { get; }

    public PropertyArrayAttribute(string? itemName = null)
    {
        ItemName = itemName
            ?? DefaultItemKey;
        ItemCountName = itemName == null
            ? DefaultItemCountKey
            : NamedItemCountKey;
    }
}
