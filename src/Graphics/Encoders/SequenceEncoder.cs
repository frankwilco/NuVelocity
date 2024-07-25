namespace NuVelocity.Graphics;

public abstract class SequenceEncoder : IDisposable
{
    protected Stream? _sequenceStream;

    protected Stream? _propertiesStream;

    protected bool _hasProperties;

    protected bool _disposedValue;

    protected bool _leaveOpen;

    public Sequence Sequence { get; protected set; }

    public BlitTypeRevision BlitTypeRevision { get; protected set; }

    public bool IsDoneDecoding { get; protected set; }

    public bool IsCompressed { get; protected set; }

    public bool IsFont { get; protected set; }

    public FontBitmap? Font { get; protected set; }

    public bool IsEmpty { get; protected set; }

    public SequenceEncoder(
        BlitTypeRevision blitTypeRevision = BlitTypeRevision.Type1)
    {
        _sequenceStream = null;
        _propertiesStream = null;
        BlitTypeRevision = blitTypeRevision;
        Sequence = new();
        Reset(true);
    }

    protected virtual void Reset(bool isPartial = false)
    {
        if (!isPartial)
        {
            Sequence = new();
        }
        _hasProperties = false;
        IsDoneDecoding = false;
        IsCompressed = false;
        IsFont = false;
        Font = null;
        IsEmpty = false;
    }

    public virtual void Decode(
        Stream sequenceStream,
        Stream? propertiesStream,
        bool leaveOpen = false)
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
        _leaveOpen = leaveOpen;

        Reset();

        if (_propertiesStream != null)
        {
            _hasProperties = PropertyListSerializer.Deserialize(
                _propertiesStream, Sequence);
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
                _sequenceStream?.Dispose();
                _propertiesStream?.Dispose();
            }
            _sequenceStream = null;
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
