namespace DrimCity.WebApi.Features.Auth.Options;

public record JwtOptions
{
    public required string Key { get; init; }

    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    public required TimeSpan Expiration { get; init; }
}

public record PasswordHashOptions
{
    public int PasswordHashLength { get; init; } = 32;

    public int SaltLength { get; init; } = 16;

    public int TimeCost { get; init; } = 4;

    public int MemoryCost { get; init; } = 65_536;

    public int Parallelization { get; init; } = 4;
}
