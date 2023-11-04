using System.Net;
using System.Web;
using Common.Tests.Database.Harnesses;
using Common.Tests.Http.Extensions;
using Common.Tests.Http.Harnesses;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Posts.Requests;
using DrimCity.WebApi.Tests.Features.Posts.Contracts;
using DrimCity.WebApi.Tests.Fixtures;
using DrimCity.WebApi.Tests.Utils;
using FluentAssertions.Equivalency;

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

        var (responsePosts, httpResponse) = await Act();

        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        responsePosts.Should().NotBeNullOrEmpty();

        responsePosts.Should().BeEquivalentTo(posts, PostEquivalencyConfig);
    }

    [Fact]
    public async Task Should_return_post_with_content_with_ellipsis()
    {
        await CreatePost(new string('a', 2010));

        var (responsePosts, httpResponse) = await Act();

        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
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

        var (responsePosts, httpResponse) = await Act();

        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
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
        var (responsePosts, httpResponse) = await Act(pageSize, pageNumber);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        responsePosts.Should().NotBeNullOrEmpty();

        responsePosts.Should().NotContainEquivalentOf(post2, PostEquivalencyConfig);
        responsePosts.Should().ContainEquivalentOf(post1, PostEquivalencyConfig);
    }

    private async Task<(PostContract[]?, HttpResponseMessage httpResponse)> Act(int? pageSize = null,
        int? pageNumber = null)
    {
        var queryParameters = HttpUtility.ParseQueryString(string.Empty);
        if (pageSize.HasValue)
        {
            queryParameters.Add("pageSize", pageSize.Value.ToString());
        }

        if (pageNumber.HasValue)
        {
            queryParameters.Add("pageNumber", pageNumber.Value.ToString());
        }

        var httpClient = _httpClient.CreateClient();
        return await httpClient.GetTyped<PostContract[]>($"/posts?{queryParameters}", CreateCancellationToken());
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
