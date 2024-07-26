using System.Text;

namespace NuVelocity.Text;

internal static class CP1252EncodingProvider
{
    static CP1252EncodingProvider()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static Encoding CP1252
    {
        get
        {
            return Encoding.GetEncoding(1252);
        }
    }
}
