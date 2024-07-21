using System.Diagnostics.CodeAnalysis;

namespace NuVelocity.Graphics;

public abstract class SequenceEncoder
{
    protected Stream? _sequenceStream;

    protected Stream? _propertiesStream;

    protected bool _hasProperties;

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
        Reset();
    }

    [MemberNotNull(nameof(Sequence))]
    protected virtual void Reset()
    {
        Sequence = new();
        _hasProperties = false;
        IsDoneDecoding = false;
        IsCompressed = false;
        IsFont = false;
        IsEmpty = false;
    }

    public virtual void Decode(
        Stream sequenceStream,
        Stream? propertiesStream)
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

        Reset();

        if (_propertiesStream != null)
        {
            _hasProperties = PropertySerializer.Deserialize(
                _propertiesStream, Sequence);
        }

        DecodeRaw();
        IsDoneDecoding = true;
    }

    protected abstract void DecodeRaw();
}
