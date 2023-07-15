﻿using ICSharpCode.SharpZipLib.Zip.Compression;
using SixLabors.ImageSharp.Formats;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters;

namespace NuVelocity.IO
{
    public class Frame
    {
        private const ushort kZlibHeader = 0xDA78;
        private const byte kFlagCompressed = 0x01;

        private const int kOffsetToZlibHeader = 9;
        private const int kOffsetFromZlibHeader = -(kOffsetToZlibHeader + 2);

        // TODO: Seems to be offsets set per byte...
        // b0 Left/X
        // b1 ??
        // b2-b3 no effect
        public byte[] Unknown1 { get; private set; }
        // b0 Top/Y
        // b1 ??
        // b2-b3 no effect
        public byte[] Unknown2 { get; private set; }

        public bool IsCompressed { get; private set; }

        private bool _isLayered;
        private byte[] _data;
        private byte[] _rawMaskData;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Frame(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            Unknown1 = reader.ReadBytes(4);
            Unknown2 = reader.ReadBytes(4);
            IsCompressed = reader.ReadBoolean();
            if (IsCompressed)
            {
                stream.Seek(kOffsetToZlibHeader, SeekOrigin.Current);
                ushort header = reader.ReadUInt16();
                stream.Seek(kOffsetFromZlibHeader, SeekOrigin.Current);
                _isLayered = header == kZlibHeader;
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
                Width = image.Width;
                Height = image.Height;
            }
            return image;
        }

        public byte[] ToBytes()
        {
            return _data;
        }
    }
}
