namespace Amas.Api.Options;

public sealed class RateLimitOptions
{
    public int GlobalPermitLimit { get; set; } = 120;
    public int GlobalWindowSeconds { get; set; } = 60;
    public int LoginPermitLimit { get; set; } = 5;
    public int LoginWindowSeconds { get; set; } = 60;
    public int ContactPermitLimit { get; set; } = 3;
    public int ContactWindowSeconds { get; set; } = 60;
}
