namespace NuVelocity.Graphics;

// TODO: check default values.
[PropertyRoot("CFontBitmap", typeof(FontBitmap))]
public class FontBitmap : Font
{
    [Property("First ASCII", defaultValue: 0)]
    public int? FirstAscii { get; set; }

    [Property("Last ASCII", defaultValue: 0)]
    public int? LastAscii { get; set; }

    [Property("Fixed Width", defaultValue: false)]
    public bool? IsFixedWidth { get; set; }

    [Property("Height Without Decenders")]
    internal int? HeightWithoutDecenders
    {
        get => XHeight;
        set => XHeight = value;
    }

    public int? XHeight { get; set; }
}
