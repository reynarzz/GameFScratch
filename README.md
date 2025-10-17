# GFS (Game From Scratch)

![Gameplay](Intro.gif)

GFS is a video game and game engine built entirely from scratch in **C#**, heavily inspired by the **Unity3D C# API**.

## Features

- Custom engine written in C#
- 2D Rendering (Sprites/Tilemap)
- Audio system.
- Input handling, physics (Box2D)
- Modular architecture for easy expansion.
- Post-processing stack system.
- Custom file system (simplified).
- Build assets in dev mode (Unity-like import system) and release mode.

## Roadmap
- [x] Implement Audio system.
- [x] File system.
- [x] Tilemap optimized auto collider.
- [x] Font rendering.
- [x] Windows deploy.
- [ ] Implement particle system.
- [ ] Optimize rendering, and expand it to support complex geometries.
- [ ] Make the demo game (Proper architecture).
- [ ] macOS, Android, and IOS platforms deploy.

## Getting Started

### Prerequisites
#### Note: If you are only interested in playing the game, download the repo, and go to DemoBin/win32

- Windows
- .NET 9.0 SDK or later: https://dotnet.microsoft.com/en-us/download/dotnet/9.0

Optional
--------
- Use an IDE like Visual Studio 2022+, Rider, or VS Code with C# support.
- Make sure your PATH includes the .NET 9.0 SDK.
