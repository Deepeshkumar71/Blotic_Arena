# Blotic Arena v1.5 - Fixed Game Launchers

## Critical Fixes

### 1. **Fixed Steam Opening Uninstaller**
- Now properly launches Steam client instead of uninstaller
- Uses protocol URL: `steam://open/main`
- Filters out uninstaller executables from registry

### 2. **Fixed Valorant & Riot Games Launch**
- Valorant now uses protocol: `valorant://launch`
- Riot Client properly detected and launched
- League of Legends uses: `league://launch`

### 3. **Protocol-Based Launcher System**
Similar to NVIDIA GeForce Experience, we now use Windows protocol handlers:
- **Steam**: `steam://open/main`
- **Valorant**: `valorant://launch`
- **League of Legends**: `league://launch`
- **Epic Games**: `com.epicgames.launcher://`

## How It Works

### Smart Launch Command Detection
1. **Check for known game protocols** (Steam, Valorant, etc.)
2. **Extract from DisplayIcon** (most reliable, excludes uninstallers)
3. **Search install directory** for main executable
4. **Filter out uninstallers** (unins*.exe, setup.exe, etc.)

### New AppInfo Property
- Added `LaunchCommand` property to store the actual command to launch
- Separate from `Path` which stores install location
- Supports both file paths and protocol URLs

### Uninstaller Filtering
Automatically excludes executables containing:
- `unins`, `uninst`
- `setup`, `install`
- `update`

## Technical Implementation

### Model Changes
```csharp
public class AppInfo
{
    public string LaunchCommand { get; set; } // NEW: Actual launch command
    public string Path { get; set; }          // Install location
    // ... other properties
}
```

### Scanner Changes
- Added `FindLaunchCommand()` method
- Protocol detection for popular game launchers
- Smart executable filtering in install directories
- DisplayIcon validation (excludes uninstallers)

### Launcher Changes
- Added protocol URL handler (`steam://`, `valorant://`, etc.)
- Uses `LaunchCommand` instead of `Path`
- Proper `UseShellExecute = true` for protocols

## Supported Game Launchers

✅ **Steam** - Opens Steam client
✅ **Valorant** - Launches via Riot Client
✅ **League of Legends** - Launches via Riot Client
✅ **Epic Games Launcher** - Opens Epic launcher
✅ **Other games** - Finds main executable automatically

## How Protocol Handlers Work

Windows registers protocol handlers in the registry:
```
HKEY_CLASSES_ROOT\steam\shell\open\command
HKEY_CLASSES_ROOT\valorant\shell\open\command
```

When you launch `steam://open/main`, Windows:
1. Looks up the protocol handler
2. Finds the registered application
3. Passes the URL to that application
4. Application handles the launch

This is the same system used by:
- NVIDIA GeForce Experience
- Discord (discord://)
- Spotify (spotify://)
- Web browsers (http://, https://)

## Build & Run

```powershell
dotnet restore
dotnet build
dotnet run
```

## Testing

1. **Steam**: Click Play → Should open Steam client
2. **Valorant**: Click Play → Should launch Riot Client with Valorant
3. **VLC**: Click Play → Should open VLC player (not folder)
4. **Other apps**: Should launch main executable, not uninstaller

## Benefits

✅ **No more uninstallers** - Smart filtering prevents launching uninstall.exe
✅ **Game launcher support** - Uses protocol URLs like NVIDIA app
✅ **Reliable launching** - Multiple fallback methods
✅ **Better UX** - Apps launch correctly every time
