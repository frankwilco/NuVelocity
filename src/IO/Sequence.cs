using ICSharpCode.SharpZipLib.Zip.Compression;

namespace NuVelocity.IO
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
            Image spritesheet = ToImage();
            if (spritesheet == null)
            {
                return null;
            }

            RawPropertyList list = RawPropertyList.FromBytes(_embeddedLists)
                .First((property) => property.Name == "CSequenceFrameInfoList");
            var frameInfos = ((RawPropertyList)list.Properties
                .First((property) => property.Name == "Frame Infos"))
                .Properties
                .Where((property) => property.Description == "CFrameInfo")
                .ToArray();

            Image[] images = new Image[frameInfos.Length];

            for (int i = 0; i < frameInfos.Length; i++)
            {
                var frameInfo = frameInfos[i] as RawPropertyList;

                int left = 0;
                int top = 0;
                int right = 0;
                int bottom = 0;

                foreach (var property in frameInfo.Properties)
                {
                    int value = int.Parse(property.Value as string);
                    switch (property.Name)
                    {
                        case "Left":
                            left = value;
                            break;
                        case "Top":
                            top = value;
                            break;
                        case "Right":
                            right = value;
                            break;
                        case "Bottom":
                            bottom = value;
                            break;
                        case "UpperLeftXOffset":
                        case "UpperLeftYOffset":
                        default:
                            break;
                    }
                }

                Rectangle cropRect = new(left, top, right - left, bottom - top);
                images[i] = spritesheet.Clone(x => x.Crop(cropRect));
            }

            return images;
        }

        public string Serialize()
        {
            if (_embeddedLists == null)
            {
                return null;
            }

            RawPropertyList list = RawPropertyList.FromBytes(_embeddedLists)
                .FirstOrDefault((property) => property.Name == "CSequence", null);

            if (list == null)
            {
                return null;
            }

            return list.Serialize();
        }
    }
}
