# Blotic Arena - EXE Build Guide

## Application Icon

✅ **Blotic Logo** is now the default app icon everywhere:
- Taskbar icon
- Window title bar icon
- Desktop shortcut icon
- File explorer icon
- Alt+Tab icon

### Icon Files:
- `Assets/blotic_logo.png` - PNG source
- `Assets/blotic_logo.ico` - Windows ICO format

## Development Mode (Current)

### Run for Testing:
```powershell
dotnet restore
dotnet build
dotnet run
```

This is the easiest way to develop and test the app.

## Production Mode - Create EXE

### Option 1: Single-File EXE (Recommended)
Creates a single `.exe` file with everything bundled:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true
```

**Output**: `bin\Release\net8.0-windows\win-x64\publish\BloticArena.exe`

**Features**:
- ✅ Single EXE file (no DLLs needed)
- ✅ Includes .NET runtime (works on any Windows PC)
- ✅ Optimized and ready to run
- ✅ ~70-100 MB file size

### Option 2: Framework-Dependent EXE (Smaller)
Requires .NET 8.0 to be installed on target PC:

```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

**Output**: `bin\Release\net8.0-windows\win-x64\publish\BloticArena.exe`

**Features**:
- ✅ Smaller file size (~5-10 MB)
- ❌ Requires .NET 8.0 Runtime on target PC
- ✅ Faster to build

### Option 3: Installer (Advanced)
For professional distribution, create an installer using:
- **WiX Toolset** - MSI installer
- **Inno Setup** - Setup.exe installer
- **NSIS** - Nullsoft installer

## Project Configuration

The project is already configured for EXE creation:

```xml
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <ApplicationIcon>Assets\blotic_logo.ico</ApplicationIcon>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PublishReadyToRun>true</PublishReadyToRun>
</PropertyGroup>
```

## Quick Build Commands

### For Testing (Fast):
```powershell
dotnet run
```

### For Production (Single EXE):
```powershell
dotnet publish -c Release
```

The EXE will be at:
```
bin\Release\net8.0-windows\win-x64\publish\BloticArena.exe
```

## Distribution

### What to Share:
1. **Single-File EXE**: Just share `BloticArena.exe`
2. **With Installer**: Create setup.exe with installer tool

### File Size:
- **Self-Contained**: ~70-100 MB (includes .NET runtime)
- **Framework-Dependent**: ~5-10 MB (requires .NET 8.0)

## Icon Everywhere

The Blotic logo will appear:
- ✅ In taskbar when app is running
- ✅ In window title bar
- ✅ In Alt+Tab switcher
- ✅ In Task Manager
- ✅ As desktop shortcut icon
- ✅ In file explorer
- ✅ In Start Menu (if installed)

## Troubleshooting

### Icon Not Showing:
1. Clean and rebuild:
   ```powershell
   dotnet clean
   dotnet build
   ```

2. Clear icon cache:
   ```powershell
   ie4uinit.exe -show
   ```

### EXE Too Large:
Use framework-dependent build:
```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

### Missing DLLs:
Use self-contained build:
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Recommended Workflow

### During Development:
```powershell
dotnet run
```
Fast, easy testing.

### For Production Release:
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
Single EXE, ready to distribute.

### For Final Distribution:
1. Build single-file EXE
2. Test on clean Windows PC
3. Create installer (optional)
4. Distribute

## File Structure

```
Blotic_Arena/
├── Assets/
│   ├── blotic_logo.png    ← Logo image
│   └── blotic_logo.ico    ← Windows icon
├── bin/
│   └── Release/
│       └── net8.0-windows/
│           └── win-x64/
│               └── publish/
│                   └── BloticArena.exe  ← Final EXE
├── BloticArena.csproj     ← Project config
└── MainWindow.xaml        ← UI
```

## Summary

✅ **Development**: Use `dotnet run` (easy, fast)
✅ **Production**: Use `dotnet publish` (creates EXE)
✅ **Icon**: Blotic logo everywhere automatically
✅ **Distribution**: Single EXE file, no installation needed
✅ **Size**: ~70-100 MB (includes everything)

The app is ready for both development and production! 🚀
