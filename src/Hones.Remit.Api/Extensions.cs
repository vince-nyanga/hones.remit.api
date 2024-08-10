namespace Hones.Remit.Api;

internal static class Extensions
{
    public static string Encode(this long value)
        => Constants.Encoder
            .EncodeLong(value)
            .ToUpperInvariant();
}