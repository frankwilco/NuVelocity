using System.Reflection;

namespace NuVelocity.Text;

internal class PropertyMetadataInfo
{
    public PropertyAttribute Attribute { get; }
    public PropertyInfo PropertyInfo { get; }
    public MethodInfo? ShouldSerializeMethodInfo { get; }

    public PropertyMetadataInfo(
        PropertyAttribute attribute,
        PropertyInfo propertyInfo,
        MethodInfo? shouldSerializeMethodInfo)
    {
        Attribute = attribute;
        PropertyInfo = propertyInfo;
        ShouldSerializeMethodInfo = shouldSerializeMethodInfo;
    }
}
