namespace Hones.Remit.Api;

internal static class Exentsions
{
    public static string Encode(this long value)
        => Constants.Encoder
            .EncodeLong(value)
            .ToUpperInvariant();
}