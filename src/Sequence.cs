using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace NuVelocity
{
    [PropertyRoot("CSequence", "Sequence")]
    public class Sequence
    {
        private const byte kSignatureStandard = 0x01;

        private bool _hasLegacyProperty = false;
        private bool _hasTypoProperty = false;
        private bool _hasMipmapProperty = false;

        public Image[] Textures { get; set; }

        public int Width { get; protected set; }

        public int Height { get; protected set; }

        public EngineSource Source { get; set; }

        [Property("Comment")]
        [PropertyDynamic]
        public string Comment { get; set; }

        [Property("Sequence of Coordinates")]
        [PropertyDynamic]
        public SequenceOfCoordinates SequenceOfCoordinates { get; set; }

        [Property("Y-Sort")]
        [PropertyInclude(EngineSource.From2008)]
        [PropertyDynamic]
        public int? YSort { get; set; }

        [Property("Poke Audio")]
        [PropertyInclude(EngineSource.From2008)]
        [PropertyDynamic]
        public string PokeAudio { get; set; }

        [Property("Editor Only")]
        [PropertyInclude(EngineSource.From2009)]
        [PropertyDynamic]
        public bool? EditorOnly { get; set;}

        [Property("Frames Per Second")]
        public int FramesPerSecond { get; set; }

        [Property("Blit Type")]
        public BlitType BlitType { get; set; }

        [Property("X Offset")]
        public int XOffset { get; set; }

        [Property("Y Offset")]
        public int YOffset { get; set; }

        [Property("Use Every")]
        [PropertyInclude(EngineSource.From2004)]
        public int UseEvery { get; set; }

        [Property("Always Include Last Frame")]
        [PropertyInclude(EngineSource.From2004)]
        public bool AlwaysIncludeLastFrame { get; set; }

        [Property("Center Hot Spot")]
        public bool CenterHotSpot { get; set; }

        [Property("Blended With Black")]
        public bool BlendedWithBlack { get; set; }

        [Property("Crop Color 0")]
        [PropertyInclude(EngineSource.From2009)]
        public bool CropColor0 { get; set; }

        [Property("Crop Clor 0")]
        [PropertyExclude(EngineSource.From2009)]
        protected bool CropClor0
        {
            get { return CropColor0; }
            set
            {
                CropColor0 = value;
                _hasTypoProperty = true;
            }
        }

        [Property("Use 8 Bit Alpha")]
        public bool Use8BitAlpha { get; set; }

        [Property("Run Length Encode")]
        [PropertyInclude(EngineSource.From2004)]
        public bool IsRle { get; set; }

        [Property("Do Dither")]
        public bool DitherImage { get; set; }

        [Property("Loss Less")]
        [PropertyExclude(EngineSource.From2004)]
        protected bool IsLosslessOld
        {
            get { return IsLossless; }
            set
            {
                IsLossless = value;
                _hasLegacyProperty = true;
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
                _hasLegacyProperty = true;
            }
        }

        [Property("JPEG Quality 2")]
        [PropertyInclude(EngineSource.From2004)]
        public int JpegQuality { get; set; }

        [Property("DDS")]
        [PropertyInclude(EngineSource.FromPS3)]
        public bool IsDds { get; set; }

        [Property("Needs Buffer")]
        [PropertyInclude(EngineSource.FromPS3)]
        public bool NeedsBuffer { get; set; }

        private bool _mipmapForNativeVersion;
        [Property("Mipmap For Native Version")]
        [PropertyInclude(EngineSource.From2008)]
        public bool MipmapForNativeVersion
        {
            get { return _mipmapForNativeVersion; }
            set
            {
                _mipmapForNativeVersion = value;
                _hasMipmapProperty = true;
            }
        }

        internal static Sequence FromStream(
            out byte[] embeddedLists,
            out byte[] sequenceSpriteSheet,
            out byte[] maskData,
            out Image spritesheet,
            Stream sequenceStream,
            Stream propertiesStream = null)
        {
            Sequence sequence = new();

            bool hasProperties = false;
            bool isCompressed = false;
            bool isFont = false;
            bool isHD = false;
            bool isEmpty = false;
            sequenceSpriteSheet = null;
            maskData = null;
            int atlasWidth = 0;
            int atlasHeight = 0;

            if (propertiesStream != null)
            {
                hasProperties = PropertySerializer.Deserialize(propertiesStream, sequence);
            }

            using BinaryReader reader = new(sequenceStream);

            // Check if the embedded lists are uncompressed.
            bool hasHeader = FrameUtils.CheckDeflateHeader(reader, false);
            // Check a different location for the deflate header
            // since it's different for fonts.
            if (!hasHeader)
            {
                isFont = FrameUtils.CheckDeflateHeader(reader, true);
                isHD = !isFont;
            }

            if (isHD)
            {
                int embeddedListsSize = reader.ReadInt32();
                embeddedLists = reader.ReadBytes(embeddedListsSize);

                hasProperties = PropertySerializer.Deserialize(embeddedLists, sequence);

                if (sequence.IsDds)
                {
                    long distanceToEof = sequenceStream.Length - sequenceStream.Position;
                    if (distanceToEof == 0)
                    {
                        isEmpty = true;
                    }
                    else
                    {
                        sequenceSpriteSheet = reader.ReadBytes(
                            (int)distanceToEof);
                    }
                }
                else
                {
                    byte unknown1 = reader.ReadByte(); // unknown value
                    int imageSize = reader.ReadInt32();
                    sequenceSpriteSheet = reader.ReadBytes(imageSize);
                    atlasWidth = reader.ReadInt32();
                    atlasHeight = reader.ReadInt32();
                }
            }
            else
            {
                if (isFont)
                {
                    int firstAscii = reader.ReadInt32();
                    int lastAscii = reader.ReadInt32();
                    int lineHeight = reader.ReadInt32();
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
                embeddedLists = new byte[frameInfoInflatedSize];
                if (inflater.Inflate(embeddedLists) != frameInfoInflatedSize)
                {
                    throw new InvalidDataException();
                }
                inflater.Reset();

                if (reader.PeekChar() == -1)
                {
                    // No sprite sheet data. This is probably an empty sequence.
                    isEmpty = true;
                }
                else
                {
                    isCompressed = reader.ReadBoolean();
                    if (isCompressed)
                    {
                        byte unknown1 = reader.ReadByte(); // unknown value
                        int imageDeflatedSize = reader.ReadInt32();
                        int imageInflatedSize = reader.ReadInt32();
                        sequenceSpriteSheet = new byte[imageInflatedSize];
                        inflater.SetInput(reader.ReadBytes(imageDeflatedSize));
                        if (inflater.Inflate(sequenceSpriteSheet) != imageInflatedSize)
                        {
                            throw new InvalidDataException();
                        }
                        atlasWidth = reader.ReadInt32();
                        atlasHeight = reader.ReadInt32();
                    }
                    else
                    {
                        int imageSize = reader.ReadInt32();
                        sequenceSpriteSheet = reader.ReadBytes(imageSize);

                        reader.ReadByte(); // 1-byte padding.
                        int maskInflatedSize = reader.ReadInt32();
                        long distanceToEof = sequenceStream.Length - sequenceStream.Position;
                        byte[] rawMaskData = reader.ReadBytes((int)distanceToEof);
                        inflater.SetInput(rawMaskData);
                        maskData = new byte[maskInflatedSize];
                        if (inflater.Inflate(maskData) != maskInflatedSize)
                        {
                            throw new InvalidDataException();
                        }
                    }
                }

                hasProperties = PropertySerializer.Deserialize(embeddedLists, sequence);
            }

            spritesheet = null;
            if (sequenceSpriteSheet == null)
            {
                if (isEmpty)
                {
                    spritesheet = new Image<Rgba32>(1, 1);
                }
            }
            else if (isHD)
            {
                if (!sequence.IsDds)
                {
                    spritesheet = FrameUtils.LoadRgbaImage(sequenceSpriteSheet, atlasWidth, atlasHeight);
                }
            }
            else if (isCompressed)
            {
                spritesheet = FrameUtils.LoadLayeredRgbaImage(sequenceSpriteSheet, atlasWidth, atlasHeight);
            }
            else
            {
                spritesheet = FrameUtils.LoadJpegImage(sequenceSpriteSheet, maskData);
                atlasWidth = spritesheet.Width;
                atlasHeight = spritesheet.Height;
            }

            SequenceFrameInfoList frameInfoList = new();
            PropertySerializer.Deserialize(embeddedLists, frameInfoList);
            // XXX: Wik and earlier don't provide all the information in
            // the sequence property list. Assume that we're lacking info
            // if JPEG quality is set to 0.
            bool likelyIncomplete = sequence.JpegQuality == 0;

            // Try to take properties from the flags property. However, not
            // all sequence properties are represented in the Flags property.
            if (!hasProperties || likelyIncomplete)
            {
                sequence.CenterHotSpot = frameInfoList.Flags.HasFlag(
                    SequenceFlags.CenterHotSpot);
                sequence.BlendedWithBlack = frameInfoList.Flags.HasFlag(
                    SequenceFlags.BlendedWithBlack);
                sequence.CropClor0 = frameInfoList.Flags.HasFlag(
                    SequenceFlags.CropColor0);
                sequence.Use8BitAlpha = frameInfoList.Flags.HasFlag(
                    SequenceFlags.Use8BitAlpha);
                sequence.IsRle = frameInfoList.Flags.HasFlag(
                    SequenceFlags.RunLengthEncode);
                sequence.DitherImage = frameInfoList.Flags.HasFlag(
                    SequenceFlags.DoDither);
                sequence.IsLossless = frameInfoList.Flags.HasFlag(
                    SequenceFlags.Lossless);
                sequence.FramesPerSecond = frameInfoList.FramesPerSecond;
                sequence.BlitType = frameInfoList.BlitTypeEnum;
                // XXX: Assume maximum image quality.
                sequence.JpegQuality = 100;
            }

            // Determine engine source based on a few indicators.
            if (!hasProperties || sequence._hasLegacyProperty)
            {
                sequence.Source = EngineSource.From1998;
            }
            else if (isHD)
            {
                sequence.Source = EngineSource.FromPS3;
            }
            else if (sequence._hasTypoProperty)
            {
                sequence.Source = EngineSource.From2004;
                if (sequence._hasMipmapProperty)
                {
                    sequence.Source = EngineSource.From2008;
                }
            }
            else
            {
                sequence.Source = EngineSource.From2009;
            }

            // Return early if there's no need to process the image further.
            if (spritesheet == null && !sequence.IsDds)
            {
                return sequence;
            }
            else if (isEmpty)
            {
                sequence.Textures = new Image[1] { spritesheet };
                return sequence;
            }

            int baseXOffset = sequence.XOffset;
            int baseYOffset = sequence.YOffset;
            bool centerHotSpot = frameInfoList.Flags.HasFlag(SequenceFlags.CenterHotSpot);

            Image[] images = new Image[frameInfoList.Values.Length];
            Point[] offsets = new Point[frameInfoList.Values.Length];

            int pixelsRead = 0;
            Size maxSize = new();
            Point hotSpot = new();
            for (int i = 0; i < frameInfoList.Values.Length; i++)
            {
                var frameInfo = frameInfoList.Values[i];
                // Represent empty frames with a 1x1 transparent image.
                if (frameInfo == null)
                {
                    images[i] = new Image<Rgba32>(1, 1);
                    offsets[i] = new Point(0, 0);
                    continue;
                }

                Point offset = new(
                    baseXOffset + frameInfo.UpperLeftXOffset,
                    baseYOffset + frameInfo.UpperLeftYOffset);

                offsets[i] = offset;
                Rectangle cropRect = new(
                    frameInfo.Left,
                    frameInfo.Top,
                    frameInfo.Right - frameInfo.Left,
                    frameInfo.Bottom - frameInfo.Top);

                Image image = null;
                if (sequence.IsDds)
                {
                    int pixels = cropRect.Width * cropRect.Height;
                    byte[] buffer = new byte[pixels];
                    Buffer.BlockCopy(sequenceSpriteSheet, pixelsRead, buffer, 0, pixels);

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
                    float deltaX = offset.X - image.Width / 2f;
                    float deltaY = offset.Y - image.Height / 2f;
                    newWidth = image.Width + 2 * Math.Abs(deltaX);
                    newHeight = image.Height + 2 * Math.Abs(deltaY);
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

            sequence.Textures = images;
            return sequence;
        }

        public static Sequence FromStream(
            Stream sequenceStream,
            Stream propertiesStream = null)
        {
            return FromStream(
                out _,
                out _,
                out _,
                out _,
                sequenceStream,
                propertiesStream);
        }
    }
}
