namespace NuVelocity.Graphics;

public abstract class FrameEncoder : IDisposable
{
    protected Stream? _frameStream;

    protected Stream? _propertiesStream;

    protected bool _disposedValue;

    protected bool _leaveOpen;

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
        Frame = new();
        Reset(true);
    }

    protected virtual void Reset(bool isPartial = false)
    {
        if (!isPartial)
        {
            Frame = new();
        }
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
        Stream? propertiesStream,
        bool leaveOpen = false)
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
        _leaveOpen = leaveOpen;

        Reset();

        if (_propertiesStream != null)
        {
            PropertyListSerializer.Deserialize(_propertiesStream, Frame);
        }

        DecodeRaw();
        IsDoneDecoding = true;
    }

    protected abstract void DecodeRaw();

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Reset(true);
            }

            if (!_leaveOpen)
            {
                _frameStream?.Dispose();
                _propertiesStream?.Dispose();
            }
            _frameStream = null;
            _propertiesStream = null;
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
