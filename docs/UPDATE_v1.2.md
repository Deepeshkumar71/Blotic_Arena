# Blotic Arena v1.2 - Windows 11 Style Update

## Major Changes

### 1. **Real Application Icons**
   - Extracts actual icons from .exe files using Windows Shell API
   - Displays real application icons instead of emojis
   - Falls back to emoji icons if extraction fails
   - Uses high-quality icon rendering

### 2. **Fixed Window Controls**
   - Minimize, Maximize, and Close buttons now clearly visible
   - White text on transparent background
   - Hover effects: gray for min/max, red for close
   - Proper Unicode symbols (─, ☐, ✕)

### 3. **Windows 11 Design Language**
   - Smaller, more compact cards (160x180px)
   - Subtle borders on cards (#2A2A2A)
   - Cleaner typography (13px app name, 10px type)
   - 48x48px icons (Windows 11 standard)
   - Pure black background (#000000)
   - Card hover effects with border highlights

### 4. **Icon Extraction System**
   - New `IconExtractor` class using P/Invoke
   - Extracts icons from:
     - Registry DisplayIcon entries
     - .exe files directly
     - Resolved .lnk shortcuts
   - Converts Win32 icons to WPF ImageSource

## Technical Details

### New Files:
- `Services/IconExtractor.cs` - Icon extraction utility

### Updated Files:
- `BloticArena.csproj` - Added System.Drawing.Common package
- `Models/AppInfo.cs` - Added IconImage property
- `Services/AppScanner.cs` - Icon extraction integration
- `MainWindow.xaml` - Real icon display, fixed window controls
- `App.xaml` - Updated color scheme

### Dependencies Added:
- System.Drawing.Common v8.0.0

## Build & Run

```powershell
cd Blotic_Arena
dotnet restore
dotnet build
dotnet run
```

## Features

✅ Real application icons extracted from executables
✅ Windows 11-style design with subtle borders
✅ Visible window control buttons
✅ Pure black theme
✅ Green Play buttons for launching apps
✅ Shortcut resolution to actual executables
✅ Search and filter functionality
✅ Smooth hover animations

## Known Improvements

- Icons are now extracted from actual executables
- Window controls are clearly visible with proper hover states
- Design matches Windows 11 Settings app style
- Cards are more compact and organized
- Better visual hierarchy with proper spacing
