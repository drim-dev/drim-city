using System.Net;
using Common.Tests.Database.Harnesses;
using Common.Tests.Http.Harnesses;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Tests.Extensions;
using DrimCity.WebApi.Tests.Features.Posts.Contracts;
using DrimCity.WebApi.Tests.Fixtures;
using FluentAssertions.Equivalency;
using RestSharp;

namespace DrimCity.WebApi.Tests.Features.Posts.Requests;

[Collection(PostsTestsCollection.Name)]
public class GetCommentsTests : IAsyncLifetime
{
    private readonly DatabaseHarness<Program, AppDbContext> _database;
    private readonly TestFixture _fixture;
    private readonly HttpClientHarness<Program> _httpClient;

    public GetCommentsTests(TestFixture fixture)
    {
        _fixture = fixture;
        _database = fixture.Database;
        _httpClient = fixture.HttpClient;
    }

    public Task InitializeAsync() => _fixture.Reset(CreateCancellationToken());

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<RestResponse<CommentContract[]>> Act(string postSlug)
    {
        var restClient = new RestClient(_httpClient.CreateClient());
        return await restClient.ExecuteGetAsync<CommentContract[]>($"/posts/{postSlug}/comments",
            CreateCancellationToken());
    }

    [Fact]
    public async Task Should_return_comments()
    {
        var account = CreateAccount();
        await _database.Save(account);

        var post = CreatePost(account.Id);
        await _database.Save(post);

        var comments = await CreateAndSaveComments(post, 3);

        var restResponse = await Act(post.Slug);

        restResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseComments = restResponse.Data;
        responseComments.Should().NotBeNullOrEmpty();

        responseComments.Should().BeEquivalentTo(comments, CommentEquivalencyOptions);
    }

    [Fact]
    public async Task Should_return_comments_only_by_post_in_request()
    {
        var account = CreateAccount();
        await _database.Save(account);

        var unexpectedPost = CreatePost(account.Id);
        var expectedPost = CreatePost(account.Id);

        await _database.Save(unexpectedPost, expectedPost);

        var unexpectedComments = await CreateAndSaveComments(unexpectedPost, 1);
        var expectedComments = await CreateAndSaveComments(expectedPost, 1);

        var restResponse = await Act(expectedPost.Slug);

        var responseComments = restResponse.Data;
        responseComments.ShouldNotBeNullOrEmpty();

        responseComments.Select(x => x.Id).Should().NotContain(unexpectedComments.Select(x => x.Id));
        responseComments.Select(x => x.Id).Should().Contain(expectedComments.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_return_not_found_when_post_does_not_exist()
    {
        var restResponse = await Act("notExistingPostSlug");

        restResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var responseComments = restResponse.Data;
        responseComments.Should().BeNull();
    }

    [Fact]
    public async Task Should_return_comments_ordered_ascending_by_created_at()
    {
        var account = CreateAccount();
        await _database.Save(account);

        var post = CreatePost(account.Id);
        await _database.Save(post);

        var comment3 = CreateComment(account.Id, post.Id, 20.October(2023).AsUtc().AddHours(3));
        var comment1 = CreateComment(account.Id, post.Id, 20.October(2023).AsUtc().AddHours(1));
        var comment2 = CreateComment(account.Id, post.Id, 20.October(2023).AsUtc().AddHours(2));
        await _database.Save(comment1, comment2, comment3);

        var restResponse = await Act(post.Slug);

        restResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseComments = restResponse.Data;
        responseComments.Should().NotBeNullOrEmpty();

        responseComments.Should().BeInAscendingOrder(commentModel => commentModel.CreatedAt);
    }

    private async Task<Comment[]> CreateAndSaveComments(Post post, int count, DateTime? createdAt = null)
    {
        var comments = Enumerable.Range(1, count)
            .Select(_ => CreateComment(post.AuthorId, post.Id, createdAt))
            .ToArray();

        await _database.Save(comments.Cast<object>().ToArray());

        return comments;
    }

    private EquivalencyAssertionOptions<Comment>
        CommentEquivalencyOptions(EquivalencyAssertionOptions<Comment> config) =>
        config
            .Excluding(comment => comment.AuthorId)
            .Excluding(comment => comment.Author)
            .Excluding(comment => comment.PostId)
            .Excluding(comment => comment.Post);
}
