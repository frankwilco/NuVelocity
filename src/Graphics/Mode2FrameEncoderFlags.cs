namespace NuVelocity.Graphics;

[Flags]
public enum Mode2FrameEncoderFlags
{
    None = 0,
    Frm8With5LayersOrFrm16With3Layers = 0b00000000_00000000_00000000_00100000,

    Frm8Rle = 0b00000000_00000000_00000000_00000010,
    Frm8With5Layers = Frm8With5LayersOrFrm16With3Layers,
    Frm8With5LayersRle = Frm8With5Layers | Frm8Rle,

    Frm16Rle1 = 0b00000000_00000000_00000000_00000100,
    Frm16Rle2 = 0b00000000_00000000_00100000_00000000,

    Frm16With5Layers = 0b00000000_00000000_00000000_01000000,
    Frm16With5LayersRle = Frm16With5Layers | Frm16Rle1 | Frm16Rle2,
    Frm16With5LayersRleRlw = Frm16With5Layers | Frm16Rle1,

    Frm16With3Layers = Frm8With5LayersOrFrm16With3Layers,
    Frm16With3LayersRle = Frm16With3Layers | Frm16Rle1 | Frm16Rle2,

    Frm24With5Layers = 0b00000000_00000000_00000000_10000000,
    Frm32With5Layers = 0b00000000_00000000_00000001_00000000,

    FrmUsesRle = Frm8Rle | Frm16Rle1 | Frm16Rle2,
}
