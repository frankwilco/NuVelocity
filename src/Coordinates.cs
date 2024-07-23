namespace NuVelocity;

[PropertyRoot("CCoordinates", "Coordinates", true)]
public class Coordinates : IPropertySerializable
{
    public int X { get; set; }
    public int Y { get; set; }

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

        if (int.TryParse(pair[0], out int xValue))
        {
            X = xValue;
        }
        if (int.TryParse(pair[1], out int yValue))
        {
            Y = yValue;
        }
    }
}
