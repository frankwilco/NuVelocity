﻿namespace NuVelocity.Graphics;

[PropertyRoot("CSequenceFrameInfoList", "Sequence Frame Info List")]
public class SequenceFrameInfoList
{
    [PropertyArray("Frame Infos", "Frame Info")]
    public FrameInfo[] Values { get; set; }

    [Property("WasRLE")]
    public bool WasRle { get; set; }

    [Property("Flags")]
    public SequenceFlags Flags { get; set; }

    [Property("BlitType",
        excludeFlags: PropertySerializationFlags.HasTextBlitType)]
    public int BlitType { get; set; }

    [Property("Blit Type",
        includeFlags: PropertySerializationFlags.HasTextBlitType)]
    public BlitType TextBlitType
    {
        get { return (BlitType)BlitType; }
        set { BlitType = (int)value; }
    }

    [Property("FPS")]
    public float FramesPerSecond { get; set; }

    public SequenceFrameInfoList()
    {
        Values = Array.Empty<FrameInfo>();
    }

    public void CopyTo(Sequence sequence, BlitTypeRevision revision)
    {
        sequence.CenterHotSpot ??= Flags.HasFlag(
            SequenceFlags.CenterHotSpot);
        sequence.BlendedWithBlack ??= Flags.HasFlag(
            SequenceFlags.BlendedWithBlack);
        sequence.CropClor0 ??= Flags.HasFlag(
            SequenceFlags.CropColor0);
        sequence.Use8BitAlpha ??= Flags.HasFlag(
            SequenceFlags.Use8BitAlpha);
        sequence.IsRle ??= Flags.HasFlag(
            SequenceFlags.RunLengthEncode);
        sequence.DoDither ??= Flags.HasFlag(
            SequenceFlags.DoDither);
        sequence.IsLossless ??= Flags.HasFlag(
            SequenceFlags.Lossless);
        sequence.BlitType ??= BlitTypeConverter.Int32ToType(
            BlitType, revision);
        sequence.FramesPerSecond ??= FramesPerSecond;
    }
}
