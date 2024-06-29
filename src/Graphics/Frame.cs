namespace NuVelocity.Graphics;

[PropertyRoot("CStandAloneFrame", "Stand Alone Frame")]
public class Frame
{
    private bool? _mipmapForNativeVersion;
    private int? _finalBitDepth;
    private bool? _removeBlackBlending;
    private bool? _removeDeadAlpha;
    private int? _jpegQuality;

    public PropertySerializationFlags Flags { get; set; }

    [Property("Comment")]
    [PropertyDynamic]
    public string? Comment { get; set; }

    [Property("Palette")]
    [PropertyDynamic]
    public PaletteHolder? Palette { get; set; }

    [Property("Run Length Encode", defaultValue: true)]
    public bool? IsRle { get; set; }

    [Property("RLE All Copy", defaultValue: false)]
    [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
    public bool? IsRleAllCopy { get; set; }

    [Property("Crop Color 0", defaultValue: true)]
    public bool? CropColor0 { get; set; }

    [Property("Do Dither", defaultValue: true)]
    [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
    public bool? DitherImage { get; set; }

    // TN: Present in some Ricochet Xtreme frame files.
    [Property("Dither", defaultValue: true)]
    [PropertyInclude(PropertySerializationFlags.HasSimpleFormat)]
    protected bool? DitherImageOld
    {
        get { return DitherImage; }
        set
        {
            DitherImage = value;
            Flags |= PropertySerializationFlags.HasSimpleFormat;
        }
    }

    [Property("Change Bit Depth", defaultValue: 1)]
    [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality)]
    public bool? ChangeBitDepth { get; set; }

    [Property("Loss Less", defaultValue: false)]
    [PropertyInclude(PropertySerializationFlags.HasLegacyImageQuality)]
    protected bool? IsLosslessOld
    {
        get { return IsLossless; }
        set
        {
            IsLossless = value;
            Flags |= PropertySerializationFlags.HasLegacyImageQuality;
        }
    }

    [Property("Loss Less 2", defaultValue: false)]
    [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality)]
    public bool? IsLossless { get; set; }

    [Property("Quality", defaultValue: 80)]
    [PropertyInclude(PropertySerializationFlags.HasLegacyImageQuality)]
    protected int? QualityOld
    {
        get { return _jpegQuality; }
        set
        {
            _jpegQuality = value;
            Flags |= PropertySerializationFlags.HasLegacyImageQuality;
        }
    }

    // TN: Present in some Ricochet Lost Worlds frame files.
    [Property("JPEG Quality", defaultValue: 80)]
    [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality |
                     PropertySerializationFlags.HasJpegQuality2)]
    protected int? JpegQualityOld
    {
        get => _jpegQuality;
        set => _jpegQuality = value;
    }

    [Property("JPEG Quality 2", defaultValue: 80)]
    [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality)]
    [PropertyInclude(PropertySerializationFlags.HasJpegQuality2)]
    public int? JpegQuality
    {
        get { return _jpegQuality; }
        set
        {
            if (_jpegQuality == null)
            {
                Flags |= PropertySerializationFlags.HasJpegQuality2;
            }
            _jpegQuality = value;
        }
    }

    [Property("Center Hot Spot", defaultValue: false)]
    [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
    public bool? CenterHotSpot { get; set; }

    [Property("Blended With Black", defaultValue: true)]
    [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality |
                     PropertySerializationFlags.HasSimpleFormat)]
    public bool? BlendedWithBlack { get; set; }

    [Property("Remove Dead Alpha", defaultValue: true)]
    [PropertyInclude(PropertySerializationFlags.HasLegacyImageQuality)]
    [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
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

    [Property("Remove Black Blending", defaultValue: true)]
    [PropertyInclude(PropertySerializationFlags.HasLegacyImageQuality)]
    [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
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

    [Property("Load Black Biased", defaultValue: false)]
    [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
    public bool? LoadBlackBiased { get; set; }

    [Property("Final Bit Depth", defaultValue: 0)]
    [PropertyInclude(PropertySerializationFlags.HasLegacyImageQuality)]
    [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
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

    [Property("Blit Type", defaultValue: Graphics.BlitType.TransparentMask)]
    [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
    public BlitType? BlitType { get; set; }

    [Property("Mipmap For Native Version", defaultValue: true)]
    [PropertyInclude(PropertySerializationFlags.HasMipmapSupport)]
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
