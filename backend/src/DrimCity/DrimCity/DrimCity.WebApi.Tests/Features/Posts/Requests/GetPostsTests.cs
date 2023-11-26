using System.Net;
using AutoBogus;
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

    private async Task<RestResponse<ProblemDetailsContract?>> ActWithProblem(string? pageSize = null) =>
        await ExecuteRequest<ProblemDetailsContract?>(pageSize);

    private async Task<RestResponse<GetPostsResponseContract>> Act(int? pageSize = null, string? pageToken = null) =>
        await ExecuteRequest<GetPostsResponseContract>(pageSize?.ToString(), pageToken);

    private async Task<RestResponse<TResponse>> ExecuteRequest<TResponse>(string? pageSize, string? pageToken = null)
    {
        var request = new RestRequest("/posts");
        if (pageSize is not null)
        {
            request.AddQueryParameter("pageSize", pageSize);
        }

        if (pageToken is not null)
        {
            request.AddQueryParameter("pageToken", pageToken);
        }

        var httpClient = _httpClient.CreateClient();
        var restClient = new RestClient(httpClient);

        return await restClient.ExecuteAsync<TResponse>(request, CreateCancellationToken());
    }

    [Fact]
    public async Task Should_return_posts()
    {
        var posts = await CreatePosts(3);

        var response = await Act();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responsePosts = response.Data?.Posts;
        responsePosts.Should().NotBeNullOrEmpty();

        responsePosts.Should().BeEquivalentTo(posts, PostEquivalencyConfig);
    }

    [Fact]
    public async Task Should_return_post_with_content_with_ellipsis()
    {
        await CreatePost(new string('a', 2010));

        var response = await Act();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responsePosts = response.Data?.Posts;
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
        var responsePosts = response.Data?.Posts;
        responsePosts.Should().NotBeNullOrEmpty();

        responsePosts.Should().BeInDescendingOrder(x => x.CreatedAt);
    }

    [Fact]
    public async Task Should_return_posts_by_pages()
    {
        var post1 = await CreatePost(createdAt: 09.November(2023).AsUtc());
        var post2 = await CreatePost(createdAt: 08.November(2023).AsUtc());
        var post3 = await CreatePost(createdAt: 07.November(2023).AsUtc());

        var responsePage1 = await Act(1);

        responsePage1.IsSuccessful.Should().BeTrue(responsePage1.ErrorException?.Message);
        responsePage1.StatusCode.Should().Be(HttpStatusCode.OK);
        responsePage1.Data.Should().NotBeNull();

        responsePage1.Data!.Posts.Should().ContainEquivalentOf(post1, PostEquivalencyConfig);
        responsePage1.Data.Posts.Should().NotContainEquivalentOf(post2, PostEquivalencyConfig);
        responsePage1.Data.NextPageToken.Should().NotBeNullOrEmpty();

        var responsePage2 = await Act(2, responsePage1.Data.NextPageToken);

        responsePage2.StatusCode.Should().Be(HttpStatusCode.OK);
        responsePage2.Data.Should().NotBeNull();

        responsePage2.Data!.Posts.Should().ContainEquivalentOf(post2, PostEquivalencyConfig);
        responsePage2.Data.Posts.Should().ContainEquivalentOf(post3, PostEquivalencyConfig);
        responsePage2.Data.Posts.Should().NotContainEquivalentOf(post1, PostEquivalencyConfig);
        responsePage2.Data.NextPageToken.Should().BeNull();
    }

    [Fact]
    public async Task Should_return_bad_request_when_query_parameters_have_not_int_values()
    {
        var response = await ActWithProblem("NaN");

        var expectedHttpStatusCode = HttpStatusCode.BadRequest;
        response.StatusCode.Should().Be(expectedHttpStatusCode);
        var problemDetails = response.Data;
        problemDetails.Should().NotBeNull();

        problemDetails!.Status.Should().Be(expectedHttpStatusCode);
        problemDetails.Title.Should().Be("Bad request");
        problemDetails.Detail.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public async Task Should_return_default_count_of_posts_if_page_size_is_unspecified(int? pageSize)
    {
        await CreatePosts(11);

        var response = await Act(pageSize);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responsePosts = response.Data?.Posts;
        responsePosts.Should().NotBeNullOrEmpty();

        var expectedDefaultCount = 10;
        responsePosts.Should().HaveCount(expectedDefaultCount);
    }

    [Fact]
    public async Task Should_return_maximum_count_of_posts_if_page_size_is_bigger_than_maximum()
    {
        const int countOfPosts = 1001;
        await CreatePosts(countOfPosts);

        var response = await Act(countOfPosts);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responsePosts = response.Data?.Posts;
        responsePosts.Should().NotBeNullOrEmpty();

        var expectedMaximumCount = 1000;
        responsePosts.Should().HaveCount(expectedMaximumCount);
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

public record GetPostsResponseContract(PostContract[] Posts, string? NextPageToken);

public class GetPostsRequestValidatorTests
{
    private readonly GetPosts.RequestValidator _requestValidator = new();

    [Fact]
    public void Should_not_have_errors_when_request_is_valid()
    {
        var request = CreateRequest();

        var result = _requestValidator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_have_error_when_page_size_is_negative()
    {
        var request = CreateRequest(-1);

        var result = _requestValidator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorCode("posts:validation:page_size_must_be_positive");
    }

    private static GetPosts.Request CreateRequest(int? pageSize = null) =>
        new AutoFaker<GetPosts.Request>()
            .RuleFor(request => request.PageSize, faker => pageSize ?? faker.Random.Int(1))
            .Generate();
}
