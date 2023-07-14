﻿using ICSharpCode.SharpZipLib.Zip.Compression;
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

        private byte[] _data;
        private byte[] _rawMaskData;

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
            if (!IsCompressed)
            {
                image = FrameUtils.LoadJpegImage(_data, _rawMaskData);
                Width = image.Width;
                Height = image.Height;
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
                    FrameUtils.ParseComponent(layer, temp, componentData, Width, Height);

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

        public byte[] ToBytes()
        {
            return _data;
        }
    }
}
