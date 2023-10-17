using System.Security.Cryptography;
using System.Text;
using DrimCity.WebApi.Features.Auth.Options;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Options;

namespace DrimCity.WebApi.Features.Auth.Services;

public class PasswordHasher
{
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

    private readonly PasswordHashOptions _options;

    public PasswordHasher(IOptions<PasswordHashOptions> options)
    {
        _options = options.Value;
    }

    public string Hash(string password)
    {
        var salt = new byte[_options.SaltLength];
        Rng.GetBytes(salt);

        var config = new Argon2Config
        {
            Version = Argon2Version.Nineteen,
            Type = Argon2Type.HybridAddressing,
            Password = Encoding.UTF8.GetBytes(password),
            HashLength = _options.PasswordHashLength,
            Salt = salt,
            TimeCost = _options.TimeCost,
            MemoryCost = _options.MemoryCost,
            Lanes = _options.Parallelization,
        };

        return Argon2.Hash(config);
    }

    public bool Verify(string password, string passwordHash) => Argon2.Verify(passwordHash, password);
}
