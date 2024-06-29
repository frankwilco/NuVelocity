namespace NuVelocity.Graphics;

[PropertyRoot("CSequenceFrameInfoList", "Sequence Frame Info List")]
public class SequenceFrameInfoList
{
    [Property("Frame Infos")]
    public FrameInfo[] Values { get; set; }

    [Property("WasRLE")]
    public bool WasRle { get; set; }

    [Property("Flags")]
    public SequenceFlags Flags { get; set; }

    [Property("BlitType")]
    [PropertyExclude(PropertySerializationFlags.HasTextBlitType)]
    public int BlitType { get; set; }

    [Property("Blit Type")]
    [PropertyInclude(PropertySerializationFlags.HasTextBlitType)]
    public BlitType TextBlitType
    {
        get { return (BlitType)BlitType; }
        set { BlitType = (int)value; }
    }

    [Property("FPS")]
    public float FramesPerSecond { get; set; }
}
