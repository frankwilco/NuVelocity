using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace NuVelocity.Rebound;

public class RoundSetUserMadeEncoder : IDisposable
{
    private const byte kFlagKnownVersion = 0x01;

    private Stream? _roundSetStream;

    private readonly Inflater _inflater;

    private bool _disposedValue;

    private bool _leaveOpen;

    public RoundSetUserMadeEncoder()
    {
        _inflater = InflaterPool.Instance.Rent();
        _roundSetStream = null;
        Reset();
    }

    private void Reset()
    {
        //TODO
    }

    public virtual void Decode(
        Stream roundSetStream,
        bool leaveOpen = false)
    {
        _roundSetStream = roundSetStream ??
            throw new ArgumentNullException(nameof(roundSetStream));
        if (roundSetStream.Length == 0)
        {
            throw new ArgumentException(null, nameof(roundSetStream));
        }
        _leaveOpen = leaveOpen;

        Reset();

        using BinaryReader reader = new(_roundSetStream, Encoding.UTF8, true);

        byte version = reader.ReadByte();
        if (version != kFlagKnownVersion)
        {
            throw new NotSupportedException("Unknown round set version.");
        }

        int deflatedSize = reader.ReadInt32();
        int inflatedSize = reader.ReadInt32();
        byte[] roundSetBytes = new byte[inflatedSize];
        _inflater.Reset();
        _inflater.SetInput(reader.ReadBytes(deflatedSize));
        if (_inflater.Inflate(roundSetBytes) != inflatedSize)
        {
            throw new InvalidDataException();
        }

        File.WriteAllBytes("dump.txt", roundSetBytes);

        RoundSetUserMade roundSet = new();
        PropertyListSerializer.Deserialize(roundSetBytes, roundSet);
        File.WriteAllBytes("dump.jpg", roundSet.RoundList[0].Thumbnail);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                InflaterPool.Instance.Return(_inflater);
                Reset();
            }

            if (!_leaveOpen)
            {
                _roundSetStream?.Dispose();
            }
            _roundSetStream = null;
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
