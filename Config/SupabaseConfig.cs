using System;

namespace BloticArena.Config
{
    /// <summary>
    /// Configuration class for Supabase connection
    /// USING WEBSITE DATABASE (sbdrzesfuweacfssdwzk) for cross-platform login
    /// </summary>
    public static class SupabaseConfig
    {
        /// <summary>
        /// Supabase project URL (shared with website)
        /// </summary>
        public static string Url => "https://sbdrzesfuweacfssdwzk.supabase.co";

        /// <summary>
        /// Supabase anonymous/public key (safe for client-side use, shared with website)
        /// </summary>
        public static string AnonKey => "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InNiZHJ6ZXNmdXdlYWNmc3Nkd3prIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjAyNjcxMzcsImV4cCI6MjA3NTg0MzEzN30.gwuBDBbyQ3hLlVmtj784aO2LJdXkMQea1DKYvuceE7o";

        /// <summary>
        /// QR session expiration time in minutes
        /// </summary>
        public static int QRSessionExpirationMinutes => 5;

        /// <summary>
        /// Polling interval for QR login status in milliseconds
        /// </summary>
        public static int QRPollingIntervalMs => 2000;

        /// <summary>
        /// Background sync interval in seconds
        /// </summary>
        public static int BackgroundSyncIntervalSeconds => 30;

        /// <summary>
        /// Auth token storage key
        /// </summary>
        public static string AuthTokenKey => "blotic_auth_token";

        /// <summary>
        /// User data storage key
        /// </summary>
        public static string UserDataKey => "blotic_user_data";
    }
}
