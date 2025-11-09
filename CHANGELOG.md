# Changelog

All notable changes to Blotic Arena will be documented in this file.

## [1.0.0] - 2025-11-09

### 🎉 Initial Production Release

#### ✨ Features
- **QR Code Authentication**: Instant login by scanning QR code with mobile device
- **Modern UI**: Pitch black theme with white particles and blue accents
- **Game Library**: Browse and launch games from registered events
- **Profile Management**: View profile with games remaining counter
- **Smooth Animations**: Welcome screen transitions and particle effects
- **Single-File Executable**: 189MB standalone .exe with no dependencies

#### 🎨 UI/UX
- Pitch black background (#000000) with white particles
- Thin sidebar (100px) with logo and navigation
- Large, scalable profile button with hover effects
- Blotic logo (70% opacity) centered in background
- Logo in center of QR code for branding
- Clean title bar with "Blotic Arena" text (28px)
- First name only display in profile button

#### 🔧 Technical
- Built with .NET 8.0 and WPF
- Self-contained deployment (win-x64)
- Custom application icon embedded
- No console window (WinExe output type)
- Supabase integration for authentication and data
- Real-time QR code polling
- Game count tracking across multiple event registrations

#### 🐛 Bug Fixes
- Fixed games count not loading on profile page
- Fixed "No Games Remaining" error showing incorrectly
- Fixed UI state reset issues after logout/re-login
- Fixed welcome screen timing and transitions
- Removed unnecessary account details (User ID, Role, Last Login, Status)

#### 📦 Build Configuration
- **OutputType**: WinExe (no console window)
- **Icon**: Embedded as Resource
- **Single File**: All dependencies bundled
- **Self-Contained**: No .NET runtime required
- **Platform**: Windows 10/11 (64-bit)

### 🎯 Key Improvements from Development
1. Removed console window on startup
2. Custom Blotic logo icon in executable
3. Optimized game count queries (sum all registrations)
4. Streamlined profile page (removed unnecessary fields)
5. Enhanced visual design (pitch black + white particles)
6. Improved button sizes and spacing
7. Added logo to QR code for branding

---

## Future Releases

### Planned Features
- [ ] Game history tracking
- [ ] Offline mode support
- [ ] Multiple language support
- [ ] Custom themes
- [ ] Game favorites system
- [ ] Notifications for new games
- [ ] Auto-update functionality

---

**Version Format**: [Major].[Minor].[Patch]
- **Major**: Breaking changes
- **Minor**: New features (backwards compatible)
- **Patch**: Bug fixes and minor improvements
