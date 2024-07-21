using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NuVelocity.Graphics.ImageSharp;

internal static class SlisHelper
{
    private const byte kSeekCommand = 0b10000000;
    private const byte kSeekMask = 0b01111111;
    private const byte kAppendCommand = 0b01000000;
    private const byte kAppendMask = 0b00111111;
    private const byte kRepeatMask = 0b00111111;

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

    internal static Image<Rgba32> LoadPlanarRgba8888Image(byte[] rawImageData, int width, int height)
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

    internal static Image<Rgba32> LoadInterleavedRgba8888Image(byte[] imageData, int width, int height)
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

    internal static Image LoadCompressedRgb565Image(byte[][]? layerData, int layerCount, int width, int height)
    {
        Rgba32[] pixelData = new Rgba32[width * height];

        var opaqueData =
            (layerData?[0]) ?? throw new InvalidDataException();
        bool has5Layers = layerCount == 5;
        var alphaData = layerData?[has5Layers ? 3 : 1];
        var translucentData = layerData?[has5Layers ? 4 : 2];
        if (alphaData == null && translucentData == null)
        {
            DecodeRleLayer(pixelData, opaqueData, null, true);
        }
        else
        {
            if (alphaData == null || translucentData == null)
            {
                throw new InvalidDataException();
            }

            DecodeRleLayer(pixelData, opaqueData, null, true);
            DecodeRleLayer(pixelData, translucentData, alphaData, false);
        }

        return Image.LoadPixelData(
            new ReadOnlySpan<Rgba32>(pixelData), width, height);
    }

    internal static Image? LoadInterleavedRgb565Image(byte[]? imageData, int width, int height)
    {
        if (imageData == null)
        {
            return null;
        }

        Bgr565[] pixelData = new Bgr565[width * height];

        int pixelIndex = 0;
        int dataIndex = 0;
        while (dataIndex < imageData.Length)
        {
            EncoderHelper.ReadRgb565Pixel(
                imageData,
                dataIndex,
                out float r,
                out float g,
                out float b);
            pixelData[pixelIndex] = new(r, g, b);
            pixelIndex++;
            dataIndex += 2;
        }

        return Image.LoadPixelData(
            new ReadOnlySpan<Bgr565>(pixelData), width, height);
    }

    internal static void DecodeRleLayer(
        Rgba32[] pixelData,
        byte[] layerData,
        byte[]? alphaData,
        bool seekIsFill)
    {
        int pixelIndex = 0;
        int appendLength = 0;
        int alphaIndex = 0;

        for (int i = 0; i < layerData.Length; i++)
        {
            byte current = layerData[i];
            if (appendLength > 0)
            {
                EncoderHelper.ReadRgb565PixelAsRgb888(
                    layerData,
                    i,
                    out byte r,
                    out byte g,
                    out byte b);
                byte a = 255;
                if (alphaData != null)
                {
                    a = alphaData[alphaIndex];
                    alphaIndex++;
                }
                pixelData[pixelIndex] = new(r, g, b, a);
                pixelIndex++;
                i++;
                appendLength--;
            }
            else if ((current & kSeekCommand) == kSeekCommand)
            {
                int seekLength = current & kSeekMask;
                while (seekLength > 0)
                {
                    if (seekIsFill)
                    { 
                        pixelData[pixelIndex] = Color.Transparent;
                    }
                    pixelIndex++;
                    seekLength--;
                }
            }
            else if ((current & kAppendCommand) == kAppendCommand)
            {
                appendLength = current & kAppendMask;
            }
            else
            {
                int repeatCount = current & kRepeatMask;
                while (repeatCount > 0)
                {
                    EncoderHelper.ReadRgb565PixelAsRgb888(
                        layerData,
                        i + 1,
                        out byte r,
                        out byte g,
                        out byte b);
                    byte a = 255;
                    if (alphaData != null)
                    {
                        a = alphaData[alphaIndex];
                    }
                    pixelData[pixelIndex] = new(r, g, b, a);
                    pixelIndex++;
                    repeatCount--;
                }
                if (alphaData != null)
                {
                    alphaIndex++;
                }
                i += 2;
            }
        }
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

    internal static void ApplyHotSpotTransform(
        Image? image,
        bool centerHotSpot,
        int hotSpotX,
        int hotSpotY)
    {
        if (image == null)
        {
            return;
        }

        Point hotSpot = new(hotSpotX, hotSpotY);
        if (centerHotSpot)
        {
            float deltaX = hotSpot.X - image.Width / 2f;
            float deltaY = hotSpot.Y - image.Height / 2f;
            float newWidth = image.Width + 2 * Math.Abs(deltaX);
            float newHeight = image.Height + 2 * Math.Abs(deltaY);
            if (hotSpot.X > 0)
            {
                newWidth += image.Width * 2;
            }
            if (hotSpot.Y > 0)
            {
                newHeight += image.Height * 2;
            }
            Size newSize = new((int)newWidth, (int)newHeight);
            int resultantX = newSize.Width / 2 + hotSpot.X;
            int resultantY = newSize.Height / 2 + hotSpot.Y;
            image.Mutate(source =>
            {
                ResizeOptions options = new()
                {
                    Size = newSize,
                    Mode = ResizeMode.Manual,
                    Sampler = KnownResamplers.NearestNeighbor,
                    PadColor = Color.Transparent,
                    TargetRectangle = new Rectangle(
                        resultantX,
                        resultantY,
                        image.Width,
                        image.Height)
                };
                source.Resize(options);
            });
        }
        // The image's center is the hot spot location or
        // it has no defined offset.
        else if ((hotSpot.X + image.Width / 2 != 0
            || hotSpot.Y + image.Height / 2 != 0)
            && (hotSpot.X != 0 || hotSpot.Y != 0))
        {
            OffsetImage(image, hotSpot);
        }
    }
}
