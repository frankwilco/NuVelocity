using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace NuVelocity.Graphics;

public abstract class SequenceEncoder
{
    private const byte kSignatureStandard = 0x01;

    private readonly Stream _sequenceStream;

    private readonly Stream? _propertiesStream;

    private readonly Inflater _inflater;

    private bool _hasProperties;

    public Sequence Sequence { get; protected set; }

    public SequenceFrameInfoList? SequenceFrameInfoList { get; protected set; }

    public EncoderFormat Format { get; protected set; }

    public BlitTypeRevision BlitTypeRevision { get; protected set; }

    public bool IsCompressed { get; protected set; }

    public bool IsFont { get; protected set; }

    public FontBitmap? Font { get; protected set; }

    public bool IsHD { get; protected set; }

    public bool IsEmpty { get; protected set; }

    public bool IsDds
    {
        get
        {
            return Sequence.Flags.HasFlag(
                PropertySerializationFlags.HasDdsSupport);
        }
    }

    public byte[]? ListData { get; protected set; }

    public byte[]? ImageData1 { get; protected set; }

    public byte[]? ImageData2 { get; protected set; }

    //

    public int BaseWidth { get; protected set; }

    public int BaseHeight { get; protected set; }

    public byte Scan1 { get; protected set; }

    public byte Scan2 { get; protected set; }

    public byte Flags { get; protected set; }

    public int Scan3 { get; protected set; }

    public SequenceEncoderMode2Game Unknown1 { get; protected set; }

    public int DynamicPropertiesLength { get; protected set; }

    public int Mode2HotSpotX { get; protected set; }

    public int Mode2HotSpotY { get; protected set; }

    public int Mode2CenterX { get; protected set; }
    
    public int Mode2CenterY { get; protected set; }

    public FrameEncoder[]? FrameData { get; protected set; }

    public SequenceEncoder(
        Stream sequenceStream,
        Stream? propertiesStream,
        EncoderFormat format,
        BlitTypeRevision blitTypeRevision = BlitTypeRevision.Type1)
    {
        _sequenceStream = sequenceStream ??
                    throw new ArgumentNullException(nameof(sequenceStream));
        if (sequenceStream.Length == 0)
        {
            throw new ArgumentException(null, nameof(sequenceStream));
        }
        _propertiesStream = propertiesStream;
        if (propertiesStream?.Length == 0)
        {
            throw new ArgumentException(null, nameof(propertiesStream));
        }
        _inflater = new();
        Format = format;
        BlitTypeRevision = blitTypeRevision;

        InitializeSequence();
        Initialize();
    }

    [MemberNotNull(nameof(Sequence))]
    protected virtual void InitializeSequence()
    {
        Sequence = new();
    }

    protected virtual void Initialize()
    {
        if (_propertiesStream != null)
        {
            _hasProperties = PropertySerializer.Deserialize(_propertiesStream, Sequence);
        }

        _hasProperties = false;
        IsCompressed = false;
        IsFont = false;
        IsHD = false;
        IsEmpty = false;
        ImageData1 = null;
        ImageData2 = null;
        BaseWidth = 0;
        BaseHeight = 0;

        switch (Format)
        {
            case EncoderFormat.Mode1:
                ParseMode1Stream();
                LoadMode1Sequence();
                break;
            case EncoderFormat.Mode2:
                ParseMode2Stream();
                LoadMode2Sequence();
                break;
            case EncoderFormat.Mode3:
                ParseMode3Stream();
                LoadMode3Sequence();
                break;
            default:
                throw new NotSupportedException();
        }
    }

    protected void ParseMode1Stream()
    {
        throw new NotImplementedException();
    }

    const byte kFlagsHasDynamicProperties = 0xD8;
    const byte kFlagsBase = 0xD0;
    const byte kScan3Magic = 0x41;
    const byte kFrameSeparator = 0x01;
    const byte kFrameSeparatorEmpty = 0x00;

    protected void ParseMode2Stream()
    {
        using BinaryReader reader = new(_sequenceStream);

        Scan1 = reader.ReadByte();
        Scan2 = reader.ReadByte();
        Flags = reader.ReadByte();
        Scan3 = reader.ReadByte();
        if (Scan3 != kScan3Magic)
        {
            throw new NotImplementedException();
        }
        Unknown1 = (SequenceEncoderMode2Game)reader.ReadInt32();

        if (Flags == kFlagsHasDynamicProperties)
        {
            DynamicPropertiesLength = reader.ReadInt32();
        }
        else if (Flags != kFlagsBase)
        {
            throw new NotImplementedException();
        }

        if (DynamicPropertiesLength > 0)
        {
            for (int i = 0; i < DynamicPropertiesLength; i++)
            {
                int propertyNameLength = reader.ReadInt32();
                byte[] propertyNameBytes = reader.ReadBytes(propertyNameLength);
                string propertyName = Encoding.ASCII.GetString(propertyNameBytes);

                // TODO: this should probably rely on something from the
                // property serializer. Hardcode some known values for now.
                if (propertyName == "Menu Position")
                {
                    Sequence.MenuPosition = new()
                    {
                        X = reader.ReadInt32(),
                        Y = reader.ReadInt32(),
                    };
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        int frameCount = reader.ReadInt32();
        FrameData = new FrameEncoder[frameCount];
        Sequence.FramesPerSecond = reader.ReadSingle();

        if (Unknown1 == SequenceEncoderMode2Game.Zax ||
            Unknown1 == SequenceEncoderMode2Game.ZaxBitmapFont ||
            Unknown1 == SequenceEncoderMode2Game.StarTrekAwayTeam ||
            Unknown1 == SequenceEncoderMode2Game.StarTrekAwayTeamBitmapFont)
        {
            Sequence.CenterHotSpot = reader.ReadBoolean();
        }

        int blitTypeLength = reader.ReadInt32();
        byte[] blitTypeBytes = reader.ReadBytes(blitTypeLength);
        string blitTypeText = Encoding.ASCII.GetString(blitTypeBytes);
        // XXX: Borrowed from the property serializer.
        Type propType = typeof(BlitType);
        foreach (var enumMember in propType.GetMembers())
        {
            if (!enumMember.IsDefined(typeof(PropertyAttribute)))
            {
                continue;
            }
            PropertyAttribute? propAttr = enumMember.GetCustomAttribute<PropertyAttribute>(true);
            if (propAttr?.Name == blitTypeText)
            {
                Sequence.BlitType = Enum.Parse<BlitType>(enumMember.Name);
                break;
            }
        }

        switch (Unknown1)
        {
            case SequenceEncoderMode2Game.Zax:
            case SequenceEncoderMode2Game.StarTrekAwayTeam:
                ParseMode2PropertyListForZax(reader);
                break;
            case SequenceEncoderMode2Game.ZaxBitmapFont:
            case SequenceEncoderMode2Game.StarTrekAwayTeamBitmapFont:
                ParseMode2PropertyListForZax(reader);
                ParseFontBitmapPropertyList(reader);
                break;
            case SequenceEncoderMode2Game.RicochetLostWorlds:
                ParseMode2PropertyListForRicochetLostWorlds(reader);
                break;
            case SequenceEncoderMode2Game.SwarmOrRicochetXtreme:
            case SequenceEncoderMode2Game.RicochetXtremeLegacy:
                ParseMode2PropertyListForSwarm(reader);
                break;
            case SequenceEncoderMode2Game.SwarmOrRicochetXtremeBitmapFont:
            case SequenceEncoderMode2Game.RicochetXtremeLegacyBitmapFont:
                ParseMode2PropertyListForSwarm(reader);
                ParseFontBitmapPropertyList(reader);
                break;
            case SequenceEncoderMode2Game.Lionheart:
                ParseMode2PropertyListForLionheart(reader);
                break;
            case SequenceEncoderMode2Game.Wik:
            default:
                throw new NotImplementedException();
        }

        Mode2HotSpotX = reader.ReadInt32();
        Mode2HotSpotY = reader.ReadInt32();
        Mode2CenterX = reader.ReadInt32();
        Mode2CenterY = reader.ReadInt32();

        int frameDataIndex = 0;
        while (_sequenceStream.Position < _sequenceStream.Length)
        {
            byte separator = reader.ReadByte();
            if (separator == kFrameSeparatorEmpty)
            {
                IsEmpty = true;
                break;
            }
            if (separator != kFrameSeparator)
            {
                throw new InvalidDataException();
            }
            FrameData[frameDataIndex] = BuildFrameEncoder(_sequenceStream);
            frameDataIndex++;
        }
    }

    protected abstract FrameEncoder BuildFrameEncoder(Stream frameStream);

    private void ParseMode2PropertyListForZax(BinaryReader reader)
    {
        Sequence.Flags |= PropertySerializationFlags.HasSimpleFormat;
        Sequence.BlendedWithBlack = reader.ReadBoolean();
        Sequence.CropAlphaChannel = reader.ReadBoolean();
        Sequence.Use8BitAlpha = reader.ReadBoolean();
        Sequence.Dither = reader.ReadBoolean();
        Sequence.XOffset = reader.ReadInt32();
        Sequence.YOffset = reader.ReadInt32();
    }

    private void ParseMode2PropertyListForRicochetLostWorlds(BinaryReader reader)
    {
        Sequence.Flags |= PropertySerializationFlags.HasJpegQuality2;
        Sequence.XOffset = reader.ReadInt32();
        Sequence.YOffset = reader.ReadInt32();
        Sequence.UseEvery = reader.ReadInt32();
        Sequence.AlwaysIncludeLastFrame = reader.ReadBoolean();
        Sequence.CenterHotSpot = reader.ReadBoolean();
        Sequence.BlendedWithBlack = reader.ReadBoolean();
        Sequence.CropAlphaChannel = reader.ReadBoolean();
        Sequence.Use8BitAlpha = reader.ReadBoolean();
        Sequence.IsRle = reader.ReadBoolean();
        Sequence.DoDither = reader.ReadBoolean();
        Sequence.LossLess2 = reader.ReadBoolean();
        Sequence.JpegQuality2 = reader.ReadInt32();
    }

    private void ParseMode2PropertyListForSwarm(BinaryReader reader)
    {
        Sequence.Flags |= PropertySerializationFlags.HasLegacyImageQuality;
        Sequence.XOffset = reader.ReadInt32();
        Sequence.YOffset = reader.ReadInt32();
        Sequence.CenterHotSpot = reader.ReadBoolean();
        Sequence.BlendedWithBlack = reader.ReadBoolean();
        Sequence.CropAlphaChannel = reader.ReadBoolean();
        Sequence.Use8BitAlpha = reader.ReadBoolean();
        Sequence.DoDither = reader.ReadBoolean();
        Sequence.IsLossless = reader.ReadBoolean();
        int jpegQuality = reader.ReadInt32();
        if (jpegQuality > 0 && jpegQuality <= 100)
        {
            Sequence.JpegQuality = jpegQuality;
        }
        else
        {
#if NV_LOG
            Console.WriteLine($"Ignoring JPEG quality value: {jpegQuality}");
#endif
        }
    }

    private void ParseMode2PropertyListForLionheart(BinaryReader reader)
    {
        Sequence.Flags |= PropertySerializationFlags.HasLegacyImageQuality;
        Sequence.XOffset = reader.ReadInt32();
        Sequence.YOffset = reader.ReadInt32();
        Sequence.CenterHotSpot = reader.ReadBoolean();
        Sequence.BlendedWithBlack = reader.ReadBoolean();
        Sequence.CropAlphaChannel = reader.ReadBoolean();
        Sequence.Use8BitAlpha = reader.ReadBoolean();
        Sequence.DoDither = reader.ReadBoolean();
    }

    private void ParseFontBitmapPropertyList(BinaryReader reader)
    {
        Font = new()
        {
            BlitType = Sequence.BlitType,
            XHeight = reader.ReadInt32(),
            FirstAscii = reader.ReadInt32(),
            LastAscii = reader.ReadInt32(),
            IsFixedWidth = reader.ReadBoolean()
        };
    }

    protected void ParseMode3Stream()
    {
        using BinaryReader reader = new(_sequenceStream);
        // Check if the embedded lists are uncompressed.
        bool hasHeader = HeaderUtils.CheckDeflateHeader(reader, false);
        // Check a different location for the deflate header
        // since it's different for fonts.
        if (!hasHeader)
        {
            IsFont = HeaderUtils.CheckDeflateHeader(reader, true);
            IsHD = !IsFont;
        }

        if (IsHD)
        {
            int embeddedListsSize = reader.ReadInt32();
            ListData = reader.ReadBytes(embeddedListsSize);

            _hasProperties = PropertySerializer.Deserialize(ListData, Sequence);

            if (IsDds)
            {
                long distanceToEof = _sequenceStream.Length - _sequenceStream.Position;
                if (distanceToEof == 0)
                {
                    IsEmpty = true;
                }
                else
                {
                    ImageData1 = reader.ReadBytes(
                        (int)distanceToEof);
                }
            }
            else
            {
                byte unknown1 = reader.ReadByte(); // unknown value
                int imageSize = reader.ReadInt32();
                ImageData1 = reader.ReadBytes(imageSize);
                BaseWidth = reader.ReadInt32();
                BaseHeight = reader.ReadInt32();
            }
        }
        else
        {
            if (IsFont)
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

            _inflater.Reset();
            int frameInfoDeflatedSize = reader.ReadInt32();
            int frameInfoInflatedSize = reader.ReadInt32();
            byte[] rawFrameInfo = reader.ReadBytes(frameInfoDeflatedSize);
            _inflater.SetInput(rawFrameInfo);
            ListData = new byte[frameInfoInflatedSize];
            if (_inflater.Inflate(ListData) != frameInfoInflatedSize)
            {
                throw new InvalidDataException();
            }

            if (reader.PeekChar() == -1)
            {
                // No sprite sheet data. This is probably an empty sequence.
                IsEmpty = true;
            }
            else
            {
                _inflater.Reset();
                IsCompressed = reader.ReadBoolean();
                if (IsCompressed)
                {
                    byte unknown1 = reader.ReadByte(); // unknown value
                    int imageDeflatedSize = reader.ReadInt32();
                    int imageInflatedSize = reader.ReadInt32();
                    ImageData1 = new byte[imageInflatedSize];
                    _inflater.SetInput(reader.ReadBytes(imageDeflatedSize));
                    if (_inflater.Inflate(ImageData1) != imageInflatedSize)
                    {
                        throw new InvalidDataException();
                    }
                    BaseWidth = reader.ReadInt32();
                    BaseHeight = reader.ReadInt32();
                }
                else
                {
                    int imageSize = reader.ReadInt32();
                    ImageData1 = reader.ReadBytes(imageSize);

                    reader.ReadByte(); // 1-byte padding.
                    int maskInflatedSize = reader.ReadInt32();
                    long distanceToEof = _sequenceStream.Length - _sequenceStream.Position;
                    byte[] rawMaskData = reader.ReadBytes((int)distanceToEof);
                    _inflater.SetInput(rawMaskData);
                    ImageData2 = new byte[maskInflatedSize];
                    if (_inflater.Inflate(ImageData2) != maskInflatedSize)
                    {
                        throw new InvalidDataException();
                    }
                }
            }

            _hasProperties = PropertySerializer.Deserialize(ListData, Sequence);
        }

        SequenceFrameInfoList = new();
        PropertySerializer.Deserialize(ListData, SequenceFrameInfoList);
        // XXX: Wik and earlier don't provide all the information in
        // the sequence property list. Assume that we're lacking info
        // if JPEG quality is set to 0 or if FPS values don't match.
        bool fpsMissing = Sequence.FramesPerSecond == null;
        bool qualityMissing = Sequence.JpegQuality == null;
        if (_hasProperties)
        {
            if (fpsMissing || qualityMissing)
            {
                Sequence.Flags |= PropertySerializationFlags.Compact;
            }
        }
        else
        {
            // XXX: Assume legacy format is in use.
            Sequence.Flags |= PropertySerializationFlags.HasLegacyImageQuality;
        }

        // Try to take properties from the flags property. However, not
        // all sequence properties are represented in the Flags property.
        Sequence.CenterHotSpot ??= SequenceFrameInfoList.Flags.HasFlag(
                SequenceFlags.CenterHotSpot);
        Sequence.BlendedWithBlack ??= SequenceFrameInfoList.Flags.HasFlag(
                SequenceFlags.BlendedWithBlack);
        Sequence.CropClor0 ??= SequenceFrameInfoList.Flags.HasFlag(
                SequenceFlags.CropColor0);
        Sequence.Use8BitAlpha ??= SequenceFrameInfoList.Flags.HasFlag(
                SequenceFlags.Use8BitAlpha);
        Sequence.IsRle ??= SequenceFrameInfoList.Flags.HasFlag(
            SequenceFlags.RunLengthEncode);
        Sequence.DoDither ??= SequenceFrameInfoList.Flags.HasFlag(
            SequenceFlags.DoDither);
        Sequence.IsLossless ??= SequenceFrameInfoList.Flags.HasFlag(
            SequenceFlags.Lossless);
        Sequence.BlitType ??= BlitTypeConverter.Int32ToType(
                SequenceFrameInfoList.BlitType, BlitTypeRevision);

        if (fpsMissing)
        {
            Sequence.FramesPerSecond = SequenceFrameInfoList.FramesPerSecond;
        }
    }

    protected abstract void LoadMode1Sequence();

    protected abstract void LoadMode2Sequence();

    protected abstract void LoadMode3Sequence();
}
