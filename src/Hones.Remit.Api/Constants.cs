using HashidsNet;

namespace Hones.Remit.Api;

internal static class Constants
{
    public static readonly Hashids Encoder = new Hashids("Hones.Remit.Api", 8);
    
}