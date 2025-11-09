-- Migration: Create qr_login_sessions table
-- Description: Tracks QR code authentication sessions for desktop login
-- Created: 2025-01-07

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

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_qr_login_sessions_session_id ON public.qr_login_sessions(session_id);
CREATE INDEX IF NOT EXISTS idx_qr_login_sessions_user_id ON public.qr_login_sessions(user_id);
CREATE INDEX IF NOT EXISTS idx_qr_login_sessions_status ON public.qr_login_sessions(status);
CREATE INDEX IF NOT EXISTS idx_qr_login_sessions_expires_at ON public.qr_login_sessions(expires_at);
CREATE INDEX IF NOT EXISTS idx_qr_login_sessions_desktop_device_id ON public.qr_login_sessions(desktop_device_id);

-- Enable Row Level Security
ALTER TABLE public.qr_login_sessions ENABLE ROW LEVEL SECURITY;

-- Create policies for qr_login_sessions table
-- Anyone can read pending sessions (needed for QR code display)
CREATE POLICY "Anyone can view pending QR sessions"
    ON public.qr_login_sessions
    FOR SELECT
    USING (status = 'pending' OR auth.uid()::text = user_id::text);

-- Anyone can insert QR sessions (desktop app creates them)
CREATE POLICY "Allow anyone to insert QR sessions"
    ON public.qr_login_sessions
    FOR INSERT
    WITH CHECK (true);

-- Users can update their own QR sessions or pending sessions
CREATE POLICY "Users can update QR sessions"
    ON public.qr_login_sessions
    FOR UPDATE
    USING (
        status = 'pending' OR 
        auth.uid()::text = user_id::text
    );

-- Function to automatically expire old QR sessions
CREATE OR REPLACE FUNCTION expire_old_qr_sessions()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE public.qr_login_sessions
    SET status = 'expired'
    WHERE expires_at < NOW() 
    AND status IN ('pending', 'scanned');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger to expire QR sessions on insert or update
CREATE TRIGGER trigger_expire_qr_sessions
    AFTER INSERT OR UPDATE ON public.qr_login_sessions
    FOR EACH STATEMENT
    EXECUTE FUNCTION expire_old_qr_sessions();

-- Function to clean up old expired sessions (run periodically)
CREATE OR REPLACE FUNCTION cleanup_expired_qr_sessions()
RETURNS void AS $$
BEGIN
    DELETE FROM public.qr_login_sessions
    WHERE status = 'expired' 
    AND created_at < NOW() - INTERVAL '24 hours';
END;
$$ LANGUAGE plpgsql;

-- Add comments for documentation
COMMENT ON TABLE public.qr_login_sessions IS 'Tracks QR code authentication sessions for desktop login';
COMMENT ON COLUMN public.qr_login_sessions.id IS 'Unique session identifier';
COMMENT ON COLUMN public.qr_login_sessions.session_id IS 'QR code session ID (shown in QR)';
COMMENT ON COLUMN public.qr_login_sessions.user_id IS 'User who scanned the QR code';
COMMENT ON COLUMN public.qr_login_sessions.status IS 'Current status: pending, scanned, authenticated, expired';
COMMENT ON COLUMN public.qr_login_sessions.created_at IS 'Session creation timestamp';
COMMENT ON COLUMN public.qr_login_sessions.expires_at IS 'When this QR session expires';
COMMENT ON COLUMN public.qr_login_sessions.desktop_device_id IS 'Desktop device identifier';
