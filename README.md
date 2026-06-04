# Laminar Engine

<p align="center">
  <img src="Engine.gif" alt="Engine Preview">
</p>

Laminar Engine is a Unity3D-inspired 2D/3D video game engine built entirely from scratch with just **C#**.

## Features

- Custom engine written in C#
- Editor (C#/C++) (ImGui): Windows, MacOs.
- 2D Rendering (Sprites/Tilemap)
- 2D Physics (Box2D)
- Sprite batching.
- Particle system.
- Dynamic text rendering.
- Audio system.
- Modular architecture for easy expansion.
- Post-processing stack system (via framebuffer passes)
- Custom file system (simplified).
- Gamepad support.
- Build assets in dev mode (Unity-like import system) and release mode.
- Cross-platform: Windows, MacOs, Android.

## Roadmap
- [x] Implement Audio system.
- [x] File system.
- [x] Tilemap optimized auto collider.
- [x] Font rendering.
- [x] Windows deploy.
- [x] Implement particle system.
- [ ] Tests.
- [ ] 3D rendering.
- [ ] Jolt 3D physics. 
- [ ] Implement exhaustive test suite.
- [ ] Optimize rendering, and expand it to support complex geometries.
- [ ] Make the demo game (Proper architecture).
- [ ] Linux, and iOS support.

### Platforms
- Windows
- MacOs
- Android (Experimental)
- iOS (In-Progress)

## Getting Started
#### Note: If you are only interested in playing the game, download the repo, and go to DemoBin/win32

- Windows or macOS: .NET 9.0 SDK or later: https://dotnet.microsoft.com/en-us/download/dotnet/9.0
- cd Editor
- dotnet build
- If using Visual Studio, select the editor as the startup project.
- Building the game: Open the editor and go to the Build tab and hit the build button on the top. The binaries can be found on: _Ship/<platform_name>, or your custom path.
  
  
Recommended
--------
- Use an IDE like Visual Studio 2022+, Rider, or VS Code with C# support.

## License
This project is licensed under the Non-Commercial Copyleft License (NCCL).

### You are allowed to:
- Fork and modify it for non-commercial purposes
- Use parts of it in your own non-commercial projects, with attribution

### You are NOT allowed to:
- Use this code in commercial games, engines, or revenue-generating products
- Sell or monetize any part of it
