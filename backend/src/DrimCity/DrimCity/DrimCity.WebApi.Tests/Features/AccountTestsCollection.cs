using DrimCity.WebApi.Tests.Fixtures;

namespace DrimCity.WebApi.Tests.Features;

[CollectionDefinition(Name)]
public class AccountTestsCollection : ICollectionFixture<TestFixture>
{
    public const string Name = nameof(AccountTestsCollection);
}
