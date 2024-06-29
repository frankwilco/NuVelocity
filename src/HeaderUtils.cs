using System.Buffers.Binary;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace NuVelocity;

internal static class HeaderUtils
{
    private const int kDeflateHeaderLength = 2;
    private const int kOffsetToDeflateHeader = 9;
    private const int kOffsetFromDeflateHeader = -(kOffsetToDeflateHeader
        + kDeflateHeaderLength);
    private const int kFontOffsetToDeflateHeader = 12 + kOffsetToDeflateHeader;
    private const int kFontOffsetFromDeflateHeader = -(kFontOffsetToDeflateHeader
        + kDeflateHeaderLength);

    internal static bool CheckDeflateHeader(BinaryReader reader, bool checkFont)
    {
        uint header;
        if (checkFont)
        {
            reader.BaseStream.Seek(kFontOffsetToDeflateHeader,
                SeekOrigin.Current);
            header = BinaryPrimitives.ReverseEndianness(
                reader.ReadUInt16());
            reader.BaseStream.Seek(kFontOffsetFromDeflateHeader,
                SeekOrigin.Current);
        }
        else
        {
            reader.BaseStream.Seek(kOffsetToDeflateHeader,
                SeekOrigin.Current);
            header = BinaryPrimitives.ReverseEndianness(
                reader.ReadUInt16());
            reader.BaseStream.Seek(kOffsetFromDeflateHeader,
                SeekOrigin.Current);
        }
        return IsDeflateHeader(header);
    }

    internal static bool IsDeflateHeader(uint header)
    {
        // Follow SharpZipLib Inflater's logic for checking the header.
        return header % 0x1F == 0 &&
            (header & 0x0f00) == Deflater.DEFLATED << 8;
    }
}