using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NuVelocity.Graphics;

public class SlisSequenceEncoder : SequenceEncoder
{
    public Image? Spritesheet { get; protected set; }

    public SlisSequenceEncoder(
        EncoderFormat format,
        Stream frameStream,
        Stream? propertiesStream)
        : base(frameStream, propertiesStream, format)
    {
    }

    public SlisSequence SlisSequence
    {
        get => (SlisSequence)Sequence;
    }

    protected override void InitializeSequence()
    {
        Sequence = new SlisSequence();
    }

    protected override void LoadMode1Sequence()
    {
        throw new NotImplementedException();
    }

    protected override void LoadMode2Sequence()
    {
        throw new NotImplementedException();
    }

    protected override void LoadMode3Sequence()
    {
        if (ImageData1 == null)
        {
            if (IsEmpty)
            {
                SlisSequence.Textures = new Image[1]
                {
                    new Image<Rgba32>(1, 1)
                };
                return;
            }
            return;
        }
        
        if (IsHD)
        {
            if (!IsDds)
            {
                Spritesheet = SlisHelper.LoadInterleavedRgbaImage(
                    ImageData1, InitialWidth, InitialHeight);
            }
        }
        else if (IsCompressed)
        {
            Spritesheet = SlisHelper.LoadPlanarRgbaImage(
                ImageData1, InitialWidth, InitialHeight);
        }
        else
        {
            Spritesheet = SlisHelper.LoadJpegImage(ImageData1, ImageData2);
            InitialWidth = Spritesheet.Width;
            InitialHeight = Spritesheet.Height;
        }

        // Return early if there's no need to process the image further.
        if (Spritesheet == null && !IsDds)
        {
            return;
        }

        int baseXOffset = Sequence.XOffset ?? 0;
        int baseYOffset = Sequence.YOffset ?? 0;

        if (SequenceFrameInfoList == null)
        {
            throw new InvalidOperationException();
        } 

        Image[] images = new Image[SequenceFrameInfoList.Values.Length];
        Point[] offsets = new Point[SequenceFrameInfoList.Values.Length];

        int pixelsRead = 0;
        Size maxSize = new();
        Point hotSpot = new();
        for (int i = 0; i < SequenceFrameInfoList.Values.Length; i++)
        {
            var frameInfo = SequenceFrameInfoList.Values[i];
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
            if (IsDds)
            {
                int pixels = cropRect.Width * cropRect.Height;
                byte[] buffer = new byte[pixels];
                Buffer.BlockCopy(ImageData1, pixelsRead, buffer, 0, pixels);

                BcDecoder decoder = new();
                image = decoder.DecodeRawToImageRgba32(buffer,
                                      cropRect.Width,
                                      cropRect.Height,
                                      BCnEncoder.Shared.CompressionFormat.Bc3);
                pixelsRead += pixels;
            }
            else
            {
                // XXX: Spritesheet will never be null.
                if (Spritesheet == null)
                {
                    continue;
                }
                image = Spritesheet.Clone(x => x.Crop(cropRect));
            }
            images[i] = image;

            float newWidth = 0;
            float newHeight = 0;
            if (!Sequence.CenterHotSpot.GetValueOrDefault())
            {
                SlisHelper.OffsetImage(image, offset);
                newWidth = image.Width;
                newHeight = image.Height;
            }
            else
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
            if (Sequence.CenterHotSpot.GetValueOrDefault())
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

        SlisSequence.Textures = images;
    }
}
