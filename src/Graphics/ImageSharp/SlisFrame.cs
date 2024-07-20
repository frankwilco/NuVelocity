using NuVelocity.Text;
using SixLabors.ImageSharp;

namespace NuVelocity.Graphics.ImageSharp;

[PropertyRoot("CStandAloneFrame", "Stand Alone Frame")]
public class SlisFrame : Frame
{
    public Image? Texture { get; set; }

    public int Width => Texture?.Width ?? 0;

    public int Height => Texture?.Height ?? 0;

    public SlisFrame(Image? image, PropertySerializationFlags flags)
        : base(flags)
    {
        Texture = image;
    }

    public SlisFrame(Image? image)
        : this(image, PropertySerializationFlags.None)
    {
    }

    public SlisFrame(PropertySerializationFlags flags)
        : this(null, flags)
    {
    }

    public SlisFrame()
        : this(null, PropertySerializationFlags.None)
    {
    }
}
