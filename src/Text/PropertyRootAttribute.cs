namespace NuVelocity.Text;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct,
    Inherited = false, AllowMultiple = false)]
public sealed class PropertyRootAttribute : Attribute
{
    public string ClassName { get; }
    public string FriendlyName { get; }
    public bool IsSingleValue { get; }

    public PropertyRootAttribute(
        string className,
        string friendlyName = "",
        bool isSingleValue = false)
    {
        ClassName = className;
        FriendlyName = friendlyName;
        IsSingleValue = isSingleValue;
    }
}
