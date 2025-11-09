using System;
using System.Threading.Tasks;
using Supabase;
using BloticArena.Config;

namespace BloticArena.Services
{
    /// <summary>
    /// Service for managing Supabase connection and operations
    /// </summary>
    public class SupabaseService
    {
        private static SupabaseService? _instance;
        private Client? _client;
        private bool _isInitialized = false;

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static SupabaseService Instance => _instance ??= new SupabaseService();

        /// <summary>
        /// Supabase client instance
        /// </summary>
        public Client? Client => _client;

        /// <summary>
        /// Check if service is initialized
        /// </summary>
        public bool IsInitialized => _isInitialized;

        private SupabaseService() { }

        /// <summary>
        /// Initialize Supabase connection
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            try
            {
                if (_isInitialized && _client != null)
                    return true;

                System.Diagnostics.Debug.WriteLine($"🔄 Initializing Supabase...");
                System.Diagnostics.Debug.WriteLine($"📍 URL: {SupabaseConfig.Url}");
                System.Diagnostics.Debug.WriteLine($"🔑 Key: {SupabaseConfig.AnonKey.Substring(0, 20)}...");

                // Test basic HTTP connectivity first
                System.Diagnostics.Debug.WriteLine($"🔍 Testing HTTP connectivity...");
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(10);
                    try
                    {
                        var response = await httpClient.GetAsync($"{SupabaseConfig.Url}/rest/v1/");
                        System.Diagnostics.Debug.WriteLine($"✅ HTTP test successful: {response.StatusCode}");
                    }
                    catch (Exception httpEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ HTTP test failed: {httpEx.Message}");
                        throw new Exception($"Cannot reach Supabase server. Please check your internet connection and firewall settings. Error: {httpEx.Message}", httpEx);
                    }
                }

                var options = new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = false
                };

                _client = new Client(SupabaseConfig.Url, SupabaseConfig.AnonKey, options);
                await _client.InitializeAsync();

                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine($"✅ Supabase initialized successfully!");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Supabase initialization error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Inner exception: {ex.InnerException.Message}");
                }
                _isInitialized = false;
                return false;
            }
        }

        /// <summary>
        /// Check connection status
        /// </summary>
        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                if (_client == null)
                    return false;

                // Simple health check - try to access the client
                return _client != null && _isInitialized;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Retry operation with exponential backoff
        /// </summary>
        public async Task<T?> RetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    if (i == maxRetries - 1)
                    {
                        System.Diagnostics.Debug.WriteLine($"Operation failed after {maxRetries} retries: {ex.Message}");
                        throw;
                    }

                    // Exponential backoff: 1s, 2s, 4s
                    await Task.Delay((int)Math.Pow(2, i) * 1000);
                }
            }

            return default;
        }
    }
}
