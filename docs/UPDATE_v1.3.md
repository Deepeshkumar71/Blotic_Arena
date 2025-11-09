# Blotic Arena v1.3 - Website Theme & Filtered Apps

## Major Changes

### 1. **Blotic Website Theme Integration**
   - Background: Dark blue-gray (#0F1419) matching website
   - Primary: Blue (#4F9CF9) 
   - Accent: Purple (#CC75DB) - Blotic signature color
   - Surface: Dark slate (#1A1F2E)
   - Border accent color on window and hover states
   - Rounded corners (12px) matching website design

### 2. **Filtered App Display - Main Apps Only**
   - **Removed**: System tools, SDKs, runtimes, prerequisites
   - **Removed**: Directory scanning (no more random executables)
   - **Kept**: Main installed applications from Registry
   - **Kept**: User applications from Start Menu

### 3. **Enhanced Filtering Rules**
   Automatically excludes:
   - Updates and redistributables
   - Visual C++ runtimes
   - .NET SDKs and runtimes
   - Development tools and headers
   - Troubleshooting tools
   - Licensing components
   - Telemetry tools
   - Universal CRT components
   - Help files, readme, documentation shortcuts
   - Uninstall shortcuts
   - Support and tutorial links

### 4. **Cleaner App List**
   - Only shows apps with valid icons or install locations
   - Filters out system components
   - Shows ~20-50 main apps instead of 374
   - Better organized and user-friendly

## Color Scheme

```
Background: #0F1419 (Dark blue-gray)
Surface:    #1A1F2E (Slate)
Primary:    #4F9CF9 (Blue)
Accent:     #CC75DB (Purple - Blotic brand)
Hover:      #252B3A (Lighter slate)
Text:       #FFFFFF (White)
Secondary:  #94A3B8 (Gray)
Play:       #10B981 (Green)
```

## What's Included Now

✅ Main installed applications (Chrome, VS Code, Discord, etc.)
✅ User-installed programs from Start Menu
✅ Games (Valorant, Steam, Epic, etc.)
✅ Productivity apps (Office, Adobe, etc.)
✅ Development tools (main apps only, not SDKs)

## What's Excluded Now

❌ System tools and utilities
❌ Runtime libraries (Visual C++, .NET, etc.)
❌ SDKs and development headers
❌ Troubleshooting tools
❌ Help files and documentation
❌ Uninstall shortcuts
❌ Random executables from Program Files

## Build & Run

```powershell
cd Blotic_Arena
dotnet restore
dotnet build
dotnet run
```

## Result

- Clean, focused app list (20-50 apps instead of 374)
- Beautiful Blotic website theme
- Purple accent borders matching brand
- Only shows apps users actually want to launch
- Professional, polished appearance
