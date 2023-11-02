using System.Net;
using Common.Tests.Database.Harnesses;
using Common.Tests.Http.Extensions;
using Common.Tests.Http.Harnesses;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Tests.Features.Posts.Contracts;
using DrimCity.WebApi.Tests.Fixtures;
using DrimCity.WebApi.Tests.Utils;

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

        responsePosts.Should().BeEquivalentTo(posts, options => options.Excluding(post => post.Author));
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

    private async Task<(PostContract[]?, HttpResponseMessage httpResponse)> Act()
    {
        var httpClient = _httpClient.CreateClient();
        return await httpClient.GetTyped<PostContract[]>("/posts", CreateCancellationToken());
    }

    private async Task CreatePost(string? content = null, DateTime? createdAt = null)
    {
        var account = CreateAccount();
        await _database.Save(account);

        var post = FakerFactory.CreatePost(account.Id, content, createdAt);
        await _database.Save(post);
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
}
