// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using BenchmarkDotNet.Running;

//BenchmarkRunner.Run<ValueResultBenchmark>();
BenchmarkSwitcher
    .FromAssembly(typeof(Program).Assembly)
    .Run(args);