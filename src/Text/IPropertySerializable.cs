namespace NuVelocity.Text
{
    public interface IPropertySerializable
    {
        public string Serialize();
        public void Deserialize(string context);
    }
}
