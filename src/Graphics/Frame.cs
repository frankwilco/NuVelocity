namespace NuVelocity.Graphics;

[PropertyRoot("CStandAloneFrame", typeof(Frame))]
public class Frame
{
    internal bool HasMipmapSupport { get; private set; } = false;

    private bool? _mipmapForNativeVersion;
    private int? _finalBitDepth;
    private bool? _removeBlackBlending;
    private bool? _removeDeadAlpha;

    public ImagePropertyListFormat Format { get; set; }

    [Property("Comment",
        isDynamic: true)]
    public string? Comment { get; set; }

    [Property("Palette",
        isDynamic: true)]
    public PaletteHolder? Palette { get; set; }

    [Property("Run Length Encode",
        defaultValue: true)]
    public bool? IsRle { get; set; }

    [Property("RLE All Copy",
        defaultValue: false)]
    public bool? IsRleAllCopy { get; set; }

    [Property("Crop Color 0",
        defaultValue: true)]
    internal bool? CropColor0 {
        get => CropAlphaChannel;
        set => CropAlphaChannel = value;
    }

    public bool? CropAlphaChannel { get; set; }

    [Property("Do Dither",
        defaultValue: true)]
    public bool? DoDither { get; set; }

    // TN: Present in some Ricochet Xtreme frame files.
    [Property("Dither",
        defaultValue: true)]
    internal bool? Dither
    {
        get { return DoDither; }
        set
        {
            DoDither = value;
            Format = ImagePropertyListFormat.Format1;
        }
    }

    [Property("Change Bit Depth",
        defaultValue: true)]
    public bool? ChangeBitDepth { get; set; }

    [Property("Loss Less",
        defaultValue: false)]
    internal bool? LossLess1
    {
        get { return IsLossless; }
        set
        {
            IsLossless = value;
            Format = ImagePropertyListFormat.Format2;
        }
    }

    [Property("Loss Less 2",
        defaultValue: false)]
    internal bool? LossLess2 {
        get => IsLossless;
        set => IsLossless = value;
    }

    public bool? IsLossless { get; set; }

    [Property("Quality",
        defaultValue: 80)]
    internal int? Quality1
    {
        get { return JpegQuality; }
        set
        {
            JpegQuality = value;
            Format = ImagePropertyListFormat.Format2;
        }
    }

    // TN: Present in some Ricochet Lost Worlds frame files.
    [Property("JPEG Quality",
        defaultValue: 80)]
    internal int? JpegQuality1
    {
        get => JpegQuality;
        set => JpegQuality = value;
    }

    [Property("JPEG Quality 2",
        defaultValue: 80)]
    internal int? JpegQuality2
    {
        get { return JpegQuality; }
        set
        {
            if (JpegQuality == null)
            {
                Format = ImagePropertyListFormat.Format3;
            }
            JpegQuality = value;
        }
    }

    public int? JpegQuality { get; set; }

    [Property("Center Hot Spot",
        defaultValue: false)]
    public bool? CenterHotSpot { get; set; }

    [Property("Blended With Black",
        defaultValue: true)]
    public bool? BlendedWithBlack { get; set; }

    [Property("Remove Dead Alpha",
        defaultValue: true)]
    public bool? RemoveDeadAlpha
    {
        get { return _removeDeadAlpha; }
        set
        {
            if (_removeDeadAlpha == null)
            {
                Format = ImagePropertyListFormat.Format2;
            }
            _removeDeadAlpha = value;
        }
    }

    [Property("Remove Black Blending",
        defaultValue: true)]
    public bool? RemoveBlackBlending
    {
        get { return _removeBlackBlending; }
        set
        {
            if (_removeBlackBlending == null)
            {
                Format = ImagePropertyListFormat.Format2;
            }
            _removeBlackBlending = value;
        }
    }

    [Property("Load Black Biased",
        defaultValue: false)]
    public bool? LoadBlackBiased { get; set; }

    [Property("Final Bit Depth",
        defaultValue: 0)]
    public int? FinalBitDepth
    {
        get { return _finalBitDepth; }
        set
        {
            if (_finalBitDepth == null)
            {
                Format = ImagePropertyListFormat.Format2;
            }
            _finalBitDepth = value;
        }
    }

    [Property("Blit Type",
        defaultValue: Graphics.BlitType.TransparentMask)]
    public BlitType? BlitType { get; set; }

    [Property("Mipmap For Native Version",
        defaultValue: true)]
    public bool? MipmapForNativeVersion
    {
        get { return _mipmapForNativeVersion; }
        set
        {
            if (_mipmapForNativeVersion == null)
            {
                HasMipmapSupport = true;
            }
            _mipmapForNativeVersion = value;
        }
    }

    public Frame(ImagePropertyListFormat format)
    {
        Format = format;
    }

    public Frame()
        : this(ImagePropertyListFormat.Format3)
    {
    }

    #region Serializer methods

    private bool ShouldSerializeIsRleAllCopy()
    {
        return Format != ImagePropertyListFormat.Format1;
    }

    private bool ShouldSerializeDoDither()
    {
        return Format != ImagePropertyListFormat.Format1;
    }

    private bool ShouldSerializeDither()
    {
        return Format == ImagePropertyListFormat.Format1;
    }

    private bool ShouldSerializeChangeBitDepth()
    {
        return Format != ImagePropertyListFormat.Format2;
    }
    private bool ShouldSerializeLossLess1()
    {
        return Format == ImagePropertyListFormat.Format2;
    }

    private bool ShouldSerializeLossLess2()
    {
        return Format == ImagePropertyListFormat.Format3;
    }

    private bool ShouldSerializeQuality1()
    {
        return Format == ImagePropertyListFormat.Format2;
    }

    private static bool ShouldSerializeJpegQuality1()
    {
        return false;
    }

    private bool ShouldSerializeJpegQuality2()
    {
        return Format == ImagePropertyListFormat.Format3;
    }

    private bool ShouldSerializeCenterHotSpot()
    {
        return Format != ImagePropertyListFormat.Format1;
    }

    private bool ShouldSerializeBlendedWithBlack()
    {
        return Format == ImagePropertyListFormat.Format3;
    }

    private bool ShouldSerializeRemoveDeadAlpha()
    {
        return Format == ImagePropertyListFormat.Format2;
    }

    private bool ShouldSerializeRemoveBlackBlending()
    {
        return Format == ImagePropertyListFormat.Format2;
    }

    private bool ShouldSerializeLoadBlackBiased()
    {
        return Format != ImagePropertyListFormat.Format1;
    }

    private bool ShouldSerializeFinalBitDepth()
    {
        return Format == ImagePropertyListFormat.Format2;
    }

    private bool ShouldSerializeBlitType()
    {
        return Format != ImagePropertyListFormat.Format1;
    }

    private bool ShouldSerializeMipmapForNativeVersion()
    {
        return HasMipmapSupport;
    }

    internal string ToDebugString()
    {
        return "[" +
            $"{nameof(HasMipmapSupport)}:{HasMipmapSupport}" +
            "]";
    }

    #endregion
}
