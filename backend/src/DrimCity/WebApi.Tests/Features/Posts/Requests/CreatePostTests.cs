using System.Net;
using AutoBogus;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using WebApi.Tests.Common.Helpers;
using WebApi.Tests.Features.Posts.Contracts;
using WebApi.Tests.Fixtures;
using Xunit;

namespace WebApi.Tests.Features.Posts.Requests;

[Collection(PostsTestsCollection.Name)]
public class CreatePostTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    public CreatePostTests(TestFixture fixture) => _fixture = fixture;

    private async Task<(PostContract?, HttpResponseMessage)> Act(CreatePostRequestContract request)
    {
        var client = _fixture.HttpClient.CreateClient();
        return await client.PostTyped<PostContract>("/posts", request, Create.CancellationToken());
    }

    [Fact]
    public async Task Should_create_post()
    {
        var request = AutoFaker.Generate<CreatePostRequestContract>();

        var (responsePost, httpResponse) = await Act(request);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        responsePost.Should().NotBeNull();

        httpResponse.Headers.Location.Should().Be($"/posts/{responsePost!.Slug}");

        responsePost.Id.Should().BeGreaterOrEqualTo(0);
        responsePost.Title.Should().Be(request.Title);
        responsePost.Content.Should().Be(request.Content);
        responsePost.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 1.Seconds());
        responsePost.Slug.Should().NotBeEmpty();

        var dbPost = await _fixture.Database.Execute(async x =>
            await x.Posts.SingleOrDefaultAsync(p => p.Id == responsePost.Id, Create.CancellationToken()));

        dbPost.Should().NotBeNull();
        dbPost!.Should().BeEquivalentTo(responsePost);
    }

    public async Task InitializeAsync()
    {
        await _fixture.Database.Clear(Create.CancellationToken());
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public record CreatePostRequestContract(string Title, string Content);
