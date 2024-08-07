using System.Reflection;

namespace NuVelocity.Text;

internal class PropertyMetadataInfo
{
    public PropertyAttribute Attribute { get; }
    public PropertyArrayAttribute? ArrayAttribute { get; }
    public PropertyInfo PropertyInfo { get; }
    public MethodInfo? ShouldSerializeMethodInfo { get; }

    public PropertyMetadataInfo(
        PropertyAttribute attribute,
        PropertyArrayAttribute? arrayAttribute,
        PropertyInfo propertyInfo,
        MethodInfo? shouldSerializeMethodInfo)
    {
        Attribute = attribute;
        ArrayAttribute = arrayAttribute;
        PropertyInfo = propertyInfo;
        ShouldSerializeMethodInfo = shouldSerializeMethodInfo;
    }
}
