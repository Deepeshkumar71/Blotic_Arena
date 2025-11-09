-- Migration: Create helper functions and views
-- Description: Utility functions for Blotic Arena operations
-- Created: 2025-01-07

-- Function to get or create user by phone number
CREATE OR REPLACE FUNCTION get_or_create_user(p_phone_number TEXT, p_username TEXT DEFAULT NULL)
RETURNS UUID AS $$
DECLARE
    v_user_id UUID;
BEGIN
    -- Try to find existing user
    SELECT id INTO v_user_id
    FROM public.users
    WHERE phone_number = p_phone_number;
    
    -- If not found, create new user
    IF v_user_id IS NULL THEN
        INSERT INTO public.users (phone_number, username, created_at)
        VALUES (p_phone_number, p_username, NOW())
        RETURNING id INTO v_user_id;
    END IF;
    
    RETURN v_user_id;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Function to update user's last login
CREATE OR REPLACE FUNCTION update_last_login(p_user_id UUID)
RETURNS void AS $$
BEGIN
    UPDATE public.users
    SET last_login = NOW()
    WHERE id = p_user_id;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Function to get active game session for user
CREATE OR REPLACE FUNCTION get_active_game_session(p_user_id UUID)
RETURNS TABLE (
    id UUID,
    session_token TEXT,
    games_remaining INTEGER,
    expires_at TIMESTAMP WITH TIME ZONE
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        gs.id,
        gs.session_token,
        gs.games_remaining,
        gs.expires_at
    FROM public.game_sessions gs
    WHERE gs.user_id = p_user_id
    AND gs.is_active = true
    AND gs.expires_at > NOW()
    ORDER BY gs.created_at DESC
    LIMIT 1;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Function to decrement game count
CREATE OR REPLACE FUNCTION decrement_game_count(p_user_id UUID, p_game_name TEXT)
RETURNS INTEGER AS $$
DECLARE
    v_session_id UUID;
    v_games_remaining INTEGER;
BEGIN
    -- Get active session
    SELECT id, games_remaining INTO v_session_id, v_games_remaining
    FROM public.game_sessions
    WHERE user_id = p_user_id
    AND is_active = true
    AND expires_at > NOW()
    ORDER BY created_at DESC
    LIMIT 1;
    
    -- If no active session or no games left, return -1
    IF v_session_id IS NULL OR v_games_remaining <= 0 THEN
        RETURN -1;
    END IF;
    
    -- Decrement games remaining
    UPDATE public.game_sessions
    SET games_remaining = games_remaining - 1
    WHERE id = v_session_id;
    
    -- Add to game history
    INSERT INTO public.game_history (user_id, game_name, source)
    VALUES (p_user_id, p_game_name, 'desktop');
    
    -- Return new count
    RETURN v_games_remaining - 1;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Function to create QR login session
CREATE OR REPLACE FUNCTION create_qr_session(
    p_session_id TEXT,
    p_desktop_device_id TEXT,
    p_expiration_minutes INTEGER DEFAULT 5
)
RETURNS UUID AS $$
DECLARE
    v_qr_session_id UUID;
BEGIN
    INSERT INTO public.qr_login_sessions (
        session_id,
        desktop_device_id,
        status,
        expires_at
    )
    VALUES (
        p_session_id,
        p_desktop_device_id,
        'pending',
        NOW() + (p_expiration_minutes || ' minutes')::INTERVAL
    )
    RETURNING id INTO v_qr_session_id;
    
    RETURN v_qr_session_id;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Function to authenticate QR session (called by mobile app)
CREATE OR REPLACE FUNCTION authenticate_qr_session(
    p_session_id TEXT,
    p_user_id UUID
)
RETURNS BOOLEAN AS $$
DECLARE
    v_status TEXT;
BEGIN
    -- Check if session exists and is pending
    SELECT status INTO v_status
    FROM public.qr_login_sessions
    WHERE session_id = p_session_id
    AND expires_at > NOW();
    
    IF v_status IS NULL THEN
        RETURN false;
    END IF;
    
    IF v_status = 'pending' THEN
        -- Update session to authenticated
        UPDATE public.qr_login_sessions
        SET 
            status = 'authenticated',
            user_id = p_user_id
        WHERE session_id = p_session_id;
        
        -- Update user's last login
        PERFORM update_last_login(p_user_id);
        
        RETURN true;
    END IF;
    
    RETURN false;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- View for active users with game counts
CREATE OR REPLACE VIEW active_users_with_games AS
SELECT 
    u.id,
    u.phone_number,
    u.username,
    u.last_login,
    COALESCE(gs.games_remaining, 0) as games_remaining,
    gs.expires_at as session_expires_at
FROM public.users u
LEFT JOIN LATERAL (
    SELECT games_remaining, expires_at
    FROM public.game_sessions
    WHERE user_id = u.id
    AND is_active = true
    AND expires_at > NOW()
    ORDER BY created_at DESC
    LIMIT 1
) gs ON true
WHERE u.last_login > NOW() - INTERVAL '30 days';

-- Add comments
COMMENT ON FUNCTION get_or_create_user IS 'Get existing user or create new one by phone number';
COMMENT ON FUNCTION update_last_login IS 'Update user last login timestamp';
COMMENT ON FUNCTION get_active_game_session IS 'Get active game session for user';
COMMENT ON FUNCTION decrement_game_count IS 'Decrement game count and record play history';
COMMENT ON FUNCTION create_qr_session IS 'Create new QR login session';
COMMENT ON FUNCTION authenticate_qr_session IS 'Authenticate QR session with user ID';
COMMENT ON VIEW active_users_with_games IS 'View of active users with their current game counts';
