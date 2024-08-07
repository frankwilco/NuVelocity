using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NuVelocity.Graphics.ImageSharp;

public static class SequenceEncoderExtensions
{
    public static Image[]? ToImages(this Mode2SequenceEncoder encoder)
    {
        if (!encoder.IsDoneDecoding)
        {
            return null;
        }

        Mode2FrameEncoder[]? frameEncoders = encoder.FrameData;
        if (frameEncoders == null)
        {
            return null;
        }

        Image[] images = new Image[frameEncoders.Length];
        for (int i = 0; i < frameEncoders.Length; i++)
        {
            Mode2FrameEncoder? frameEncoder = frameEncoders[i];
            Image? frameImage = frameEncoder.ToImage();
            if (frameEncoder == null || frameImage == null)
            {
                images[i] = new Image<Rgba32>(1, 1);
                continue;
            }
            images[i] = frameImage;
        }

        return images;
    }

    private static void ThrowIfSpriteAtlasDimensionsAreZero(Mode3SequenceEncoder encoder)
    {
        if (encoder.AtlasWidth == null || encoder.AtlasHeight == null)
        {
            throw new InvalidDataException();
        }
    }

    public static Image? ToSpriteAtlasImage(this Mode3SequenceEncoder encoder)
    {
        if (!encoder.IsDoneDecoding)
        {
            return null;
        }

        if (encoder.ImageData == null)
        {
            return null;
        }

        Image? spriteAtlas = null;
        if (encoder.IsHD)
        {
            if (!encoder.Sequence.HasDdsSupport)
            {
                ThrowIfSpriteAtlasDimensionsAreZero(encoder);
                spriteAtlas = SlisHelper.LoadInterleavedRgba8888Image(
                    encoder.ImageData,
                    encoder.AtlasWidth!.Value,
                    encoder.AtlasHeight!.Value);
            }
        }
        else if (encoder.IsCompressed)
        {
            ThrowIfSpriteAtlasDimensionsAreZero(encoder);
            spriteAtlas = SlisHelper.LoadPlanarRgba8888Image(
                encoder.ImageData,
                encoder.AtlasWidth!.Value,
                encoder.AtlasHeight!.Value);
        }
        else
        {
            spriteAtlas = SlisHelper.LoadJpegImage(
                encoder.ImageData,
                encoder.AlphaChannelData);
        }

        // Return early if there's no need to process the image further.
        if (spriteAtlas == null && !encoder.Sequence.HasDdsSupport)
        {
            return null;
        }

        return spriteAtlas;
    }

    public static Image[]? ToImages(this Mode3SequenceEncoder encoder)
    {
        if (!encoder.IsDoneDecoding)
        {
            return null;
        }

        if (encoder.ImageData == null)
        {
            if (encoder.IsEmpty)
            {
                return new Image[1]
                {
                    new Image<Rgba32>(1, 1)
                };
            }
            return null;
        }

        Image? spriteAtlas = encoder.ToSpriteAtlasImage();

        int baseXOffset = encoder.Sequence.XOffset ?? 0;
        int baseYOffset = encoder.Sequence.YOffset ?? 0;

        if (encoder.SequenceFrameInfoList == null)
        {
            throw new InvalidOperationException();
        } 

        Image[] images = new Image[encoder.SequenceFrameInfoList.Values.Length];
        Point[] offsets = new Point[encoder.SequenceFrameInfoList.Values.Length];

        int pixelsRead = 0;
        Size maxSize = new();
        Point hotSpot = new();
        for (int i = 0; i < encoder.SequenceFrameInfoList.Values.Length; i++)
        {
            var frameInfo = encoder.SequenceFrameInfoList.Values[i];
            // Represent empty frames with a 1x1 transparent image.
            if (frameInfo == null)
            {
                images[i] = new Image<Rgba32>(1, 1);
                offsets[i] = new Point(0, 0);
                continue;
            }

            Point offset = new(
                baseXOffset + frameInfo.UpperLeftXOffset,
                baseYOffset + frameInfo.UpperLeftYOffset);

            offsets[i] = offset;
            Rectangle cropRect = new(
                frameInfo.Left,
                frameInfo.Top,
                frameInfo.Right - frameInfo.Left,
                frameInfo.Bottom - frameInfo.Top);

            Image image;
            if (encoder.Sequence.HasDdsSupport)
            {
                int pixels = cropRect.Width * cropRect.Height;
                byte[] buffer = new byte[pixels];
                Buffer.BlockCopy(encoder.ImageData, pixelsRead, buffer, 0, pixels);

                BcDecoder decoder = new();
                image = decoder.DecodeRawToImageRgba32(buffer,
                                      cropRect.Width,
                                      cropRect.Height,
                                      BCnEncoder.Shared.CompressionFormat.Bc3);
                pixelsRead += pixels;
            }
            else
            {
                // XXX: This should never be null.
                if (spriteAtlas == null)
                {
                    continue;
                }
                image = spriteAtlas.Clone(x => x.Crop(cropRect));
            }
            images[i] = image;

            float newWidth = 0;
            float newHeight = 0;
            if (encoder.Sequence.CenterHotSpot.GetValueOrDefault())
            {
                float deltaX = offset.X - image.Width / 2f;
                float deltaY = offset.Y - image.Height / 2f;
                newWidth = image.Width + 2 * Math.Abs(deltaX);
                newHeight = image.Height + 2 * Math.Abs(deltaY);
                if (offset.X > 0)
                {
                    newWidth += image.Width * 2;
                }
                if (offset.Y > 0)
                {
                    newHeight += image.Height * 2;
                }
            }
            else
            {
                SlisHelper.OffsetImage(image, offset);
                newWidth = image.Width;
                newHeight = image.Height;
            }
            if (newWidth >= maxSize.Width)
            {
                maxSize.Width = (int)newWidth;
                hotSpot.X = maxSize.Width / 2;
            }
            if (newHeight >= maxSize.Height)
            {
                maxSize.Height = (int)newHeight;
                hotSpot.Y = maxSize.Height / 2;
            }
        }

        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];
            Point offset = offsets[i];

            // Case 1: Simple image padding if the hot spot is not centered.
            int resultantX = 0;
            int resultantY = 0;

            // Case 2: The image's position should be adjusted relative
            // to the hot spot location of the frame with the largest
            // dimensions in the sequence.
            if (encoder.Sequence.CenterHotSpot.GetValueOrDefault())
            {
                resultantX = hotSpot.X + offset.X;
                resultantY = hotSpot.Y + offset.Y;
            }

            image.Mutate(source =>
            {
                ResizeOptions options = new()
                {
                    Size = maxSize,
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

        return images;
    }
}
