# DemoEngine
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/DemoBytom/DemoEngine/blob/master/LICENSE)

**DemoEngine** is a simple game engine written in C#, .NET Core 3.0 and DirectX.

Engine is currently developed for Windows 10 platform, with references to `Windows.Forms` and DirectX, but the code outside of those, should work wherever .NET Core 3.0 works. A provided layer of abstraction over platform specific codebase should make it possible to add cross platform later. 

Project is using [Vortice.Windows](https://github.com/amerkoleci/Vortice.Windows) as a DirectX wrapper, with parts of the main form handling based on [SharpDX](https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX.Desktop/RenderForm.cs)

## Credits
Development, contributions and bugfixes by:
 - Michał Dembski
 
## Build
In order to compile, you need **Visual Studio 2019, or newer** with the following workloads and components:
 - [x] .NET Core cross-platform development
 - [x] .NET desktop development
 - [x] [.NET Core 3.0.100 SDK](https://github.com/dotnet/core/blob/master/release-notes/3.0/3.0.0/3.0.0-download.md)
