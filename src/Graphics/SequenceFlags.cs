namespace NuVelocity.Graphics
{
    [Flags]
    public enum SequenceFlags
    {
        CenterHotSpot = 1 << 0,
        BlendedWithBlack = 1 << 1,
        CropColor0 = 1 << 2,
        Use8BitAlpha = 1 << 3,
        RunLengthEncode = 1 << 4,
        DoDither = 1 << 5,
        Lossless = 1 << 6,
        Unused = 1 << 7
    }
}
