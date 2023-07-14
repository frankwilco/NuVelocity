using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Xml.Linq;

namespace Velocity
{
    public class Sequence
    {
        private const byte kFlagZlibCompression = 0x01;
        private const byte kFlagNoCompression = 0x55;

        public int Flags { get; private set; }

        private byte[] _frameInfo;
        private byte[] _sequenceSpriteSheet;
        private byte[] _maskData;

        public Sequence(Stream stream)
        {
            using BinaryReader reader = new(stream);
            Flags = reader.ReadByte();
            switch (Flags)
            {
                case kFlagZlibCompression:
                    break;
                case kFlagNoCompression:
                    throw new NotImplementedException();
                default:
                    throw new InvalidDataException();
            }

            Inflater inflater = new();
            int frameInfoDeflatedSize = reader.ReadInt32();
            int frameInfoInflatedSize = reader.ReadInt32();
            byte[] rawFrameInfo = reader.ReadBytes(frameInfoDeflatedSize);
            inflater.SetInput(rawFrameInfo);
            _frameInfo = new byte[frameInfoInflatedSize];
            if (inflater.Inflate(_frameInfo) != frameInfoInflatedSize)
            {
                throw new InvalidDataException();
            }
            inflater.Reset();

            reader.ReadByte(); // 1-byte padding.
            int imageSize = reader.ReadInt32();
            _sequenceSpriteSheet = reader.ReadBytes(imageSize);

            reader.ReadByte(); // 1-byte padding.
            int maskInflatedSize = reader.ReadInt32();
            long distanceToEof = stream.Length - stream.Position;
            byte[] rawMaskData = reader.ReadBytes((int)distanceToEof);
            inflater.SetInput(rawMaskData);
            _maskData = new byte[maskInflatedSize];
            if (inflater.Inflate(_maskData) != maskInflatedSize)
            {
                throw new InvalidDataException();
            }
        }

        public Image ToImage()
        {
            Image<Rgba32> image = Image.Load<Rgba32>(new ReadOnlySpan<byte>(_sequenceSpriteSheet));

            byte[] temp = new byte[_maskData.Length];
            _maskData.CopyTo(temp, 0);
            byte[] componentData = new byte[image.Width * image.Height];
            FrameUtils.ParseComponent(0, temp, componentData, image.Width, image.Height);
            image.ProcessPixelRows(FrameUtils.MaskFromByteArray(componentData));

            return image;
        }

        public Image[] ToImages()
        {
            throw new NotImplementedException();
        }
    }
}
