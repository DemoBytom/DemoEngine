// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

var builder = DistributedApplication.CreateBuilder(args);

builder
    .AddProject<Projects.Demo_Engine>("demo-engine", "Demo.Engine")
    .WithIconName("Games")
    ;

await using var app = builder.Build();

await app.RunAsync();