using System.Xml.Linq;

namespace NuVelocity.IO
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
                byte[] maskData = new byte[rawMaskData.Length];
                rawMaskData.CopyTo(maskData, 0);
                byte[] componentData = new byte[image.Width * image.Height];
                ParseComponent(0, maskData, componentData, image.Width, image.Height);
                image.ProcessPixelRows(MaskFromByteArray(componentData));
            }

            return image;
        }

        internal static Image<Rgba32> LoadLayeredRgbaImage(byte[] rawImageData, int width, int height)
        {
            byte[] imageData = new byte[rawImageData.Length];
            rawImageData.CopyTo(imageData, 0);

            Rgba32[] pixelData = new Rgba32[width * height];
            Array.Fill(pixelData, new Rgba32());

            for (int layer = 0; layer < 4; layer++)
            {
                byte[] componentData = new byte[width * height];
                ParseComponent(layer, imageData, componentData, width, height);

                for (int pixelIndex = 0; pixelIndex < pixelData.Length; pixelIndex++)
                {
                    switch (layer)
                    {
                        case 0:
                            pixelData[pixelIndex].R = componentData[pixelIndex];
                            break;
                        case 1:
                            pixelData[pixelIndex].G = componentData[pixelIndex];
                            break;
                        case 2:
                            pixelData[pixelIndex].B = componentData[pixelIndex];
                            break;
                        case 3:
                            pixelData[pixelIndex].A = componentData[pixelIndex];
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            return Image.LoadPixelData(
                new ReadOnlySpan<Rgba32>(pixelData), width, height);
        }

        internal static Image<Rgba32> LoadRgbaImage(byte[] imageData, int width, int height)
        {
            Rgba32[] pixelData = new Rgba32[width * height];

            int pixelIndex = 0;
            int dataIndex = 0;
            while (dataIndex < imageData.Length)
            {
                pixelData[pixelIndex] = new Rgba32(
                    imageData[dataIndex],
                    imageData[dataIndex + 1],
                    imageData[dataIndex + 2],
                    imageData[dataIndex + 3]);
                dataIndex += 4;
                pixelIndex++;
            }

            return Image.LoadPixelData(
                new ReadOnlySpan<Rgba32>(pixelData), width, height);
        }
    }
}
