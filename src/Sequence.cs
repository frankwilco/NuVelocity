using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using ICSharpCode.SharpZipLib.Zip.Compression;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NuVelocity
{
    [PropertyRoot("CSequence", "Sequence")]
    public class Sequence
    {
        private const byte kSignatureStandard = 0x01;

        private bool? _mipmapForNativeVersion;
        private int? _ySort;
        private string? _pokeAudio;
        private bool? _editorOnly;
        private bool? _cropColor0;
        private int? _jpegQuality;
        private bool? _isDds;
        private bool? _needsBuffer;

        public Image[] Textures { get; set; }

        public int Width { get; protected set; }

        public int Height { get; protected set; }

        public PropertySerializationFlags Flags { get; set; }

        [Property("Comment")]
        [PropertyDynamic]
        public string? Comment { get; set; }

        // TN: Exclusive to Ricochet Lost Worlds and Ricochet Infinity.
        [Property("Sequence of Coordinates")]
        [PropertyDynamic]
        public SequenceOfCoordinates? SequenceOfCoordinates { get; set; }

        // TN: Exclusive to Build In Time and Costume Chaos.
        [Property("Y-Sort")]
        [PropertyInclude(PropertySerializationFlags.HasYSort)]
        [PropertyDynamic]
        public int? YSort
        {
            get { return _ySort; }
            set
            {
                _ySort = value;
                Flags |= PropertySerializationFlags.HasYSort;
            }
        }

        // TN: Exclusive to Build In Time.
        [Property("Poke Audio")]
        [PropertyInclude(PropertySerializationFlags.HasPokeAudio)]
        [PropertyDynamic]
        public string? PokeAudio
        {
            get { return _pokeAudio; }
            set
            {
                _pokeAudio = value;
                Flags |= PropertySerializationFlags.HasPokeAudio;
            }
        }

        // TN: Exclusive to Costume Chaos.
        [Property("Editor Only")]
        [PropertyDynamic]
        [PropertyInclude(PropertySerializationFlags.HasEditorOnly)]
        public bool? EditorOnly
        {
            get { return _editorOnly; }
            set
            {
                _editorOnly = value;
                Flags |= PropertySerializationFlags.HasEditorOnly;
            }
        }

        [Property("Frames Per Second", defaultValue: 15)]
        [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
        public float? FramesPerSecond { get; set; }

        [Property("Blit Type", defaultValue: BlitType1.TransparentMask)]
        public BlitType1? BlitType { get; set; }

        [Property("X Offset", defaultValue: 0)]
        public int? XOffset { get; set; }

        [Property("Y Offset", defaultValue: 0)]
        public int? YOffset { get; set; }

        [Property("Use Every", defaultValue: 1)]
        [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality |
                         PropertySerializationFlags.HasSimpleFormat)]
        public int? UseEvery { get; set; }

        [Property("Always Include Last Frame", defaultValue: false)]
        [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality |
                         PropertySerializationFlags.HasSimpleFormat)]
        public bool? AlwaysIncludeLastFrame { get; set; }

        [Property("Center Hot Spot", defaultValue: true)]
        public bool? CenterHotSpot { get; set; }

        [Property("Blended With Black", defaultValue: true)]
        [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality |
                         PropertySerializationFlags.HasSimpleFormat)]
        public bool? BlendedWithBlack { get; set; }

        [Property("Crop Color 0", defaultValue: true)]
        [PropertyInclude(PropertySerializationFlags.HasFixedCropColor0Name)]
        public bool? CropColor0
        {
            get { return _cropColor0; }
            set
            {
                if (_cropColor0 == null)
                {
                    Flags |= PropertySerializationFlags.HasFixedCropColor0Name;
                }
                _cropColor0 = value;
            }
        }

        [Property("Crop Clor 0", defaultValue: true)]
        [PropertyExclude(PropertySerializationFlags.HasFixedCropColor0Name)]
        protected bool? CropClor0
        {
            get { return _cropColor0; }
            set { _cropColor0 = value; }
        }

        [Property("Use 8 Bit Alpha", defaultValue: false)]
        public bool? Use8BitAlpha { get; set; }

        [Property("Run Length Encode", defaultValue: true)]
        [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality |
                         PropertySerializationFlags.HasSimpleFormat)]
        public bool? IsRle { get; set; }

        [Property("Do Dither", defaultValue: true)]
        [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
        public bool? DitherImage { get; set; }

        // TN: Present in Star Trek Away Team sequence files.
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

        [Property("Loss Less", defaultValue: false)]
        [PropertyInclude(PropertySerializationFlags.HasLegacyImageQuality)]
        [PropertyExclude(PropertySerializationFlags.HasSimpleFormat)]
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
        [PropertyExclude(PropertySerializationFlags.HasLegacyImageQuality |
                         PropertySerializationFlags.HasSimpleFormat)]
        public bool? IsLossless { get; set; }

        [Property("Quality", defaultValue: 65)]
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

        [Property("DDS")]
        [PropertyInclude(PropertySerializationFlags.HasDdsSupport)]
        public bool? IsDds
        {
            get { return _isDds; }
            set
            {
                if (_isDds == null)
                {
                    Flags |= PropertySerializationFlags.HasDdsSupport;
                }
                _isDds = value;
            }
        }

        [Property("Needs Buffer")]
        [PropertyInclude(PropertySerializationFlags.HasDdsSupport)]
        public bool? NeedsBuffer
        {
            get { return _needsBuffer; }
            set
            {
                if (_needsBuffer == null)
                {
                    Flags |= PropertySerializationFlags.HasDdsSupport;
                }
                _needsBuffer = value;
            }
        }

        // TN: Present in Swarm Gold, Ricochet Infinity HD, Big Kahuna Reef 3,
        // Build In Time, and Costume Chaos.
        [Property("Mipmap For Native Version", defaultValue: true)]
        [PropertyInclude(PropertySerializationFlags.HasMipmapSupport |
                         PropertySerializationFlags.HasDdsSupport)]
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

        public Sequence()
        {
            Flags = PropertySerializationFlags.None;
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
            bool isDds = false;
            sequenceSpriteSheet = null;
            maskData = null;
            int atlasWidth = 0;
            int atlasHeight = 0;

            if (propertiesStream != null)
            {
                hasProperties = PropertySerializer.Deserialize(propertiesStream, sequence);
                isDds = sequence.Flags.HasFlag(PropertySerializationFlags.HasDdsSupport);
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
                isDds = sequence.Flags.HasFlag(PropertySerializationFlags.HasDdsSupport);

                if (isDds)
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
                if (!isDds)
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
            // if JPEG quality is set to 0 or if FPS values don't match.
            bool fpsMissing = sequence.FramesPerSecond == null;
            bool qualityMissing = sequence.JpegQuality == null;
            if (hasProperties)
            {
                if (fpsMissing || qualityMissing)
                {
                    sequence.Flags |= PropertySerializationFlags.Compact;
                }
            }
            else
            {
                // XXX: Assume legacy format is in use.
                sequence.Flags |= PropertySerializationFlags.HasLegacyImageQuality;
            }

            // Try to take properties from the flags property. However, not
            // all sequence properties are represented in the Flags property.
            sequence.CenterHotSpot ??= frameInfoList.Flags.HasFlag(
                    SequenceFlags.CenterHotSpot);
            sequence.BlendedWithBlack ??= frameInfoList.Flags.HasFlag(
                    SequenceFlags.BlendedWithBlack);
            sequence.CropClor0 ??= frameInfoList.Flags.HasFlag(
                    SequenceFlags.CropColor0);
            sequence.Use8BitAlpha ??= frameInfoList.Flags.HasFlag(
                    SequenceFlags.Use8BitAlpha);
            sequence.IsRle ??= frameInfoList.Flags.HasFlag(
                SequenceFlags.RunLengthEncode);
            sequence.DitherImage ??= frameInfoList.Flags.HasFlag(
                SequenceFlags.DoDither);
            sequence.IsLossless ??= frameInfoList.Flags.HasFlag(
                SequenceFlags.Lossless);

            if (sequence.BlitType == null)
            {
                sequence.BlitType ??= frameInfoList.BlitTypeEnum;
            }
            if (sequence.BlitType != frameInfoList.BlitTypeEnum) {
                throw new InvalidDataException();
            }

            if (fpsMissing)
            {
                sequence.FramesPerSecond = frameInfoList.FramesPerSecond;
            }
            if (qualityMissing)
            {
                // XXX: Assume maximum image quality.
                sequence._jpegQuality = 100;
            }

            // Return early if there's no need to process the image further.
            if (spritesheet == null && !isDds)
            {
                return sequence;
            }
            else if (isEmpty)
            {
                sequence.Textures = new Image[1] { spritesheet };
                return sequence;
            }

            int baseXOffset = sequence.XOffset ?? 0;
            int baseYOffset = sequence.YOffset ?? 0;
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
                if (isDds)
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
