namespace NuVelocity.Text
{
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field,
        Inherited = false, AllowMultiple = false)]
    public sealed class PropertyExcludeAttribute : Attribute
    {
        public PropertyExcludeAttribute()
        {
            Flags = PropertySerializationFlags.None;
        }

        public PropertyExcludeAttribute(PropertySerializationFlags flags)
        {
            Flags = flags;
        }

        public PropertySerializationFlags Flags { get; }
    }
}
