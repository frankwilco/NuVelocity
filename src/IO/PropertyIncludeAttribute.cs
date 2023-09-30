namespace NuVelocity.IO
{
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field,
        Inherited = false, AllowMultiple = false)]
    public sealed class PropertyIncludeAttribute : Attribute
    {
        public PropertyIncludeAttribute(EngineSource source)
        {
            Source = source;
        }

        public EngineSource Source { get; }
    }
}
