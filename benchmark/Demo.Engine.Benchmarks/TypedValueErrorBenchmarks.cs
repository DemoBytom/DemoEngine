// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Jobs;
using Demo.Tools.Common.ValueResults;
using Microsoft.Extensions.Logging;
using static Demo.Tools.Common.ValueResults.TypedValueError;

namespace Demo.Engine.Benchmarks;

[MemoryDiagnoser]
[JitStatsDiagnoser]
//[SimpleJob(runtimeMoniker: RuntimeMoniker.Net90, baseline: true)]
[SimpleJob(runtimeMoniker: RuntimeMoniker.Net10_0)]
public class TypedValueErrorBenchmarks
{
    private readonly ILogger<TypedValueErrorBenchmarks> _logger;

    private const string PARAM_NAME = "TEST_INT";
    private const string ERROR_MESSAGE = "Error Message";

    public TypedValueErrorBenchmarks()
    {
        _logger = LoggerFactory
            .Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning))
            .CreateLogger<TypedValueErrorBenchmarks>();
    }

    [Benchmark(Baseline = true)]
    public ValueResult<int, TypedValueError> OutOfRangeError()
    {
        var error = new TypedValueError(
                ErrorTypes.OutOfRange,
                new ArgumentOutOfRangeError(
                    PARAM_NAME,
                    ERROR_MESSAGE));

        return ValueResult<int, TypedValueError>.Failure(error);
    }

    [Benchmark]
    public TypedValueError CreateOutOfRangeErrorAlone()
        => new(
            ErrorTypes.OutOfRange,
            new ArgumentOutOfRangeError(
                PARAM_NAME,
                ERROR_MESSAGE));

    [Benchmark]
    public ArgumentOutOfRangeError CreateArgumentOutOfRangeErrorAlone()
        => new(
            PARAM_NAME,
            ERROR_MESSAGE);

    [Benchmark]
    public TypedValueError<ArgumentOutOfRangeError> CreateTypedValueError_Generic()
        => new TypedValueError<ArgumentOutOfRangeError>(
            ErrorTypes.OutOfRange,
            new ArgumentOutOfRangeError(
                PARAM_NAME,
                ERROR_MESSAGE));

    [Benchmark]
    public ValueResult<int, ValueError> ValueResult_CallLogAndReturnFailure_NoExtraParams()
        => ValueResult.LogAndReturnFailure<int>(
            _logger,
            logger => logger.LogInformation("Logging out of range error"),
            ERROR_MESSAGE);

    [Benchmark]
    public ValueResult<int, ValueError> ValueResult_CallLogAndReturnFailure_NoExtraParams_SourceGeneratedLogger()
        => ValueResult.LogAndReturnFailure<int>(
            _logger,
            LoggingExtensions.LogOutOfRangeError,
            ERROR_MESSAGE);

    [Benchmark]
    [SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "This is not an issue in this benchmark")]
    public ValueResult<int, ValueError> ValueResult_CallLogAndReturnFailure_WithParams()
        => ValueResult.LogAndReturnFailure<int, string, string>(
            _logger,
            (
                (logger, s1, s2) => logger.LogInformation("Logging out of range error for {paramName}, {errorMessage}", s1, s2),
                PARAM_NAME,
                ERROR_MESSAGE
            ),
            ERROR_MESSAGE);

    [Benchmark]
    public ValueResult<int, ValueError> ValueResult_CallLogAndReturnFailure_WithParams_SourceGeneratedLogger()
        => ValueResult.LogAndReturnFailure<int, string, string>(
            _logger,
            (
                LoggingExtensions.LogOutOfRangeError,
                PARAM_NAME,
                ERROR_MESSAGE
            ),
            ERROR_MESSAGE);

    [Benchmark]
    public ValueResult<int, ValueError> ValueResult_CallLogAndReturnFailure_WithParams_SourceGeneratedLogger_SplitCall()
        => ValueResult
            .LogAndReturn(
                _logger,
                LoggingExtensions.LogOutOfRangeError,
                PARAM_NAME,
                ERROR_MESSAGE)
            .Failure<int>(ERROR_MESSAGE);

}

public static partial class LoggingExtensions
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Logging out of range error")]
    public static partial void LogOutOfRangeError(
        this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Logging out of range error for {paramName}, {errorMessage}")]
    public static partial void LogOutOfRangeError(
        this ILogger logger,
        string paramName,
        string errorMessage);
}