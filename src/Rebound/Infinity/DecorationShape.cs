namespace NuVelocity.Rebound.Infinity;

[PropertyRoot("CDecorationShape", typeof(DecorationShape))]
public class DecorationShape : Decoration
{
    [Property("Shape")]
    public string Shape { get; set; }

    // filename reference to frame file.
    [Property("Fill Texture")]
    public string FillTexture { get; set; }

    [Property("Fill Color")]
    public RgbColor FillColor { get; set; }

    [Property("Additive Blend Fill")]
    public bool AdditiveBlendFill { get; set; }

    [Property("Texture Offset")]
    public Coordinates TextureOffset { get; set; }

    [Property("Texture Relative to Prop")]
    public bool TextureRelativeToProp { get; set; }

    [Property("Reflect instead of Wrap")]
    public bool ReflectInsteadOfWrap { get; set; }

    // filename reference to frame file.
    [Property("Outline Texture")]
    public string OutlineTexture { get; set; }

    [Property("Outline Color")]
    public RgbColor OutlineColor { get; set; }

    [Property("Additive Blend Outline")]
    public bool AdditiveBlendOutline { get; set; }

    [Property("Outline Width")]
    public int OutlineWidth { get; set; }
}
