// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Platform.DirectX12.Shaders;
using Microsoft.Extensions.Logging;
using TUnit.Assertions.Should;
using TUnit.Assertions.Should.Extensions;

namespace Demo.Engine.UTs;

public class ShaderCompilationTests
{
    [Test]
    public async Task TestShaderDiscSaveAsync()
    {
        // Arrange
        var loggerMock = ILogger<ShaderCompiler>.Mock(MockBehavior.Loose);
        var loggerMock2 = ILogger<EngineShaderManager>.Mock(MockBehavior.Loose);
        var contentFileProvider = IContentFileProvider.Mock(MockBehavior.Strict);
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
            loggerMock2.Object,
            contentFileProvider.Object);

        // Act
        var shaders = GetShaders();
        _ = await engineShaderManager.SaveEngineShaders(shaders);
        _ = await engineShaderManager.LoadEngineShaders();

        var shader = engineShaderManager.GetShader(ShaderId.FullscreenTriangle);

        await shader.ID.Should().BeEqualTo(ShaderId.FullscreenTriangle);
        await shader.Size.Should().BeEqualTo(6);
        await shader.ShaderBlob.ToArray().Should().BeEquivalentTo((byte[])[55, 123, 55, 46, 23, 123]);

        var shader2 = engineShaderManager.GetShader((ShaderId)2);
        await shader2.ID.Should().BeEqualTo((ShaderId)2);
        await shader2.Size.Should().BeEqualTo(4);
        await shader2.ShaderBlob.ToArray().Should().BeEquivalentTo((byte[])[154, 21, 14, 33]);

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