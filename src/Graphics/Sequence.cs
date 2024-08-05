namespace NuVelocity.Graphics;

[PropertyRoot("CSequence", typeof(Sequence))]
public class Sequence
{
    private bool? _mipmapForNativeVersion;
    private int? _ySort;
    private string? _pokeAudio;
    private bool? _editorOnly;
    private bool? _isDds;
    private bool? _needsBuffer;

    public PropertySerializationFlags Flags { get; set; }

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
        isDynamic: true,
        includeFlags: PropertySerializationFlags.HasYSort)]
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
    [Property("Poke Audio",
        isDynamic: true,
        includeFlags: PropertySerializationFlags.HasPokeAudio)]
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
    [Property("Editor Only",
        isDynamic: true,
        includeFlags: PropertySerializationFlags.HasEditorOnly)]
    public bool? EditorOnly
    {
        get { return _editorOnly; }
        set
        {
            _editorOnly = value;
            Flags |= PropertySerializationFlags.HasEditorOnly;
        }
    }

    [Property("Frames Per Second",
        defaultValue: 15.0f,
        excludeFlags: PropertySerializationFlags.ImageFormat1)]
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
        defaultValue: 1,
        excludeFlags: PropertySerializationFlags.ImageFormat2 |
            PropertySerializationFlags.ImageFormat1)]
    public int? UseEvery { get; set; }

    [Property("Always Include Last Frame",
        defaultValue: false,
        excludeFlags: PropertySerializationFlags.ImageFormat2 |
            PropertySerializationFlags.ImageFormat1)]
    public bool? AlwaysIncludeLastFrame { get; set; }

    [Property("Center Hot Spot",
        defaultValue: true)]
    public bool? CenterHotSpot { get; set; }

    [Property("Blended With Black",
        defaultValue: true,
        excludeFlags: PropertySerializationFlags.ImageFormat2 |
            PropertySerializationFlags.ImageFormat1)]
    public bool? BlendedWithBlack { get; set; }

    [Property("Crop Color 0",
        defaultValue: true,
        includeFlags: PropertySerializationFlags.HasFixedCropColor0Name)]
    internal bool? CropColor0
    {
        get { return CropAlphaChannel; }
        set
        {
            if (CropAlphaChannel == null)
            {
                Flags |= PropertySerializationFlags.HasFixedCropColor0Name;
            }
            CropAlphaChannel = value;
        }
    }

    [Property("Crop Clor 0",
        defaultValue: true,
        excludeFlags: PropertySerializationFlags.HasFixedCropColor0Name)]
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
        defaultValue: true,
        excludeFlags: PropertySerializationFlags.ImageFormat2 |
            PropertySerializationFlags.ImageFormat1)]
    public bool? IsRle { get; set; }

    [Property("Do Dither",
        defaultValue: true,
        excludeFlags: PropertySerializationFlags.ImageFormat1)]
    public bool? DoDither { get; set; }

    // TN: Present in Star Trek Away Team sequence files.
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

    [Property("Loss Less",
        defaultValue: false,
        includeFlags: PropertySerializationFlags.ImageFormat2,
        excludeFlags: PropertySerializationFlags.ImageFormat1)]
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
        excludeFlags: PropertySerializationFlags.ImageFormat2 |
            PropertySerializationFlags.ImageFormat1)]
    internal bool? LossLess2
    {
        get => IsLossless;
        set => IsLossless = value;
    }

    public bool? IsLossless { get; set; }

    [Property("Quality",
        defaultValue: 65,
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

    [Property("Quality2",
        defaultValue: 65,
        isTransient: true)]
    internal int? Quality2
    {
        get => JpegQuality;
        set => JpegQuality = value;
    }

    [Property("JPEG Quality",
        defaultValue: 80,
        excludeFlags: PropertySerializationFlags.ImageFormat2 |
            PropertySerializationFlags.ImageFormat3 |
            PropertySerializationFlags.ImageFormat1)]
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

    [Property("DDS",
        includeFlags: PropertySerializationFlags.HasDdsSupport)]
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

    [Property("Needs Buffer",
        includeFlags: PropertySerializationFlags.HasDdsSupport)]
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
    [Property("Mipmap For Native Version",
        defaultValue: true,
        includeFlags: PropertySerializationFlags.HasMipmapSupport |
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
