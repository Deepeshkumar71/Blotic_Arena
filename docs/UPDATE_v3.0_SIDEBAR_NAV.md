# Blotic Arena v3.0 - Sidebar Navigation & Favorites

## Major Updates

### 1. **Fixed Icon & Card Sizes**
- **Cards**: 180x240px (was 240x320px) - More compact
- **Icons**: 80x80px (was 120x120px) - Proper sizing, no tearing
- **Text**: 14px (was 18px) - Better proportions
- **Play Button**: Smaller, cleaner (12px text)
- **Window**: 1300x750 (was 1400x800)

### 2. **Sidebar Navigation**
- **220px sidebar** on the left
- Blotic logo at top (48x48px)
- "BLOTIC ARENA" branding
- Navigation buttons:
  - 🏠 **Home** - Favorite games
  - 🎮 **My Games** - All games library

### 3. **Two-Page System**

#### Home Page:
- Shows **favorite games only**
- Games you've selected to display
- Quick access to most-played games
- Empty by default (add favorites from My Games)

#### My Games Page:
- Shows **all detected games**
- Full game library
- Search functionality
- Select games to add to Home

### 4. **Favorites System**
- Favorites saved to: `%AppData%\BloticArena\favorites.txt`
- Persists between sessions
- Easy to manage
- Automatic loading on startup

## UI Layout

```
┌──────────┬────────────────────────────────────┐
│          │  [Logo] Blotic Arena    [─][☐][✕] │
│  BLOTIC  ├────────────────────────────────────┤
│  ARENA   │  🔍 Search...         [🔄 Refresh] │
│          ├────────────────────────────────────┤
│  🏠 Home │  HOME / MY GAMES                   │
│  🎮 Games│  5 favorite games                  │
│          ├────────────────────────────────────┤
│          │  ┌──────┐  ┌──────┐  ┌──────┐     │
│          │  │ Icon │  │ Icon │  │ Icon │     │
│          │  │ Name │  │ Name │  │ Name │     │
│          │  │ PLAY │  │ PLAY │  │ PLAY │     │
│          │  └──────┘  └──────┘  └──────┘     │
│          ├────────────────────────────────────┤
│          │  Ready  5 games found              │
└──────────┴────────────────────────────────────┘
```

## How It Works

### On Startup:
1. Loads all games from system
2. Loads favorites from file
3. Shows **Home page** with favorites
4. Highlights Home button

### Navigation:
- Click **Home** → See favorite games
- Click **My Games** → See all games
- Active page highlighted in blue
- Inactive page in dark gray

### Adding Favorites (Future):
Right-click game → "Add to Home"
Or click star icon on card

## Card Design

### Compact Cards:
- **Size**: 180x240px
- **Icon**: 80x80px (no stretching/tearing)
- **Name**: 14px, max 2 lines
- **Type**: 10px subtitle
- **Play Button**: Full-width, 12px text

### Better Proportions:
- Icons fit perfectly (80x80)
- No image distortion
- Clean, professional look
- Consistent spacing

## Technical Details

### Files:
- **Favorites**: `%AppData%\BloticArena\favorites.txt`
- **Format**: One game name per line

### Navigation State:
- `_currentPage`: "Home" or "MyGames"
- `_favoriteGames`: List of favorite games
- `_allApps`: All detected games

### Methods:
- `ShowHomePage()`: Display favorites
- `ShowMyGamesPage()`: Display all games
- `LoadFavorites()`: Load from file
- `SaveFavorites()`: Save to file
- `UpdateNavigation()`: Update button styles

## Features

### Home Page:
✅ Quick access to favorites
✅ Clean, focused view
✅ Only games you want
✅ Fast launching

### My Games Page:
✅ Complete game library
✅ All detected games
✅ Search functionality
✅ Manage collection

### Sidebar:
✅ Always visible
✅ Easy navigation
✅ Blotic branding
✅ Clean design

## Next Steps (Future Enhancements)

### Add to Favorites:
- Right-click menu on cards
- Star icon to toggle favorite
- Drag & drop to Home

### Remove from Favorites:
- Right-click on Home page
- Unstar icon
- Remove button

### Organize Favorites:
- Drag to reorder
- Custom sorting
- Categories

## Build & Run

```powershell
dotnet restore
dotnet build
dotnet run
```

## Result

A professional game launcher with:
- ✅ Sidebar navigation
- ✅ Home page for favorites
- ✅ My Games page for library
- ✅ Proper icon sizing (no tearing)
- ✅ Compact, clean cards
- ✅ Persistent favorites
- ✅ Modern UI design

Perfect for organizing and launching your game collection! 🎮
