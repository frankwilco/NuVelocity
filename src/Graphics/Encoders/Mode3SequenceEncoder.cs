using System.Diagnostics.CodeAnalysis;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace NuVelocity.Graphics;

public class Mode3SequenceEncoder : SequenceEncoder, IDisposable
{
    private const byte kSignatureStandard = 0x01;

    protected Inflater _inflater;

    public byte? Scan1 { get; protected set; }

    public byte? Scan2 { get; protected set; }

    public bool IsHD { get; protected set; }

    public byte[]? ListData { get; protected set; }

    public byte[]? ImageData { get; protected set; }

    public byte[]? AlphaChannelData { get; protected set; }

    public int? AtlasWidth { get; protected set; }

    public int? AtlasHeight { get; protected set; }

    public SequenceFrameInfoList SequenceFrameInfoList { get; protected set; }

    public Mode3SequenceEncoder(
        BlitTypeRevision blitTypeRevision = BlitTypeRevision.Type1)
        : base(blitTypeRevision)
    {
        _inflater = InflaterPool.Instance.Rent();
        SequenceFrameInfoList = new SequenceFrameInfoList();
    }

    protected override void Reset(bool isPartial = false)
    {
        Scan1 = null;
        Scan2 = null;
        IsHD = default;
        ListData = null;
        ImageData = null;
        AlphaChannelData = null;
        AtlasWidth = null;
        AtlasHeight = null;
        if (!isPartial)
        {
            SequenceFrameInfoList = new();
        }

        base.Reset(isPartial);
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
            DecodeHDHeader(reader);
        }
        else
        {
            DecodeStandardHeader(reader);
        }

        _hasProperties =
            PropertyListSerializer.Deserialize(ListData, Sequence);
        if (!_hasProperties)
        {
            // XXX: Assume legacy format is in use.
            Sequence.Format = ImagePropertyListFormat.Format2;
        }

        bool hasFrameInfoList =
            PropertyListSerializer.Deserialize(ListData, SequenceFrameInfoList);
        if (hasFrameInfoList)
        {
            // Try to take properties from the flags property. However, not
            // all sequence properties are represented in the Flags property.
            SequenceFrameInfoList.CopyTo(Sequence, BlitTypeRevision);
        }
    }

    [MemberNotNull(nameof(ListData))]
    private void DecodeStandardHeader(BinaryReader reader)
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
                Scan1 = reader.ReadByte(); // unknown value
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
                long distanceToEof = reader.BaseStream.Length - reader.BaseStream.Position;
                byte[] rawMaskData = reader.ReadBytes((int)distanceToEof);
                _inflater.SetInput(rawMaskData);
                AlphaChannelData = new byte[maskInflatedSize];
                if (_inflater.Inflate(AlphaChannelData) != maskInflatedSize)
                {
                    throw new InvalidDataException();
                }
            }
        }
    }

    [MemberNotNull(nameof(ListData))]
    private void DecodeHDHeader(BinaryReader reader)
    {
        int embeddedListsSize = reader.ReadInt32();
        ListData = reader.ReadBytes(embeddedListsSize);

        if (Sequence.HasDdsSupport)
        {
            long distanceToEof =
                reader.BaseStream.Length - reader.BaseStream.Position;
            if (distanceToEof == 0)
            {
                IsEmpty = true;
                return;
            }
            ImageData = reader.ReadBytes((int)distanceToEof);
            return;
        }
        Scan2 = reader.ReadByte(); // unknown value
        int imageSize = reader.ReadInt32();
        ImageData = reader.ReadBytes(imageSize);
        AtlasWidth = reader.ReadInt32();
        AtlasHeight = reader.ReadInt32();
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
