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
        result.Error.InnerError.ShouldBeOfType<GeneralError>();
        result.Error.InnerError.Message.ShouldBe("General Error");
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
        error.InnerError.ShouldBeOfType<ArgumentOutOfRangeError>();
        error.InnerError.Message.ShouldBe("Error Message");
    }

    [Fact]
    public void OutOfRangeError_Generic()
    {
        var error = TypedValueError.OutOfRange<int>(
            "PARAM_NAME",
            "Error Message");

        error.Error.ErrorType.ShouldBe(TypedValueError.ErrorTypes.OutOfRange);
        error.Error.Message.ShouldBe("Error Message");
        error.Error.InnerError.ShouldBeOfType<ArgumentOutOfRangeError>();
        error.Error.InnerError.Message.ShouldBe("Error Message");
    }
}