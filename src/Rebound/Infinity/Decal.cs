namespace NuVelocity.Rebound.Infinity;

[PropertyRoot("CDecal", typeof(Decal))]
public class Decal : Decoration
{
    // Reflexive Only/Offset
    [Property("Offset")]
    public FloatCoordinates Offset { get; set; }

    [Property("Mirror")]
    public bool Mirror { get; set; }

    // Reflexive Only/Intensify
    [Property("Intensify")]
    public bool Intensify { get; set; }

    [Property("Scale X")]
    public float ScaleX { get; set; }

    [Property("Scale Y")]
    public float ScaleY { get; set; }

    // TODO: unit - Degrees
    [Property("Rotation")]
    public float Rotation { get; set; }

    [Property("Color")]
    public RgbaColor Color { get; set; }

    [Property("Image Source")]
    public FrameSelector ImageSource { get; set; }
}
