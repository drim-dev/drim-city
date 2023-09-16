using System.Net;
using AutoBogus;
using DrimCity.WebApi.Features.Posts.Requests;
using DrimCity.WebApi.Tests.Common.Helpers;
using DrimCity.WebApi.Tests.Features.Posts.Contracts;
using DrimCity.WebApi.Tests.Fixtures;
using FluentAssertions;
using FluentAssertions.Extensions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DrimCity.WebApi.Tests.Features.Posts.Requests;

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

public class CreatePostValidatorTests
{
    private readonly CreatePost.RequestValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_have_error_when_title_empty(string title)
    {
        var request = new CreatePost.Request(title, "content");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title).WithErrorCode("posts:validation:title_required");
    }

    [Fact]
    public void Should_have_error_when_title_exceeds_max_length()
    {
        var request = new CreatePost.Request(new string('a', 301), "content");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title).WithErrorCode("posts:validation:title_exceeds_max_length");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_have_error_when_content_empty(string content)
    {
        var request = new CreatePost.Request("title", content);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Content).WithErrorCode("posts:validation:content_required");
    }

    [Fact]
    public void Should_have_error_when_content_exceeds_max_length()
    {
        var request = new CreatePost.Request("title", new string('a', 100_001));

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Content).WithErrorCode("posts:validation:content_exceeds_max_length");
    }

    [Fact]
    public void Should_not_have_errors_when_request_valid()
    {
        var request = new CreatePost.Request("title", "content");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
