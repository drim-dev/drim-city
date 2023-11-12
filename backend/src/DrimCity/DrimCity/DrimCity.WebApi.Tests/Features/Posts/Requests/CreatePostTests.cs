using System.Net;
using AutoBogus;
using Bogus;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Posts.Requests;
using DrimCity.WebApi.Tests.Extensions;
using DrimCity.WebApi.Tests.Features.Posts.Contracts;
using DrimCity.WebApi.Tests.Fixtures;
using RestSharp;

namespace DrimCity.WebApi.Tests.Features.Posts.Requests;

[Collection(PostsTestsCollection.Name)]
public class CreatePostTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    public CreatePostTests(TestFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.Reset(CreateCancellationToken());

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task<RestResponse<PostContract>> Act(HttpClient client,
        CreatePostRequestContract request) =>
        await new RestClient(client).ExecutePostAsync<PostContract>("/posts", request, CreateCancellationToken());

    [Fact]
    public async Task Should_create_post()
    {
        var request = AutoFaker.Generate<CreatePostRequestContract>();

        var (httpClient, account) = await _fixture.CreatedAuthedHttpClient();

        var restResponse = await Act(httpClient, request);

        restResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var responsePost = restResponse.Data;
        responsePost.ShouldNotBeNull();

        restResponse.Headers.Location().Should().Be($"/posts/{responsePost.Slug}");

        responsePost.Id.Should().BeGreaterOrEqualTo(0);
        responsePost.Title.Should().Be(request.Title);
        responsePost.Content.Should().Be(request.Content);
        responsePost.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 10.Seconds());
        responsePost.AuthorId.Should().Be(account.Id);
        responsePost.Slug.Should().NotBeEmpty();

        var dbPost = await _fixture.Database.SingleOrDefault<Post>(x => x.Id == responsePost.Id,
            CreateCancellationToken());

        dbPost.ShouldNotBeNull();
        dbPost.Id.Should().Be(responsePost.Id);
        dbPost.Title.Should().Be(responsePost.Title);
        dbPost.Content.Should().Be(responsePost.Content);
        dbPost.CreatedAt.Should().BeCloseTo(responsePost.CreatedAt, 10.Seconds());
        dbPost.AuthorId.Should().Be(responsePost.AuthorId);
        dbPost.Slug.Should().Be(responsePost.Slug);
    }

    [Fact]
    public async Task Should_not_authenticate_with_wrong_jwt()
    {
        var request = AutoFaker.Generate<CreatePostRequestContract>();

        var (httpClient, _) = await _fixture.CreateWronglyAuthedHttpClient();

        var restResponse = await Act(httpClient, request);

        restResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public record CreatePostRequestContract(string Title, string Content);

public class CreatePostValidatorTests
{
    private readonly CreatePost.RequestValidator _validator = new();

    [Fact]
    public void Should_not_have_errors_when_request_is_valid()
    {
        var request = CreateRequest();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_have_error_when_title_empty(string title)
    {
        var request = CreateRequestWithTitle(title);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorCode("posts:validation:title_must_not_be_empty");
    }

    [Fact]
    public void Should_have_error_when_title_greater_max_length()
    {
        var request = CreateRequestWithTitle(new string('a', 301));

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorCode("posts:validation:title_must_be_less_or_equal_max_length");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_have_error_when_content_empty(string content)
    {
        var request = CreateRequestWithContent(content);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorCode("posts:validation:content_must_not_be_empty");
    }

    [Fact]
    public void Should_have_error_when_content_greater_max_length()
    {
        var request = CreateRequestWithContent(new string('a', 100_001));

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorCode("posts:validation:content_must_be_less_or_equal_max_length");
    }

    private CreatePost.Request CreateRequestWithTitle(string? title) =>
        CreateRequestFaker()
            .RuleFor(request => request.Title, title)
            .Generate();

    private CreatePost.Request CreateRequestWithContent(string? content) =>
        CreateRequestFaker()
            .RuleFor(request => request.Content, content)
            .Generate();

    private CreatePost.Request CreateRequest(string? title = null, string? content = null) =>
        CreateRequestFaker(title, content)
            .Generate();

    private static Faker<CreatePost.Request> CreateRequestFaker(string? title = null, string? content = null) =>
        new AutoFaker<CreatePost.Request>()
            .RuleFor(request => request.Title, faker => title ?? faker.Random.Word())
            .RuleFor(request => request.Content, faker => content ?? faker.Random.Words());
}
