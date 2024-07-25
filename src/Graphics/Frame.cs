namespace NuVelocity.Graphics;

[PropertyRoot("CStandAloneFrame", "Stand Alone Frame")]
public class Frame
{
    private bool? _mipmapForNativeVersion;
    private int? _finalBitDepth;
    private bool? _removeBlackBlending;
    private bool? _removeDeadAlpha;

    public PropertySerializationFlags Flags { get; set; }

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
        defaultValue: false,
        excludeFlags: PropertySerializationFlags.ImageFormat1)]
    public bool? IsRleAllCopy { get; set; }

    [Property("Crop Color 0",
        defaultValue: true)]
    internal bool? CropColor0 {
        get => CropAlphaChannel;
        set => CropAlphaChannel = value;
    }

    public bool? CropAlphaChannel { get; set; }

    [Property("Do Dither",
        defaultValue: true,
        excludeFlags: PropertySerializationFlags.ImageFormat1)]
    public bool? DoDither { get; set; }

    // TN: Present in some Ricochet Xtreme frame files.
    [Property("Dither",
        defaultValue: true,
        includeFlags: PropertySerializationFlags.ImageFormat1)]
    internal bool? Dither
    {
        get { return DoDither; }
        set
        {
            DoDither = value;
            Flags |= PropertySerializationFlags.ImageFormat1;
        }
    }

    [Property("Change Bit Depth",
        defaultValue: true,
        excludeFlags: PropertySerializationFlags.ImageFormat2)]
    public bool? ChangeBitDepth { get; set; }

    [Property("Loss Less",
        defaultValue: false,
        includeFlags: PropertySerializationFlags.ImageFormat2)]
    internal bool? LossLess1
    {
        get { return IsLossless; }
        set
        {
            IsLossless = value;
            Flags |= PropertySerializationFlags.ImageFormat2;
        }
    }

    [Property("Loss Less 2",
        defaultValue: false,
        excludeFlags: PropertySerializationFlags.ImageFormat2)]
    internal bool? LossLess2 {
        get => IsLossless;
        set => IsLossless = value;
    }

    public bool? IsLossless { get; set; }

    [Property("Quality",
        defaultValue: 80,
        includeFlags: PropertySerializationFlags.ImageFormat2)]
    internal int? Quality1
    {
        get { return JpegQuality; }
        set
        {
            JpegQuality = value;
            Flags |= PropertySerializationFlags.ImageFormat2;
        }
    }

    // TN: Present in some Ricochet Lost Worlds frame files.
    [Property("JPEG Quality",
        defaultValue: 80,
        excludeFlags: PropertySerializationFlags.ImageFormat2 |
            PropertySerializationFlags.ImageFormat3)]
    internal int? JpegQuality1
    {
        get => JpegQuality;
        set => JpegQuality = value;
    }

    [Property("JPEG Quality 2",
        defaultValue: 80,
        excludeFlags: PropertySerializationFlags.ImageFormat2,
        includeFlags: PropertySerializationFlags.ImageFormat3)]
    internal int? JpegQuality2
    {
        get { return JpegQuality; }
        set
        {
            if (JpegQuality == null)
            {
                Flags |= PropertySerializationFlags.ImageFormat3;
            }
            JpegQuality = value;
        }
    }

    public int? JpegQuality { get; set; }

    [Property("Center Hot Spot",
        defaultValue: false,
        excludeFlags: PropertySerializationFlags.ImageFormat1)]
    public bool? CenterHotSpot { get; set; }

    [Property("Blended With Black",
        defaultValue: true,
        excludeFlags: PropertySerializationFlags.ImageFormat2 |
            PropertySerializationFlags.ImageFormat1)]
    public bool? BlendedWithBlack { get; set; }

    [Property("Remove Dead Alpha",
        defaultValue: true,
        includeFlags: PropertySerializationFlags.ImageFormat2,
        excludeFlags: PropertySerializationFlags.ImageFormat1)]
    public bool? RemoveDeadAlpha
    {
        get { return _removeDeadAlpha; }
        set
        {
            if (_removeDeadAlpha == null)
            {
                Flags |= PropertySerializationFlags.ImageFormat2;
            }
            _removeDeadAlpha = value;
        }
    }

    [Property("Remove Black Blending",
        defaultValue: true,
        includeFlags: PropertySerializationFlags.ImageFormat2,
        excludeFlags: PropertySerializationFlags.ImageFormat1)]
    public bool? RemoveBlackBlending
    {
        get { return _removeBlackBlending; }
        set
        {
            if (_removeBlackBlending == null)
            {
                Flags |= PropertySerializationFlags.ImageFormat2;
            }
            _removeBlackBlending = value;
        }
    }

    [Property("Load Black Biased",
        defaultValue: false,
        excludeFlags: PropertySerializationFlags.ImageFormat1)]
    public bool? LoadBlackBiased { get; set; }

    [Property("Final Bit Depth",
        defaultValue: 0,
        includeFlags: PropertySerializationFlags.ImageFormat2,
        excludeFlags: PropertySerializationFlags.ImageFormat1)]
    public int? FinalBitDepth
    {
        get { return _finalBitDepth; }
        set
        {
            if (_finalBitDepth == null)
            {
                Flags |= PropertySerializationFlags.ImageFormat2;
            }
            _finalBitDepth = value;
        }
    }

    [Property("Blit Type",
        defaultValue: Graphics.BlitType.TransparentMask,
        excludeFlags: PropertySerializationFlags.ImageFormat1)]
    public BlitType? BlitType { get; set; }

    [Property("Mipmap For Native Version",
        defaultValue: true,
        includeFlags: PropertySerializationFlags.HasMipmapSupport)]
    public bool? MipmapForNativeVersion
    {
        get { return _mipmapForNativeVersion; }
        set
        {
            if (_mipmapForNativeVersion == null)
            {
                Flags |= PropertySerializationFlags.HasMipmapSupport;
            }
            _mipmapForNativeVersion = value;
        }
    }

    public Frame(PropertySerializationFlags flags)
    {
        Flags = flags;
    }

    public Frame()
        : this(PropertySerializationFlags.None)
    {
    }
}
