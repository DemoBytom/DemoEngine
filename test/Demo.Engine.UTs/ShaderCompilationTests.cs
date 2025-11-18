// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Platform.DirectX12.Shaders;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace Demo.Engine.UTs;

public class ShaderCompilationTests
{
    [Test]
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

        shader.ID.ShouldBe(ShaderId.FullscreenTriangle);
        shader.Size.ShouldBe(6);
        shader.ShaderBlob.ToArray().ShouldBe([55, 123, 55, 46, 23, 123]);

        var shader2 = engineShaderManager.GetShader((ShaderId)2);
        shader2.ID.ShouldBe((ShaderId)2);
        shader2.Size.ShouldBe(4);
        shader2.ShaderBlob.ToArray().ShouldBe([154, 21, 14, 33]);

        static async IAsyncEnumerable<Task<ShaderContent>> GetShaders()
        {
            var blob = new byte[] { 55, 123, 55, 46, 23, 123 };
            var blob2 = new byte[] { 154, 21, 14, 33 };

            await Task.Yield();

            yield return Task.FromResult(new ShaderContent(
                ShaderId.FullscreenTriangle,
                blob));

            yield return Task.FromResult(new ShaderContent(
                (ShaderId)2,
                blob2));
        }
    }
}