# Blotic Arena - Supabase Integration TODO

## Project Overview
Integrate Supabase as a cross-server authentication and game sync system for Blotic Arena. This will enable:
- QR code login from website events
- Account synchronization between website and desktop app
- Game chances tracking across platforms
- Backup server functionality for the website

## Supabase Configuration
**Project URL:** `[CONFIGURED IN CONFIG FILE]`

**Keys:**
- Keys are stored securely in `Config/SupabaseConfig.cs`
- Never commit API keys to version control
- Use environment variables for production deployment

---

## Phase 1: Setup & Configuration ✅

### 1.1 Install Supabase Client
- [x] Add `supabase-csharp` NuGet package to BloticArena.csproj
- [x] Add configuration file for Supabase credentials
- [x] Create `SupabaseConfig.cs` class to store API keys and URL
- [ ] Add environment variable support for secure key storage (optional)

### 1.2 Database Schema Design ✅
- [x] Create `users` table
  - `id` (uuid, primary key)
  - `phone_number` (text, unique)
  - `username` (text)
  - `email` (text, optional)
  - `created_at` (timestamp)
  - `last_login` (timestamp)
  
- [x] Create `game_sessions` table
  - `id` (uuid, primary key)
  - `user_id` (uuid, foreign key to users)
  - `session_token` (text, unique)
  - `games_remaining` (integer)
  - `expires_at` (timestamp)
  - `created_at` (timestamp)
  - `is_active` (boolean)
  
- [x] Create `qr_login_sessions` table
  - `id` (uuid, primary key)
  - `session_id` (text, unique) - matches QR code session
  - `user_id` (uuid, foreign key to users, nullable)
  - `status` (text) - 'pending', 'scanned', 'authenticated', 'expired'
  - `created_at` (timestamp)
  - `expires_at` (timestamp)
  - `desktop_device_id` (text)

- [x] Create `game_history` table
  - `id` (uuid, primary key)
  - `user_id` (uuid, foreign key to users)
  - `game_name` (text)
  - `played_at` (timestamp)
  - `source` (text) - 'website' or 'desktop'

**SQL Migration Files Created:**
- `Database/000_run_all_migrations.sql` - Master script
- `Database/001_create_users_table.sql`
- `Database/002_create_game_sessions_table.sql`
- `Database/003_create_qr_login_sessions_table.sql`
- `Database/004_create_game_history_table.sql`
- `Database/005_create_helper_functions.sql`
- `Database/README.md` - Setup instructions

**✅ DATABASE DEPLOYED:**
- All tables created successfully via MCP
- Helper functions deployed
- RLS policies active
- Triggers configured

---

## Phase 2: Backend Service Layer 🔧

### 2.1 Create Supabase Service ✅
- [x] Create `Services/SupabaseService.cs`
- [x] Implement connection initialization
- [x] Add error handling and retry logic
- [x] Implement connection status checking

### 2.2 Authentication Service
- [ ] Create `Services/AuthService.cs`
- [ ] Implement QR session creation
  - Generate unique session ID
  - Store in `qr_login_sessions` table
  - Set expiration (5 minutes)
- [ ] Implement QR session polling
  - Check if user scanned QR code
  - Retrieve user data when authenticated
- [ ] Implement logout functionality
- [ ] Add session token management

### 2.3 User Sync Service
- [ ] Create `Services/UserSyncService.cs`
- [ ] Implement user profile sync
  - Fetch user data from Supabase
  - Update local cache
- [ ] Implement games remaining sync
  - Fetch game chances from server
  - Update UI in real-time
- [ ] Add background sync (every 30 seconds when logged in)

---

## Phase 3: UI Updates 🎨

### 3.1 Profile Page Enhancement
- [ ] Update `ShowProfilePage()` to check login status
- [ ] Show QR code when not logged in
- [ ] Show user profile when logged in
  - Display username/phone
  - Show games remaining count
  - Add logout button
  - Show last sync time

### 3.2 Login Flow UI
- [ ] Add loading spinner while generating QR
- [ ] Add polling indicator ("Waiting for scan...")
- [ ] Add success animation when login completes
- [ ] Add error handling UI (expired, failed, etc.)
- [ ] Add "Refresh QR" button if expired

### 3.3 Main UI Updates
- [ ] Update top-right login button
  - Show "Login" when not authenticated
  - Show username when authenticated
  - Add dropdown menu (Profile, Logout)
- [ ] Add games remaining badge
  - Show count in header or sidebar
  - Animate when count changes
  - Warning when count is low (< 3)

---

## Phase 4: Core Features Implementation 💻

### 4.1 QR Code Login System
- [ ] Update `GenerateQRCode()` method
  - Create session in Supabase
  - Generate QR with session ID
  - Store session ID locally
- [ ] Implement polling mechanism
  - Poll every 2 seconds
  - Check session status in Supabase
  - Stop polling after 5 minutes or success
- [ ] Handle authentication success
  - Retrieve user data
  - Store auth token locally
  - Update UI to logged-in state
  - Sync game data

### 4.2 Game Launch Integration
- [ ] Update `LaunchApplication()` method
- [ ] Check if user is logged in before launch
- [ ] Check games remaining count
- [ ] Decrement game count on launch
- [ ] Sync with Supabase after launch
- [ ] Show error if no games remaining
- [ ] Add "Get More Games" prompt

### 4.3 Session Management
- [ ] Implement persistent login
  - Store auth token in AppData
  - Auto-login on app start
  - Validate token with Supabase
- [ ] Implement token refresh
- [ ] Handle session expiration
- [ ] Add "Remember Me" option

---

## Phase 5: Data Models ✅

### 5.1 Create Data Models
- [x] Create `Models/User.cs`
  ```csharp
  - Id (Guid)
  - PhoneNumber (string)
  - Username (string)
  - Email (string)
  - GamesRemaining (int)
  - LastSync (DateTime)
  ```

- [x] Create `Models/QRLoginSession.cs`
  ```csharp
  - SessionId (string)
  - Status (enum: Pending, Scanned, Authenticated, Expired)
  - CreatedAt (DateTime)
  - ExpiresAt (DateTime)
  ```

- [x] Create `Models/GameSession.cs`
  ```csharp
  - UserId (Guid)
  - SessionToken (string)
  - GamesRemaining (int)
  - ExpiresAt (DateTime)
  ```

---

## Phase 6: Error Handling & Edge Cases 🛡️

### 6.1 Network Error Handling
- [ ] Handle no internet connection
- [ ] Handle Supabase server down
- [ ] Implement offline mode
  - Cache last known game count
  - Queue game launches for sync
  - Show offline indicator

### 6.2 Authentication Edge Cases
- [ ] Handle QR code expiration
- [ ] Handle multiple login attempts
- [ ] Handle concurrent sessions
- [ ] Handle token expiration during gameplay

### 6.3 Game Count Edge Cases
- [ ] Handle negative game counts
- [ ] Handle sync conflicts
- [ ] Handle race conditions (multiple devices)
- [ ] Add optimistic UI updates

---

## Phase 7: Testing & Validation ✅

### 7.1 Unit Tests
- [ ] Test Supabase connection
- [ ] Test QR session creation
- [ ] Test authentication flow
- [ ] Test game count sync
- [ ] Test error scenarios

### 7.2 Integration Tests
- [ ] Test full login flow
- [ ] Test game launch with auth
- [ ] Test multi-device sync
- [ ] Test session expiration

### 7.3 Manual Testing
- [ ] Test QR login from website
- [ ] Test game count sync across devices
- [ ] Test offline behavior
- [ ] Test error recovery

---

## Phase 8: Security & Best Practices 🔒

### 8.1 Security Implementation
- [ ] Never expose service_role key in client
- [ ] Use anon key for client operations
- [ ] Implement Row Level Security (RLS) in Supabase
- [ ] Encrypt stored tokens
- [ ] Add rate limiting for QR generation
- [ ] Validate all user inputs

### 8.2 Performance Optimization
- [ ] Cache user data locally
- [ ] Implement debounced sync
- [ ] Optimize polling frequency
- [ ] Add connection pooling
- [ ] Minimize API calls

---

## Phase 9: Documentation 📝

### 9.1 Code Documentation
- [ ] Add XML comments to all public methods
- [ ] Document Supabase schema
- [ ] Create API integration guide
- [ ] Add inline code comments

### 9.2 User Documentation
- [ ] Create login guide
- [ ] Create troubleshooting guide
- [ ] Add FAQ section
- [ ] Create video tutorial

---

## Phase 10: Deployment & Monitoring 🚀

### 10.1 Deployment Preparation
- [ ] Update build configuration
- [ ] Test single-file EXE with Supabase
- [ ] Create installer with dependencies
- [ ] Test on clean Windows machine

### 10.2 Monitoring Setup
- [ ] Add logging for Supabase operations
- [ ] Track authentication success rate
- [ ] Monitor API usage
- [ ] Set up error reporting

---

## Future Enhancements 🌟

### Website Integration (Later Phase)
- [ ] Update website QR scan to create Supabase session
- [ ] Modify game chance reduction to sync with Supabase
- [ ] Add cross-platform game history
- [ ] Implement leaderboards

### Advanced Features
- [ ] Add friends system
- [ ] Add achievements
- [ ] Add game statistics
- [ ] Add social features
- [ ] Add cloud save for game settings

---

## Priority Order

**HIGH PRIORITY (Do First):**
1. Phase 1: Setup & Configuration
2. Phase 2.1: Create Supabase Service
3. Phase 2.2: Authentication Service
4. Phase 4.1: QR Code Login System
5. Phase 3.1: Profile Page Enhancement

**MEDIUM PRIORITY (Do Next):**
6. Phase 2.3: User Sync Service
7. Phase 4.2: Game Launch Integration
8. Phase 3.3: Main UI Updates
9. Phase 6: Error Handling

**LOW PRIORITY (Do Later):**
10. Phase 7: Testing
11. Phase 9: Documentation
12. Phase 10: Deployment

---

## Notes
- Keep service_role key secure - never commit to Git
- Test thoroughly before deploying to users
- Website changes will come later - focus on Arena app first
- This server will also serve as backup for main website
