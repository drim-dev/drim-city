using System.Net;
using Common.Tests.Http.Extensions;
using DrimCity.WebApi.Features.Auth.Requests;
using DrimCity.WebApi.Features.Auth.Services;
using DrimCity.WebApi.Tests.Features.Auth.Contracts;
using DrimCity.WebApi.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace DrimCity.WebApi.Tests.Features.Auth.Requests;

[Collection(AuthTestsCollection.Name)]
public class AuthenticateTests
{
    private readonly TestFixture _fixture;

    public AuthenticateTests(TestFixture fixture) => _fixture = fixture;

    private async Task<(TokenContract?, HttpResponseMessage)> Act(AuthenticateRequestContract request)
    {
        var client = _fixture.HttpClient.CreateClient();
        return await client.PostTyped<TokenContract>("/auth", request, CreateCancellationToken());
    }

    [Fact]
    public async Task Should_authenticate_if_correct_credentials()
    {
        const string password = "Qwert1234!";

        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();

        var account = CreateAccount(passwordHash: passwordHasher.Hash(password));
        await _fixture.Database.Save(account);

        var (token, httpResponse) = await Act(new AuthenticateRequestContract(account.Login, password));

        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // JWT generation is tested in JwtGeneratorTests
        token.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_not_authenticate_if_incorrect_login()
    {
        const string password = "Qwert1234!";

        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();

        var account = CreateAccount(passwordHash: passwordHasher.Hash(password));
        await _fixture.Database.Save(account);

        var (_, httpResponse) = await Act(new AuthenticateRequestContract(account.Login + "1", password));

        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_not_authenticate_if_incorrect_password()
    {
        const string password = "Qwert1234!";

        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();

        var account = CreateAccount(passwordHash: passwordHasher.Hash(password));
        await _fixture.Database.Save(account);

        var (_, httpResponse) = await Act(new AuthenticateRequestContract(account.Login, password + "1"));

        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public record AuthenticateRequestContract(string Login, string Password);

public class AuthenticateValidatorTests
{
    private readonly Authenticate.RequestValidator _validator = new();

    [Fact]
    public void Should_not_have_errors_when_request_valid()
    {
        var request = new Authenticate.Request("sam", "Qwerty1234!");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_have_error_when_login_empty(string login)
    {
        var request = new Authenticate.Request(login, "Qwerty1234!");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Login)
            .WithErrorCode("auth:validation:login_required");
    }

    [Fact]
    public void Should_have_error_when_login_less_min_length()
    {
        var request = new Authenticate.Request("sa", "Qwerty1234!");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Login)
            .WithErrorCode("auth:validation:login_must_be_greater_or_equal_min_length");
    }

    [Fact]
    public void Should_have_error_when_login_greater_max_length()
    {
        var request = new Authenticate.Request(new string('a', 33), "Qwerty1234!");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Login)
            .WithErrorCode("auth:validation:login_must_be_less_or_equal_max_length");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_have_error_when_password_empty(string password)
    {
        var request = new Authenticate.Request("sam", password);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorCode("auth:validation:password_required");
    }

    [Fact]
    public void Should_have_error_when_password_less_min_length()
    {
        var request = new Authenticate.Request("sam", "Qwer12!");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorCode("auth:validation:password_must_be_greater_or_equal_min_length");
    }

    [Fact]
    public void Should_have_error_when_password_greater_max_length()
    {
        var request = new Authenticate.Request("sam", "Qwerty1234!" + new string('a', 512));

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorCode("auth:validation:password_must_be_less_or_equal_max_length");
    }
}
