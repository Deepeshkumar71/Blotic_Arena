# Blotic Arena - Project Structure

## 📁 Folder Organization

### **Root Directory**
```
Blotic_Arena/
├── Assets/                     # Static assets and resources
│   ├── blotic_logo.png        # Application logo (PNG format)
│   ├── blotic_logo.ico        # Application logo (ICO format)
│   └── Blotic.ico             # Main application icon
├── Build/                      # Build outputs and executables
│   └── BloticArena.exe        # Production executable
├── Config/                     # Configuration files
│   └── SupabaseConfig.cs      # Backend configuration
├── Controls/                   # Custom WPF controls
├── Database/                   # Database-related files
├── Documentation/              # Project documentation
│   ├── CHANGELOG.md           # Version history
│   ├── PRODUCTION_RELEASE.md  # Production release notes
│   ├── PRODUCTION_RELEASE_v1.1.0.md # v1.1.0 release notes
│   ├── README.md              # Main project documentation
│   └── PROJECT_STRUCTURE.md   # This file
├── Media/                      # Media files and assets
│   ├── blotic-video-compressed.mp4 # Main video background
│   └── blotic-video-compressed.aac # Alternative video format
├── Models/                     # Data models and entities
├── Services/                   # Business logic and services
│   └── SupabaseService.cs     # Backend integration service
├── docs/                       # Additional documentation
├── bin/                        # Build output directory
├── obj/                        # Build intermediate files
├── App.xaml                    # Application definition
├── App.xaml.cs                # Application code-behind
├── MainWindow.xaml             # Main window UI definition
├── MainWindow.xaml.cs          # Main window code-behind
├── BloticArena.csproj          # Project file
└── BloticArena.sln             # Solution file
```

## 🎯 Key Components

### **Core Application Files**
- **`MainWindow.xaml`** - Main UI layout with video background and screensaver
- **`MainWindow.xaml.cs`** - Application logic, particle animations, screensaver
- **`App.xaml`** - Application resources and styling
- **`BloticArena.csproj`** - Project configuration and dependencies

### **Media Assets**
- **`Media/blotic-video-compressed.mp4`** - Primary video background (7.4MB)
- **`Assets/Blotic.ico`** - Application icon for executable and system integration
- **`Assets/blotic_logo.png`** - Logo image for UI elements

### **Configuration**
- **`Config/SupabaseConfig.cs`** - Backend connection settings
- **`Services/SupabaseService.cs`** - Backend service implementation

### **Documentation**
- **`Documentation/PRODUCTION_RELEASE_v1.1.0.md`** - Latest release documentation
- **`Documentation/CHANGELOG.md`** - Version history and changes
- **`Documentation/README.md`** - Main project documentation

## 🔧 File Path References

### **Updated Paths in Code**
All file references have been updated to reflect the new organization:

**Project File (`BloticArena.csproj`):**
```xml
<ApplicationIcon>Assets\Blotic.ico</ApplicationIcon>
<Content Include="Media\blotic-video-compressed.mp4">
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</Content>
```

**XAML Files (`MainWindow.xaml`):**
```xml
<!-- Home Video -->
<MediaElement Source="Media/blotic-video-compressed.mp4" />

<!-- Screensaver Video -->
<MediaElement Source="Media/blotic-video-compressed.mp4" />
```

**C# Code (`MainWindow.xaml.cs`):**
```csharp
// Video file paths
var possiblePaths = new[]
{
    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media", "blotic-video-compressed.mp4"),
    System.IO.Path.Combine(Environment.CurrentDirectory, "Media", "blotic-video-compressed.mp4"),
    @"d:\GitHub\Blotic Arena\Blotic_Arena\Media\blotic-video-compressed.mp4"
};
```

## 🚀 Build and Deployment

### **Development Build**
```bash
dotnet build
dotnet run
```

### **Production Build**
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

**Output Location:** `bin\Release\net8.0-windows\win-x64\publish\`

### **Deployment Package**
The production build creates a self-contained executable with all dependencies:
- **`BloticArena.exe`** - Main executable (182MB)
- **`Media/blotic-video-compressed.mp4`** - Required video file
- **Supporting DLLs** - WPF and DirectX components

## 📋 Maintenance Notes

### **Adding New Media Files**
1. Place files in the `Media/` folder
2. Update `BloticArena.csproj` to include new files:
   ```xml
   <Content Include="Media\new-file.ext">
     <CopyToOutputDirectory>Always</CopyToOutputDirectory>
   </Content>
   ```
3. Update code references to use `Media/` path

### **Adding New Documentation**
1. Place markdown files in `Documentation/` folder
2. Update this structure document if needed
3. Reference from main README.md

### **Version Updates**
1. Update version in `BloticArena.csproj`
2. Create new release documentation in `Documentation/`
3. Update `CHANGELOG.md` with changes
4. Create production build

## 🔗 Related Files

- **Backend Configuration:** `Config/SupabaseConfig.cs`
- **Main Documentation:** `Documentation/README.md`
- **Release Notes:** `Documentation/PRODUCTION_RELEASE_v1.1.0.md`
- **Version History:** `Documentation/CHANGELOG.md`

---

**Last Updated:** November 12, 2025  
**Version:** 1.1.0  
**Organization Status:** ✅ Complete
