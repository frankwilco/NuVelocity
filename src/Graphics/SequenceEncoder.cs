using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Diagnostics.CodeAnalysis;

namespace NuVelocity.Graphics;

public abstract class SequenceEncoder
{
    private const byte kSignatureStandard = 0x01;

    private readonly Stream _sequenceStream;

    private readonly Stream? _propertiesStream;

    private readonly Inflater _inflater;

    private bool _hasProperties;

    public Sequence Sequence { get; protected set; }

    public SequenceFrameInfoList? SequenceFrameInfoList { get; protected set; }

    public EncoderFormat Format { get; protected set; }

    public BlitTypeRevision BlitTypeRevision { get; protected set; }

    public bool IsCompressed { get; protected set; }

    public bool IsFont { get; protected set; }

    public bool IsHD { get; protected set; }

    public bool IsEmpty { get; protected set; }

    public bool IsDds
    {
        get
        {
            return Sequence.Flags.HasFlag(
                PropertySerializationFlags.HasDdsSupport);
        }
    }

    public byte[]? ListData { get; protected set; }

    public byte[]? ImageData1 { get; protected set; }

    public byte[]? ImageData2 { get; protected set; }

    public int InitialWidth { get; protected set; }

    public int InitialHeight { get; protected set; }

    public SequenceEncoder(
        Stream sequenceStream,
        Stream? propertiesStream,
        EncoderFormat format,
        BlitTypeRevision blitTypeRevision = BlitTypeRevision.Type1)
    {
        _sequenceStream = sequenceStream ??
                    throw new ArgumentNullException(nameof(sequenceStream));
        if (sequenceStream.Length == 0)
        {
            throw new ArgumentException(null, nameof(sequenceStream));
        }
        _propertiesStream = propertiesStream;
        if (propertiesStream?.Length == 0)
        {
            throw new ArgumentException(null, nameof(propertiesStream));
        }
        _inflater = new();
        Format = format;
        BlitTypeRevision = blitTypeRevision;

        InitializeSequence();
        Initialize();
    }

    [MemberNotNull(nameof(Sequence))]
    protected virtual void InitializeSequence()
    {
        Sequence = new();
    }

    protected virtual void Initialize()
    {
        if (_propertiesStream != null)
        {
            _hasProperties = PropertySerializer.Deserialize(_propertiesStream, Sequence);
        }

        _hasProperties = false;
        IsCompressed = false;
        IsFont = false;
        IsHD = false;
        IsEmpty = false;
        ImageData1 = null;
        ImageData2 = null;
        InitialWidth = 0;
        InitialHeight = 0;

        switch (Format)
        {
            case EncoderFormat.Mode1:
                ParseMode1Stream();
                LoadMode1Sequence();
                break;
            case EncoderFormat.Mode2:
                ParseMode2Stream();
                LoadMode2Sequence();
                break;
            case EncoderFormat.Mode3:
                ParseMode3Stream();
                LoadMode3Sequence();
                break;
            default:
                throw new NotSupportedException();
        }
    }

    protected void ParseMode1Stream()
    {
        throw new NotImplementedException();
    }

    protected void ParseMode2Stream()
    {
        throw new NotImplementedException();
    }

    protected void ParseMode3Stream()
    {
        using BinaryReader reader = new(_sequenceStream);

        // Check if the embedded lists are uncompressed.
        bool hasHeader = HeaderUtils.CheckDeflateHeader(reader, false);
        // Check a different location for the deflate header
        // since it's different for fonts.
        if (!hasHeader)
        {
            IsFont = HeaderUtils.CheckDeflateHeader(reader, true);
            IsHD = !IsFont;
        }

        if (IsHD)
        {
            int embeddedListsSize = reader.ReadInt32();
            ListData = reader.ReadBytes(embeddedListsSize);

            _hasProperties = PropertySerializer.Deserialize(ListData, Sequence);

            if (IsDds)
            {
                long distanceToEof = _sequenceStream.Length - _sequenceStream.Position;
                if (distanceToEof == 0)
                {
                    IsEmpty = true;
                }
                else
                {
                    ImageData1 = reader.ReadBytes(
                        (int)distanceToEof);
                }
            }
            else
            {
                byte unknown1 = reader.ReadByte(); // unknown value
                int imageSize = reader.ReadInt32();
                ImageData1 = reader.ReadBytes(imageSize);
                InitialWidth = reader.ReadInt32();
                InitialHeight = reader.ReadInt32();
            }
        }
        else
        {
            if (IsFont)
            {
                int firstAscii = reader.ReadInt32();
                int lastAscii = reader.ReadInt32();
                int lineHeight = reader.ReadInt32();
            }

            int signature = reader.ReadByte();
            if (signature != kSignatureStandard)
            {
                throw new InvalidDataException();
            }

            _inflater.Reset();
            int frameInfoDeflatedSize = reader.ReadInt32();
            int frameInfoInflatedSize = reader.ReadInt32();
            byte[] rawFrameInfo = reader.ReadBytes(frameInfoDeflatedSize);
            _inflater.SetInput(rawFrameInfo);
            ListData = new byte[frameInfoInflatedSize];
            if (_inflater.Inflate(ListData) != frameInfoInflatedSize)
            {
                throw new InvalidDataException();
            }

            if (reader.PeekChar() == -1)
            {
                // No sprite sheet data. This is probably an empty sequence.
                IsEmpty = true;
            }
            else
            {
                _inflater.Reset();
                IsCompressed = reader.ReadBoolean();
                if (IsCompressed)
                {
                    byte unknown1 = reader.ReadByte(); // unknown value
                    int imageDeflatedSize = reader.ReadInt32();
                    int imageInflatedSize = reader.ReadInt32();
                    ImageData1 = new byte[imageInflatedSize];
                    _inflater.SetInput(reader.ReadBytes(imageDeflatedSize));
                    if (_inflater.Inflate(ImageData1) != imageInflatedSize)
                    {
                        throw new InvalidDataException();
                    }
                    InitialWidth = reader.ReadInt32();
                    InitialHeight = reader.ReadInt32();
                }
                else
                {
                    int imageSize = reader.ReadInt32();
                    ImageData1 = reader.ReadBytes(imageSize);

                    reader.ReadByte(); // 1-byte padding.
                    int maskInflatedSize = reader.ReadInt32();
                    long distanceToEof = _sequenceStream.Length - _sequenceStream.Position;
                    byte[] rawMaskData = reader.ReadBytes((int)distanceToEof);
                    _inflater.SetInput(rawMaskData);
                    ImageData2 = new byte[maskInflatedSize];
                    if (_inflater.Inflate(ImageData2) != maskInflatedSize)
                    {
                        throw new InvalidDataException();
                    }
                }
            }

            _hasProperties = PropertySerializer.Deserialize(ListData, Sequence);
        }

        SequenceFrameInfoList = new();
        PropertySerializer.Deserialize(ListData, SequenceFrameInfoList);
        // XXX: Wik and earlier don't provide all the information in
        // the sequence property list. Assume that we're lacking info
        // if JPEG quality is set to 0 or if FPS values don't match.
        bool fpsMissing = Sequence.FramesPerSecond == null;
        bool qualityMissing = Sequence.JpegQuality == null;
        if (_hasProperties)
        {
            if (fpsMissing || qualityMissing)
            {
                Sequence.Flags |= PropertySerializationFlags.Compact;
            }
        }
        else
        {
            // XXX: Assume legacy format is in use.
            Sequence.Flags |= PropertySerializationFlags.HasLegacyImageQuality;
        }

        // Try to take properties from the flags property. However, not
        // all sequence properties are represented in the Flags property.
        Sequence.CenterHotSpot ??= SequenceFrameInfoList.Flags.HasFlag(
                SequenceFlags.CenterHotSpot);
        Sequence.BlendedWithBlack ??= SequenceFrameInfoList.Flags.HasFlag(
                SequenceFlags.BlendedWithBlack);
        Sequence.CropClor0 ??= SequenceFrameInfoList.Flags.HasFlag(
                SequenceFlags.CropColor0);
        Sequence.Use8BitAlpha ??= SequenceFrameInfoList.Flags.HasFlag(
                SequenceFlags.Use8BitAlpha);
        Sequence.IsRle ??= SequenceFrameInfoList.Flags.HasFlag(
            SequenceFlags.RunLengthEncode);
        Sequence.DoDither ??= SequenceFrameInfoList.Flags.HasFlag(
            SequenceFlags.DoDither);
        Sequence.IsLossless ??= SequenceFrameInfoList.Flags.HasFlag(
            SequenceFlags.Lossless);
        Sequence.BlitType ??= BlitTypeConverter.Int32ToType(
                SequenceFrameInfoList.BlitType, BlitTypeRevision);

        if (fpsMissing)
        {
            Sequence.FramesPerSecond = SequenceFrameInfoList.FramesPerSecond;
        }
    }

    protected abstract void LoadMode1Sequence();

    protected abstract void LoadMode2Sequence();

    protected abstract void LoadMode3Sequence();
}
