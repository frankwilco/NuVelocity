using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NuVelocity.Graphics;

public class SlisFrameEncoder : FrameEncoder
{
    private Image? _image;

    public SlisFrameEncoder(
        EncoderFormat format,
        Stream frameStream,
        Stream? propertiesStream)
        : base(frameStream, propertiesStream, format)
    {
        ProcessOffsets();
        SlisFrame.Texture = _image;
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
        if (PixelFormat != PixelFormat.Rgb565)
        {
            throw new NotImplementedException();
        }

        if (IsCompressed)
        {
            LoadCompressedRgb565Image();
            return;
        }
        LoadInterleavedRgb565Image();
    }

    private void LoadCompressedRgb565Image()
    {
        Rgba32[] pixelData = new Rgba32[BaseWidth * BaseHeight];

        var opaqueData =
            (LayerData?[0]) ?? throw new InvalidDataException();
        bool has5Layers = LayerCount == 5;
        var alphaData = LayerData?[has5Layers ? 3 : 1];
        var translucentData = LayerData?[has5Layers ? 4 : 2];
        if (alphaData == null && translucentData == null)
        {
            SlisHelper.DecodeRleLayer(pixelData, opaqueData, null, true);
        }
        else
        {
            if (alphaData == null || translucentData == null)
            {
                throw new InvalidDataException();
            }

            SlisHelper.DecodeRleLayer(pixelData, opaqueData, null, true);
            SlisHelper.DecodeRleLayer(pixelData, translucentData, alphaData, false);
        }

        _image = Image.LoadPixelData(
            new ReadOnlySpan<Rgba32>(pixelData), BaseWidth, BaseHeight);
    }

    private void LoadInterleavedRgb565Image()
    {
        Bgr565[] pixelData = new Bgr565[BaseWidth * BaseHeight];

        var imageData = LayerData?[0];
        if (imageData == null)
        {
            return;
        }

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

        _image = Image.LoadPixelData(
            new ReadOnlySpan<Bgr565>(pixelData), BaseWidth, BaseHeight);
    }

    protected override void LoadMode3Frame()
    {
        if (LayerData?[0] == null)
        {
            return;
        }

        if (IsCompressed)
        {
            if (IsPlanar)
            {
                _image = SlisHelper.LoadPlanarRgbaImage(LayerData[0], BaseWidth, BaseHeight);
                return;
            }
            _image = SlisHelper.LoadInterleavedRgbaImage(LayerData[0], BaseWidth, BaseHeight);
            return;
        }

        _image = SlisHelper.LoadJpegImage(LayerData[0], LayerData[1]);
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
    }
}
