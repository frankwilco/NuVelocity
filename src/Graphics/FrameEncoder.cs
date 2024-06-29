using System.Diagnostics.CodeAnalysis;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace NuVelocity.Graphics;

public abstract class FrameEncoder
{
    private const byte kFlagCompressed = 0x01;

    private readonly Stream _frameStream;

    private readonly Stream? _propertiesStream;

    private readonly Inflater _inflater;

    public Frame Frame { get; protected set; }

    public FrameFormat Format { get; protected set; }

    public bool IsCompressed { get; protected set; }

    public bool IsPlanar { get; protected set; }

    public int OffsetX { get; protected set; }

    public int OffsetY { get; protected set; }

    public byte[]? ImageData1 { get; protected set; }

    public byte[]? ImageData2 { get; protected set; }

    public int InitialWidth { get; protected set; }

    public int InitialHeight { get; protected set; }

    public FrameEncoder(
        Stream frameStream,
        Stream? propertiesStream,
        FrameFormat format)
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
        ImageData1 = null;
        InitialWidth = 0;
        InitialHeight = 0;

        switch (Format)
        {
            case FrameFormat.Mode1:
                ParseMode1Stream();
                LoadMode1Frame();
                break;
            case FrameFormat.Mode2:
                ParseMode2Stream();
                LoadMode2Frame();
                break;
            case FrameFormat.Mode3:
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
        throw new NotImplementedException();
    }

    protected void ParseMode3Stream()
    {
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
                ImageData1 = new byte[inflatedSize];
                if (_inflater.Inflate(ImageData1) == 0)
                {
                    throw new InvalidDataException();
                }
            }
            else
            {
                rawSize = reader.ReadInt32();
                ImageData1 = reader.ReadBytes(rawSize);
            }

            InitialWidth = reader.ReadInt32();
            InitialHeight = reader.ReadInt32();
            return;
        }

        int _rawSize = reader.ReadInt32();
        ImageData1 = reader.ReadBytes(_rawSize);
        int distanceToEof = (int)(_frameStream.Length - _frameStream.Position);
        if (distanceToEof > 0)
        {
            reader.ReadByte(); // 1 byte padding.
            int maskInflatedSize = reader.ReadInt32();

            _inflater.Reset();
            _inflater.SetInput(reader.ReadBytes(distanceToEof - 5));
            ImageData2 = new byte[maskInflatedSize];
            if (_inflater.Inflate(ImageData2) == 0)
            {
                throw new InvalidDataException();
            }
        }
    }

    protected abstract void LoadMode1Frame();

    protected abstract void LoadMode2Frame();

    protected abstract void LoadMode3Frame();
}
