using ICSharpCode.SharpZipLib.Zip.Compression;
using SixLabors.ImageSharp.Formats;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters;

namespace NuVelocity.IO
{
    public class Frame
    {
        private const byte kFlagCompressed = 0x01;

        public Point Offset { get; private set; }

        public bool IsCompressed { get; private set; }

        private bool _isLayered;
        private byte[] _data;
        private byte[] _rawMaskData;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Frame(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            int offsetX = reader.ReadInt32();
            int offsetY = reader.ReadInt32();
            Offset = new(offsetX, offsetY);
            IsCompressed = reader.ReadBoolean();
            if (IsCompressed)
            {
                _isLayered = FrameUtils.HasDeflateHeader(reader);
                if (_isLayered)
                {
                    int _rawSize = reader.ReadByte();
                    if (_rawSize != kFlagCompressed)
                    {
                        throw new InvalidDataException();
                    }
                    int deflatedSize = reader.ReadInt32();
                    int inflatedSize = reader.ReadInt32();

                    var inflater = new Inflater();
                    inflater.SetInput(reader.ReadBytes(deflatedSize));
                    _data = new byte[inflatedSize];
                    if (inflater.Inflate(_data) == 0)
                    {
                        throw new InvalidDataException();
                    }
                }
                else
                {
                    int _rawSize = reader.ReadInt32();
                    _data = reader.ReadBytes(_rawSize);
                }

                Width = reader.ReadInt32();
                Height = reader.ReadInt32();
            }
            else
            {
                int _rawSize = reader.ReadInt32();
                _data = reader.ReadBytes(_rawSize);
                int distanceToEof = (int)(stream.Length - stream.Position);
                if (distanceToEof <= 0)
                {
                    // EOF.
                    return;
                }
                reader.ReadByte(); // 1 byte padding.
                int maskInflatedSize = reader.ReadInt32();

                var inflater = new Inflater();
                inflater.SetInput(reader.ReadBytes(distanceToEof - 5));
                _rawMaskData = new byte[maskInflatedSize];
                if (inflater.Inflate(_rawMaskData) == 0)
                {
                    throw new InvalidDataException();
                }
            }
        }

        public Image ToImage()
        {
            Image<Rgba32> image;
            if (IsCompressed)
            {
                if (_isLayered)
                {
                    image = FrameUtils.LoadLayeredRgbaImage(_data, Width, Height);
                }
                else
                {
                    image = FrameUtils.LoadRgbaImage(_data, Width, Height);
                }
            }
            else
            {
                image = FrameUtils.LoadJpegImage(_data, _rawMaskData);
                // FIXME: fix width/height handling, display only.
            }

            RawProperty centerHotSpotProp = RawList.Properties
                .FirstOrDefault((property) => property.Name == "Center Hot Spot", null);
            bool centerHotSpot = centerHotSpotProp == null
                ? false
                : ((string)centerHotSpotProp.Value) == "1";

            Size size = new(image.Width, image.Height);
            if (centerHotSpot)
            {
                float deltaX = Offset.X - (image.Width / 2f);
                float deltaY = Offset.Y - (image.Height / 2f);
                float newWidth = image.Width + (2 * Math.Abs(deltaX));
                float newHeight = image.Height + (2 * Math.Abs(deltaY));
                if (Offset.X > 0)
                {
                    newWidth += image.Width * 2;
                }
                if (Offset.Y > 0)
                {
                    newHeight += image.Height * 2;
                }
                size.Width = (int)newWidth;
                size.Height = (int)newHeight;
            }

            Point hotSpot = new(size.Width / 2, size.Height / 2);
            if (centerHotSpot)
            {
                int resultantX = hotSpot.X + Offset.X;
                int resultantY = hotSpot.Y + Offset.Y;
                image.Mutate(source =>
                {
                    ResizeOptions options = new()
                    {
                        Size = size,
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
            else
            {
                // The image's center is the hot spot location or
                // it has no defined offset.
                if (((Offset.X + hotSpot.X) == 0 && (Offset.Y + hotSpot.Y) == 0)
                    || (Offset.X == 0 && Offset.Y == 0))
                {
                    return image;
                }
                else
                {
                    FrameUtils.OffsetImage(image, Offset);
                }
            }

            return image;
        }

        public byte[] ToBytes()
        {
            return _data;
        }

        public RawPropertyList RawList { get; private set; }
        public void ReadPropertiesFromStream(Stream stream)
        {
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            RawList = RawPropertyList.FromBytes(data)
                .FirstOrDefault((property) => property.Name == "CStandAloneFrame", null);
        }

    }
}
