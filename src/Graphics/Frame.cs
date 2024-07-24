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
        excludeFlags: PropertySerializationFlags.HasSimpleFormat)]
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
        excludeFlags: PropertySerializationFlags.HasSimpleFormat)]
    public bool? DoDither { get; set; }

    // TN: Present in some Ricochet Xtreme frame files.
    [Property("Dither",
        defaultValue: true,
        includeFlags: PropertySerializationFlags.HasSimpleFormat)]
    internal bool? Dither
    {
        get { return DoDither; }
        set
        {
            DoDither = value;
            Flags |= PropertySerializationFlags.HasSimpleFormat;
        }
    }

    [Property("Change Bit Depth",
        defaultValue: true,
        excludeFlags: PropertySerializationFlags.HasLegacyImageQuality)]
    public bool? ChangeBitDepth { get; set; }

    [Property("Loss Less",
        defaultValue: false,
        includeFlags: PropertySerializationFlags.HasLegacyImageQuality)]
    internal bool? LossLess1
    {
        get { return IsLossless; }
        set
        {
            IsLossless = value;
            Flags |= PropertySerializationFlags.HasLegacyImageQuality;
        }
    }

    [Property("Loss Less 2",
        defaultValue: false,
        excludeFlags: PropertySerializationFlags.HasLegacyImageQuality)]
    internal bool? LossLess2 {
        get => IsLossless;
        set => IsLossless = value;
    }

    public bool? IsLossless { get; set; }

    [Property("Quality",
        defaultValue: 80,
        includeFlags: PropertySerializationFlags.HasLegacyImageQuality)]
    internal int? Quality1
    {
        get { return JpegQuality; }
        set
        {
            JpegQuality = value;
            Flags |= PropertySerializationFlags.HasLegacyImageQuality;
        }
    }

    // TN: Present in some Ricochet Lost Worlds frame files.
    [Property("JPEG Quality",
        defaultValue: 80,
        excludeFlags: PropertySerializationFlags.HasLegacyImageQuality |
            PropertySerializationFlags.HasJpegQuality2)]
    internal int? JpegQuality1
    {
        get => JpegQuality;
        set => JpegQuality = value;
    }

    [Property("JPEG Quality 2",
        defaultValue: 80,
        excludeFlags: PropertySerializationFlags.HasLegacyImageQuality,
        includeFlags: PropertySerializationFlags.HasJpegQuality2)]
    internal int? JpegQuality2
    {
        get { return JpegQuality; }
        set
        {
            if (JpegQuality == null)
            {
                Flags |= PropertySerializationFlags.HasJpegQuality2;
            }
            JpegQuality = value;
        }
    }

    public int? JpegQuality { get; set; }

    [Property("Center Hot Spot",
        defaultValue: false,
        excludeFlags: PropertySerializationFlags.HasSimpleFormat)]
    public bool? CenterHotSpot { get; set; }

    [Property("Blended With Black",
        defaultValue: true,
        excludeFlags: PropertySerializationFlags.HasLegacyImageQuality |
            PropertySerializationFlags.HasSimpleFormat)]
    public bool? BlendedWithBlack { get; set; }

    [Property("Remove Dead Alpha",
        defaultValue: true,
        includeFlags: PropertySerializationFlags.HasLegacyImageQuality,
        excludeFlags: PropertySerializationFlags.HasSimpleFormat)]
    public bool? RemoveDeadAlpha
    {
        get { return _removeDeadAlpha; }
        set
        {
            if (_removeDeadAlpha == null)
            {
                Flags |= PropertySerializationFlags.HasLegacyImageQuality;
            }
            _removeDeadAlpha = value;
        }
    }

    [Property("Remove Black Blending",
        defaultValue: true,
        includeFlags: PropertySerializationFlags.HasLegacyImageQuality,
        excludeFlags: PropertySerializationFlags.HasSimpleFormat)]
    public bool? RemoveBlackBlending
    {
        get { return _removeBlackBlending; }
        set
        {
            if (_removeBlackBlending == null)
            {
                Flags |= PropertySerializationFlags.HasLegacyImageQuality;
            }
            _removeBlackBlending = value;
        }
    }

    [Property("Load Black Biased",
        defaultValue: false,
        excludeFlags: PropertySerializationFlags.HasSimpleFormat)]
    public bool? LoadBlackBiased { get; set; }

    [Property("Final Bit Depth",
        defaultValue: 0,
        includeFlags: PropertySerializationFlags.HasLegacyImageQuality,
        excludeFlags: PropertySerializationFlags.HasSimpleFormat)]
    public int? FinalBitDepth
    {
        get { return _finalBitDepth; }
        set
        {
            if (_finalBitDepth == null)
            {
                Flags |= PropertySerializationFlags.HasLegacyImageQuality;
            }
            _finalBitDepth = value;
        }
    }

    [Property("Blit Type",
        defaultValue: Graphics.BlitType.TransparentMask,
        excludeFlags: PropertySerializationFlags.HasSimpleFormat)]
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
