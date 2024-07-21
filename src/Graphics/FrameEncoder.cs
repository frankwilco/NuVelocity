using System.Diagnostics.CodeAnalysis;

namespace NuVelocity.Graphics;

public abstract class FrameEncoder
{
    protected Stream? _frameStream;

    protected Stream? _propertiesStream;

    public Frame Frame { get; protected set; }

    public bool IsDoneDecoding { get; protected set; }

    public bool IsCompressed { get; protected set; }

    public int HotSpotX { get; protected set; }

    public int HotSpotY { get; protected set; }

    public int BaseWidth { get; protected set; }

    public int BaseHeight { get; protected set; }

    public PixelFormat PixelFormat { get; protected set; }

    public byte[][]? LayerData { get; protected set; }

    public int LayerCount { get; protected set; }

    public FrameEncoder()
    {
        _frameStream = null;
        _propertiesStream = null;
        Reset();
    }

    [MemberNotNull(nameof(Frame))]
    protected virtual void Reset()
    {
        Frame = new();
        IsDoneDecoding = false;
        IsCompressed = default;
        HotSpotX = default;
        HotSpotY = default;
        LayerCount = default;
        LayerData = null;
        BaseWidth = default;
        BaseHeight = default;
    }

    public virtual void Decode(
        Stream frameStream,
        Stream? propertiesStream)
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

        Reset();

        if (_propertiesStream != null)
        {
            PropertySerializer.Deserialize(_propertiesStream, Frame);
        }

        DecodeRaw();
        IsDoneDecoding = true;
    }

    protected abstract void DecodeRaw();
}
