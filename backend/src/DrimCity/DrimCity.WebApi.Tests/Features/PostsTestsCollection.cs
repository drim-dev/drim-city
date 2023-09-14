using DrimCity.WebApi.Tests.Fixtures;
using Xunit;

namespace DrimCity.WebApi.Tests.Features;

[CollectionDefinition(Name)]
public class PostsTestsCollection : ICollectionFixture<TestFixture>
{
    public const string Name = nameof(PostsTestsCollection);

    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
