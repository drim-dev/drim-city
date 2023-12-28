using System.Net;
using Common.Tests.Database.Harnesses;
using Common.Tests.Http.Harnesses;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Features.Accounts.Requests;
using DrimCity.WebApi.Tests.Features.Accounts.Contracts;
using DrimCity.WebApi.Tests.Fixtures;
using RestSharp;

namespace DrimCity.WebApi.Tests.Features.Accounts.Requests;

[Collection(AccountTestsCollection.Name)]
public class GetAccountProfileTests : IAsyncLifetime
{
    private readonly DatabaseHarness<Program, AppDbContext> _database;
    private readonly HttpClientHarness<Program> _httpClient;

    public GetAccountProfileTests(TestFixture testFixture)
    {
        _database = testFixture.Database;
        _httpClient = testFixture.HttpClient;
    }

    public async Task InitializeAsync() =>
        await _database.Clear(CreateCancellationToken());

    public Task DisposeAsync() =>
        Task.CompletedTask;

    private async Task<RestResponse<AccountProfileContract>> Act(string login)
    {
        var request = new RestRequest($"/accounts/{login}");

        var httpClient = _httpClient.CreateClient();
        var restClient = new RestClient(httpClient);

        return await restClient.ExecuteAsync<AccountProfileContract>(request, CreateCancellationToken());
    }

    [Fact]
    public async Task Should_return_profile()
    {
        var account = CreateAccount();

        await _database.Save(account);

        var response = await Act(account.Login);

        var profile = response.Data;

        profile.Should().NotBeNull();
        profile!.Login.Should().Be(account.Login);
        profile.CreatedAt.Should().Be(account.CreatedAt);
    }

    [Fact]
    public async Task Should_return_not_found_if_no_account()
    {
        var account = CreateAccount();

        await _database.Save(account);

        var response = await Act(account.Login + "a");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

public class GetAccountProfileValidatorTests
{
    private readonly GetAccountProfile.RequestValidator _validator = new();

    [Fact]
    public void Should_not_have_errors_when_request_is_valid()
    {
        var request = new GetAccountProfile.Request("sam");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_have_error_when_login_empty(string login)
    {
        var request = new GetAccountProfile.Request(login);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Login)
            .WithErrorCode("auth:validation:login_must_not_be_empty");
    }

    [Fact]
    public void Should_have_error_when_login_less_min_length()
    {
        var request = new GetAccountProfile.Request("sa");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Login)
            .WithErrorCode("auth:validation:login_must_be_greater_or_equal_min_length");
    }

    [Fact]
    public void Should_have_error_when_login_greater_max_length()
    {
        var request = new GetAccountProfile.Request(new string('a', 33));

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Login)
            .WithErrorCode("auth:validation:login_must_be_less_or_equal_max_length");
    }
}
