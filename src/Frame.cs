using ICSharpCode.SharpZipLib.Zip.Compression;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace NuVelocity
{
    [PropertyRoot("CStandAloneFrame", "Stand Alone Frame")]
    public class Frame
    {
        private const byte kFlagCompressed = 0x01;

        private bool? _mipmapForNativeVersion;
        private int? _finalBitDepth;
        private bool? _removeBlackBlending;
        private bool? _removeDeadAlpha;
        private int? _jpegQuality;

        public Image Texture { get; set; }

        public int Width => Texture.Width;

        public int Height => Texture.Height;

        public PropertySerializationFlags Flags { get; set; }

        [Property("Comment")]
        [PropertyDynamic]
        public string? Comment { get; set; }

        [Property("Palette")]
        [PropertyDynamic]
        public PaletteHolder? Palette { get; set; }

        [Property("Run Length Encode", defaultValue: true)]
        public bool? IsRle { get; set; }

        [Property("RLE All Copy", defaultValue: false)]
        [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
        public bool? IsRleAllCopy { get; set; }

        [Property("Crop Color 0", defaultValue: true)]
        public bool? CropColor0 { get; set; }

        [Property("Do Dither", defaultValue: true)]
        [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
        public bool? DitherImage { get; set; }

        // TN: Present in some Ricochet Xtreme frame files.
        [Property("Dither", defaultValue: true)]
        [PropertyInclude(PropertySerializationFlags.HasSimpleFormat)]
        protected bool? DitherImageOld
        {
            get { return DitherImage; }
            set
            {
                DitherImage = value;
                Flags |= PropertySerializationFlags.HasSimpleFormat;
            }
        }

        [Property("Change Bit Depth")]
        [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality)]
        public bool? ChangeBitDepth { get; set; }

        [Property("Loss Less", defaultValue: false)]
        [PropertyInclude(PropertySerializationFlags.HasLegacyImageQuality)]
        protected bool? IsLosslessOld
        {
            get { return IsLossless; }
            set
            {
                IsLossless = value;
                Flags |= PropertySerializationFlags.HasLegacyImageQuality;
            }
        }

        [Property("Loss Less 2", defaultValue: false)]
        [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality)]
        public bool? IsLossless { get; set; }

        [Property("Quality", defaultValue: 80)]
        [PropertyInclude(PropertySerializationFlags.HasLegacyImageQuality)]
        protected int? QualityOld
        {
            get { return _jpegQuality; }
            set
            {
                _jpegQuality = value;
                Flags |= PropertySerializationFlags.HasLegacyImageQuality;
            }
        }

        // TN: Present in some Ricochet Lost Worlds frame files.
        [Property("JPEG Quality", defaultValue: 80)]
        [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality |
                         PropertySerializationFlags.HasJpegQuality2)]
        protected int? JpegQualityOld
        {
            get => _jpegQuality;
            set => _jpegQuality = value;
        }

        [Property("JPEG Quality 2", defaultValue: 80)]
        [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality)]
        [PropertyInclude(PropertySerializationFlags.HasJpegQuality2)]
        public int? JpegQuality
        {
            get { return _jpegQuality; }
            set
            {
                if (_jpegQuality == null)
                {
                    Flags |= PropertySerializationFlags.HasJpegQuality2;
                }
                _jpegQuality = value;
            }
        }

        [Property("Center Hot Spot", defaultValue: false)]
        [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
        public bool? CenterHotSpot { get; set; }

        [Property("Blended With Black", defaultValue: true)]
        [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality |
                         PropertySerializationFlags.HasSimpleFormat)]
        public bool? BlendedWithBlack { get; set; }

        [Property("Remove Dead Alpha", defaultValue: true)]
        [PropertyInclude(PropertySerializationFlags.HasLegacyImageQuality)]
        [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
        public bool? RemoveDeadAlpha
        {
            get { return _removeDeadAlpha; }
            set
            {
                if (_removeDeadAlpha == null)
                {
                    Flags |= PropertySerializationFlags.HasLegacyImageQuality;
                }
                _removeDeadAlpha = value;
            }
        }

        [Property("Remove Black Blending", defaultValue: true)]
        [PropertyInclude(PropertySerializationFlags.HasLegacyImageQuality)]
        [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
        public bool? RemoveBlackBlending
        {
            get { return _removeBlackBlending; }
            set
            {
                if (_removeBlackBlending == null)
                {
                    Flags |= PropertySerializationFlags.HasLegacyImageQuality;
                }
                _removeBlackBlending = value;
            }
        }

        [Property("Load Black Biased", defaultValue: false)]
        [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
        public bool? LoadBlackBiased { get; set; }

        [Property("Final Bit Depth", defaultValue: 0)]
        [PropertyInclude(PropertySerializationFlags.HasLegacyImageQuality)]
        [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
        public int? FinalBitDepth
        {
            get { return _finalBitDepth; }
            set
            {
                if (_finalBitDepth == null)
                {
                    Flags |= PropertySerializationFlags.HasLegacyImageQuality;
                }
                _finalBitDepth = value;
            }
        }

        [Property("Blit Type", defaultValue: NuVelocity.BlitType.TransparentMask)]
        [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
        public BlitType? BlitType { get; set; }

        [Property("Mipmap For Native Version", defaultValue: true)]
        [PropertyInclude(PropertySerializationFlags.HasMipmapSupport)]
        public bool? MipmapForNativeVersion
        {
            get { return _mipmapForNativeVersion; }
            set
            {
                if (_mipmapForNativeVersion == null)
                {
                    Flags |= PropertySerializationFlags.HasMipmapSupport;
                }
                _mipmapForNativeVersion = value;
            }
        }

        public Frame(Image image = null)
        {
            Flags = PropertySerializationFlags.None;
            Texture = image;
        }

        internal static Frame FromStream(
            out byte[] imageData,
            out byte[] maskData,
            Stream frameStream,
            Stream propertiesStream = null)
        {
            imageData = null;
            maskData = null;

            if (frameStream.Length == 0)
            {
                return null;
            }

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

            if (frame.CenterHotSpot.GetValueOrDefault())
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
