namespace NuVelocity;

[PropertyRoot("NVFloatCoordinates", typeof(FloatCoordinates))]
public class FloatCoordinates : IPropertyListSerializable
{
    public float X { get; set; }
    public float Y { get; set; }

    public string Serialize()
    {
        return $"{X},{Y}";
    }

    public void Deserialize(string context)
    {
        if (string.IsNullOrWhiteSpace(context))
        {
            return;
        }

        string[] pair = context.Split(',');
        if (pair.Length != 2)
        {
            return;
        }

        if (float.TryParse(pair[0], out float xValue))
        {
            X = xValue;
        }
        if (float.TryParse(pair[1], out float yValue))
        {
            Y = yValue;
        }
    }
}
