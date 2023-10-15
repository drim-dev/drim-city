namespace DrimCity.WebApi.Features.Accounts.Options;

public class PasswordHashOptions
{
    public int PasswordHashLength { get; set; } = 32;

    public int SaltLength { get; set; } = 16;

    public int TimeCost { get; set; } = 4;

    public int MemoryCost { get; set; } = 65_536;

    public int Parallelization { get; set; } = 4;
}