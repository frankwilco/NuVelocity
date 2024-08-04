using System.Text;

namespace NuVelocity.Text;

internal class PropertyListReader : BinaryReader
{
    private const char LF = '\n';
    private const char CR = '\r';

    protected long _streamLength;

    public PropertyListReader(Stream input)
        : base(input)
    {
        _streamLength = input.Length;
    }

    public PropertyListReader(Stream input, Encoding encoding)
        : base(input, encoding)
    {
        _streamLength = input.Length;
    }

    public PropertyListReader(Stream input, Encoding encoding, bool leaveOpen)
        : base(input, encoding, leaveOpen)
    {
        _streamLength = input.Length;
    }

    public bool EndOfStream
    {
        get
        {
            return BaseStream.Position >= BaseStream.Length;
        }
    }

    // XXX: This might be slow since it doesn't do any buffering.
    // It does the job for now, but we might want to revisit it later.
    public string? ReadLine()
    {
        if (EndOfStream)
        {
            return null;
        }

        StringBuilder builder = new();
        while (!EndOfStream)
        {
            char currentChar = ReadChar();
            if (currentChar == LF)
            {
                 break;
            }
            if (currentChar == CR)
            {
                if (PeekChar() != LF)
                {
                    break;
                }
                continue;
            }

            builder.Append(currentChar);
        }

        return builder.ToString();
    }

    public void SkipLine()
    {
        if (EndOfStream)
        {
            return;
        }

        while (!EndOfStream)
        {
            char currentChar = ReadChar();
            if (currentChar == LF)
            {
                break;
            }
            if (currentChar == CR)
            {
                if (PeekChar() != LF)
                {
                    break;
                }
                continue;
            }
        }
    }
}
