// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Demo.Engine.UTs;

public class ScopeTests
{
    [Test]
    public void TestScopes()
    {
        var services = new ServiceCollection();
        _ = services.AddScoped<SingletonService>();

        using var sp = services.BuildServiceProvider();

        var outerSingleton = sp.GetRequiredService<SingletonService>();

        using var scope = sp.CreateScope();
        var innerSingleton = scope.ServiceProvider.GetRequiredService<SingletonService>();
        var innerSingleton2 = scope.ServiceProvider.GetRequiredService<SingletonService>();

        outerSingleton.ID.ShouldNotBe(innerSingleton.ID);

        innerSingleton2.ID.ShouldBe(innerSingleton2.ID);
    }

    private class SingletonService
    {
        public Guid ID { get; } = Guid.NewGuid();
    }
}