using System.Net;
using Common.Tests.Database.Harnesses;
using Common.Tests.Http.Extensions;
using Common.Tests.Http.Harnesses;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Posts.Models;
using DrimCity.WebApi.Tests.Features.Utils;
using DrimCity.WebApi.Tests.Fixtures;
using FluentAssertions.Equivalency;

namespace DrimCity.WebApi.Tests.Features.Posts.Requests;

[Collection(PostsTestsCollection.Name)]
public class GetCommentsTests : IAsyncLifetime
{
    private readonly DatabaseHarness<Program, AppDbContext> _database;
    private readonly HttpClientHarness<Program> _httpClient;

    public GetCommentsTests(TestFixture fixture)
    {
        _database = fixture.Database;
        _httpClient = fixture.HttpClient;
    }

    public async Task InitializeAsync()
    {
        await _database.Clear(CreateCancellationToken());
    }

    public Task DisposeAsync() =>
        Task.CompletedTask;

    [Fact]
    public async Task Should_return_comments()
    {
        var post = await CreatePost("postSlug");
        var comments = await CreateComments(post, 3);

        var (responseComments, httpResponse) = await Act(post.Slug);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        responseComments.Should().NotBeNullOrEmpty();

        responseComments.Should().BeEquivalentTo(comments, CommentEquivalencyOptions);
    }

    [Fact]
    public async Task Should_return_comments_only_by_post_in_request()
    {
        var unexpectedPost = await CreatePost("unexpectedPostSlug");
        var unexpectedComments = await CreateComments(unexpectedPost, 1);
        var expectedPost = await CreatePost("expectedPostSlug");
        var expectedComments = await CreateComments(expectedPost, 1);

        var (responseComments, _) = await Act(expectedPost.Slug);

        responseComments.Should().NotBeNullOrEmpty();

        responseComments!.Select(x => x.Id).Should().NotContain(unexpectedComments.Select(x => x.Id));
        responseComments!.Select(x => x.Id).Should().Contain(expectedComments.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_return_not_found_when_post_does_not_exist()
    {
        var (responseComments, httpResponse) = await Act("notExistingPostSlug");

        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        responseComments.Should().BeNull();
    }

    [Fact]
    public async Task Should_return_comments_ordered_ascending_by_created_at()
    {
        var post = await CreatePost("postSlug");
        await CreateComment(post, 20.October(2023).AsUtc().AddHours(3));
        await CreateComment(post, 20.October(2023).AsUtc().AddHours(1));
        await CreateComment(post, 20.October(2023).AsUtc().AddHours(2));

        var (responseComments, httpResponse) = await Act(post.Slug);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        responseComments.Should().NotBeNullOrEmpty();

        responseComments.Should().BeInAscendingOrder(commentModel => commentModel.CreatedAt);
    }

    private async Task<(CommentModel[]?, HttpResponseMessage httpResponse)> Act(string postSlug)
    {
        var httpClient = _httpClient.CreateClient();
        return await httpClient.GetTyped<CommentModel[]>($"/posts/{postSlug}/comments", CreateCancellationToken());
    }

    private async Task<Post> CreatePost(string slug)
    {
        return await _database.Execute(async dbContext =>
        {
            var post = FakerFactory.CreatePost(slug);
            await dbContext.Posts.AddAsync(post, CreateCancellationToken());
            await dbContext.SaveChangesAsync();
            return post;
        });
    }

    private Task<Comment[]> CreateComment(Post post, DateTime? createdAt = null) =>
        CreateComments(post, 1, createdAt);

    private async Task<Comment[]> CreateComments(Post post, int count, DateTime? createdAt = null)
    {
        return await _database.Execute(async dbContext =>
        {
            var comments = Enumerable.Range(1, count)
                .Select(_ => FakerFactory.CreateComment(post.Id, createdAt))
                .ToArray();

            await dbContext.Comments.AddRangeAsync(comments, CreateCancellationToken());
            await dbContext.SaveChangesAsync(CreateCancellationToken());

            return comments;
        });
    }

    private EquivalencyAssertionOptions<Comment> CommentEquivalencyOptions(EquivalencyAssertionOptions<Comment> config)
    {
        return config
            .Excluding(comment => comment.PostId)
            .Excluding(comment => comment.Post);
    }
}
