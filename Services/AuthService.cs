using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using BloticArena.Config;
using BloticArena.Models;
using Supabase;

namespace BloticArena.Services
{
    /// <summary>
    /// Service for handling QR code authentication and user sessions
    /// </summary>
    public class AuthService
    {
        private static AuthService? _instance;
        private Timer? _pollingTimer;
        private string? _currentSessionId;
        private User? _currentUser;
        private bool _isAuthenticated = false;

        /// <summary>
        /// Event fired when authentication succeeds
        /// </summary>
        public event EventHandler<User>? AuthenticationSucceeded;

        /// <summary>
        /// Event fired when authentication fails
        /// </summary>
        public event EventHandler<string>? AuthenticationFailed;

        /// <summary>
        /// Event fired when QR session expires
        /// </summary>
        public event EventHandler? SessionExpired;

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static AuthService Instance => _instance ??= new AuthService();

        /// <summary>
        /// Current authenticated user
        /// </summary>
        public User? CurrentUser => _currentUser;

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        public bool IsAuthenticated => _isAuthenticated;

        /// <summary>
        /// Current QR session ID
        /// </summary>
        public string? CurrentSessionId => _currentSessionId;

        private AuthService() { }

        /// <summary>
        /// Create a new QR login session
        /// </summary>
        public async Task<string?> CreateQRSessionAsync()
        {
            try
            {
                var client = SupabaseService.Instance.Client;
                if (client == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Supabase client not initialized");
                    return null;
                }

                // Generate unique session ID
                _currentSessionId = Guid.NewGuid().ToString();
                var deviceId = GetDeviceId();

                // Create session in database directly
                var newSession = new QRLoginSession
                {
                    SessionId = _currentSessionId,
                    DesktopDeviceId = deviceId,
                    StatusEnum = QRSessionStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(SupabaseConfig.QRSessionExpirationMinutes)
                };

                await client.From<QRLoginSession>().Insert(newSession);

                System.Diagnostics.Debug.WriteLine($"✅ QR session created: {_currentSessionId}");
                return _currentSessionId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error creating QR session: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Inner exception: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        /// <summary>
        /// Start polling for QR authentication
        /// </summary>
        public void StartPolling()
        {
            if (_pollingTimer != null)
            {
                StopPolling();
            }

            _pollingTimer = new Timer(SupabaseConfig.QRPollingIntervalMs);
            _pollingTimer.Elapsed += async (sender, e) => await CheckAuthenticationStatusAsync();
            _pollingTimer.Start();

            System.Diagnostics.Debug.WriteLine("🔄 Started polling for QR authentication");
        }

        /// <summary>
        /// Stop polling for QR authentication
        /// </summary>
        public void StopPolling()
        {
            if (_pollingTimer != null)
            {
                _pollingTimer.Stop();
                _pollingTimer.Dispose();
                _pollingTimer = null;
                System.Diagnostics.Debug.WriteLine("⏹️ Stopped polling");
            }
        }

        /// <summary>
        /// Check authentication status
        /// </summary>
        private async Task CheckAuthenticationStatusAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentSessionId))
                    return;

                var client = SupabaseService.Instance.Client;
                if (client == null)
                    return;

                // Query QR session status
                System.Diagnostics.Debug.WriteLine($"🔍 Querying session: {_currentSessionId}");
                
                var response = await client
                    .From<QRLoginSession>()
                    .Where(x => x.SessionId == _currentSessionId)
                    .Get();

                if (response?.Models == null || response.Models.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ QR session not found for ID: {_currentSessionId}");
                    StopPolling();
                    SessionExpired?.Invoke(this, EventArgs.Empty);
                    return;
                }
                
                var session = response.Models[0];

                System.Diagnostics.Debug.WriteLine($"📊 QR Status: {session.Status}, UserId: {session.UserId}, ExpiresAt: {session.ExpiresAt}");

                // Check if authenticated
                if (session.StatusEnum == QRSessionStatus.Authenticated && session.UserId.HasValue)
                {
                    StopPolling();
                    System.Diagnostics.Debug.WriteLine($"✅ Authentication detected! User ID: {session.UserId.Value}");
                    await HandleAuthenticationSuccessAsync(session.UserId.Value);
                }
                // Check if expired (only if not authenticated)
                else if (session.StatusEnum == QRSessionStatus.Expired || 
                        (session.StatusEnum == QRSessionStatus.Pending && session.ExpiresAt < DateTime.UtcNow))
                {
                    StopPolling();
                    System.Diagnostics.Debug.WriteLine("⏰ QR session expired");
                    SessionExpired?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error checking auth status: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle successful authentication
        /// </summary>
        private async Task HandleAuthenticationSuccessAsync(Guid userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 Handling authentication for user: {userId}");
                
                var client = SupabaseService.Instance.Client;
                if (client == null)
                {
                    throw new Exception("Supabase client not initialized");
                }

                // Fetch user profile from profiles table
                System.Diagnostics.Debug.WriteLine($"📥 Fetching profile for user: {userId}");
                var profileResponse = await client
                    .From<Profile>()
                    .Where(x => x.Id == userId)
                    .Get();

                Profile? profile = null;
                if (profileResponse?.Models != null && profileResponse.Models.Count > 0)
                {
                    profile = profileResponse.Models[0];
                    System.Diagnostics.Debug.WriteLine($"✅ Profile fetched: {profile.FullName ?? profile.Email}");
                }

                // Create user object with profile data
                var user = new User
                {
                    Id = userId,
                    Username = profile?.FullName ?? profile?.Email ?? "Arena User",
                    PhoneNumber = profile?.Phone,
                    Email = profile?.Email,
                    CreatedAt = profile?.CreatedAt ?? DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow
                };

                _currentUser = user;
                _isAuthenticated = true;

                System.Diagnostics.Debug.WriteLine($"✅ Authentication successful! User: {user.Username}");
                AuthenticationSucceeded?.Invoke(this, user);

                // Save auth state locally
                await SaveAuthStateAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error handling authentication: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                AuthenticationFailed?.Invoke(this, ex.Message);
            }
        }

        /// <summary>
        /// Fetch active game session for current user
        /// </summary>
        private async Task FetchGameSessionAsync()
        {
            try
            {
                if (_currentUser == null)
                    return;

                var client = SupabaseService.Instance.Client;
                if (client == null)
                    return;

                // Use RPC to get active session
                var parameters = new Dictionary<string, object>
                {
                    { "p_user_id", _currentUser.Id }
                };
                var response = await client.Rpc("get_active_game_session", parameters);

                // Parse response to get games remaining
                // Note: This is a simplified version - you may need to adjust based on actual response format
                if (response != null)
                {
                    System.Diagnostics.Debug.WriteLine($"📊 Fetched game session data");
                    // Update user's games remaining from response
                    // _currentUser.GamesRemaining = ... (parse from response)
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error fetching game session: {ex.Message}");
            }
        }

        /// <summary>
        /// Save authentication state to local storage
        /// </summary>
        private async Task SaveAuthStateAsync()
        {
            try
            {
                if (_currentUser == null)
                    return;

                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var bloticPath = System.IO.Path.Combine(appDataPath, "BloticArena");
                
                if (!System.IO.Directory.Exists(bloticPath))
                    System.IO.Directory.CreateDirectory(bloticPath);

                var authFile = System.IO.Path.Combine(bloticPath, "auth.json");
                
                var authData = new
                {
                    UserId = _currentUser.Id,
                    PhoneNumber = _currentUser.PhoneNumber,
                    Username = _currentUser.Username,
                    LastLogin = DateTime.UtcNow
                };

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(authData);
                await System.IO.File.WriteAllTextAsync(authFile, json);

                System.Diagnostics.Debug.WriteLine("💾 Auth state saved");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error saving auth state: {ex.Message}");
            }
        }

        /// <summary>
        /// Load authentication state from local storage
        /// </summary>
        public async Task<bool> LoadAuthStateAsync()
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var authFile = System.IO.Path.Combine(appDataPath, "BloticArena", "auth.json");

                if (!System.IO.File.Exists(authFile))
                    return false;

                var json = await System.IO.File.ReadAllTextAsync(authFile);
                var authData = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(json, new
                {
                    UserId = Guid.Empty,
                    PhoneNumber = "",
                    Username = "",
                    LastLogin = DateTime.MinValue
                });

                if (authData == null)
                    return false;

                // Verify user still exists in database
                var client = SupabaseService.Instance.Client;
                if (client == null)
                    return false;

                var user = await client
                    .From<User>()
                    .Where(x => x.Id == authData.UserId)
                    .Single();

                if (user != null)
                {
                    _currentUser = user;
                    _isAuthenticated = true;
                    await FetchGameSessionAsync();
                    
                    System.Diagnostics.Debug.WriteLine($"✅ Auto-login successful! User: {user.Username ?? user.PhoneNumber}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error loading auth state: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Logout current user
        /// </summary>
        public async Task LogoutAsync()
        {
            try
            {
                StopPolling();
                _currentUser = null;
                _isAuthenticated = false;
                _currentSessionId = null;

                // Delete auth file
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var authFile = System.IO.Path.Combine(appDataPath, "BloticArena", "auth.json");
                
                if (System.IO.File.Exists(authFile))
                    System.IO.File.Delete(authFile);

                System.Diagnostics.Debug.WriteLine("👋 User logged out");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error during logout: {ex.Message}");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Get unique device identifier
        /// </summary>
        private string GetDeviceId()
        {
            try
            {
                var machineName = Environment.MachineName;
                var userName = Environment.UserName;
                return $"{machineName}_{userName}";
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Sync game count with server
        /// </summary>
        public async Task<int> SyncGameCountAsync()
        {
            try
            {
                if (_currentUser == null || !_isAuthenticated)
                    return -1;

                await FetchGameSessionAsync();
                return _currentUser.GamesRemaining;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error syncing game count: {ex.Message}");
                return -1;
            }
        }
    }
}
