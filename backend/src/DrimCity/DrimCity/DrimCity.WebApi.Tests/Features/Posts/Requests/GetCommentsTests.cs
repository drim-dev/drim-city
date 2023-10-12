using System.Net;
using Common.Tests.Database.Harnesses;
using Common.Tests.Http.Extensions;
using Common.Tests.Http.Harnesses;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Posts.Models;
using DrimCity.WebApi.Tests.Fixtures;

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
        var post = await CreatePost();
        var comments = await CreateComments(post, 3);

        var (responseComments, httpResponse) = await Act(post);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        responseComments.Should().NotBeNullOrEmpty();

        responseComments.Should().BeEquivalentTo(comments,
            config => config
                .Excluding(comment => comment.PostId)
                .Excluding(comment => comment.Post));
    }

    private async Task<(CommentModel[]?, HttpResponseMessage httpResponse)> Act(Post post)
    {
        var httpClient = _httpClientHarness.CreateClient();
        return await httpClient.GetTyped<CommentModel[]>($"/posts/{post.Slug}/comments", CreateCancellationToken());
    }

    private async Task<Post> CreatePost()
    {
        return await _database.Execute(async dbContext =>
        {
            var post = new Post(0, "anyTitle", "anyContent", DateTime.UtcNow, 0, "postSlug");
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
}