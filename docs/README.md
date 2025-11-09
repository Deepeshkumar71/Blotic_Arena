# 🎮 Blotic Arena

A modern, beautiful Windows application launcher that displays all installed applications on your PC in a sleek, searchable interface.

## ✨ Features

- **🔍 Smart Search** - Quickly find any application by name or type
- **🎨 Modern UI** - Beautiful dark theme with smooth animations
- **📦 Comprehensive Scanning** - Detects apps from:
  - Windows Registry (installed programs)
  - Program Files directories
  - Start Menu shortcuts
  - Portable applications
- **🚀 Quick Launch** - Click any app to launch it instantly
- **🔄 Real-time Refresh** - Update the app list anytime
- **🎯 Smart Icons** - Recognizes popular apps and assigns appropriate emoji icons

## 🛠️ Requirements

- Windows 10 or later
- .NET 8.0 Runtime or SDK

## 📥 Installation

### Option 1: Build from Source

1. **Install .NET 8.0 SDK**
   - Download from [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)

2. **Clone or download this repository**

3. **Build the application**
   ```powershell
   cd Blotic_Arena
   dotnet build
   ```

4. **Run the application**
   ```powershell
   dotnet run
   ```

### Option 2: Create Executable

To create a standalone executable:

```powershell
cd Blotic_Arena
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The executable will be in: `bin\Release\net8.0-windows\win-x64\publish\BloticArena.exe`

## 🎯 Usage

1. **Launch the application** - Double-click `BloticArena.exe` or run via `dotnet run`

2. **Browse applications** - Scroll through the grid of detected applications

3. **Search** - Type in the search box to filter applications by name

4. **Launch apps** - Click on any application card to launch it

5. **Refresh** - Click the refresh button to rescan for new applications

## 🎨 Features Breakdown

### Application Detection
Blotic Arena scans multiple locations to find all your applications:
- Registry entries for installed programs
- Program Files and Program Files (x86)
- Start Menu shortcuts
- AppData local programs
- Portable applications

### Smart Filtering
The app automatically filters out:
- System updates and components
- Redistributables
- Uninstallers
- Background services

### Icon Recognition
Over 50 popular applications are recognized and assigned custom emoji icons including:
- Browsers (Chrome, Firefox, Edge, Brave, Opera)
- Development tools (VS Code, Visual Studio, Git, Docker)
- Communication (Discord, Slack, Teams, Zoom)
- Media (Spotify, VLC, OBS)
- And many more!

## 🔧 Customization

### Adding Custom Icons

Edit `Services/AppScanner.cs` and add entries to the `AppIcons` dictionary:

```csharp
{ "myapp", "🎯" },
```

### Changing Theme Colors

Edit `App.xaml` and modify the color brushes:

```xaml
<SolidColorBrush x:Key="PrimaryBrush" Color="#6366F1"/>
<SolidColorBrush x:Key="BackgroundBrush" Color="#0F172A"/>
```

## 🐛 Troubleshooting

### No applications showing up
- Try clicking the Refresh button
- Run the app as Administrator for better registry access
- Check Windows Event Viewer for any errors

### Application won't launch
- Verify the application is still installed
- Check if the application requires administrator privileges
- Some apps may have been moved or uninstalled

### Performance issues
- The initial scan may take a few seconds on systems with many applications
- Subsequent searches are instant using cached results

## 📝 Technical Details

- **Framework**: WPF (Windows Presentation Foundation)
- **Language**: C# 12
- **Target**: .NET 8.0
- **UI**: XAML with custom styling
- **Architecture**: MVVM-inspired pattern

## 🤝 Contributing

Feel free to submit issues, fork the repository, and create pull requests for any improvements.

## 📄 License

This project is open source and available for personal and commercial use.

## 🎉 Credits

Created with ❤️ for the Blotic community

---

**Enjoy using Blotic Arena! 🎮**
