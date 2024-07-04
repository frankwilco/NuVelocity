namespace NuVelocity.Graphics;

internal class EncoderHelper
{
    internal static void ReadRgb565Pixel(
        byte[] imageData,
        int dataIndex,
        out ushort r,
        out ushort g,
        out ushort b)
    {
        ushort pixel = BitConverter.ToUInt16(imageData, dataIndex);
        r = (ushort)(pixel >> 11);
        g = (ushort)((ushort)(pixel << 5) >> 10);
        b = (ushort)((ushort)(pixel << 11) >> 11);
    }

    internal static void ReadRgb565Pixel(
        byte[] imageData,
        int dataIndex,
        out float r,
        out float g,
        out float b)
    {
        ReadRgb565Pixel(
            imageData,
            dataIndex,
            out ushort ushortR,
            out ushort ushortG,
            out ushort ushortB);
        r = ushortR / 31.0f;
        g = ushortG / 63.0f;
        b = ushortB / 31.0f;
    }

    internal static void ReadRgb565PixelAsRgb888(
        byte[] imageData,
        int dataIndex,
        out byte r,
        out byte g,
        out byte b)
    {
        ReadRgb565Pixel(
            imageData,
            dataIndex,
            out ushort ushortR,
            out ushort ushortG,
            out ushort ushortB);
        r = (byte)((ushortR * 255 + 15) / 31);
        g = (byte)((ushortG * 255 + 31) / 63);
        b = (byte)((ushortB * 255 + 15) / 31);
    }
}
