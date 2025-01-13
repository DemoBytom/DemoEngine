// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Platform;
using Microsoft.Extensions.Hosting;

namespace Demo.Engine.Core.Platform;

internal sealed class ContentFileProvider(
    IHostEnvironment environment)
    : IContentFileProvider
{
    private readonly IHostEnvironment _environment = environment;

    public Stream CreateFile(
        string fileSubPath)
    {
        var path = Path.Combine(
            _environment.ContentRootPath,
            fileSubPath);
        var dirPath = Path.GetDirectoryName(path);

        ArgumentException.ThrowIfNullOrEmpty(dirPath);

        if (!Directory.Exists(dirPath))
        {
            _ = Directory.CreateDirectory(dirPath);
        }

        return new FileStream(
                path: path,
                mode: FileMode.Create,
                access: FileAccess.Write);
    }

    public Stream OpenFile(
        string fileSubPath)
        => new FileStream(
            path: Path.Combine(
                _environment.ContentRootPath,
                fileSubPath),
            mode: FileMode.Open,
            access: FileAccess.Read);

    public bool FileExists(
        string fileSubPath)
        => File.Exists(
            Path.Combine(
                _environment.ContentRootPath,
                fileSubPath));
}