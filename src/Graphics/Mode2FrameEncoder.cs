using System.Text;

namespace NuVelocity.Graphics;

public class Mode2FrameEncoder : FrameEncoder
{
    private static readonly byte[] kMode1Header = new byte[] { 0x46, 0x52, 0x41, 0x4D, 0x45 };

    public bool IsEmbedded { get; protected set; }

    public int FormatVersion { get; protected set; }

    public int Unknown1 { get; protected set; }

    public Mode2FrameEncoderFlags Flags { get; protected set; }

    public int[][]? LayerPixelOffsets { get; protected set; }

    public Mode2FrameEncoder(bool isEmbedded = false)
        : base()
    {
        IsEmbedded = isEmbedded;
    }

    protected override void Reset(bool disposing = false)
    {
        FormatVersion = default;
        Unknown1 = default;
        Flags = Mode2FrameEncoderFlags.None;
        LayerPixelOffsets = null;

        base.Reset(disposing);
    }

    protected override void DecodeRaw()
    {
        if (_frameStream == null)
        {
            throw new InvalidOperationException();
        }
        using BinaryReader reader = new(_frameStream, Encoding.UTF8, true);

        if (!IsEmbedded)
        {
            // FIXME: There are .frm16 files that are in the Mode 1 format
            // for some reason. Skip those for now.
            byte[] header = reader.ReadBytes(5);
            if (header.SequenceEqual(kMode1Header))
            {
                return;
            }
            _frameStream.Seek(0, SeekOrigin.Begin);
        }

        // FIXME: check value first before casting to enum.
        FormatVersion = reader.ReadByte();
        PixelFormat = (PixelFormat)reader.ReadByte();
        // XXX: Should the hot spot values be negative by default?
        HotSpotX = -reader.ReadInt16();
        HotSpotY = -reader.ReadInt16();
        BaseWidth = reader.ReadInt16();
        BaseHeight = reader.ReadInt16();
        Unknown1 = reader.ReadInt16();
        Flags = (Mode2FrameEncoderFlags)reader.ReadInt32();

        IsCompressed =
            (Flags & Mode2FrameEncoderFlags.FrmUsesRle) > 0;
        bool has5Layers =
            Flags != Mode2FrameEncoderFlags.Frm16With3Layers &&
            Flags != Mode2FrameEncoderFlags.Frm16With3LayersRle;
        LayerCount = has5Layers ? 5 : 3;
        int[] layerSizes = new int[LayerCount];
        for (int i = 0; i < LayerCount; i++)
        {
            layerSizes[i] = reader.ReadInt32();
        }

        LayerData = new byte[LayerCount][];
        LayerPixelOffsets = new int[LayerCount][];
        for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++)
        {
            int layerSize = layerSizes[layerIndex];
            if (layerSize == 0)
            {
                continue;
            }
            int layerSizeInLayer = reader.ReadInt32();
            if (layerSize == layerSizeInLayer)
            {
                layerSize -= 4;
            }
            else
            {
                reader.BaseStream.Seek(-4, SeekOrigin.Current);
            }
            LayerData[layerIndex] = reader.ReadBytes(layerSize);

            int pixelOffsetCount = BaseHeight;
            int pixelOffsetSize = pixelOffsetCount * 4;
            int pixelOffsetBytesRead = 0;
            LayerPixelOffsets[layerIndex] = new int[pixelOffsetCount];
            for (int i = 0; i < pixelOffsetCount; i++)
            {
                LayerPixelOffsets[layerIndex][i] = reader.ReadInt32();
                pixelOffsetBytesRead += 4;
            }
            if (pixelOffsetBytesRead != pixelOffsetSize)
            {
                throw new InvalidDataException();
            }
        }
    }
}
