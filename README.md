# DemoEngine
[![GitHub](https://img.shields.io/github/license/DemoBytom/DemoEngine?style=for-the-badge)](https://github.com/DemoBytom/DemoEngine/blob/master/LICENSE)  
`main` branch status:  
[![Coveralls](https://img.shields.io/coverallsCoverage/github/DemoBytom/DemoEngine?branch=main&style=for-the-badge&logo=coveralls)](https://coveralls.io/github/DemoBytom/DemoEngine?branch=main) [![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/DemoBytom/DemoEngine/CI.yml?branch=main&style=for-the-badge&logo=GitHub)](https://github.com/DemoBytom/DemoEngine/actions/workflows/CI.yml?query=branch%3Amain) [![GitHub branch check runs](https://img.shields.io/github/check-runs/DemoBytom/DemoEngine/main?style=for-the-badge&logo=GitHub)](https://github.com/DemoBytom/DemoEngine/commits/main/)

 **DemoEngine** is a simple game engine written in C#, .NET 10.0 and DirectX.

Engine is currently developed for Windows and Windows-on-ARM platforms, with references to `Windows.Forms` and DirectX.

Project is using [Vortice.Windows](https://github.com/amerkoleci/Vortice.Windows) as a DirectX wrapper, with parts of the main form handling based on [SharpDX](https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX.Desktop/RenderForm.cs).  

## Build
In order to compile, you need **Visual Studio 2026, or newer** setup for C# development, and [.NET Core 10.0.102 SDK](https://github.com/dotnet/core/blob/main/release-notes/10.0/10.0.2/10.0.2.md).  

**Command line** build can be done using [Nuke](https://nuke.build/) using the following commands:
```
dotnet tool restore
dotnet nuke
```

## Credits
Development, contributions and bugfixes by:
* [![](https://img.shields.io/badge/DemoBytom-Micha%C5%82%20Dembski-informational?logo=github&style=flat-square)](https://github.com/DemoBytom)
