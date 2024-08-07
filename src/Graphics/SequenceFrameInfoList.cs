namespace NuVelocity.Graphics;

[PropertyRoot("CSequenceFrameInfoList", typeof(SequenceFrameInfoList))]
public class SequenceFrameInfoList
{
    internal bool HasTextBlitType { get; private set; } = false;

    [Property("Frame Infos")]
    [PropertyArray("Frame Info")]
    public FrameInfo[] Values { get; set; }

    [Property("WasRLE")]
    public bool WasRle { get; set; }

    [Property("Flags")]
    public SequenceFlags Flags { get; set; }

    [Property("BlitType")]
    public int BlitType { get; set; }

    [Property("Blit Type")]
    public BlitType TextBlitType
    {
        get
        {
            if (!HasTextBlitType)
            {
                HasTextBlitType = true;
            }
            return (BlitType)BlitType;
        }
        set
        {
            BlitType = (int)value;
        }
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

    #region Serializer methods

    private bool ShouldSerializeBlitType()
    {
        return !HasTextBlitType;
    }

    private bool ShouldSerializeTextBlitType()
    {
        return HasTextBlitType;
    }

    #endregion
}
