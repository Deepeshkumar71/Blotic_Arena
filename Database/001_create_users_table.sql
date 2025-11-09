-- Migration: Create users table
-- Description: Stores user account information for Blotic Arena
-- Created: 2025-01-07

-- Create users table
CREATE TABLE IF NOT EXISTS public.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    phone_number TEXT UNIQUE NOT NULL,
    username TEXT,
    email TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    last_login TIMESTAMP WITH TIME ZONE
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_users_phone_number ON public.users(phone_number);
CREATE INDEX IF NOT EXISTS idx_users_email ON public.users(email);
CREATE INDEX IF NOT EXISTS idx_users_created_at ON public.users(created_at);

-- Enable Row Level Security
ALTER TABLE public.users ENABLE ROW LEVEL SECURITY;

-- Create policies for users table
-- Users can read their own data
CREATE POLICY "Users can view their own profile"
    ON public.users
    FOR SELECT
    USING (auth.uid()::text = id::text);

-- Users can update their own data
CREATE POLICY "Users can update their own profile"
    ON public.users
    FOR UPDATE
    USING (auth.uid()::text = id::text);

-- Allow insert for new users (service role or authenticated users)
CREATE POLICY "Allow authenticated users to insert"
    ON public.users
    FOR INSERT
    WITH CHECK (true);

-- Add comments for documentation
COMMENT ON TABLE public.users IS 'Stores user account information for Blotic Arena';
COMMENT ON COLUMN public.users.id IS 'Unique user identifier';
COMMENT ON COLUMN public.users.phone_number IS 'User phone number (unique)';
COMMENT ON COLUMN public.users.username IS 'Display name for the user';
COMMENT ON COLUMN public.users.email IS 'User email address (optional)';
COMMENT ON COLUMN public.users.created_at IS 'Account creation timestamp';
COMMENT ON COLUMN public.users.last_login IS 'Last login timestamp';
