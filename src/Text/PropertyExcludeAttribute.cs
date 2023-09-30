namespace NuVelocity.Text
{
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field,
        Inherited = false, AllowMultiple = false)]
    public sealed class PropertyExcludeAttribute : Attribute
    {
        public PropertyExcludeAttribute(EngineSource source)
        {
            Source = source;
        }

        public EngineSource Source { get; }
    }
}
