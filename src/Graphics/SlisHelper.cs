using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NuVelocity.Graphics;

internal static class SlisHelper
{
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

    internal static Image<Rgba32> LoadJpegImage(byte[] imageData, byte[]? rawMaskData = null)
    {
        Image<Rgba32> image = Image.Load<Rgba32>(
            new ReadOnlySpan<byte>(imageData));

        if (rawMaskData != null)
        {
            byte[] maskData = new byte[rawMaskData.Length];
            rawMaskData.CopyTo(maskData, 0);
            byte[] componentData = new byte[image.Width * image.Height];
            EncoderHelper.ApplyRowOffsetAddition(0, maskData, componentData, image.Width, image.Height);
            image.ProcessPixelRows(MaskFromByteArray(componentData));
        }

        return image;
    }

    internal static Image<Rgba32> LoadPlanarRgbaImage(byte[] rawImageData, int width, int height)
    {
        byte[] imageData = new byte[rawImageData.Length];
        rawImageData.CopyTo(imageData, 0);

        Rgba32[] pixelData = new Rgba32[width * height];
        Array.Fill(pixelData, new Rgba32());

        for (int plane = 0; plane < 4; plane++)
        {
            byte[] componentData = new byte[width * height];
            EncoderHelper.ApplyRowOffsetAddition(plane, imageData, componentData, width, height);

            for (int pixelIndex = 0; pixelIndex < pixelData.Length; pixelIndex++)
            {
                switch (plane)
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

    internal static Image<Rgba32> LoadInterleavedRgbaImage(byte[] imageData, int width, int height)
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

    internal static void OffsetImage(Image image, Point offset)
    {
        image.Mutate(source =>
        {
            int growWidth = offset.X;
            int growHeight = offset.Y;
            if (offset.X < 0)
            {
                growWidth = image.Width + offset.X * 2;
            }
            if (offset.Y < 0)
            {
                growHeight = image.Height + offset.Y * 2;
            }
            int newWidth = image.Width + Math.Abs(growWidth);
            int newHeight = image.Height + Math.Abs(growHeight);

            AnchorPositionMode positionMode = AnchorPositionMode.BottomRight;
            if (growWidth >= 0)
            {
                positionMode = AnchorPositionMode.Right;
                if (growHeight >= 0)
                {
                    positionMode = AnchorPositionMode.BottomRight;
                }
                else
                {
                    positionMode = AnchorPositionMode.TopRight;
                }
            }
            else
            {
                positionMode = AnchorPositionMode.Left;
                if (growHeight >= 0)
                {
                    positionMode = AnchorPositionMode.BottomLeft;
                }
                else
                {
                    positionMode = AnchorPositionMode.TopLeft;
                }
            }

            ResizeOptions options = new()
            {
                Position = positionMode,
                Size = new(newWidth, newHeight),
                Mode = ResizeMode.BoxPad,
                Sampler = KnownResamplers.NearestNeighbor,
                PadColor = Color.Transparent
            };
            source.Resize(options);
        });
    }
}
