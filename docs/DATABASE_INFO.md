# Blotic Arena - Database Information

## Database Details

**Database Name (Project Ref):** `rtngwbfpreyyrwrquoy`

**Full URL:** `https://rtngwbfpreyyrwrquoy.supabase.co`

**Dashboard:** https://supabase.com/dashboard/project/rtngwbfpreyyrwrquoy

## Tables Created

### 1. `users` 
- User accounts for Blotic Arena
- Columns: id, phone_number, username, email, created_at, last_login
- RLS: Enabled ✅

### 2. `game_sessions`
- Tracks game allowances per user
- Columns: id, user_id, session_token, games_remaining, expires_at, created_at, is_active
- RLS: Enabled ✅

### 3. `qr_login_sessions`
- QR code authentication sessions
- Columns: id, session_id, user_id, status, created_at, expires_at, desktop_device_id
- RLS: Enabled ✅
- Status values: 'pending', 'scanned', 'authenticated', 'expired'

### 4. `game_history`
- Cross-platform play history
- Columns: id, user_id, game_name, played_at, source
- RLS: Enabled ✅
- Source values: 'website', 'desktop'

## Access Keys

**Anon Key (Client-side):** Stored in `Config/SupabaseConfig.cs`
- Safe for client applications
- Respects RLS policies

**Service Role Key (Server-side):** Stored in `.mcp/mcp_config.json`
- Admin access
- Bypasses RLS
- Never expose in client code

## Connection Status

To check if the app is connected to the database:
1. Run the app
2. Check debug output for "✅ Supabase initialized successfully"
3. Try generating a QR code - it should create a session in the database

## Troubleshooting

### Error: "Failed to create login session"

**Possible causes:**
1. Internet connection issue
2. Supabase not initialized
3. RLS policy blocking insert

**Solutions:**
1. Check internet connection
2. Restart the application
3. Check debug output for detailed error messages
4. Verify Supabase dashboard is accessible

### Viewing Data

**Via Supabase Dashboard:**
1. Go to: https://supabase.com/dashboard/project/rtngwbfpreyyrwrquoy/editor
2. Select table from left sidebar
3. View/edit data

**Via SQL Editor:**
1. Go to: https://supabase.com/dashboard/project/rtngwbfpreyyrwrquoy/sql
2. Run queries like:
```sql
SELECT * FROM qr_login_sessions ORDER BY created_at DESC LIMIT 10;
SELECT * FROM users;
SELECT * FROM game_sessions WHERE is_active = true;
```

## Recent Fixes

### Fixed: Infinite Loop in Triggers (2025-01-07)
- **Problem:** Triggers were causing stack overflow
- **Solution:** Removed problematic triggers, handle expiration in application code

### Fixed: RPC Function Issues
- **Problem:** `create_qr_session()` function was broken
- **Solution:** Changed to direct INSERT statements

## Current Status

✅ Database: Connected and working
✅ Tables: All 4 tables created
✅ RLS: Policies active
✅ Insert: Working (direct INSERT)
✅ Query: Working
✅ Authentication: Ready for QR login

## Next Steps

1. Test QR code generation in the app
2. Implement mobile/website QR scanner
3. Complete authentication flow
4. Add game count sync
5. Implement game launch tracking
