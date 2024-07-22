using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace NuVelocity.Graphics;

public class Mode3SequenceEncoder : SequenceEncoder, IDisposable
{
    private const byte kSignatureStandard = 0x01;

    protected Inflater _inflater;

    public bool IsDds
    {
        get
        {
            return Sequence.Flags.HasFlag(
                PropertySerializationFlags.HasDdsSupport);
        }
    }

    public bool IsHD { get; protected set; }

    public byte[]? ListData { get; protected set; }

    public byte[]? ImageData { get; protected set; }

    public byte[]? AlphaChannelData { get; protected set; }

    public int? AtlasWidth { get; protected set; }

    public int? AtlasHeight { get; protected set; }

    public SequenceFrameInfoList? SequenceFrameInfoList { get; protected set; }

    public Mode3SequenceEncoder(
        BlitTypeRevision blitTypeRevision = BlitTypeRevision.Type1)
        : base(blitTypeRevision)
    {
        _inflater = InflaterPool.Instance.Rent();
    }

    protected override void Reset(bool disposing = false)
    {
        IsHD = default;
        ListData = null;
        ImageData = null;
        AlphaChannelData = null;
        AtlasWidth = null;
        AtlasHeight = null;
        SequenceFrameInfoList = null;

        base.Reset(disposing);
    }

    protected override void DecodeRaw()
    {
        if (_sequenceStream == null)
        {
            throw new InvalidOperationException();
        }
        using BinaryReader reader = new(_sequenceStream, Encoding.UTF8, true);

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
                    ImageData = reader.ReadBytes(
                        (int)distanceToEof);
                }
            }
            else
            {
                byte unknown1 = reader.ReadByte(); // unknown value
                int imageSize = reader.ReadInt32();
                ImageData = reader.ReadBytes(imageSize);
                AtlasWidth = reader.ReadInt32();
                AtlasHeight = reader.ReadInt32();
            }
        }
        else
        {
            if (IsFont)
            {
                Font = new()
                {
                    FirstAscii = reader.ReadInt32(),
                    LastAscii = reader.ReadInt32(),
                    XHeight = reader.ReadInt32()
                };
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
                    ImageData = new byte[imageInflatedSize];
                    _inflater.SetInput(reader.ReadBytes(imageDeflatedSize));
                    if (_inflater.Inflate(ImageData) != imageInflatedSize)
                    {
                        throw new InvalidDataException();
                    }
                    AtlasWidth = reader.ReadInt32();
                    AtlasHeight = reader.ReadInt32();
                }
                else
                {
                    int imageSize = reader.ReadInt32();
                    ImageData = reader.ReadBytes(imageSize);

                    reader.ReadByte(); // 1-byte padding.
                    int maskInflatedSize = reader.ReadInt32();
                    long distanceToEof = _sequenceStream.Length - _sequenceStream.Position;
                    byte[] rawMaskData = reader.ReadBytes((int)distanceToEof);
                    _inflater.SetInput(rawMaskData);
                    AlphaChannelData = new byte[maskInflatedSize];
                    if (_inflater.Inflate(AlphaChannelData) != maskInflatedSize)
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

    protected override void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                InflaterPool.Instance.Return(_inflater);
            }

            _disposedValue = true;
        }

        base.Dispose(disposing);
    }
}
