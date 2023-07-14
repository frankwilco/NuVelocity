using ICSharpCode.SharpZipLib.Zip.Compression;

namespace Velocity
{
    public class Sequence
    {
        private const byte kFlagStandard = 0x01;
        private const byte kFlagHighDefinition = 0x55;

        public int Flags { get; private set; }

        public bool IsCompressed { get; private set; }

        internal byte[] _embeddedLists;
        internal byte[] _sequenceSpriteSheet;
        internal byte[] _rawMaskData;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Sequence(Stream stream)
        {
            using BinaryReader reader = new(stream);
            Flags = reader.ReadByte();
            switch (Flags)
            {
                case kFlagStandard:
                    break;
                case kFlagHighDefinition:
                    throw new NotImplementedException();
                default:
                    throw new InvalidDataException();
            }

            Inflater inflater = new();
            int frameInfoDeflatedSize = reader.ReadInt32();
            int frameInfoInflatedSize = reader.ReadInt32();
            byte[] rawFrameInfo = reader.ReadBytes(frameInfoDeflatedSize);
            inflater.SetInput(rawFrameInfo);
            _embeddedLists = new byte[frameInfoInflatedSize];
            if (inflater.Inflate(_embeddedLists) != frameInfoInflatedSize)
            {
                throw new InvalidDataException();
            }
            inflater.Reset();

            if (reader.PeekChar() == -1)
            {
                // No sprite sheet data. This is probably an empty sequence.
                return;
            }

            IsCompressed = reader.ReadBoolean();
            if (IsCompressed)
            {
                byte unknown1 = reader.ReadByte(); // unknown value
                int imageDeflatedSize = reader.ReadInt32();
                int imageInflatedSize = reader.ReadInt32();
                _sequenceSpriteSheet = new byte[imageInflatedSize];
                inflater.SetInput(reader.ReadBytes(imageDeflatedSize));
                if (inflater.Inflate(_sequenceSpriteSheet) != imageInflatedSize)
                {
                    throw new InvalidDataException();
                }
                Width = reader.ReadInt32();
                Height = reader.ReadInt32();
            }
            else
            {
                int imageSize = reader.ReadInt32();
                _sequenceSpriteSheet = reader.ReadBytes(imageSize);

                reader.ReadByte(); // 1-byte padding.
                int maskInflatedSize = reader.ReadInt32();
                long distanceToEof = stream.Length - stream.Position;
                byte[] rawMaskData = reader.ReadBytes((int)distanceToEof);
                inflater.SetInput(rawMaskData);
                _rawMaskData = new byte[maskInflatedSize];
                if (inflater.Inflate(_rawMaskData) != maskInflatedSize)
                {
                    throw new InvalidDataException();
                }
            }
        }

        public Image ToImage()
        {
            if (_sequenceSpriteSheet == null)
            {
                return null;
            }

            Image<Rgba32> image;
            if (IsCompressed)
            {
                image = FrameUtils.LoadLayeredRgbaImage(_sequenceSpriteSheet, Width, Height);
            }
            else
            {
                image = FrameUtils.LoadJpegImage(_sequenceSpriteSheet, _rawMaskData);
                Width = image.Width;
                Height = image.Height;
            }
            return image;
        }

        public Image[] ToImages()
        {
            throw new NotImplementedException();
        }
    }
}
