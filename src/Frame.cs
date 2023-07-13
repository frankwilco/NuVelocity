using ICSharpCode.SharpZipLib.Zip.Compression;
using SixLabors.ImageSharp.Formats;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters;

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

        private void ParseComponent(int layer, byte[] input, byte[] buffer)
        {
            int rawIndex = layer * Width * Height;
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
                        input[rawIndex] += input[rawIndex - 1];
                    }
                    buffer[pixelIndex] = input[rawIndex];

                    pixelIndex++;
                    rawIndex++;
                }
            }
        }

        public Image ToImage()
        {
            Image<Rgba32> image;
            if (!IsCompressed)
            {
                image = Image.Load<Rgba32>(new ReadOnlySpan<byte>(_data));
                Width = image.Width;
                Height = image.Height;

                byte[] temp = new byte[_maskData.Length];
                _maskData.CopyTo(temp, 0);
                byte[] componentData = new byte[Width * Height];
                ParseComponent(0, temp, componentData);

                int pixelIndex = 0;
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                        for (int x = 0; x < pixelRow.Length; x++)
                        {
                            pixelRow[x].A = componentData[pixelIndex++];
                        }
                    }
                });
            }
            else
            {
                byte[] temp = new byte[_data.Length];
                _data.CopyTo(temp, 0);

                Rgba32[] pixelData = new Rgba32[Width * Height];
                Array.Fill(pixelData, new Rgba32());

                for (int layer = 0; layer < 4; layer++)
                {
                    byte[] componentData = new byte[Width * Height];
                    ParseComponent(layer, temp, componentData);

                    for (int pixelIndex = 0; pixelIndex < pixelData.Length; pixelIndex++)
                    {
                        switch (layer)
                        {
                            case 0:
                                pixelData[pixelIndex].R = componentData[pixelIndex];
                                break;
                            case 1:
                                pixelData[pixelIndex].G = componentData[pixelIndex];
                                break;
                            case 2:
                                pixelData[pixelIndex].B = componentData[pixelIndex];
                                break;
                            case 3:
                                pixelData[pixelIndex].A = componentData[pixelIndex];
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                    }
                }

                image = Image.LoadPixelData(new ReadOnlySpan<Rgba32>(pixelData), Width, Height);
            }
            return image;
        }
    }
}
