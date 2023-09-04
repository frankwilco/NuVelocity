using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace NuVelocity.IO
{
    public class Sequence
    {
        private const byte kSignatureStandard = 0x01;

        public bool IsCompressed { get; private set; }

        private bool _isHD;
        private bool _isImageDds;
        private bool _isEmpty;

        public byte[] _embeddedLists;
        public byte[] _sequenceSpriteSheet;
        public byte[] _rawMaskData;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Sequence(Stream stream)
        {
            using BinaryReader reader = new(stream);

            // Check if the embedded lists are uncompressed.
            _isHD = !FrameUtils.HasDeflateHeader(reader);
            if (_isHD)
            {
                int embeddedListsSize = reader.ReadInt32();
                _embeddedLists = reader.ReadBytes(embeddedListsSize);
                
                var ddsProperty = RawList.Properties.First((property) => property.Name == "DDS");
                _isImageDds = (ddsProperty.Value as string) == "1";

                if (_isImageDds)
                {
                    long distanceToEof = stream.Length - stream.Position;
                    if (distanceToEof == 0)
                    {
                        _isEmpty = true;
                    }
                    else
                    {
                        _sequenceSpriteSheet = reader.ReadBytes(
                            (int)distanceToEof);
                    }
                }
                else
                {
                    byte unknown1 = reader.ReadByte(); // unknown value
                    int imageSize = reader.ReadInt32();
                    _sequenceSpriteSheet = reader.ReadBytes(imageSize);
                    Width = reader.ReadInt32();
                    Height = reader.ReadInt32();
                }
                return;
            }

            int signature = reader.ReadByte();
            if (signature != kSignatureStandard)
            {
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
                _isEmpty = true;
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
                if (_isEmpty)
                {
                    return new Image<Rgba32>(1, 1);
                }
                return null;
            }

            if (_isHD)
            {
                if (_isImageDds)
                {
                    return null;
                }
                return FrameUtils.LoadRgbaImage(_sequenceSpriteSheet, Width, Height);
            }

            if (IsCompressed)
            {
                return FrameUtils.LoadLayeredRgbaImage(_sequenceSpriteSheet, Width, Height);
            }

            var image = FrameUtils.LoadJpegImage(_sequenceSpriteSheet, _rawMaskData);
            Width = image.Width;
            Height = image.Height;
            return image;
        }

        public Image[] ToImages()
        {
            Image spritesheet = ToImage();
            if (spritesheet == null && !_isImageDds)
            {
                return null;
            }
            if (_isEmpty)
            {
                return new Image[1] { spritesheet };
            }

            RawPropertyList list = RawPropertyList.FromBytes(_embeddedLists)
                .First((property) => property.Name == "CSequenceFrameInfoList");
            var frameInfos = ((RawPropertyList)list.Properties
                .First((property) => property.Name == "Frame Infos"))
                .Properties
                .Where((property) => property.Name == "Frame Info")
                .ToArray();
            var flags = (SequenceFlags)Enum.Parse(
                typeof(SequenceFlags),
                list.Properties.First(
                    (property) => property.Name == "Flags").Value as string);

            int baseXOffset = 0;
            int baseYOffset = 0;
            bool centerHotSpot = flags.HasFlag(SequenceFlags.CenterHotSpot);

            if (RawList != null)
            {
                baseXOffset = int.Parse(RawList.Properties
                    .First((property) => property.Name == "X Offset").Value as string);
                baseYOffset = int.Parse(RawList.Properties
                    .First((property) => property.Name == "Y Offset").Value as string);
            }

            Image[] images = new Image[frameInfos.Length];
            Point[] offsets = new Point[frameInfos.Length];

            int pixelsRead = 0;
            Size maxSize = new();
            Point hotSpot = new();
            for (int i = 0; i < frameInfos.Length; i++)
            {
                int left = 0;
                int top = 0;
                int right = 0;
                int bottom = 0;
                Point offset = new(baseXOffset, baseYOffset);

                var frameInfo = frameInfos[i] as RawPropertyList;
                // Represent empty frames with a 1x1 transparent image.
                if (frameInfo == null)
                {
                    images[i] = new Image<Rgba32>(1, 1);
                    offsets[i] = new Point(0, 0);
                    continue;
                }

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
                            offset.X += value;
                            break;
                        case "UpperLeftYOffset":
                            offset.Y += value;
                            break;
                        default:
                            break;
                    }
                }

                offsets[i] = offset;
                Rectangle cropRect = new(left, top, right - left, bottom - top);
                Image image = null;
                if (_isImageDds)
                {
                    int pixels = cropRect.Width * cropRect.Height;
                    byte[] buffer = new byte[pixels];
                    Buffer.BlockCopy(_sequenceSpriteSheet, pixelsRead, buffer, 0, pixels);

                    BcDecoder decoder = new();
                    image = decoder.DecodeRawToImageRgba32(buffer,
                                          cropRect.Width,
                                          cropRect.Height,
                                          BCnEncoder.Shared.CompressionFormat.Bc3);
                    pixelsRead += pixels;
                }
                else
                {
                    image = spritesheet.Clone(x => x.Crop(cropRect));
                }
                images[i] = image;

                float newWidth = 0;
                float newHeight = 0;
                if (!centerHotSpot)
                {
                    FrameUtils.OffsetImage(image, offset);
                    newWidth = image.Width;
                    newHeight = image.Height;
                }
                else
                {
                    float deltaX = offset.X - (image.Width / 2f);
                    float deltaY = offset.Y - (image.Height / 2f);
                    newWidth = image.Width + (2 * Math.Abs(deltaX));
                    newHeight = image.Height + (2 * Math.Abs(deltaY));
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
                if (centerHotSpot)
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

        private RawPropertyList _properties;
        public RawPropertyList RawList
        {
            get
            {
                if (_embeddedLists == null)
                {
                    return null;
                }

                if (_properties == null)
                {
                    _properties = RawPropertyList.FromBytes(_embeddedLists)
                        .FirstOrDefault((property) => property.Name == "CSequence", null);
                }

                return _properties;
            }
        }
    }
}
