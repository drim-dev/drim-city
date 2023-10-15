using DrimCity.WebApi.Features.Posts.Services;

namespace DrimCity.WebApi.Tests.Features.Posts.Services;

public class SlugGeneratorTests
{
    private const string Text = "The Tale of Drim City";

    [Fact]
    public void Should_create_slug()
    {
        var slug = SlugGenerator.CreateSlug(Text);

        slug.Should().StartWith("the-tale-of-drim-city-");

        var suffix = slug[^8..];
        suffix.Should().MatchRegex(@"^[a-f0-9]{8}$");
    }

    [Fact]
    public void Should_create_different_slugs_for_same_text()
    {
        var slug1 = SlugGenerator.CreateSlug(Text);
        var slug2 = SlugGenerator.CreateSlug(Text);

        slug1.Should().NotBe(slug2);
    }

    [Fact]
    public void Should_create_slug_from_non_ascii_text()
    {
        var slug = SlugGenerator.CreateSlug("История о Дрим Сити");

        slug.Should().StartWith("istoriya-o-drim-siti-");

        var suffix = slug[^8..];
        suffix.Should().MatchRegex(@"^[a-f0-9]{8}$");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_throw_when_text_is_empty(string text)
    {
        Action act = () => SlugGenerator.CreateSlug(text);
        act.Should().Throw<ArgumentException>().WithMessage("*text*");
    }
}
