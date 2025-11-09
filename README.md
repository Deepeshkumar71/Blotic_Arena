# 🎮 Blotic Arena - Desktop Gaming Launcher

A modern, sleek desktop application for managing and launching games from the Blotic Arena platform.

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![License](https://img.shields.io/badge/license-Proprietary-red)

## 🚀 Quick Start

### For Users
1. Download `BloticArena.exe` from the repository (stored via Git LFS)
2. Double-click to run - **No installation required!**
3. Scan the QR code with your phone to login
4. Start playing your favorite games!

**Note**: The executable is tracked with Git LFS. Clone the repository to get the full application.

### Production Build
- **File**: `BloticArena.exe` (189 MB)
- **Type**: Self-contained, single-file executable
- **No dependencies**: Everything bundled in one file

## ✨ Features

- **Modern UI**: Dark theme with purple accents matching the Blotic website
- **QR Code Login**: Scan QR code with your phone to login instantly
- **Game Library**: Browse and launch your favorite games
- **Profile Management**: View your profile and game statistics
- **Particle Effects**: Beautiful animated background

## 📋 Requirements

- Windows 10/11 (64-bit)
- .NET 8.0 Runtime (included in the executable)
- Internet connection for authentication

## 🛠️ Development

### Project Structure
```
Blotic_Arena/
├── BloticArena.exe          # Ready-to-run executable
├── docs/                    # Documentation files
├── Assets/                  # Images and icons
├── Config/                  # Configuration files
├── Controls/                # Custom UI controls
├── Database/                # Database models
├── Models/                  # Data models
├── Services/                # Business logic services
├── MainWindow.xaml          # Main UI layout
└── MainWindow.xaml.cs       # Main UI logic
```

### Build from Source

```bash
# Clean previous builds
dotnet clean

# Build the project
dotnet build

# Run in development
dotnet run

# Publish standalone executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 📚 Documentation

All documentation files are organized in the `docs/` folder:
- Build guides
- Database information
- Update logs
- Integration guides

## 🎨 Theme Colors

- **Background**: #000000 (Pitch Black)
- **Surface**: #0A0A0A (Near Black)
- **Primary**: #4F9CF9 (Blue)
- **Accent**: #CC75DB (Purple - Blotic brand)
- **Play Button**: #10B981 (Green)
- **Particles**: #FFFFFF (White)

## 🔗 Links

- [Blotic Website](https://blotic-bvucoep.vercel.app/)
- [GitHub Repository](https://github.com/Deepeshkumar71/Blotic_Arena)

## 📝 License

Copyright © 2025 Blotic BVUCOEP. All rights reserved.

---

**Version**: 1.0.0  
**Last Updated**: November 2025
