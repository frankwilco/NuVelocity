namespace NuVelocity.Text
{
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field,
        Inherited = false, AllowMultiple = false)]
    public class PropertyAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        public bool Editable { get; }
        public object? DefaultValue { get; }

        public PropertyAttribute(
            string name,
            string description = "",
            bool editable = true,
            object? defaultValue = null)
        {
            Name = name;
            Description = description;
            Editable = editable;
            DefaultValue = defaultValue;
        }
    }
}
