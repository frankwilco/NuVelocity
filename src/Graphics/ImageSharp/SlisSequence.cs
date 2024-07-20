using NuVelocity.Text;
using SixLabors.ImageSharp;

namespace NuVelocity.Graphics.ImageSharp;

[PropertyRoot("CSequence", "Sequence")]
public class SlisSequence : Sequence
{
    public Image[]? Textures { get; set; }

    public int Width { get; protected set; }

    public int Height { get; protected set; }

    public SlisSequence(PropertySerializationFlags flags)
        : base(flags)
    {
    }

    public SlisSequence()
        : base()
    {
    }
}
