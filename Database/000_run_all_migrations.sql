-- Blotic Arena Database Setup
-- Master migration script to run all migrations in order
-- Created: 2025-01-07
-- 
-- INSTRUCTIONS:
-- 1. Open Supabase SQL Editor (https://supabase.com/dashboard/project/rtngwbfpreyyrwrquoy/sql)
-- 2. Copy and paste this entire file
-- 3. Click "Run" to execute all migrations
-- 
-- OR run individual migration files in order:
-- 001_create_users_table.sql
-- 002_create_game_sessions_table.sql
-- 003_create_qr_login_sessions_table.sql
-- 004_create_game_history_table.sql
-- 005_create_helper_functions.sql

-- ============================================================================
-- MIGRATION 001: Create users table
-- ============================================================================

-- Create users table
CREATE TABLE IF NOT EXISTS public.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    phone_number TEXT UNIQUE NOT NULL,
    username TEXT,
    email TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    last_login TIMESTAMP WITH TIME ZONE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_users_phone_number ON public.users(phone_number);
CREATE INDEX IF NOT EXISTS idx_users_email ON public.users(email);
CREATE INDEX IF NOT EXISTS idx_users_created_at ON public.users(created_at);

-- Enable RLS
ALTER TABLE public.users ENABLE ROW LEVEL SECURITY;

-- Create policies
CREATE POLICY "Users can view their own profile"
    ON public.users FOR SELECT
    USING (auth.uid()::text = id::text);

CREATE POLICY "Users can update their own profile"
    ON public.users FOR UPDATE
    USING (auth.uid()::text = id::text);

CREATE POLICY "Allow authenticated users to insert"
    ON public.users FOR INSERT
    WITH CHECK (true);

-- ============================================================================
-- MIGRATION 002: Create game_sessions table
-- ============================================================================

-- Create game_sessions table
CREATE TABLE IF NOT EXISTS public.game_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
    session_token TEXT UNIQUE NOT NULL,
    games_remaining INTEGER NOT NULL DEFAULT 0,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    is_active BOOLEAN DEFAULT true
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_game_sessions_user_id ON public.game_sessions(user_id);
CREATE INDEX IF NOT EXISTS idx_game_sessions_session_token ON public.game_sessions(session_token);
CREATE INDEX IF NOT EXISTS idx_game_sessions_is_active ON public.game_sessions(is_active);
CREATE INDEX IF NOT EXISTS idx_game_sessions_expires_at ON public.game_sessions(expires_at);

-- Enable RLS
ALTER TABLE public.game_sessions ENABLE ROW LEVEL SECURITY;

-- Create policies
CREATE POLICY "Users can view their own game sessions"
    ON public.game_sessions FOR SELECT
    USING (auth.uid()::text = user_id::text);

CREATE POLICY "Users can update their own game sessions"
    ON public.game_sessions FOR UPDATE
    USING (auth.uid()::text = user_id::text);

CREATE POLICY "Allow authenticated users to insert game sessions"
    ON public.game_sessions FOR INSERT
    WITH CHECK (auth.uid()::text = user_id::text);

-- Create trigger function
CREATE OR REPLACE FUNCTION expire_old_game_sessions()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE public.game_sessions
    SET is_active = false
    WHERE expires_at < NOW() AND is_active = true;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create trigger
DROP TRIGGER IF EXISTS trigger_expire_game_sessions ON public.game_sessions;
CREATE TRIGGER trigger_expire_game_sessions
    AFTER INSERT OR UPDATE ON public.game_sessions
    FOR EACH STATEMENT
    EXECUTE FUNCTION expire_old_game_sessions();

-- ============================================================================
-- MIGRATION 003: Create qr_login_sessions table
-- ============================================================================

-- Create qr_login_sessions table
CREATE TABLE IF NOT EXISTS public.qr_login_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    session_id TEXT UNIQUE NOT NULL,
    user_id UUID REFERENCES public.users(id) ON DELETE SET NULL,
    status TEXT NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'scanned', 'authenticated', 'expired')),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    desktop_device_id TEXT
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_qr_login_sessions_session_id ON public.qr_login_sessions(session_id);
CREATE INDEX IF NOT EXISTS idx_qr_login_sessions_user_id ON public.qr_login_sessions(user_id);
CREATE INDEX IF NOT EXISTS idx_qr_login_sessions_status ON public.qr_login_sessions(status);
CREATE INDEX IF NOT EXISTS idx_qr_login_sessions_expires_at ON public.qr_login_sessions(expires_at);

-- Enable RLS
ALTER TABLE public.qr_login_sessions ENABLE ROW LEVEL SECURITY;

-- Create policies
CREATE POLICY "Anyone can view pending QR sessions"
    ON public.qr_login_sessions FOR SELECT
    USING (status = 'pending' OR auth.uid()::text = user_id::text);

CREATE POLICY "Allow anyone to insert QR sessions"
    ON public.qr_login_sessions FOR INSERT
    WITH CHECK (true);

CREATE POLICY "Users can update QR sessions"
    ON public.qr_login_sessions FOR UPDATE
    USING (status = 'pending' OR auth.uid()::text = user_id::text);

-- Create trigger function
CREATE OR REPLACE FUNCTION expire_old_qr_sessions()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE public.qr_login_sessions
    SET status = 'expired'
    WHERE expires_at < NOW() AND status IN ('pending', 'scanned');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create trigger
DROP TRIGGER IF EXISTS trigger_expire_qr_sessions ON public.qr_login_sessions;
CREATE TRIGGER trigger_expire_qr_sessions
    AFTER INSERT OR UPDATE ON public.qr_login_sessions
    FOR EACH STATEMENT
    EXECUTE FUNCTION expire_old_qr_sessions();

-- ============================================================================
-- MIGRATION 004: Create game_history table
-- ============================================================================

-- Create game_history table
CREATE TABLE IF NOT EXISTS public.game_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
    game_name TEXT NOT NULL,
    played_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    source TEXT NOT NULL CHECK (source IN ('website', 'desktop'))
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_game_history_user_id ON public.game_history(user_id);
CREATE INDEX IF NOT EXISTS idx_game_history_game_name ON public.game_history(game_name);
CREATE INDEX IF NOT EXISTS idx_game_history_played_at ON public.game_history(played_at);

-- Enable RLS
ALTER TABLE public.game_history ENABLE ROW LEVEL SECURITY;

-- Create policies
CREATE POLICY "Users can view their own game history"
    ON public.game_history FOR SELECT
    USING (auth.uid()::text = user_id::text);

CREATE POLICY "Users can insert their own game history"
    ON public.game_history FOR INSERT
    WITH CHECK (auth.uid()::text = user_id::text);

-- ============================================================================
-- MIGRATION 005: Create helper functions
-- ============================================================================

-- Function: Get or create user
CREATE OR REPLACE FUNCTION get_or_create_user(p_phone_number TEXT, p_username TEXT DEFAULT NULL)
RETURNS UUID AS $$
DECLARE
    v_user_id UUID;
BEGIN
    SELECT id INTO v_user_id FROM public.users WHERE phone_number = p_phone_number;
    IF v_user_id IS NULL THEN
        INSERT INTO public.users (phone_number, username, created_at)
        VALUES (p_phone_number, p_username, NOW())
        RETURNING id INTO v_user_id;
    END IF;
    RETURN v_user_id;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Function: Update last login
CREATE OR REPLACE FUNCTION update_last_login(p_user_id UUID)
RETURNS void AS $$
BEGIN
    UPDATE public.users SET last_login = NOW() WHERE id = p_user_id;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Function: Get active game session
CREATE OR REPLACE FUNCTION get_active_game_session(p_user_id UUID)
RETURNS TABLE (id UUID, session_token TEXT, games_remaining INTEGER, expires_at TIMESTAMP WITH TIME ZONE) AS $$
BEGIN
    RETURN QUERY
    SELECT gs.id, gs.session_token, gs.games_remaining, gs.expires_at
    FROM public.game_sessions gs
    WHERE gs.user_id = p_user_id AND gs.is_active = true AND gs.expires_at > NOW()
    ORDER BY gs.created_at DESC LIMIT 1;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Function: Decrement game count
CREATE OR REPLACE FUNCTION decrement_game_count(p_user_id UUID, p_game_name TEXT)
RETURNS INTEGER AS $$
DECLARE
    v_session_id UUID;
    v_games_remaining INTEGER;
BEGIN
    SELECT id, games_remaining INTO v_session_id, v_games_remaining
    FROM public.game_sessions
    WHERE user_id = p_user_id AND is_active = true AND expires_at > NOW()
    ORDER BY created_at DESC LIMIT 1;
    
    IF v_session_id IS NULL OR v_games_remaining <= 0 THEN RETURN -1; END IF;
    
    UPDATE public.game_sessions SET games_remaining = games_remaining - 1 WHERE id = v_session_id;
    INSERT INTO public.game_history (user_id, game_name, source) VALUES (p_user_id, p_game_name, 'desktop');
    
    RETURN v_games_remaining - 1;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Function: Create QR session
CREATE OR REPLACE FUNCTION create_qr_session(p_session_id TEXT, p_desktop_device_id TEXT, p_expiration_minutes INTEGER DEFAULT 5)
RETURNS UUID AS $$
DECLARE
    v_qr_session_id UUID;
BEGIN
    INSERT INTO public.qr_login_sessions (session_id, desktop_device_id, status, expires_at)
    VALUES (p_session_id, p_desktop_device_id, 'pending', NOW() + (p_expiration_minutes || ' minutes')::INTERVAL)
    RETURNING id INTO v_qr_session_id;
    RETURN v_qr_session_id;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Function: Authenticate QR session
CREATE OR REPLACE FUNCTION authenticate_qr_session(p_session_id TEXT, p_user_id UUID)
RETURNS BOOLEAN AS $$
DECLARE
    v_status TEXT;
BEGIN
    SELECT status INTO v_status FROM public.qr_login_sessions
    WHERE session_id = p_session_id AND expires_at > NOW();
    
    IF v_status IS NULL THEN RETURN false; END IF;
    IF v_status = 'pending' THEN
        UPDATE public.qr_login_sessions SET status = 'authenticated', user_id = p_user_id
        WHERE session_id = p_session_id;
        PERFORM update_last_login(p_user_id);
        RETURN true;
    END IF;
    RETURN false;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- ============================================================================
-- MIGRATION COMPLETE
-- ============================================================================

SELECT 'All migrations completed successfully!' as status;
