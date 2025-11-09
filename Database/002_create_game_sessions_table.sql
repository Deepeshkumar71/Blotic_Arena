-- Migration: Create game_sessions table
-- Description: Tracks user game allowances and session tokens
-- Created: 2025-01-07

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

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_game_sessions_user_id ON public.game_sessions(user_id);
CREATE INDEX IF NOT EXISTS idx_game_sessions_session_token ON public.game_sessions(session_token);
CREATE INDEX IF NOT EXISTS idx_game_sessions_is_active ON public.game_sessions(is_active);
CREATE INDEX IF NOT EXISTS idx_game_sessions_expires_at ON public.game_sessions(expires_at);

-- Enable Row Level Security
ALTER TABLE public.game_sessions ENABLE ROW LEVEL SECURITY;

-- Create policies for game_sessions table
-- Users can read their own game sessions
CREATE POLICY "Users can view their own game sessions"
    ON public.game_sessions
    FOR SELECT
    USING (auth.uid()::text = user_id::text);

-- Users can update their own game sessions
CREATE POLICY "Users can update their own game sessions"
    ON public.game_sessions
    FOR UPDATE
    USING (auth.uid()::text = user_id::text);

-- Allow insert for new sessions
CREATE POLICY "Allow authenticated users to insert game sessions"
    ON public.game_sessions
    FOR INSERT
    WITH CHECK (auth.uid()::text = user_id::text);

-- Function to automatically expire old sessions
CREATE OR REPLACE FUNCTION expire_old_game_sessions()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE public.game_sessions
    SET is_active = false
    WHERE expires_at < NOW() AND is_active = true;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger to expire sessions on insert or update
CREATE TRIGGER trigger_expire_game_sessions
    AFTER INSERT OR UPDATE ON public.game_sessions
    FOR EACH STATEMENT
    EXECUTE FUNCTION expire_old_game_sessions();

-- Add comments for documentation
COMMENT ON TABLE public.game_sessions IS 'Tracks user game allowances and session tokens';
COMMENT ON COLUMN public.game_sessions.id IS 'Unique session identifier';
COMMENT ON COLUMN public.game_sessions.user_id IS 'Reference to user who owns this session';
COMMENT ON COLUMN public.game_sessions.session_token IS 'Unique token for this session';
COMMENT ON COLUMN public.game_sessions.games_remaining IS 'Number of games left to play';
COMMENT ON COLUMN public.game_sessions.expires_at IS 'When this session expires';
COMMENT ON COLUMN public.game_sessions.created_at IS 'Session creation timestamp';
COMMENT ON COLUMN public.game_sessions.is_active IS 'Whether this session is currently active';
