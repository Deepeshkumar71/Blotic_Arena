# 🚀 Blotic Arena - Production Release v1.0.0

## 📦 Release Package

### Main Executable
- **File**: `BloticArena.exe`
- **Size**: 189 MB
- **Type**: Self-contained, single-file executable
- **Platform**: Windows 10/11 (64-bit)
- **Dependencies**: None (everything bundled)

### What's Included
✅ .NET 8.0 Runtime (embedded)
✅ All required DLLs (embedded)
✅ Application icon (embedded)
✅ Assets and resources (embedded)
✅ No external dependencies needed

## 🎯 Deployment Instructions

### For End Users
1. Download `BloticArena.exe`
2. Place it anywhere on your computer
3. Double-click to run
4. **That's it!** No installation needed

### For Distribution
- Upload `BloticArena.exe` to GitHub Releases
- Users can download and run immediately
- No installer required
- 100% portable

## ✨ Key Features

### Authentication
- QR Code login system
- Instant authentication via mobile scan
- Secure Supabase integration
- Session management

### User Interface
- **Theme**: Pitch black background with white particles
- **Logo**: 70% opacity centered background logo
- **Sidebar**: 100px thin sidebar with navigation
- **Profile**: Large scalable button with first name display
- **Animations**: Smooth transitions and particle effects

### Game Management
- Browse registered events
- View games remaining count
- Launch games with one click
- Track game history (coming soon)

### Profile
- User information display
- Games remaining counter (sums all registrations)
- Profile picture support
- Clean, minimal design

## 🔧 Technical Specifications

### Build Configuration
```xml
<OutputType>WinExe</OutputType>
<TargetFramework>net8.0-windows</TargetFramework>
<SelfContained>true</SelfContained>
<RuntimeIdentifier>win-x64</RuntimeIdentifier>
<PublishSingleFile>true</PublishSingleFile>
<ApplicationIcon>Assets\blotic_logo.ico</ApplicationIcon>
```

### Build Command
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

### System Requirements
- **OS**: Windows 10 version 1809 or later, Windows 11
- **Architecture**: x64 (64-bit)
- **RAM**: 512 MB minimum, 1 GB recommended
- **Disk Space**: 200 MB
- **Internet**: Required for authentication and game data

## 🎨 Visual Design

### Color Palette
| Element | Color | Hex Code |
|---------|-------|----------|
| Background | Pitch Black | #000000 |
| Surface | Near Black | #0A0A0A |
| Primary | Blue | #4F9CF9 |
| Accent | Purple | #CC75DB |
| Play Button | Green | #10B981 |
| Particles | White | #FFFFFF |
| Text Primary | White | #FFFFFF |

### UI Components
- **Sidebar**: 100px width, black background
- **Title Bar**: 28px font, no logo
- **Profile Button**: 160x55px, scalable on hover
- **Logo**: 450x450px, 70% opacity, centered
- **Particles**: 100 white dots, fast movement
- **QR Code**: Centered logo overlay

## 📝 Changelog

See [CHANGELOG.md](CHANGELOG.md) for detailed version history.

## 🐛 Known Issues

None reported in v1.0.0

## 🔒 Security

- Credentials stored securely in Supabase
- No local password storage
- QR code sessions expire after 5 minutes
- HTTPS communication only

## 📞 Support

For issues or questions:
- GitHub Issues: [Create an issue](../../issues)
- Email: support@blotic.bvucoep.edu.in
- Website: https://blotic.bvucoep.edu.in

## 📄 License

Copyright © 2025 Blotic BVUCOEP. All rights reserved.

---

**Release Date**: November 9, 2025  
**Build Number**: 1.0.0.0  
**Compiled**: .NET 8.0.11  
**Target**: win-x64
