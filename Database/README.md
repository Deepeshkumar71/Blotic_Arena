# Blotic Arena Database Migrations

This folder contains SQL migration scripts for setting up the Supabase database for Blotic Arena.

## Quick Start

### Option 1: Run All Migrations at Once (Recommended)
1. Open Supabase SQL Editor: https://supabase.com/dashboard/project/rtngwbfpreyyrwrquoy/sql
2. Open `000_run_all_migrations.sql`
3. Copy the entire file content
4. Paste into Supabase SQL Editor
5. Click **"Run"**

### Option 2: Run Individual Migrations
Run each file in order:
1. `001_create_users_table.sql`
2. `002_create_game_sessions_table.sql`
3. `003_create_qr_login_sessions_table.sql`
4. `004_create_game_history_table.sql`
5. `005_create_helper_functions.sql`

## Database Schema

### Tables

#### 1. `users`
Stores user account information.

| Column | Type | Description |
|--------|------|-------------|
| id | UUID | Primary key |
| phone_number | TEXT | Unique phone number |
| username | TEXT | Display name |
| email | TEXT | Email address (optional) |
| created_at | TIMESTAMP | Account creation time |
| last_login | TIMESTAMP | Last login time |

#### 2. `game_sessions`
Tracks user game allowances and active sessions.

| Column | Type | Description |
|--------|------|-------------|
| id | UUID | Primary key |
| user_id | UUID | Foreign key to users |
| session_token | TEXT | Unique session token |
| games_remaining | INTEGER | Number of games left |
| expires_at | TIMESTAMP | Session expiration |
| created_at | TIMESTAMP | Session creation time |
| is_active | BOOLEAN | Whether session is active |

#### 3. `qr_login_sessions`
Tracks QR code authentication sessions for desktop login.

| Column | Type | Description |
|--------|------|-------------|
| id | UUID | Primary key |
| session_id | TEXT | QR code session ID |
| user_id | UUID | User who scanned (nullable) |
| status | TEXT | pending/scanned/authenticated/expired |
| created_at | TIMESTAMP | Session creation time |
| expires_at | TIMESTAMP | Session expiration (5 min) |
| desktop_device_id | TEXT | Desktop identifier |

#### 4. `game_history`
Tracks game play history across platforms.

| Column | Type | Description |
|--------|------|-------------|
| id | UUID | Primary key |
| user_id | UUID | Foreign key to users |
| game_name | TEXT | Name of game played |
| played_at | TIMESTAMP | When game was played |
| source | TEXT | 'website' or 'desktop' |

## Helper Functions

### User Management
- `get_or_create_user(phone_number, username)` - Get existing or create new user
- `update_last_login(user_id)` - Update user's last login timestamp

### Game Sessions
- `get_active_game_session(user_id)` - Get user's active game session
- `decrement_game_count(user_id, game_name)` - Decrement games and record history

### QR Authentication
- `create_qr_session(session_id, device_id, expiration_minutes)` - Create QR login session
- `authenticate_qr_session(session_id, user_id)` - Authenticate QR session with user

## Security Features

### Row Level Security (RLS)
All tables have RLS enabled with policies:
- Users can only view/update their own data
- QR sessions are publicly readable when pending
- Game history is private to each user

### Automatic Cleanup
- Expired game sessions are automatically marked inactive
- Expired QR sessions are automatically marked expired
- Old expired QR sessions are cleaned up after 24 hours

## Testing the Setup

After running migrations, test with these queries:

```sql
-- Create a test user
SELECT get_or_create_user('+1234567890', 'Test User');

-- Create a QR session
SELECT create_qr_session('test-session-123', 'desktop-001', 5);

-- Check if tables exist
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('users', 'game_sessions', 'qr_login_sessions', 'game_history');

-- View all functions
SELECT routine_name FROM information_schema.routines 
WHERE routine_schema = 'public' 
AND routine_type = 'FUNCTION';
```

## Rollback

To rollback all migrations:

```sql
-- Drop tables (in reverse order due to foreign keys)
DROP TABLE IF EXISTS public.game_history CASCADE;
DROP TABLE IF EXISTS public.qr_login_sessions CASCADE;
DROP TABLE IF EXISTS public.game_sessions CASCADE;
DROP TABLE IF EXISTS public.users CASCADE;

-- Drop functions
DROP FUNCTION IF EXISTS get_or_create_user CASCADE;
DROP FUNCTION IF EXISTS update_last_login CASCADE;
DROP FUNCTION IF EXISTS get_active_game_session CASCADE;
DROP FUNCTION IF EXISTS decrement_game_count CASCADE;
DROP FUNCTION IF EXISTS create_qr_session CASCADE;
DROP FUNCTION IF EXISTS authenticate_qr_session CASCADE;
DROP FUNCTION IF EXISTS expire_old_game_sessions CASCADE;
DROP FUNCTION IF EXISTS expire_old_qr_sessions CASCADE;
```

## Next Steps

After running migrations:
1. ✅ Database schema is ready
2. ⏳ Implement AuthService in C#
3. ⏳ Connect desktop app to Supabase
4. ⏳ Test QR login flow
5. ⏳ Implement game count sync

## Support

For issues or questions:
- Check Supabase logs: https://supabase.com/dashboard/project/rtngwbfpreyyrwrquoy/logs
- Review RLS policies if access denied
- Verify API keys in `SupabaseConfig.cs`
