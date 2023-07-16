using ICSharpCode.SharpZipLib.Zip.Compression;
using SixLabors.ImageSharp.Formats;
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

            Point hotSpot = new(image.Width / 2, image.Height / 2);
            // The image's center is the hot spot location or
            // it has no defined offset.
            if (((Offset.X + hotSpot.X) == 0 && (Offset.Y + hotSpot.Y) == 0)
                || Offset.X == 0 || Offset.Y == 0)
            {
                return image;
            }

            FrameUtils.OffsetImage(image, Offset);

            return image;
        }

        public byte[] ToBytes()
        {
            return _data;
        }
    }
}
