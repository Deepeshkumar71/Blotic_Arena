# Recent Changes

## Version 1.1 - UI Improvements

### Changes Made:

1. **Pure Black Theme**
   - Changed background from dark blue (#0F172A) to pure black (#000000)
   - Updated surface color to near-black (#0A0A0A)
   - Improved contrast with white text (#FFFFFF)

2. **Fixed Window Control Buttons**
   - Made minimize, maximize, and close buttons more visible
   - Increased font size and weight for better visibility
   - Used proper TextBlock elements for better rendering

3. **Added Play Button**
   - Each app card now has a green "▶ Play" button
   - Button is clearly visible and clickable
   - Directly launches the application when clicked
   - Hover effect changes button to darker green

4. **Improved Shortcut Resolution**
   - Shortcuts (.lnk files) are now resolved to their actual executable paths
   - This fixes the issue where clicking Valorant opened Documents folder
   - Uses Windows Script Host COM object to resolve shortcuts properly

5. **Added Game Icons**
   - Added Valorant icon (🎯)
   - Added League of Legends icon (⚔️)
   - Added Riot Games icon (🎮)

### How to Build and Run:

```powershell
cd Blotic_Arena
dotnet build
dotnet run
```

### Features:
- Click the green Play button to launch any application
- Search for apps in real-time
- Refresh to rescan for new applications
- Pure black theme for better aesthetics
- Properly resolves shortcuts to actual executables
