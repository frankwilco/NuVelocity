namespace NuVelocity.Graphics;

[PropertyRoot("CSequence", typeof(Sequence))]
public class Sequence
{
    internal bool HasFixedCropColor0Name { get; private set; } = false;
    internal bool HasDdsSupport { get; private set; } = false;
    internal bool HasMipmapSupport { get; private set; } = false;

    private bool? _mipmapForNativeVersion;
    private bool? _isDds;
    private bool? _needsBuffer;

    public ImagePropertyListFormat Format { get; set; }

    [Property("Comment",
        isDynamic: true)]
    public string? Comment { get; set; }

    // TN: Exclusive to Lionheart.
    [Property("Menu Position",
        isDynamic: true)]
    public Coordinates? MenuPosition { get; set; }

    // TN: Exclusive to Ricochet Lost Worlds and Ricochet Infinity.
    [Property("Sequence of Coordinates",
        isDynamic: true)]
    public SequenceOfCoordinates? SequenceOfCoordinates { get; set; }

    // TN: Exclusive to Build In Time and Costume Chaos.
    [Property("Y-Sort",
        isDynamic: true)]
    public int? YSort { get; set; }

    // TN: Exclusive to Build In Time.
    [Property("Poke Audio",
        isDynamic: true)]
    public string? PokeAudio { get; set; }

    // TN: Exclusive to Costume Chaos.
    [Property("Editor Only",
        isDynamic: true)]
    public bool? EditorOnly { get; set; }

    [Property("Frames Per Second",
        defaultValue: 15.0f)]
    public float? FramesPerSecond { get; set; }

    [Property("Blit Type",
        defaultValue: Graphics.BlitType.TransparentMask)]
    public BlitType? BlitType { get; set; }

    [Property("X Offset",
        defaultValue: 0)]
    public int? XOffset { get; set; }

    [Property("Y Offset",
        defaultValue: 0)]
    public int? YOffset { get; set; }

    [Property("Use Every",
        defaultValue: 1)]
    public int? UseEvery { get; set; }

    [Property("Always Include Last Frame",
        defaultValue: false)]
    public bool? AlwaysIncludeLastFrame { get; set; }

    [Property("Center Hot Spot",
        defaultValue: true)]
    public bool? CenterHotSpot { get; set; }

    [Property("Blended With Black",
        defaultValue: true)]
    public bool? BlendedWithBlack { get; set; }

    [Property("Crop Color 0",
        defaultValue: true)]
    internal bool? CropColor0
    {
        get { return CropAlphaChannel; }
        set
        {
            if (CropAlphaChannel == null)
            {
                HasFixedCropColor0Name = true; ;
            }
            CropAlphaChannel = value;
        }
    }

    [Property("Crop Clor 0",
        defaultValue: true)]
    internal bool? CropClor0
    {
        get => CropAlphaChannel;
        set => CropAlphaChannel = value;
    }

    public bool? CropAlphaChannel { get; set; }

    [Property("Use 8 Bit Alpha",
        defaultValue: false)]
    public bool? Use8BitAlpha { get; set; }

    [Property("Run Length Encode",
        defaultValue: true)]
    public bool? IsRle { get; set; }

    [Property("Do Dither",
        defaultValue: true)]
    public bool? DoDither { get; set; }

    // TN: Present in Star Trek Away Team sequence files.
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
    internal bool? LossLess2
    {
        get => IsLossless;
        set => IsLossless = value;
    }

    public bool? IsLossless { get; set; }

    [Property("Quality",
        defaultValue: 65)]
    internal int? Quality1
    {
        get { return JpegQuality; }
        set
        {
            JpegQuality = value;
            Format = ImagePropertyListFormat.Format2;
        }
    }

    [Property("Quality2",
        defaultValue: 65)]
    internal int? Quality2
    {
        get => JpegQuality;
        set => JpegQuality = value;
    }

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

    [Property("DDS")]
    public bool? IsDds
    {
        get { return _isDds; }
        set
        {
            if (_isDds == null)
            {
                HasDdsSupport = true;
            }
            _isDds = value;
        }
    }

    [Property("Needs Buffer")]
    public bool? NeedsBuffer
    {
        get { return _needsBuffer; }
        set
        {
            if (_needsBuffer == null)
            {
                HasDdsSupport = true;
            }
            _needsBuffer = value;
        }
    }

    // TN: Present in Swarm Gold, Ricochet Infinity HD, Big Kahuna Reef 3,
    // Build In Time, and Costume Chaos.
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

    public Sequence(ImagePropertyListFormat format)
    {
        Format = format;
    }

    public Sequence()
        : this(ImagePropertyListFormat.Format3)
    {
    }

    #region Serializer methods

    private bool ShouldSerializeFramesPerSecond()
    {
        return Format != ImagePropertyListFormat.Format1;
    }

    private bool ShouldSerializeUseEvery()
    {
        return Format == ImagePropertyListFormat.Format3;
    }

    private bool ShouldSerializeAlwaysIncludeLastFrame()
    {
        return Format == ImagePropertyListFormat.Format3;
    }

    private bool ShouldSerializeBlendedWithBlack()
    {
        return Format == ImagePropertyListFormat.Format3;
    }

    private bool ShouldSerializeCropColor0()
    {
        return HasFixedCropColor0Name;
    }

    private bool ShouldSerializeCropClor0()
    {
        return !HasFixedCropColor0Name;
    }

    private bool ShouldSerializeIsRle()
    {
        return Format == ImagePropertyListFormat.Format3;
    }

    private bool ShouldSerializeDoDither()
    {
        return Format != ImagePropertyListFormat.Format1;
    }

    private bool ShouldSerializeDither()
    {
        return Format == ImagePropertyListFormat.Format1;
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

    private static bool ShouldSerializeQuality2()
    {
        return false;
    }

    private static bool ShouldSerializeJpegQuality1()
    {
        return false;
    }

    private bool ShouldSerializeJpegQuality2()
    {
        return Format == ImagePropertyListFormat.Format3;
    }

    private bool ShouldSerializeIsDds()
    {
        return HasDdsSupport;
    }

    private bool ShouldSerializeNeedsBuffer()
    {
        return HasDdsSupport;
    }

    private bool ShouldSerializeMipmapForNativeVersion()
    {
        return HasDdsSupport || HasMipmapSupport;
    }

    internal string ToDebugString()
    {
        return "[" +
            $"{nameof(HasFixedCropColor0Name)}:{HasFixedCropColor0Name}," +
            $"{nameof(HasDdsSupport)}:{HasDdsSupport}," +
            $"{nameof(HasMipmapSupport)}:{HasMipmapSupport}" +
            "]";
    }

    #endregion
}
