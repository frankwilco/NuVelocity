namespace NuVelocity.Text;

[Flags]
public enum PropertySerializationFlags
{
    None =                     0,
    Compact =                  1 << 0,

    HasJpegQuality2 =          1 << 1,
    HasSimpleFormat =          1 << 2,
    HasLegacyImageQuality =    1 << 3,
    HasMipmapSupport =         1 << 4,

    HasYSort =                 1 << 5,
    HasPokeAudio =             1 << 6,
    HasEditorOnly =            1 << 7,
    HasFixedCropColor0Name =   1 << 8,
    HasDdsSupport =            1 << 9,

    HasTextBlitType =          1 << 10,
}
