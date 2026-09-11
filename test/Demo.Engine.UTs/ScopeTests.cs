// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Microsoft.Extensions.DependencyInjection;
using TUnit.Assertions.Should;
using TUnit.Assertions.Should.Extensions;

namespace Demo.Engine.UTs;

public class ScopeTests
{
    [Test]
    public async Task TestScopes()
    {
        var services = new ServiceCollection();
        _ = services.AddScoped<SingletonService>();

        using var sp = services.BuildServiceProvider();

        var outerSingleton = sp.GetRequiredService<SingletonService>();

        await using var scope = sp.CreateAsyncScope();
        var innerSingleton = scope.ServiceProvider.GetRequiredService<SingletonService>();
        var innerSingleton2 = scope.ServiceProvider.GetRequiredService<SingletonService>();

        await outerSingleton.ID.Should().NotBeEqualTo(innerSingleton.ID);

        await innerSingleton2.ID.Should().BeEqualTo(innerSingleton2.ID);
    }

    private sealed class SingletonService
    {
        public Guid ID { get; } = Guid.NewGuid();
    }
}