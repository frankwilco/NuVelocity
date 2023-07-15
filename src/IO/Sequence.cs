﻿using BCnEncoder.Decoder;
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
                    _sequenceSpriteSheet = reader.ReadBytes(
                        (int)(stream.Length - stream.Position));
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

            RawPropertyList list = RawPropertyList.FromBytes(_embeddedLists)
                .First((property) => property.Name == "CSequenceFrameInfoList");
            var frameInfos = ((RawPropertyList)list.Properties
                .First((property) => property.Name == "Frame Infos"))
                .Properties
                .Where((property) => property.Description == "CFrameInfo")
                .ToArray();

            Image[] images = new Image[frameInfos.Length];
            Point[] anchors = new Point[frameInfos.Length];

            int pixelsRead = 0;
            int maxWidth = 0;
            int maxHeight = 0;
            for (int i = 0; i < frameInfos.Length; i++)
            {
                int left = 0;
                int top = 0;
                int right = 0;
                int bottom = 0;
                int anchorX = 0;
                int anchorY = 0;

                var frameInfo = frameInfos[i] as RawPropertyList;
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
                            anchorX = value;
                            break;
                        case "UpperLeftYOffset":
                            anchorY = value;
                            break;
                        default:
                            break;
                    }
                }

                anchors[i] = new Point(anchorX, anchorY);
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

                if (image.Width > maxWidth)
                {
                    maxWidth = image.Width;
                }
                if (image.Height > maxHeight)
                {
                    maxHeight = image.Height;
                }

                images[i] = image;
            }

            Point hotSpot = new(maxWidth / 2, maxHeight / 2);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                Point anchor = anchors[i];

                // Case 1: The image's center is the hot spot location.
                if ((anchor.X + hotSpot.X) == 0 &&
                    (anchor.Y + hotSpot.Y) == 0)
                {
                    continue;
                }

                // Case 2: The origin (0,0) is the hot spot location.
                if (anchor.X == 0 && anchor.Y == 0)
                {
                    int deltaX = anchor.X - (image.Width / 2);
                    int deltaY = anchor.Y - (image.Height / 2);
                    int newWidth = image.Width + (2 * Math.Abs(deltaX));
                    int newHeight = image.Height + (2 * Math.Abs(deltaY));
                    image.Mutate(source =>
                    {
                        ResizeOptions options = new()
                        {
                            Position = AnchorPositionMode.BottomRight,
                            Size = new(newWidth, newHeight),
                            Mode = ResizeMode.BoxPad,
                            Sampler = KnownResamplers.NearestNeighbor,
                            PadColor = Color.Transparent
                        };
                        source.Resize(options);
                    });
                    continue;
                }

                int resultantX = hotSpot.X + anchor.X;
                int resultantY = hotSpot.Y + anchor.Y;

                bool padLeft = resultantX < 0;
                bool padRight = resultantX >= maxWidth;
                bool padTop = resultantY < 0;
                bool padBottom = resultantY >= maxHeight;
                // Case 3: The image's position goes beyond the dimensions
                // of the largest frame in the sequence.
                if (padLeft || padRight || padTop || padBottom)
                {
                    image.Mutate(source =>
                    {
                        int newWidth = image.Width;
                        int newHeight = image.Height;

                        AnchorPositionMode positionMode = AnchorPositionMode.Center;
                        if (padLeft)
                        {
                            newWidth += Math.Abs(resultantX);
                            positionMode = AnchorPositionMode.Left;
                            if (padTop)
                            {
                                newHeight += Math.Abs(resultantY);
                                positionMode = AnchorPositionMode.TopLeft;
                            }
                            else if (padBottom)
                            {
                                newHeight += Math.Abs(maxHeight - resultantY);
                                positionMode = AnchorPositionMode.BottomLeft;
                            }
                        }
                        else if (padRight)
                        {
                            newWidth += Math.Abs(maxWidth - resultantX);
                            positionMode = AnchorPositionMode.Right;
                            if (padTop)
                            {
                                newHeight += Math.Abs(resultantY);
                                positionMode = AnchorPositionMode.TopRight;
                            }
                            else if (padBottom)
                            {
                                newHeight += Math.Abs(maxHeight - resultantY);
                                positionMode = AnchorPositionMode.BottomRight;
                            }
                        }
                        else if (padTop)
                        {
                            newHeight += Math.Abs(resultantY);
                            positionMode = AnchorPositionMode.Top;
                        }
                        else if (padBottom)
                        {
                            newHeight += Math.Abs(maxHeight - resultantY);
                            positionMode = AnchorPositionMode.Bottom;
                        }

                        ResizeOptions options = new()
                        {
                            Position = positionMode,
                            Size = new(newWidth, newHeight),
                            Mode = ResizeMode.BoxPad,
                            Sampler = KnownResamplers.NearestNeighbor,
                            PadColor = Color.Transparent
                        };
                        source.Resize(options);
                    });
                    continue;
                }

                // Case 4: The image's position should be adjusted relative
                // to the hot spot location of the frame with the largest
                // dimensions in the sequence.
                image.Mutate(source =>
                {
                    ResizeOptions options = new()
                    {
                        Size = new Size(maxWidth, maxHeight),
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
