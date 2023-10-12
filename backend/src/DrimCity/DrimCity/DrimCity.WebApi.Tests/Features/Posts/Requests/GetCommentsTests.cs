using System.Net;
using Common.Tests.Database.Harnesses;
using Common.Tests.Http.Extensions;
using Common.Tests.Http.Harnesses;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Posts.Models;
using DrimCity.WebApi.Tests.Fixtures;
using FluentAssertions.Equivalency;

namespace DrimCity.WebApi.Tests.Features.Posts.Requests;

[Collection(PostsTestsCollection.Name)]
public class GetCommentsTests : IAsyncLifetime
{
    private readonly DatabaseHarness<Program, AppDbContext> _database;
    private readonly HttpClientHarness<Program> _httpClientHarness;

    public GetCommentsTests(TestFixture fixture)
    {
        _database = fixture.Database;
        _httpClientHarness = fixture.HttpClient;
    }

    public async Task InitializeAsync()
    {
        await _database.Clear(CreateCancellationToken());
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    private async Task Should_return_comments()
    {
        var post = await CreatePost("postSlug");
        var comments = await CreateComments(post, 3);

        var (responseComments, httpResponse) = await Act(post);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        responseComments.Should().NotBeNullOrEmpty();

        responseComments.Should().BeEquivalentTo(comments, CommentEquivalencyOptions);
    }

    [Fact]
    private async Task Should_return_comments_only_by_post_in_request()
    {
        var unexpectedPost = await CreatePost("unexpectedPostSlug");
        var unexpectedComments = await CreateComments(unexpectedPost, 1);
        var expectedPost = await CreatePost("expectedPostSlug");
        var expectedComments = await CreateComments(expectedPost, 1);

        var (responseComments, _) = await Act(expectedPost);

        responseComments.Should().NotBeNullOrEmpty();

        responseComments!.Select(x => x.Id).Should().NotContain(unexpectedComments.Select(x => x.Id));
        responseComments!.Select(x => x.Id).Should().Contain(expectedComments.Select(x => x.Id));
    }

    private async Task<(CommentModel[]?, HttpResponseMessage httpResponse)> Act(Post post)
    {
        var httpClient = _httpClientHarness.CreateClient();
        return await httpClient.GetTyped<CommentModel[]>($"/posts/{post.Slug}/comments", CreateCancellationToken());
    }

    private async Task<Post> CreatePost(string slug)
    {
        return await _database.Execute(async dbContext =>
        {
            var post = new Post(0, "anyTitle", "anyContent", DateTime.UtcNow, 0, slug);
            await dbContext.Posts.AddAsync(post, CreateCancellationToken());
            await dbContext.SaveChangesAsync();
            return post;
        });
    }

    private async Task<Comment[]> CreateComments(Post post, int count)
    {
        return await _database.Execute(async dbContext =>
        {
            var comments = Enumerable.Range(0, count)
                .Select(index => new Comment($"commentContent{index}", DateTime.UtcNow, 1, post.Id))
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