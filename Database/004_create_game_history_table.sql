-- Migration: Create game_history table
-- Description: Tracks game play history across website and desktop
-- Created: 2025-01-07

-- Create game_history table
CREATE TABLE IF NOT EXISTS public.game_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
    game_name TEXT NOT NULL,
    played_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    source TEXT NOT NULL CHECK (source IN ('website', 'desktop'))
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_game_history_user_id ON public.game_history(user_id);
CREATE INDEX IF NOT EXISTS idx_game_history_game_name ON public.game_history(game_name);
CREATE INDEX IF NOT EXISTS idx_game_history_played_at ON public.game_history(played_at);
CREATE INDEX IF NOT EXISTS idx_game_history_source ON public.game_history(source);

-- Enable Row Level Security
ALTER TABLE public.game_history ENABLE ROW LEVEL SECURITY;

-- Create policies for game_history table
-- Users can read their own game history
CREATE POLICY "Users can view their own game history"
    ON public.game_history
    FOR SELECT
    USING (auth.uid()::text = user_id::text);

-- Users can insert their own game history
CREATE POLICY "Users can insert their own game history"
    ON public.game_history
    FOR INSERT
    WITH CHECK (auth.uid()::text = user_id::text);

-- Function to get user's most played games
CREATE OR REPLACE FUNCTION get_most_played_games(p_user_id UUID, p_limit INTEGER DEFAULT 10)
RETURNS TABLE (
    game_name TEXT,
    play_count BIGINT,
    last_played TIMESTAMP WITH TIME ZONE
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        gh.game_name,
        COUNT(*) as play_count,
        MAX(gh.played_at) as last_played
    FROM public.game_history gh
    WHERE gh.user_id = p_user_id
    GROUP BY gh.game_name
    ORDER BY play_count DESC, last_played DESC
    LIMIT p_limit;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Function to get user's recent games
CREATE OR REPLACE FUNCTION get_recent_games(p_user_id UUID, p_limit INTEGER DEFAULT 10)
RETURNS TABLE (
    game_name TEXT,
    played_at TIMESTAMP WITH TIME ZONE,
    source TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        gh.game_name,
        gh.played_at,
        gh.source
    FROM public.game_history gh
    WHERE gh.user_id = p_user_id
    ORDER BY gh.played_at DESC
    LIMIT p_limit;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Add comments for documentation
COMMENT ON TABLE public.game_history IS 'Tracks game play history across website and desktop';
COMMENT ON COLUMN public.game_history.id IS 'Unique history entry identifier';
COMMENT ON COLUMN public.game_history.user_id IS 'User who played the game';
COMMENT ON COLUMN public.game_history.game_name IS 'Name of the game played';
COMMENT ON COLUMN public.game_history.played_at IS 'When the game was played';
COMMENT ON COLUMN public.game_history.source IS 'Where the game was played: website or desktop';
