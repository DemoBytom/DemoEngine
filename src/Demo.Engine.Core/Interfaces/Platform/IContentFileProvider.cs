// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Core.Interfaces.Platform;

public interface IContentFileProvider
{
    Stream CreateFile(string fileSubPath);

    bool FileExists(string fileSubPath);

    string GetAbsolutePath(string path);

    Stream OpenFile(string fileSubPath);
}