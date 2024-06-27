namespace NuVelocity.Text
{
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field,
        Inherited = false, AllowMultiple = false)]
    public sealed class PropertyIncludeAttribute : Attribute
    {
        public PropertyIncludeAttribute()
        {
            Flags = PropertySerializationFlags.None;
        }

        public PropertyIncludeAttribute(PropertySerializationFlags flags)
        {
            Flags = flags;
        }

        public PropertySerializationFlags Flags { get; }
    }
}
