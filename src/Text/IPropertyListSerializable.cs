namespace NuVelocity.Text;

public interface IPropertyListSerializable
{
    public string Serialize();
    public void Deserialize(string context);
}
