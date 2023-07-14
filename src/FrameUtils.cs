using System.Xml.Linq;

namespace Velocity
{
    internal static class FrameUtils
    {
        internal static void ParseComponent(
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

        internal static PixelAccessorAction<Rgba32> MaskFromByteArray(byte[] maskData)
        {
            return accessor =>
            {
                int pixelIndex = 0;
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                    for (int x = 0; x < pixelRow.Length; x++)
                    {
                        pixelRow[x].A = maskData[pixelIndex++];
                    }
                }
            };
        }

        internal static Image<Rgba32> LoadJpegImage(byte[] imageData, byte[] rawMaskData = null)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(
                new ReadOnlySpan<byte>(imageData));

            if (rawMaskData != null)
            {
                byte[] maskDataCopy = new byte[rawMaskData.Length];
                rawMaskData.CopyTo(maskDataCopy, 0);
                byte[] componentData = new byte[image.Width * image.Height];
                ParseComponent(0, maskDataCopy, componentData, image.Width, image.Height);
                image.ProcessPixelRows(MaskFromByteArray(componentData));
            }

            return image;
        }
    }
}
