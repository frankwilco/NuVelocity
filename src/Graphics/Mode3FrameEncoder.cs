using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace NuVelocity.Graphics;

public class Mode3FrameEncoder : FrameEncoder
{
    private const byte kFlagCompressed = 0x01;

    protected readonly Inflater _inflater;

    public bool IsPlanar { get; protected set; }

    public Mode3FrameEncoder()
        : base()
    {
        _inflater = InflaterPool.Instance.Rent();
    }

    protected override void Reset(bool isPartial = false)
    {
        IsPlanar = default;

        base.Reset(isPartial);
    }

    protected override void DecodeRaw()
    {
        if (_frameStream == null)
        {
            throw new InvalidOperationException();
        }
        using BinaryReader reader = new(_frameStream, Encoding.UTF8, true);

        PixelFormat = PixelFormat.Rgb888;
        LayerCount = 2;
        LayerData = new byte[LayerCount][];

        HotSpotX = reader.ReadInt32();
        HotSpotY = reader.ReadInt32();
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
