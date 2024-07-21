using SixLabors.ImageSharp;

namespace NuVelocity.Graphics.ImageSharp;

public static class FrameEncoderExtensions
{
    public static Image? ToImage(this Mode2FrameEncoder encoder)
    {
        if (!encoder.IsDoneDecoding)
        {
            return null;
        }

        if (encoder.PixelFormat != PixelFormat.Rgb565)
        {
            throw new NotImplementedException();
        }
        Image? image;
        if (encoder.IsCompressed)
        {
            image = SlisHelper.LoadCompressedRgb565Image(
                encoder.LayerData,
                encoder.LayerCount,
                encoder.BaseWidth,
                encoder.BaseHeight);
        }
        else
        {
            image = SlisHelper.LoadInterleavedRgb565Image(
                encoder.LayerData?[0],
                encoder.BaseWidth,
                encoder.BaseHeight);
        }
        SlisHelper.ApplyHotSpotTransform(
            image,
            encoder.Frame.CenterHotSpot.GetValueOrDefault(),
            encoder.HotSpotX,
            encoder.HotSpotY);
        return image;
    }

    public static Image? ToImage(this Mode3FrameEncoder encoder)
    {
        if (!encoder.IsDoneDecoding)
        {
            return null;
        }

        if (encoder.LayerData?[0] == null)
        {
            return null;
        }
        Image? image;
        if (encoder.IsCompressed)
        {
            if (encoder.IsPlanar)
            {
                image = SlisHelper.LoadPlanarRgba8888Image(
                    encoder.LayerData[0],
                    encoder.BaseWidth,
                    encoder.BaseHeight);
            }
            else
            {
                image = SlisHelper.LoadInterleavedRgba8888Image(
                    encoder.LayerData[0],
                    encoder.BaseWidth,
                    encoder.BaseHeight);
            }
        }
        else
        {
            image = SlisHelper.LoadJpegImage(
                encoder.LayerData[0],
                encoder.LayerData[1]);
        }
        SlisHelper.ApplyHotSpotTransform(
            image,
            encoder.Frame.CenterHotSpot.GetValueOrDefault(),
            encoder.HotSpotX,
            encoder.HotSpotY);
        return image;
    }
}
