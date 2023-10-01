using ICSharpCode.SharpZipLib.Zip.Compression;

namespace NuVelocity
{
    [PropertyRoot("CStandAloneFrame", "Stand Alone Frame")]
    public class Frame
    {
        private const byte kFlagCompressed = 0x01;

        private bool _usedLegacyProperty = false;

        public Image Texture { get; set; }

        public int Width => Texture.Width;

        public int Height => Texture.Height;

        public EngineSource Source { get; set; }

        [Property("Comment")]
        [PropertyDynamic]
        public string Comment { get; set; }

        [Property("Run Length Encode")]
        public bool IsRle { get; set; }

        [Property("RLE All Copy")]
        public bool IsRleAllCopy { get; set; }

        [Property("Crop Color 0")]
        public bool CropColor0 { get; set; }

        [Property("Do Dither")]
        public bool DitherImage { get; set; }

        [Property("Change Bit Depth")]
        [PropertyInclude(EngineSource.From2004)]
        public bool ChangeBitDepth { get; set; }

        [Property("Loss Less")]
        [PropertyExclude(EngineSource.From2004)]
        protected bool IsLosslessOld
        {
            get { return IsLossless; }
            set
            {
                IsLossless = value;
                _usedLegacyProperty = true;
            }
        }

        [Property("Loss Less 2")]
        [PropertyInclude(EngineSource.From2004)]
        public bool IsLossless { get; set; }

        [Property("Quality")]
        [PropertyExclude(EngineSource.From2004)]
        protected int QualityOld
        {
            get { return JpegQuality; }
            set
            {
                JpegQuality = value;
                _usedLegacyProperty = true;
            }
        }

        [Property("JPEG Quality 2")]
        [PropertyInclude(EngineSource.From2004)]
        public int JpegQuality { get; set; }

        [Property("Center Hot Spot")]
        public bool CenterHotSpot { get; set; }

        [Property("Blended With Black")]
        [PropertyInclude(EngineSource.From2004)]
        public bool BlendedWithBlack { get; set; }

        [Property("Load Black Biased")]
        [PropertyInclude(EngineSource.From2004)]
        public bool LoadBlackBiased { get; set; }

        [Property("Blit Type")]
        [PropertyInclude(EngineSource.From2004)]
        public BlitType BlitType { get; set; }

        [Property("Mipmap For Native Version")]
        [PropertyInclude(EngineSource.From2008)]
        public bool MipmapForNativeVersion { get; set; }

        public Frame(Image image = null)
        {
            Source = EngineSource.From2008;
            Texture = image;
        }

        internal static Frame FromStream(
            out byte[] imageData,
            out byte[] maskData,
            Stream frameStream,
            Stream propertiesStream = null)
        {
            maskData = null;

            BinaryReader reader = new(frameStream);
            Point offset = new(reader.ReadInt32(), reader.ReadInt32());
            bool isCompressed = reader.ReadBoolean();
            bool isLayered = false;
            int initialWidth = 0;
            int initialHeight = 0;

            if (isCompressed)
            {
                isLayered = FrameUtils.CheckDeflateHeader(reader, false);
                if (isLayered)
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
                    imageData = new byte[inflatedSize];
                    if (inflater.Inflate(imageData) == 0)
                    {
                        throw new InvalidDataException();
                    }
                }
                else
                {
                    int _rawSize = reader.ReadInt32();
                    imageData = reader.ReadBytes(_rawSize);
                }

                initialWidth = reader.ReadInt32();
                initialHeight = reader.ReadInt32();
            }
            else
            {
                int _rawSize = reader.ReadInt32();
                imageData = reader.ReadBytes(_rawSize);
                int distanceToEof = (int)(frameStream.Length - frameStream.Position);
                if (distanceToEof > 0)
                {
                    reader.ReadByte(); // 1 byte padding.
                    int maskInflatedSize = reader.ReadInt32();

                    var inflater = new Inflater();
                    inflater.SetInput(reader.ReadBytes(distanceToEof - 5));
                    maskData = new byte[maskInflatedSize];
                    if (inflater.Inflate(maskData) == 0)
                    {
                        throw new InvalidDataException();
                    }
                }
            }

            Frame frame = new();

            if (propertiesStream != null)
            {
                PropertySerializer.Deserialize(propertiesStream, frame);
            }

            Image image = null;
            if (isCompressed)
            {
                if (isLayered)
                {
                    image = FrameUtils.LoadLayeredRgbaImage(imageData, initialWidth, initialHeight);
                }
                else
                {
                    image = FrameUtils.LoadRgbaImage(imageData, initialWidth, initialHeight);
                }
            }
            else
            {
                image = FrameUtils.LoadJpegImage(imageData, maskData);
            }

            if (frame.CenterHotSpot)
            {
                float deltaX = offset.X - image.Width / 2f;
                float deltaY = offset.Y - image.Height / 2f;
                float newWidth = image.Width + 2 * Math.Abs(deltaX);
                float newHeight = image.Height + 2 * Math.Abs(deltaY);
                if (offset.X > 0)
                {
                    newWidth += image.Width * 2;
                }
                if (offset.Y > 0)
                {
                    newHeight += image.Height * 2;
                }
                Size newSize = new((int)newWidth, (int)newHeight);
                int resultantX = newSize.Width / 2 + offset.X;
                int resultantY = newSize.Height / 2 + offset.Y;
                image.Mutate(source =>
                {
                    ResizeOptions options = new()
                    {
                        Size = newSize,
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
            // The image's center is the hot spot location or
            // it has no defined offset.
            else if ((offset.X + image.Width / 2 != 0
                || offset.Y + image.Height / 2 != 0)
                && (offset.X != 0 || offset.Y != 0))
            {
                FrameUtils.OffsetImage(image, offset);
            }

            frame.Texture = image;

            if (frame._usedLegacyProperty)
            {
                frame.Source = EngineSource.From1998;
            }

            return frame;
        }

        public static Frame FromStream(
            Stream frameStream,
            Stream propertiesStream = null)
        {
            return FromStream(out _, out _, frameStream, propertiesStream);
        }
    }
}
