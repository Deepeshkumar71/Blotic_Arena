# Blotic Arena v1.4 - Category Filtering

## New Feature: Category-Based Filtering

### Category Buttons
Added a horizontal scrollable category filter bar above the search box with the following categories:

- **All** - Shows all applications (default, highlighted in blue)
- **🌐 Browsers** - Chrome, Firefox, Edge, Brave, Opera
- **💻 Development** - VS Code, Visual Studio, Git, Docker, IDEs
- **🎮 Games** - Steam, Epic, Valorant, League of Legends
- **💬 Communication** - Discord, Slack, Teams, Zoom, Outlook
- **🎬 Media** - Spotify, VLC, Media Players, OBS
- **📊 Productivity** - Office apps, Word, Excel, PowerPoint, Notepad
- **🎨 Design** - Photoshop, Illustrator, Premiere, Gimp, Blender, Figma
- **🔧 Utilities** - WinRAR, 7-Zip, Calculator, Terminal
- **📦 Other** - Uncategorized applications

### Smart Categorization
Apps are automatically categorized based on their names:
- VLC → Media
- Chrome → Browsers
- VS Code → Development
- Discord → Communication
- Valorant → Games
- Photoshop → Design
- Excel → Productivity

### Features
1. **Click any category** to filter apps instantly
2. **Selected category** is highlighted in blue
3. **Search works with categories** - search within selected category
4. **Horizontal scroll** for category buttons on smaller screens
5. **Smooth filtering** - instant results

### UI Layout
```
┌─────────────────────────────────────────┐
│  Title Bar                              │
├─────────────────────────────────────────┤
│  [All] [Browsers] [Dev] [Games] ...    │ ← Category Filter
├─────────────────────────────────────────┤
│  🔍 Search...              [Refresh]    │ ← Search Bar
├─────────────────────────────────────────┤
│  ┌───┐ ┌───┐ ┌───┐                     │
│  │App│ │App│ │App│  ...                │ ← Filtered Apps
│  └───┘ └───┘ └───┘                     │
└─────────────────────────────────────────┘
```

### How It Works
1. **Category Selection**: Click any category button
2. **Visual Feedback**: Selected button turns blue
3. **Instant Filter**: Apps are filtered by category
4. **Combined Search**: Search text filters within selected category
5. **Status Update**: Shows count of filtered apps

### Example Usage
- Click "🎮 Games" → See only Valorant, Steam, Epic, etc.
- Click "💻 Development" → See only VS Code, Git, Docker, etc.
- Click "🌐 Browsers" + Search "chrome" → Find Chrome specifically
- Click "All" → Return to viewing all apps

## Technical Implementation

### Model Changes
- Added `Category` property to `AppInfo` model

### Scanner Changes
- Added `GetCategoryForApp()` method with smart categorization logic
- Categories assigned during app scanning

### UI Changes
- Category buttons in horizontal scrollable container
- Active category highlighted with primary color
- Search and category filters work together

### Code Changes
- `MainWindow.xaml.cs`: Added `CategoryButton_Click()` and `FilterApps()` methods
- `AppScanner.cs`: Added category detection logic
- `MainWindow.xaml`: Added category button bar

## Build & Run

```powershell
dotnet restore
dotnet build
dotnet run
```

## Benefits

✅ **Organized browsing** - Find apps by type
✅ **Quick access** - Filter to specific categories
✅ **Better UX** - Visual category organization
✅ **Smart categorization** - Automatic app classification
✅ **Combined filtering** - Category + search together
✅ **Clean interface** - Horizontal scrollable buttons
