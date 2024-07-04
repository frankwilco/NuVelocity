namespace NuVelocity.Graphics;

internal class EncoderHelper
{
    internal static void ApplyRowOffsetAddition(
        int layer, byte[] input, byte[] buffer, int width, int height)
    {
        int rawIndex = layer * width * height;
        int pixelIndex = 0;

        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                if (row == 0 && column == 0)
                {
                    // The base pixel is used as-is.
                }
                else
                {
                    input[rawIndex] += input[rawIndex - 1];
                }
                buffer[pixelIndex] = input[rawIndex];

                pixelIndex++;
                rawIndex++;
            }
        }
    }

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
