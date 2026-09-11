// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Tools.Common.ValueResults;
using TUnit.Assertions.Should;
using TUnit.Assertions.Should.Extensions;

namespace Demo.Tools.Common.UTs.ValueResults;

public class ValueResultsTests
{
    [Test]
    public async Task TestGeneralTypedError()
    {
        var result = TypedValueError.General<int>("General Error");

        var errorType = result.Error.ErrorType;
        var message = result.Error.Message;
        var innerError = result.Error.InnerError;
        var innerErrorMessage = result.Error.InnerError.Message;

        await errorType.Should().BeEqualTo(TypedValueError.ErrorTypes.General);
        await message.Should().BeEqualTo("General Error");
        await innerError.Should().BeOfType(typeof(GeneralError));
        await innerErrorMessage.Should().BeEqualTo("General Error");
    }

    [Test]
    public async Task OutOfRangeErrorAsync()
    {
        var error = new TypedValueError(
                TypedValueError.ErrorTypes.OutOfRange,
                new ArgumentOutOfRangeError(
                    "PARAM_NAME",
                    "Error Message"));

        var errorType = error.ErrorType;
        var message = error.Message;
        var innerError = error.InnerError;
        var innerErrorMessage = error.InnerError.Message;

        await errorType.Should().BeEqualTo(TypedValueError.ErrorTypes.OutOfRange);
        await message.Should().BeEqualTo("Error Message");
        await innerError.Should().BeOfType(typeof(ArgumentOutOfRangeError));
        await innerErrorMessage.Should().BeEqualTo("Error Message");
    }

    [Test]
    public async Task OutOfRangeError_GenericAsync()
    {
        var error = TypedValueError.OutOfRange<int>(
            "PARAM_NAME",
            "Error Message");

        var errorType = error.Error.ErrorType;
        var message = error.Error.Message;
        var innerError = error.Error.InnerError;
        var innerErrorMessage = error.Error.InnerError.Message;

        await errorType.Should().BeEqualTo(TypedValueError.ErrorTypes.OutOfRange);
        await message.Should().BeEqualTo("Error Message");
        await innerError.Should().BeOfType(typeof(ArgumentOutOfRangeError));
        await innerErrorMessage.Should().BeEqualTo("Error Message");
    }
}