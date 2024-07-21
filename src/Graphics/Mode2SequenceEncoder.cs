using System.Reflection;
using System.Text;

namespace NuVelocity.Graphics;

public class Mode2SequenceEncoder : SequenceEncoder
{
    private const byte kFlagsHasDynamicProperties = 0xD8;
    private const byte kFlagsBase = 0xD0;
    private const byte kScan3Magic = 0x41;
    private const byte kFrameSeparator = 0x01;
    private const byte kFrameSeparatorEmpty = 0x00;

    public Mode2SequenceEncoder(
        BlitTypeRevision blitTypeRevision = BlitTypeRevision.Type1)
        : base(blitTypeRevision)
    {
    }

    public byte Scan1 { get; protected set; }

    public byte Scan2 { get; protected set; }

    public byte Flags { get; protected set; }

    public int Scan3 { get; protected set; }

    public Mode2SequenceEncoderSourceFormat SourceFormat { get; protected set; }

    public int DynamicPropertiesLength { get; protected set; }

    public int HotSpotX { get; protected set; }

    public int HotSpotY { get; protected set; }

    public int CenterX { get; protected set; }

    public int CenterY { get; protected set; }

    public Mode2FrameEncoder[]? FrameData { get; protected set; }

    protected override void Reset(bool disposing = false)
    {
        Scan1 = default;
        Scan2 = default;
        Flags = default;
        Scan3 = default;
        SourceFormat = Mode2SequenceEncoderSourceFormat.None;
        DynamicPropertiesLength = default;
        HotSpotX = default;
        HotSpotY = default;
        CenterX = default;
        CenterY = default;
        FrameData = null;

        base.Reset(disposing);
    }

    protected override void DecodeRaw()
    {
        if (_sequenceStream == null)
        {
            throw new InvalidOperationException();
        }
        using BinaryReader reader = new(_sequenceStream, Encoding.UTF8, true);

        Scan1 = reader.ReadByte();
        Scan2 = reader.ReadByte();
        Flags = reader.ReadByte();
        Scan3 = reader.ReadByte();
        if (Scan3 != kScan3Magic)
        {
            throw new NotImplementedException();
        }
        SourceFormat = (Mode2SequenceEncoderSourceFormat)reader.ReadInt32();

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
        FrameData = new Mode2FrameEncoder[frameCount];
        Sequence.FramesPerSecond = reader.ReadSingle();

        if (SourceFormat == Mode2SequenceEncoderSourceFormat.Zax ||
            SourceFormat == Mode2SequenceEncoderSourceFormat.ZaxBitmapFont ||
            SourceFormat == Mode2SequenceEncoderSourceFormat.StarTrekAwayTeam ||
            SourceFormat == Mode2SequenceEncoderSourceFormat.StarTrekAwayTeamBitmapFont)
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

        switch (SourceFormat)
        {
            case Mode2SequenceEncoderSourceFormat.Zax:
            case Mode2SequenceEncoderSourceFormat.StarTrekAwayTeam:
                DecodeMode2PropertyListForZax(reader);
                break;
            case Mode2SequenceEncoderSourceFormat.ZaxBitmapFont:
            case Mode2SequenceEncoderSourceFormat.StarTrekAwayTeamBitmapFont:
                DecodeMode2PropertyListForZax(reader);
                DecodeFontBitmapPropertyList(reader);
                break;
            case Mode2SequenceEncoderSourceFormat.RicochetLostWorlds:
                DecodeMode2PropertyListForRicochetLostWorlds(reader);
                break;
            case Mode2SequenceEncoderSourceFormat.SwarmOrRicochetXtreme:
            case Mode2SequenceEncoderSourceFormat.RicochetXtremeLegacy:
                DecodeMode2PropertyListForSwarm(reader);
                break;
            case Mode2SequenceEncoderSourceFormat.SwarmOrRicochetXtremeBitmapFont:
            case Mode2SequenceEncoderSourceFormat.RicochetXtremeLegacyBitmapFont:
                DecodeMode2PropertyListForSwarm(reader);
                DecodeFontBitmapPropertyList(reader);
                break;
            case Mode2SequenceEncoderSourceFormat.Lionheart:
                DecodeMode2PropertyListForLionheart(reader);
                break;
            case Mode2SequenceEncoderSourceFormat.Wik:
            default:
                throw new NotImplementedException();
        }

        HotSpotX = reader.ReadInt32();
        HotSpotY = reader.ReadInt32();
        CenterX = reader.ReadInt32();
        CenterY = reader.ReadInt32();

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
            Mode2FrameEncoder embeddedFrameEncoder = new(true);
            embeddedFrameEncoder.Decode(_sequenceStream, null, true);
            FrameData[frameDataIndex] = embeddedFrameEncoder;
            frameDataIndex++;
        }
    }

    private void DecodeMode2PropertyListForZax(BinaryReader reader)
    {
        Sequence.Flags |= PropertySerializationFlags.HasSimpleFormat;
        Sequence.BlendedWithBlack = reader.ReadBoolean();
        Sequence.CropAlphaChannel = reader.ReadBoolean();
        Sequence.Use8BitAlpha = reader.ReadBoolean();
        Sequence.Dither = reader.ReadBoolean();
        Sequence.XOffset = reader.ReadInt32();
        Sequence.YOffset = reader.ReadInt32();
    }

    private void DecodeMode2PropertyListForRicochetLostWorlds(BinaryReader reader)
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

    private void DecodeMode2PropertyListForSwarm(BinaryReader reader)
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

    private void DecodeMode2PropertyListForLionheart(BinaryReader reader)
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

    private void DecodeFontBitmapPropertyList(BinaryReader reader)
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
}
