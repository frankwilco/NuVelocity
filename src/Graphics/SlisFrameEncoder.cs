using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace NuVelocity.Graphics;

public class SlisFrameEncoder : FrameEncoder
{
    private Image? _image;

    public SlisFrameEncoder(
        FrameFormat format,
        Stream frameStream,
        Stream? propertiesStream)
        : base(frameStream, propertiesStream, format)
    {
        ProcessOffsets();
    }

    public SlisFrame SlisFrame
    {
        get => (SlisFrame)Frame;
    }

    protected override void InitializeFrame()
    {
        Frame = new SlisFrame();
    }

    protected override void LoadMode1Frame()
    {
        throw new NotImplementedException();
    }

    protected override void LoadMode2Frame()
    {
        throw new NotImplementedException();
    }

    protected override void LoadMode3Frame()
    {
        if (ImageData1 == null)
        {
            return;
        }

        if (IsCompressed)
        {
            if (IsPlanar)
            {
                _image = SlisHelper.LoadPlanarRgbaImage(ImageData1, InitialWidth, InitialHeight);
                return;
            }
            _image = SlisHelper.LoadInterleavedRgbaImage(ImageData1, InitialWidth, InitialHeight);
            return;
        }

        _image = SlisHelper.LoadJpegImage(ImageData1, ImageData2);
    }

    private void ProcessOffsets()
    {
        if (_image == null)
        {
            return;
        }

        Point offset = new(OffsetX, OffsetY);
        if (Frame.CenterHotSpot.GetValueOrDefault())
        {
            float deltaX = offset.X - _image.Width / 2f;
            float deltaY = offset.Y - _image.Height / 2f;
            float newWidth = _image.Width + 2 * Math.Abs(deltaX);
            float newHeight = _image.Height + 2 * Math.Abs(deltaY);
            if (offset.X > 0)
            {
                newWidth += _image.Width * 2;
            }
            if (offset.Y > 0)
            {
                newHeight += _image.Height * 2;
            }
            Size newSize = new((int)newWidth, (int)newHeight);
            int resultantX = newSize.Width / 2 + offset.X;
            int resultantY = newSize.Height / 2 + offset.Y;
            _image.Mutate(source =>
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
                        _image.Width,
                        _image.Height)
                };
                source.Resize(options);
            });
        }
        // The image's center is the hot spot location or
        // it has no defined offset.
        else if ((offset.X + _image.Width / 2 != 0
            || offset.Y + _image.Height / 2 != 0)
            && (offset.X != 0 || offset.Y != 0))
        {
            SlisHelper.OffsetImage(_image, offset);
        }

        SlisFrame.Texture = _image;
    }
}
