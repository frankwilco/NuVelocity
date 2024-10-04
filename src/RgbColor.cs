namespace NuVelocity;

[PropertyRoot("NVRgbColor", typeof(RgbColor))]
public struct RgbColor : IPropertyListSerializable
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }

    public void Deserialize(string context)
    {
        if (string.IsNullOrWhiteSpace(context))
        {
            return;
        }

        string[] components = context.Split(',');
        if (components.Length != 3)
        {
            return;
        }

        if (byte.TryParse(components[0], out byte red))
        {
            R = red;
        }
        if (byte.TryParse(components[1], out byte green))
        {
            G = green;
        }
        if (byte.TryParse(components[2], out byte blue))
        {
            B = blue;
        }
    }

    public readonly string Serialize() => $"{R},{G},{B}";
}
