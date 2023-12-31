using DrimCity.WebApi.Features.Auth.Options;
using DrimCity.WebApi.Features.Auth.Services;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Options;

namespace DrimCity.WebApi.Tests.Features.Auth.Services;

public class PasswordHasherTests
{
    private readonly PasswordHashOptions _options;
    private readonly PasswordHasher _hasher;

    public PasswordHasherTests()
    {
        _options = new PasswordHashOptions
        {
            PasswordHashLength = 64,
            SaltLength = 32,
            TimeCost = 8,
            MemoryCost = 131_072,
            Parallelization = 4,
        };
        _hasher = new PasswordHasher(new OptionsWrapper<PasswordHashOptions>(_options));
    }

    [Fact]
    public void Should_hash_password()
    {
        var hash = _hasher.Hash("password");

        hash.Should().NotBeNullOrEmpty();
        var parts = hash.Split('$');
        parts.Should().HaveCount(6);
        parts[0].Should().Be("");
        parts[1].Should().Be("argon2id");
        parts[2].Should().Be("v=19");
        parts[3].Should().Be($"m={_options.MemoryCost},t={_options.TimeCost},p={_options.Parallelization}");

        var config = new Argon2Config();
        config.DecodeString(hash, out var hashArray).Should().BeTrue();
        hashArray!.Buffer.Should().HaveCount(_options.PasswordHashLength);
        config.Salt.Should().HaveCount(_options.SaltLength);
    }

    [Fact]
    public void Should_produce_different_hashes_for_different_passwords()
    {
        var hash1 = _hasher.Hash("password1");
        var hash2 = _hasher.Hash("password2");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Should_produce_different_hashes_for_same_passwords()
    {
        var hash1 = _hasher.Hash("password");
        var hash2 = _hasher.Hash("password");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Should_verify_password()
    {
        const string hash = "$argon2id$v=19$m=65536,t=4,p=4$brMiyJUHhoCtPSVF1uo1kw$8zm3YLc1wyyDvl5qTZzRYFaIHSbixtrCWJnqbjYCFMo";

        _hasher.Verify("password", hash).Should().BeTrue();
        _hasher.Verify("wrong_password", hash).Should().BeFalse();
    }
}
