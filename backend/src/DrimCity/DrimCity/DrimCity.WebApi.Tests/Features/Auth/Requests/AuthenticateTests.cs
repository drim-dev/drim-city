using System.Net;
using Common.Tests.Database.Harnesses;
using Common.Tests.Http.Harnesses;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Features.Auth.Requests;
using DrimCity.WebApi.Features.Auth.Services;
using DrimCity.WebApi.Tests.Extensions;
using DrimCity.WebApi.Tests.Features.Auth.Contracts;
using DrimCity.WebApi.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;

namespace DrimCity.WebApi.Tests.Features.Auth.Requests;

[Collection(AuthTestsCollection.Name)]
public class AuthenticateTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;
    private readonly HttpClientHarness<Program> _httpClient;
    private readonly DatabaseHarness<Program, AppDbContext> _database;
    private AsyncServiceScope _scope;
    private PasswordHasher? _passwordHasher;

    public AuthenticateTests(TestFixture fixture)
    {
        _fixture = fixture;
        _httpClient = _fixture.HttpClient;
        _database = _fixture.Database;
    }

    public async Task InitializeAsync()
    {
        await _fixture.Reset(CreateCancellationToken());
        _scope = _fixture.Factory.Services.CreateAsyncScope();
        _passwordHasher = _scope.ServiceProvider.GetRequiredService<PasswordHasher>();
    }

    public async Task DisposeAsync() => await _scope.DisposeAsync();

    private async Task<RestResponse<TokenContract>> Act(AuthenticateRequestContract request)
    {
        var client = new RestClient(_httpClient.CreateClient());
        return await client.ExecutePostAsync<TokenContract>("/auth", request, CreateCancellationToken());
    }

    [Fact]
    public async Task Should_authenticate_if_correct_credentials()
    {
        const string password = "Qwert1234!";

        var account = CreateAccount(passwordHash: _passwordHasher!.Hash(password));
        await _database.Save(account);

        var httpResponse = await Act(new AuthenticateRequestContract(account.Login, password));

        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // JWT generation is tested in JwtGeneratorTests
        var token = httpResponse.Data;
        token.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_not_authenticate_if_incorrect_login()
    {
        const string password = "Qwert1234!";

        var account = CreateAccount(passwordHash: _passwordHasher!.Hash(password));
        await _database.Save(account);

        var restResponse = await Act(new AuthenticateRequestContract(account.Login + "1", password));

        restResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_not_authenticate_if_incorrect_password()
    {
        const string password = "Qwert1234!";

        var account = CreateAccount(passwordHash: _passwordHasher!.Hash(password));
        await _fixture.Database.Save(account);

        var restResponse = await Act(new AuthenticateRequestContract(account.Login, password + "1"));

        restResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public record AuthenticateRequestContract(string Login, string Password);

public class AuthenticateValidatorTests
{
    private readonly Authenticate.RequestValidator _validator = new();

    [Fact]
    public void Should_not_have_errors_when_request_is_valid()
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
            .WithErrorCode("auth:validation:login_must_not_be_empty");
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
            .WithErrorCode("auth:validation:password_must_not_be_empty");
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
