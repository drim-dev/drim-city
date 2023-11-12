using System.Net;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Auth.Requests;
using DrimCity.WebApi.Tests.Common.Contracts;
using DrimCity.WebApi.Tests.Extensions;
using DrimCity.WebApi.Tests.Features.Auth.Contracts;
using DrimCity.WebApi.Tests.Fixtures;
using RestSharp;

namespace DrimCity.WebApi.Tests.Features.Auth.Requests;

[Collection(AuthTestsCollection.Name)]
public class CreateAccountTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    public CreateAccountTests(TestFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.Reset(CreateCancellationToken());

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<RestResponse<T>> Act<T>(CreateAccountRequestContract request)
    {
        var client = new RestClient(_fixture.HttpClient.CreateClient());
        return await client.ExecutePostAsync<T>("/auth/accounts", request, CreateCancellationToken());
    }

    [Fact]
    public async Task Should_create_account()
    {
        const string login = "Sam";

        var request = new CreateAccountRequestContract(login, "Qwer1234!");

        var restResponse = await Act<AccountContract>(request);

        restResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseAccount = restResponse.Data;
        responseAccount.ShouldNotBeNull();

        restResponse.Headers.Location().Should().Be($"/auth/accounts/{responseAccount.Login}");

        responseAccount.Login.Should().Be(login.ToLower());
        responseAccount.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 1.Seconds());

        var dbAccount = await _fixture.Database.SingleOrDefault<Account>(x => x.Login == responseAccount.Login,
            CreateCancellationToken());

        dbAccount.ShouldNotBeNull();
        dbAccount.Id.Should().BeGreaterOrEqualTo(0);
        dbAccount.Login.Should().Be(login.ToLower());
        dbAccount.CreatedAt.Should().BeCloseTo(responseAccount.CreatedAt, 100.Microseconds());
        dbAccount.PasswordHash.Should().NotBeEmpty();

        // Password hash generation is tested in PasswordHasherTests
        dbAccount.PasswordHash.Split('$').Should().HaveCount(6);
    }

    [Theory]
    [InlineData("sam")]
    [InlineData("Sam")]
    public async Task Should_return_logic_conflict_error_if_account_already_exists_case_insensitive(string login)
    {
        await _fixture.Database.Save(CreateAccount(login));

        var request = new CreateAccountRequestContract(login, "Qwer1234!");

        var restResponse = await Act<ProblemDetailsContract>(request);

        restResponse.ShouldBeLogicConflictError("Account already exists", "auth:logic:account_already_exists");
    }
}

public record CreateAccountRequestContract(string Login, string Password);

public class CreateAccountValidatorTests
{
    private readonly CreateAccount.RequestValidator _validator = new();

    [Fact]
    public void Should_not_have_errors_when_request_is_valid()
    {
        var request = new CreateAccount.Request("sam", "Qwerty1234!");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_have_error_when_login_empty(string login)
    {
        var request = new CreateAccount.Request(login, "Qwerty1234!");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Login)
            .WithErrorCode("auth:validation:login_must_not_be_empty");
    }

    [Fact]
    public void Should_have_error_when_login_less_min_length()
    {
        var request = new CreateAccount.Request("sa", "Qwerty1234!");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Login)
            .WithErrorCode("auth:validation:login_must_be_greater_or_equal_min_length");
    }

    [Fact]
    public void Should_have_error_when_login_greater_max_length()
    {
        var request = new CreateAccount.Request(new string('a', 33), "Qwerty1234!");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Login)
            .WithErrorCode("auth:validation:login_must_be_less_or_equal_max_length");
    }

    [Theory]
    [InlineData("!")]
    [InlineData("+")]
    [InlineData("=")]
    [InlineData("%")]
    public void Should_have_error_when_login_contains_forbidden_symbols(string symbol)
    {
        var request = new CreateAccount.Request("sam" + symbol, "Qwerty1234!");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Login)
            .WithErrorCode("auth:validation:login_must_contain_specific_symbols");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_have_error_when_password_empty(string password)
    {
        var request = new CreateAccount.Request("sam", password);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorCode("auth:validation:password_must_not_be_empty");
    }

    [Fact]
    public void Should_have_error_when_password_less_min_length()
    {
        var request = new CreateAccount.Request("sam", "Qwer12!");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorCode("auth:validation:password_must_be_greater_or_equal_min_length");
    }

    [Fact]
    public void Should_have_error_when_password_greater_max_length()
    {
        var request = new CreateAccount.Request("sam", "Qwerty1234!" + new string('a', 512));

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorCode("auth:validation:password_must_be_less_or_equal_max_length");
    }

    [Fact]
    public void Should_have_error_when_password_not_contains_uppercase_letter()
    {
        var request = new CreateAccount.Request("sam", "qwerty1234!");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorCode("auth:validation:password_must_contain_uppercase_letter");
    }

    [Fact]
    public void Should_have_error_when_password_not_contains_lowercase_letter()
    {
        var request = new CreateAccount.Request("sam", "QWERTY1234!");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorCode("auth:validation:password_must_contain_lowercase_letter");
    }

    [Fact]
    public void Should_have_error_when_password_not_contains_number()
    {
        var request = new CreateAccount.Request("sam", "QWERTYqwer!");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorCode("auth:validation:password_must_contain_number");
    }

    [Fact]
    public void Should_have_error_when_password_not_contains_special_symbol()
    {
        var request = new CreateAccount.Request("sam", "QWERTYqwer4");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorCode("auth:validation:password_must_contain_special_symbol");
    }
}
