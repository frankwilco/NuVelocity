namespace NuVelocity.Graphics;

[PropertyRoot("CSequence", "Sequence")]
public class Sequence
{
    private bool? _mipmapForNativeVersion;
    private int? _ySort;
    private string? _pokeAudio;
    private bool? _editorOnly;
    private bool? _cropColor0;
    // FIXME: restore to private accessibility.
    internal int? _jpegQuality;
    private bool? _isDds;
    private bool? _needsBuffer;

    public PropertySerializationFlags Flags { get; set; }

    [Property("Comment")]
    [PropertyDynamic]
    public string? Comment { get; set; }

    // TN: Exclusive to Ricochet Lost Worlds and Ricochet Infinity.
    [Property("Sequence of Coordinates")]
    [PropertyDynamic]
    public SequenceOfCoordinates? SequenceOfCoordinates { get; set; }

    // TN: Exclusive to Build In Time and Costume Chaos.
    [Property("Y-Sort")]
    [PropertyInclude(PropertySerializationFlags.HasYSort)]
    [PropertyDynamic]
    public int? YSort
    {
        get { return _ySort; }
        set
        {
            _ySort = value;
            Flags |= PropertySerializationFlags.HasYSort;
        }
    }

    // TN: Exclusive to Build In Time.
    [Property("Poke Audio")]
    [PropertyInclude(PropertySerializationFlags.HasPokeAudio)]
    [PropertyDynamic]
    public string? PokeAudio
    {
        get { return _pokeAudio; }
        set
        {
            _pokeAudio = value;
            Flags |= PropertySerializationFlags.HasPokeAudio;
        }
    }

    // TN: Exclusive to Costume Chaos.
    [Property("Editor Only")]
    [PropertyDynamic]
    [PropertyInclude(PropertySerializationFlags.HasEditorOnly)]
    public bool? EditorOnly
    {
        get { return _editorOnly; }
        set
        {
            _editorOnly = value;
            Flags |= PropertySerializationFlags.HasEditorOnly;
        }
    }

    [Property("Frames Per Second", defaultValue: 15)]
    [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
    public float? FramesPerSecond { get; set; }

    [Property("Blit Type", defaultValue: Graphics.BlitType.TransparentMask)]
    public BlitType? BlitType { get; set; }

    [Property("X Offset", defaultValue: 0)]
    public int? XOffset { get; set; }

    [Property("Y Offset", defaultValue: 0)]
    public int? YOffset { get; set; }

    [Property("Use Every", defaultValue: 1)]
    [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality |
                     PropertySerializationFlags.HasSimpleFormat)]
    public int? UseEvery { get; set; }

    [Property("Always Include Last Frame", defaultValue: false)]
    [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality |
                     PropertySerializationFlags.HasSimpleFormat)]
    public bool? AlwaysIncludeLastFrame { get; set; }

    [Property("Center Hot Spot", defaultValue: true)]
    public bool? CenterHotSpot { get; set; }

    [Property("Blended With Black", defaultValue: true)]
    [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality |
                     PropertySerializationFlags.HasSimpleFormat)]
    public bool? BlendedWithBlack { get; set; }

    [Property("Crop Color 0", defaultValue: true)]
    [PropertyInclude(PropertySerializationFlags.HasFixedCropColor0Name)]
    public bool? CropColor0
    {
        get { return _cropColor0; }
        set
        {
            if (_cropColor0 == null)
            {
                Flags |= PropertySerializationFlags.HasFixedCropColor0Name;
            }
            _cropColor0 = value;
        }
    }

    // FIXME: restore to protected accessibility.
    [Property("Crop Clor 0", defaultValue: true)]
    [PropertyExclude(PropertySerializationFlags.HasFixedCropColor0Name)]
    internal bool? CropClor0
    {
        get { return _cropColor0; }
        set { _cropColor0 = value; }
    }

    [Property("Use 8 Bit Alpha", defaultValue: false)]
    public bool? Use8BitAlpha { get; set; }

    [Property("Run Length Encode", defaultValue: true)]
    [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality |
                     PropertySerializationFlags.HasSimpleFormat)]
    public bool? IsRle { get; set; }

    [Property("Do Dither", defaultValue: true)]
    [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
    public bool? DitherImage { get; set; }

    // TN: Present in Star Trek Away Team sequence files.
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

    [Property("Loss Less", defaultValue: false)]
    [PropertyInclude(PropertySerializationFlags.HasLegacyImageQuality)]
    [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
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
    [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality |
                     PropertySerializationFlags.HasSimpleFormat)]
    public bool? IsLossless { get; set; }

    [Property("Quality", defaultValue: 65)]
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

    [Property("DDS")]
    [PropertyInclude(PropertySerializationFlags.HasDdsSupport)]
    public bool? IsDds
    {
        get { return _isDds; }
        set
        {
            if (_isDds == null)
            {
                Flags |= PropertySerializationFlags.HasDdsSupport;
            }
            _isDds = value;
        }
    }

    [Property("Needs Buffer")]
    [PropertyInclude(PropertySerializationFlags.HasDdsSupport)]
    public bool? NeedsBuffer
    {
        get { return _needsBuffer; }
        set
        {
            if (_needsBuffer == null)
            {
                Flags |= PropertySerializationFlags.HasDdsSupport;
            }
            _needsBuffer = value;
        }
    }

    // TN: Present in Swarm Gold, Ricochet Infinity HD, Big Kahuna Reef 3,
    // Build In Time, and Costume Chaos.
    [Property("Mipmap For Native Version", defaultValue: true)]
    [PropertyInclude(PropertySerializationFlags.HasMipmapSupport |
                     PropertySerializationFlags.HasDdsSupport)]
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

    public Sequence(PropertySerializationFlags flags)
    {
        Flags = flags;
    }

    public Sequence()
        : this(PropertySerializationFlags.None)
    {
    }
}
