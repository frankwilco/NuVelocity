using ICSharpCode.SharpZipLib.Zip.Compression;
using SixLabors.ImageSharp.Formats;
using System.Net.Http.Headers;

namespace Velocity
{
    public class Frame
    {
        private const byte kFlagCompressed = 0x01;

        // Possibly: state ID
        public int Unknown1 { get; private set; }
        // Possibly: state group
        public int Unknown2 { get; private set; }

        public bool IsCompressed { get; private set; }

        private int _rawSize;
        private int _deflatedSize;
        private int _inflatedSize;

        private byte[] _data;
        private byte[] _maskData;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Frame(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            Unknown1 = reader.ReadInt32();
            Unknown2 = reader.ReadInt32();
            IsCompressed = reader.ReadBoolean();
            if (IsCompressed)
            {
                _rawSize = reader.ReadByte();
                if (_rawSize != kFlagCompressed)
                {
                    throw new InvalidDataException();
                }
                _deflatedSize = reader.ReadInt32();
                _inflatedSize = reader.ReadInt32();

                var inflater = new Inflater();
                inflater.SetInput(reader.ReadBytes(_deflatedSize));
                _data = new byte[_inflatedSize];
                if (inflater.Inflate(_data) == 0)
                {
                    throw new InvalidDataException();
                }

                Width = reader.ReadInt32();
                Height = reader.ReadInt32();
            }
            else
            {
                _rawSize = reader.ReadInt32();
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
                _maskData = new byte[maskInflatedSize];
                if (inflater.Inflate(_maskData) == 0)
                {
                    throw new InvalidDataException();
                }
            }
        }

        public Image ToImage()
        {
            Image image;
            if (!IsCompressed)
            {
                image = Image.Load(new ReadOnlySpan<byte>(_data));
                // FIXME: process mask
            }
            else
            {
                byte[] temp = new byte[_data.Length];
                _data.CopyTo(temp, 0);

                Rgba32[] pixelData = new Rgba32[Width * Height];
                Array.Fill(pixelData, new Rgba32());

                int rawIndex = 0;
                for (int layer = 1; layer <= 4; layer++)
                {
                    int pixelIndex = 0;
                    for (int row = 0; row < Height; row++)
                    {
                        for (int column = 0; column < Width; column++)
                        {
                            if (row == 0 && column == 0)
                            {
                                // The base pixel is used as-is.
                            }
                            else
                            {
                                temp[rawIndex] += temp[rawIndex - 1];
                            }
                            byte component = temp[rawIndex];

                            switch (layer)
                            {
                                case 1:
                                    pixelData[pixelIndex].R = component;
                                    break;
                                case 2:
                                    pixelData[pixelIndex].G = component;
                                    break;
                                case 3:
                                    pixelData[pixelIndex].B = component;
                                    break;
                                case 4:
                                    pixelData[pixelIndex].A = component;
                                    break;
                                default:
                                    throw new InvalidOperationException();
                            }

                            pixelIndex++;
                            rawIndex++;
                        }
                    }
                }

                image = Image.LoadPixelData(new ReadOnlySpan<Rgba32>(pixelData), Width, Height);
            }
            return image;
        }
    }
}
