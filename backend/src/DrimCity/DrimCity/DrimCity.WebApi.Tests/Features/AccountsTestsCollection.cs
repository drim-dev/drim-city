using DrimCity.WebApi.Tests.Fixtures;
using Xunit;

namespace DrimCity.WebApi.Tests.Features;

[CollectionDefinition(Name)]
public class AccountsTestsCollection : ICollectionFixture<TestFixture>
{
    public const string Name = nameof(AccountsTestsCollection);
}