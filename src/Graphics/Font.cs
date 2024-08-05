using System.Drawing;

namespace NuVelocity.Graphics;

// TODO: check default values.
[PropertyRoot("CFont", typeof(Font))]
public class Font
{
    [Property("Font Family", defaultValue: "Resources/Fonts/TRUE TYPES/!default.ttf")]
    public string? FontFamily { get; set; }

    [Property("Blit Type", defaultValue: Graphics.BlitType.TransparentMask)]
    public BlitType? BlitType { get; set; }

    // FIXME: colors are not handled by serialization code.
    [Property("Generated Color")]
    public Color? GeneratedColor { get; set; }

    [Property("Point Size", defaultValue: 12)]
    public int? PointSize { get; set; }

    [Property("Generate All Caps", defaultValue: false)]
    public bool? GenerateAllCaps { get; set; }
}
