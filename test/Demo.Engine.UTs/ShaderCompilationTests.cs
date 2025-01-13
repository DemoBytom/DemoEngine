// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Platform.DirectX12.Shaders;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Demo.Engine.UTs;

public class ShaderCompilationTests
{
    [Fact]
    public async Task TestShaderDiscSaveAsync()
    {
        // Arrange
        var loggerMock = Substitute.For<ILogger<ShaderCompiler>>();
        var loggerMock2 = Substitute.For<ILogger<EngineShaderManager>>();
        var contentFileProvider = Substitute.For<IContentFileProvider>();

        //I expect a 34 byte file
        var fileBuffer = new byte[34];

        _ = contentFileProvider
            .CreateFile(
                EngineShaderManager.ENGINE_SHADERS_BIN_FILE)
            .Returns(_
                => new MemoryStream(fileBuffer));

        _ = contentFileProvider
            .OpenFile(
                EngineShaderManager.ENGINE_SHADERS_BIN_FILE)
            .Returns(_
                => new MemoryStream(fileBuffer));

        var engineShaderManager = new EngineShaderManager(
            loggerMock2,
            contentFileProvider);

        // Act
        var shaders = GetShaders();
        _ = await engineShaderManager.SaveEngineShaders(shaders);
        _ = await engineShaderManager.LoadEngineShaders();

        var shader = engineShaderManager.GetShader(ShaderId.FullscreenTriangle);

        _ = shader.ID.Should().Be(ShaderId.FullscreenTriangle);
        _ = shader.Size.Should().Be(6);
        _ = shader.ShaderBlob.ToArray().Should().BeEquivalentTo(new byte[] { 55, 123, 55, 46, 23, 123 });

        var shader2 = engineShaderManager.GetShader((ShaderId)2);
        _ = shader2.ID.Should().Be((ShaderId)2);
        _ = shader2.Size.Should().Be(4);
        _ = shader2.ShaderBlob.ToArray().Should().BeEquivalentTo(new byte[] { 154, 21, 14, 33 });

        static async IAsyncEnumerable<ShaderContent> GetShaders()
        {
            var blob = new byte[] { 55, 123, 55, 46, 23, 123 };
            var blob2 = new byte[] { 154, 21, 14, 33 };

            await Task.Yield();

            yield return new ShaderContent(
                 ShaderId.FullscreenTriangle,
                 blob);

            yield return new ShaderContent(
                (ShaderId)2,
                blob2);
        }
    }
}