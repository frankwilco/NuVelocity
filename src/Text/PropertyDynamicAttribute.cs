namespace NuVelocity.Text
{
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field,
        Inherited = false, AllowMultiple = false)]
    public sealed class PropertyDynamicAttribute : Attribute
    {
        public PropertyDynamicAttribute() { }
    }
}
