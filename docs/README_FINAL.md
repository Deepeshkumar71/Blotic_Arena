# 🎮 Blotic Arena - Game Launcher

A modern, beautiful game launcher for Windows with the Blotic brand.

![Blotic Arena](Assets/blotic_logo.png)

## Features

✨ **Modern UI** - Legion Arena inspired design
🎮 **Games Only** - Focused game launcher
🚀 **Quick Launch** - Protocol-based game launching
🎨 **Particle Background** - Animated purple/blue particles
🖼️ **Large Icons** - 120x120px game icons
▶️ **Prominent Play Buttons** - Full-width, easy to click
🔍 **Search** - Find games quickly
🎯 **Smart Detection** - Auto-detects Steam, Valorant, Epic, etc.

## Quick Start

### Development Mode:
```powershell
cd Blotic_Arena
dotnet restore
dotnet build
dotnet run
```

### Create EXE:
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

EXE location: `bin\Release\net8.0-windows\win-x64\publish\BloticArena.exe`

## Supported Games

- ✅ Steam
- ✅ Valorant
- ✅ League of Legends
- ✅ Epic Games Launcher
- ✅ Minecraft
- ✅ Roblox
- ✅ Any game with "game" in the name

## UI Design

### Layout:
- **Title Bar**: Blotic logo + app name
- **Search Bar**: Find games quickly
- **Header**: "MY GAMES" with game count
- **Game Cards**: Large 240x320px cards
- **Play Buttons**: Full-width, prominent

### Card Design:
- 120x120px game icon
- 18px game name (bold)
- Full-width PLAY button
- Purple accent on hover

## Technical Stack

- **Framework**: .NET 8.0 + WPF
- **Language**: C#
- **UI**: XAML
- **Icons**: Windows Shell API extraction
- **Launching**: Protocol URLs (steam://, valorant://, etc.)

## Project Structure

```
Blotic_Arena/
├── Assets/
│   ├── blotic_logo.png
│   └── blotic_logo.ico
├── Models/
│   └── AppInfo.cs
├── Services/
│   ├── AppScanner.cs
│   └── IconExtractor.cs
├── App.xaml
├── MainWindow.xaml
├── MainWindow.xaml.cs
└── BloticArena.csproj
```

## Configuration

### Project Settings:
- **Target**: Windows x64
- **Framework**: .NET 8.0
- **Output**: Single-file EXE
- **Icon**: Blotic logo
- **Size**: ~70-100 MB (self-contained)

### Theme Colors:
- **Background**: #0F1419 (Dark blue-gray)
- **Surface**: #1A1F2E (Slate)
- **Primary**: #4F9CF9 (Blue)
- **Accent**: #CC75DB (Purple - Blotic brand)
- **Play Button**: #10B981 (Green)

## Building for Production

### Single-File EXE (Recommended):
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true
```

### Framework-Dependent (Smaller):
```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

## Distribution

### What to Share:
- `BloticArena.exe` (single file, no installation needed)

### Requirements:
- Windows 10/11 (x64)
- No .NET installation needed (self-contained)

### File Size:
- Self-contained: ~70-100 MB
- Framework-dependent: ~5-10 MB (requires .NET 8.0)

## Features in Detail

### Game Detection:
- Scans Windows Registry
- Scans Start Menu shortcuts
- Filters out non-games automatically
- Extracts real game icons

### Game Launching:
- Protocol URLs for Steam, Valorant, Epic
- Direct executable launching
- Shortcut resolution
- Smart uninstaller avoidance

### UI Features:
- Animated particle background (80 particles)
- Search functionality
- Hover effects
- Responsive layout
- Modern card design

## Version History

- **v2.0**: Legion Arena inspired redesign
- **v1.7**: Games-only launcher
- **v1.6**: Particle background + fixed layout
- **v1.5**: Fixed game launchers (Steam, Valorant)
- **v1.4**: Category filtering
- **v1.3**: Website theme integration
- **v1.2**: Real icon extraction
- **v1.1**: Fixed window controls
- **v1.0**: Initial release

## Credits

- **Design**: Inspired by Legion Arena
- **Branding**: Blotic
- **Framework**: .NET + WPF
- **Icons**: Windows Shell API

## License

© 2025 Blotic. All rights reserved.

---

**Made with ❤️ for gamers**
