namespace NuVelocity.IO
{
    public class RawProperty
    {
        protected const string kDefaultName = "CObject";
        protected const string kDefaultDescription = "";

        public RawProperty(string name = kDefaultName,
                        string description = kDefaultDescription,
                        object value = null)
        {
            Name = name;
            Description = description;
            Value = value;
        }

        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public object Value { get; protected set; }

        public override string ToString()
        {
            return $"{Name}={Value}";
        }
    }
}
