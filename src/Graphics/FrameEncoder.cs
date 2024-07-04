using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace NuVelocity.Graphics;

public abstract class FrameEncoder
{
    private const byte kFlagCompressed = 0x01;

    private readonly Stream _frameStream;

    private readonly Stream? _propertiesStream;

    private readonly Inflater _inflater;

    public Frame Frame { get; protected set; }

    public EncoderFormat Format { get; protected set; }

    public bool IsCompressed { get; protected set; }

    public bool IsPlanar { get; protected set; }

    public int OffsetX { get; protected set; }

    public int OffsetY { get; protected set; }

    public int BaseWidth { get; protected set; }

    public int BaseHeight { get; protected set; }

    public PixelFormat PixelFormat { get; protected set; }

    public int FormatVersion { get; protected set; }

    public int Unknown1 { get; protected set; }

    public EncoderMode2Flags Mode2Flags { get; protected set; }

    public byte[][]? LayerData { get; protected set; }

    public int LayerCount { get; protected set; }

    public int[][]? LayerPixelOffsets { get; protected set; }

    public FrameEncoder(
        Stream frameStream,
        Stream? propertiesStream,
        EncoderFormat format)
    {
        _frameStream = frameStream ??
                    throw new ArgumentNullException(nameof(frameStream));
        if (frameStream.Length == 0)
        {
            throw new ArgumentException(null, nameof(frameStream));
        }
        _propertiesStream = propertiesStream;
        if (propertiesStream?.Length == 0)
        {
            throw new ArgumentException(null, nameof(propertiesStream));
        }
        _inflater = new();
        Format = format;

        InitializeFrame();
        Initialize();
    }

    [MemberNotNull(nameof(Frame))]
    protected virtual void InitializeFrame()
    {
        Frame = new();
    }

    protected virtual void Initialize()
    {
        if (_propertiesStream != null)
        {
            PropertySerializer.Deserialize(_propertiesStream, Frame);
        }

        IsCompressed = false;
        IsPlanar = false;
        OffsetX = 0;
        OffsetY = 0;
        LayerCount = 0;
        LayerData = null;
        LayerPixelOffsets = null;
        BaseWidth = 0;
        BaseHeight = 0;

        switch (Format)
        {
            case EncoderFormat.Mode1:
                ParseMode1Stream();
                LoadMode1Frame();
                break;
            case EncoderFormat.Mode2:
                ParseMode2Stream();
                LoadMode2Frame();
                break;
            case EncoderFormat.Mode3:
                ParseMode3Stream();
                LoadMode3Frame();
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
        using BinaryReader reader = new(_frameStream);

        // FIXME: check value first before casting to enum.
        FormatVersion = reader.ReadByte();
        PixelFormat = (PixelFormat)reader.ReadByte();
        // XXX: Should the offset values be negative by default?
        OffsetX = -reader.ReadInt16();
        OffsetY = -reader.ReadInt16();
        BaseWidth = reader.ReadInt16();
        BaseHeight = reader.ReadInt16();
        Unknown1 = reader.ReadInt16();
        Mode2Flags = (EncoderMode2Flags)reader.ReadInt32();

        IsCompressed =
            (Mode2Flags & EncoderMode2Flags.FrmUsesRle) > 0;
        bool has5Layers =
            Mode2Flags != EncoderMode2Flags.Frm16With3Layers &&
            Mode2Flags != EncoderMode2Flags.Frm16With3LayersRle;
        LayerCount = has5Layers ? 5 : 3;
        int[] layerSizes = new int[LayerCount];
        for (int i = 0; i < LayerCount; i++)
        {
            layerSizes[i] = reader.ReadInt32();
        }

        LayerData = new byte[LayerCount][];
        LayerPixelOffsets = new int[LayerCount][];
        for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++)
        {
            int layerSize = layerSizes[layerIndex];
            if (layerSize == 0)
            {
                continue;
            }
            int layerSizeInLayer = reader.ReadInt32();
            if (layerSize == layerSizeInLayer)
            {
                layerSize -= 4;
            }
            else
            {
                reader.BaseStream.Seek(-4, SeekOrigin.Current);
            }
            LayerData[layerIndex] = reader.ReadBytes(layerSize);

            int pixelOffsetCount = BaseHeight;
            int pixelOffsetSize = pixelOffsetCount * 4;
            int pixelOffsetBytesRead = 0;
            LayerPixelOffsets[layerIndex] = new int[pixelOffsetCount];
            for (int i = 0; i < pixelOffsetCount; i++)
            {
                LayerPixelOffsets[layerIndex][i] = reader.ReadInt32();
                pixelOffsetBytesRead += 4;
            }
            if (pixelOffsetBytesRead != pixelOffsetSize)
            {
                throw new InvalidDataException();
            }
        }
    }

    protected void ParseMode3Stream()
    {
        PixelFormat = PixelFormat.Rgb888;
        FormatVersion = -1;
        LayerCount = 2;
        LayerData = new byte[LayerCount][];

        using BinaryReader reader = new(_frameStream);

        OffsetX = reader.ReadInt32();
        OffsetY = reader.ReadInt32();
        IsCompressed = reader.ReadBoolean();

        if (IsCompressed)
        {
            IsPlanar = HeaderUtils.CheckDeflateHeader(reader, false);
            int rawSize;
            if (IsPlanar)
            {
                rawSize = reader.ReadByte();
                if (rawSize != kFlagCompressed)
                {
                    throw new InvalidDataException();
                }
                int deflatedSize = reader.ReadInt32();
                int inflatedSize = reader.ReadInt32();

                _inflater.Reset();
                _inflater.SetInput(reader.ReadBytes(deflatedSize));
                LayerData[0] = new byte[inflatedSize];
                if (_inflater.Inflate(LayerData[0]) == 0)
                {
                    throw new InvalidDataException();
                }
            }
            else
            {
                rawSize = reader.ReadInt32();
                LayerData[0] = reader.ReadBytes(rawSize);
            }

            BaseWidth = reader.ReadInt32();
            BaseHeight = reader.ReadInt32();
            return;
        }

        int _rawSize = reader.ReadInt32();
        LayerData[0] = reader.ReadBytes(_rawSize);
        int distanceToEof = (int)(_frameStream.Length - _frameStream.Position);
        if (distanceToEof > 0)
        {
            reader.ReadByte(); // 1 byte padding.
            int maskInflatedSize = reader.ReadInt32();

            _inflater.Reset();
            _inflater.SetInput(reader.ReadBytes(distanceToEof - 5));
            LayerData[1] = new byte[maskInflatedSize];
            if (_inflater.Inflate(LayerData[1]) == 0)
            {
                throw new InvalidDataException();
            }
        }
    }

    protected abstract void LoadMode1Frame();

    protected abstract void LoadMode2Frame();

    protected abstract void LoadMode3Frame();
}
