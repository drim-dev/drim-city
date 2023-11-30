#nullable disable

using System.Diagnostics.CodeAnalysis;
using FluentAssertions.Collections;
using FluentAssertions.Primitives;

namespace DrimCity.WebApi.Tests.Extensions;

public static class FluentAssertionsExtensions
{
    public static AndConstraint<ObjectAssertions> ShouldNotBeNull([NotNull] this object actualValue) =>
        actualValue.Should().NotBeNull();

    public static AndConstraint<GenericCollectionAssertions<T>> ShouldNotBeNullOrEmpty<T>(
        [NotNull] this IEnumerable<T> actualValue) =>
        actualValue.Should().NotBeNullOrEmpty();
}
