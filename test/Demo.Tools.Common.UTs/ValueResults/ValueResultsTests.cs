// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Tools.Common.ValueResults;
using Shouldly;
using Xunit;

namespace Demo.Tools.Common.UTs.ValueResults;

public class ValueResultsTests
{
    [Fact]
    public void TestGeneralTypedError()
    {
        var result = TypedValueError.General<int>("General Error");

        result.Error.ErrorType.ShouldBe(TypedValueError.ErrorTypes.General);
        result.Error.Message.ShouldBe("General Error");
        result.Error.Error.ShouldBeOfType<GeneralError>();
        result.Error.Error.Message.ShouldBe("General Error");
    }

    [Fact]
    public void OutOfRangeError()
    {
        var error = new TypedValueError(
                TypedValueError.ErrorTypes.OutOfRange,
                new ArgumentOutOfRangeError(
                    "PARAM_NAME",
                    "Error Message"));

        error.ErrorType.ShouldBe(TypedValueError.ErrorTypes.OutOfRange);
        error.Message.ShouldBe("Error Message");
        error.Error.ShouldBeOfType<ArgumentOutOfRangeError>();
        error.Error.Message.ShouldBe("Error Message");
    }

    [Fact]
    public void OutOfRangeError_Generic()
    {
        var error = TypedValueError<ArgumentOutOfRangeError>.OutOfRange(
            "PARAM_NAME",
            "Error Message");

        error.ErrorType.ShouldBe(TypedValueError.ErrorTypes.OutOfRange);
        error.Message.ShouldBe("Error Message");
        error.Error.ShouldBeOfType<ArgumentOutOfRangeError>();
        error.Error.Message.ShouldBe("Error Message");
    }

    [Fact]
    public void LoggableError()
    {
        var loggableError = new LoggableError(
            $"Error occured: {1}",
            "Error occured: {someInt}",
            [1]);

        loggableError.Message.ShouldBe("Error occured: 1");
    }
}