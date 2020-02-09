# DemoEngine
[![GitHub](https://img.shields.io/github/license/DemoBytom/DemoEngine?style=for-the-badge)](https://github.com/DemoBytom/DemoEngine/blob/master/LICENSE) [![Coveralls github](https://img.shields.io/coveralls/github/DemoBytom/DemoEngine?style=for-the-badge&logo=coveralls)](https://coveralls.io/github/DemoBytom/DemoEngine)

**DemoEngine** is a simple game engine written in C#, .NET Core 3.1 and DirectX.

Engine is currently developed for Windows 10 platform, with references to `Windows.Forms` and DirectX, but the code outside of those, should work wherever .NET Core 3.1 works. A provided layer of abstraction over platform specific codebase should make it possible to add cross platform later.

Project is using [Vortice.Windows](https://github.com/amerkoleci/Vortice.Windows) as a DirectX wrapper, with parts of the main form handling based on [SharpDX](https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX.Desktop/RenderForm.cs)

## Build status:
|Branch|Status|Code coverage|
|---:|:---|:---|
|master|![GitHub Workflow Status (branch)](https://img.shields.io/github/workflow/status/DemoBytom/DemoEngine/CI/master?logo=Github&logoColor=white&style=flat-square)|[![Coveralls github branch](https://img.shields.io/coveralls/github/DemoBytom/DemoEngine/master?logo=coveralls&style=flat-square)](https://coveralls.io/github/DemoBytom/DemoEngine?branch=master)|
|develop|![GitHub Workflow Status (branch)](https://img.shields.io/github/workflow/status/DemoBytom/DemoEngine/CI/develop?logo=Github&logoColor=white&style=flat-square)|[![Coveralls github branch](https://img.shields.io/coveralls/github/DemoBytom/DemoEngine/develop?logo=coveralls&style=flat-square)](https://coveralls.io/github/DemoBytom/DemoEngine?branch=develop)|
|most recent|![GitHub Workflow Status](https://img.shields.io/github/workflow/status/DemoBytom/DemoEngine/CI?logo=github&logoColor=white&style=flat-square)|[![Coveralls github branch](https://img.shields.io/coveralls/github/DemoBytom/DemoEngine?logo=coveralls&style=flat-square)](https://coveralls.io/github/DemoBytom/DemoEngine)|

## Build
In order to compile, you need **Visual Studio 2019, or newer** with the following workloads and components:
 - [x] .NET Core cross-platform development
 - [x] .NET desktop development
 - [x] [.NET Core 3.1.100 SDK](https://github.com/dotnet/core/blob/master/release-notes/3.1/3.1.0/3.1.0.md)

**Command line** build can be done using [Nuke](https://nuke.build/):
* Using **Global Tool**:
   - Installation  
   `$ dotnet tool install Nuke.GlobalTool --global`
   - To run the build using Global Tool  
   `$ nuke Full`
 * Using **PowerShell**
   - `PS> .\build.ps1 Full`
 * Using **Shell**
   - `$ ./build.sh Full`
 * Support plugins are available for:
   - [JetBrains ReSharper](https://nuke.build/resharper)
   - [JetBrains Rider](https://nuke.build/rider)
   - [Microsoft VisualStudio](https://nuke.build/visualstudio)
   - [Microsoft VSCode](https://nuke.build/vscode)

## Credits
Development, contributions and bugfixes by:
* [![](https://img.shields.io/badge/DemoBytom-Micha%C5%82%20Dembski-informational?logo=github&style=flat-square)](https://github.com/DemoBytom)
