using System.Net;
using Common.Tests.Database.Harnesses;
using Common.Tests.Http.Harnesses;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Posts.Requests;
using DrimCity.WebApi.Tests.Common.Contracts;
using DrimCity.WebApi.Tests.Features.Posts.Contracts;
using DrimCity.WebApi.Tests.Fixtures;
using DrimCity.WebApi.Tests.Utils;
using FluentAssertions.Equivalency;
using Microsoft.AspNetCore.Http;
using RestSharp;

namespace DrimCity.WebApi.Tests.Features.Posts.Requests;

[Collection(PostsTestsCollection.Name)]
public class GetPostsTests : IAsyncLifetime
{
    private readonly DatabaseHarness<Program, AppDbContext> _database;
    private readonly HttpClientHarness<Program> _httpClient;

    public GetPostsTests(TestFixture testFixture)
    {
        _database = testFixture.Database;
        _httpClient = testFixture.HttpClient;
    }

    public async Task InitializeAsync() =>
        await _database.Clear(CreateCancellationToken());

    public Task DisposeAsync() =>
        Task.CompletedTask;

    [Fact]
    public async Task Should_return_posts()
    {
        var posts = await CreatePosts(3);

        var response = await Act();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responsePosts = response.Data;
        responsePosts.Should().NotBeNullOrEmpty();

        responsePosts.Should().BeEquivalentTo(posts, PostEquivalencyConfig);
    }

    [Fact]
    public async Task Should_return_post_with_content_with_ellipsis()
    {
        await CreatePost(new string('a', 2010));

        var response = await Act();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responsePosts = response.Data;
        responsePosts.Should().NotBeNullOrEmpty();

        responsePosts.Should().ContainSingle();
        var actualPost = responsePosts!.Single();
        actualPost.Content.Should().EndWith("...");
        actualPost.Content.Should().HaveLength(2000 + 3);
    }

    [Fact]
    public async Task Should_return_posts_ordered_descending_by_created_at()
    {
        await CreatePost(createdAt: 02.November(2023).AsUtc().AddHours(3));
        await CreatePost(createdAt: 02.November(2023).AsUtc().AddHours(1));
        await CreatePost(createdAt: 02.November(2023).AsUtc().AddHours(2));

        var response = await Act();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responsePosts = response.Data;
        responsePosts.Should().NotBeNullOrEmpty();

        responsePosts.Should().BeInDescendingOrder(x => x.CreatedAt);
    }

    [Fact]
    public async Task Should_return_posts_by_pages()
    {
        var post1 = await CreatePost(createdAt: 02.November(2023).AsUtc().AddHours(1));
        var post2 = await CreatePost(createdAt: 02.November(2023).AsUtc().AddHours(2));

        const int pageSize = 1;
        const int pageNumber = 2;
        var response = await Act(pageSize, pageNumber);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responsePosts = response.Data;
        responsePosts.Should().NotBeNullOrEmpty();

        responsePosts.Should().NotContainEquivalentOf(post2, PostEquivalencyConfig);
        responsePosts.Should().ContainEquivalentOf(post1, PostEquivalencyConfig);
    }

    [Fact]
    public async Task Should_return_bad_request_when_query_parameters_have_not_int_values()
    {
        var response = await ActWithProblem("NaN", "NaN");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = response.Data;
        problemDetails.Should().NotBeNull();

        problemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Bad request");
    }

    private async Task<RestResponse<ProblemDetailsContract?>> ActWithProblem(string? pageSize = null,
        string? pageNumber = null) =>
        await ExecuteRequest<ProblemDetailsContract?>(pageSize, pageNumber);

    private async Task<RestResponse<PostContract[]>> Act(int? pageSize = null,
        int? pageNumber = null) =>
        await ExecuteRequest<PostContract[]>(pageSize?.ToString(), pageNumber?.ToString());

    private async Task<RestResponse<TResponse>> ExecuteRequest<TResponse>(string? pageSize, string? pageNumber)
    {
        var request = new RestRequest("/posts");
        if (pageSize is not null)
        {
            request.AddQueryParameter("pageSize", pageSize);
        }

        if (pageNumber is not null)
        {
            request.AddQueryParameter("pageNumber", pageNumber);
        }

        //TODO: question: what is about to use RestSharp library? It provides ease way to build requests and ease access to response status, object, raw content, etc.
        var httpClient = _httpClient.CreateClient();
        var restClient = new RestClient(httpClient);
        return await restClient.ExecuteAsync<TResponse>(request, CreateCancellationToken());
    }

    private async Task<Post> CreatePost(string? content = null, DateTime? createdAt = null)
    {
        var account = CreateAccount();
        await _database.Save(account);

        var post = FakerFactory.CreatePost(account.Id, content, createdAt);
        await _database.Save(post);

        return post;
    }

    private async Task<Post[]> CreatePosts(int count)
    {
        var account = CreateAccount();
        await _database.Save(account);

        var posts = Enumerable.Range(1, count)
            .Select(_ => FakerFactory.CreatePost(account.Id))
            .ToArray();
        await _database.Save(posts.Cast<object>().ToArray());

        return posts;
    }

    private static EquivalencyAssertionOptions<Post> PostEquivalencyConfig(EquivalencyAssertionOptions<Post> options)
    {
        return options.Excluding(post => post.Author);
    }
}

public class GetPostsRequestValidatorTests
{
    private const int AnyValidPageSize = 10;
    private const int AnyValidPageNumber = 1;

    private readonly GetPosts.RequestValidator _requestValidator = new();

    [Fact]
    public void Should_not_have_errors_when_request_is_valid()
    {
        var request = new GetPosts.Request(AnyValidPageSize, AnyValidPageNumber);

        var result = _requestValidator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_have_error_when_page_size_less_than_1(int pageSize)
    {
        //TODO: question: should we use AutoBogus faker for testing request validators?
        var request = new GetPosts.Request(pageSize, AnyValidPageNumber);

        var result = _requestValidator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorCode("posts:validation:page_size_must_be_greater_or_equal_one");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_have_error_when_page_number_less_than_1(int pageNumber)
    {
        var request = new GetPosts.Request(AnyValidPageSize, pageNumber);

        var result = _requestValidator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PageNumber)
            .WithErrorCode("posts:validation:page_number_must_be_greater_or_equal_one");
    }
}
