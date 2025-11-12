using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Windows.Threading;
using BloticArena.Models;
using BloticArena.Services;
using System.IO;
using IOPath = System.IO.Path;
using IOFile = System.IO.File;
using IODirectory = System.IO.Directory;
using QRCoder;
using DrawingColor = System.Drawing.Color;
using DrawingBitmap = System.Drawing.Bitmap;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace BloticArena
{
    public partial class MainWindow : Window
    {
        private List<AppInfo> _allApps = new();
        private List<AppInfo> _favoriteGames = new();
        private readonly AppScanner _scanner = new();
        private readonly Random _random = new();
        private string _currentPage = "Home";
        
        // Screensaver fields
        private DispatcherTimer _screensaverTimer;
        private readonly List<Ellipse> _screensaverParticles = new();
        private readonly List<System.Windows.Shapes.Line> _screensaverParticleLines = new();
        private DispatcherTimer _screensaverParticleTimer;
        private bool _isScreensaverActive = false;
        private DateTime _lastActivityTime = DateTime.Now;

        // Home page video fields
        private readonly List<Ellipse> _homeParticles = new();
        private readonly List<System.Windows.Shapes.Line> _homeParticleLines = new();
        private DispatcherTimer _homeParticleTimer;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            LoadFavorites();
            InitializeSupabase();
            LoadApplications();
            
            // Initialize home video particles
            InitializeHomeVideo();
            
            // Initialize screensaver
            InitializeScreensaver();
        }

        private async void InitializeSupabase()
        {
            try
            {
                var success = await Services.SupabaseService.Instance.InitializeAsync();
                if (success)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Supabase initialized successfully");
                    
                    // Subscribe to auth events BEFORE auto-login
                    Services.AuthService.Instance.AuthenticationSucceeded += OnAuthenticationSucceeded;
                    Services.AuthService.Instance.AuthenticationFailed += OnAuthenticationFailed;
                    Services.AuthService.Instance.SessionExpired += OnSessionExpired;
                    
                    // Try to auto-login
                    var autoLogin = await Services.AuthService.Instance.LoadAuthStateAsync();
                    if (autoLogin)
                    {
                        System.Diagnostics.Debug.WriteLine("✅ Auto-login successful");
                        var user = Services.AuthService.Instance.CurrentUser;
                        if (user != null)
                        {
                            // Show welcome screen first, then update UI
                            await ShowWelcomeAnimationAsync(user.Username ?? user.PhoneNumber ?? "User");
                            UpdateUIForAuthenticatedUser();
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Supabase initialization failed");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Supabase initialization error: {ex.Message}");
            }
        }

        private async void OnAuthenticationSucceeded(object? sender, User user)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                System.Diagnostics.Debug.WriteLine($"🎉 User authenticated: {user.Username ?? user.PhoneNumber}");
                
                // Show welcome screen immediately (don't wait for profile data)
                var welcomeTask = ShowWelcomeAnimationAsync(user.Username ?? user.PhoneNumber ?? "User");
                
                // Load profile data in parallel (background)
                var profileTask = Task.Run(async () =>
                {
                    await Task.Delay(100); // Small delay to let welcome animation start
                    await Dispatcher.InvokeAsync(() => UpdateUIForAuthenticatedUser());
                });
                
                // Wait for welcome animation to complete
                await welcomeTask;
            });
        }

        private async Task ShowWelcomeAnimationAsync(string username)
        {
            // Fade out QR and fade in welcome simultaneously on the SAME screen
            if (QRCodePanel.Visibility == Visibility.Visible)
            {
                // Prepare welcome overlay (hidden, ready to fade in)
                WelcomeOverlay.Visibility = Visibility.Visible;
                WelcomeOverlay.Opacity = 1;
                UsernameText.Text = username;
                WelcomeText.Opacity = 0;
                UsernameText.Opacity = 0;
                
                // Fade out QR code
                var fadeOutQR = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(400),
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseIn }
                };
                QRCodePanel.BeginAnimation(OpacityProperty, fadeOutQR);
                
                // Simultaneously fade in welcome text (starts immediately)
                var welcomeAnim = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(600),
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
                };
                WelcomeText.BeginAnimation(OpacityProperty, welcomeAnim);
                
                // Fade in username slightly after
                await Task.Delay(250);
                var usernameAnim = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(600),
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
                };
                UsernameText.BeginAnimation(OpacityProperty, usernameAnim);
                
                // Wait for QR to finish fading
                await Task.Delay(250);
                QRCodePanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                // If QR not visible, just show welcome normally
                WelcomeOverlay.Visibility = Visibility.Visible;
                UsernameText.Text = username;
                
                var welcomeAnim = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
                };
                WelcomeText.BeginAnimation(OpacityProperty, welcomeAnim);
                
                await Task.Delay(200);
                var usernameAnim = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(600),
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
                };
                UsernameText.BeginAnimation(OpacityProperty, usernameAnim);
            }
            
            // Display welcome message
            await Task.Delay(1500);
            
            // Create a TaskCompletionSource to wait for fade out animation
            var tcs = new TaskCompletionSource<bool>();
            
            var fadeOut = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(600)
            };
            
            fadeOut.Completed += (s, e) =>
            {
                WelcomeOverlay.Visibility = Visibility.Collapsed;
                WelcomeText.Opacity = 0;
                UsernameText.Opacity = 0;
                
                // Navigate to home page after animation
                _currentPage = "Home";
                UpdateNavigation();
                ShowHomePage();
                
                tcs.SetResult(true);
            };
            
            WelcomeOverlay.BeginAnimation(OpacityProperty, fadeOut);
            
            // Wait for animation to complete
            await tcs.Task;
        }

        private void OnAuthenticationFailed(object? sender, string error)
        {
            Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine($"❌ Authentication failed: {error}");
                Controls.CustomAlert.Show(AlertContainer, "Authentication Error", 
                    $"Authentication failed: {error}", 
                    Controls.CustomAlert.AlertType.Error);
            });
        }

        private void OnSessionExpired(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("⏰ QR session expired");
                Controls.CustomAlert.Show(AlertContainer, "Session Expired", 
                    "QR code expired. Please generate a new one.", 
                    Controls.CustomAlert.AlertType.Warning);
            });
        }

        private async void UpdateUIForAuthenticatedUser()
        {
            var user = Services.AuthService.Instance.CurrentUser;
            if (user != null)
            {
                // Show only first name
                var username = user.Username ?? "Profile";
                var firstName = username.Split(' ')[0]; // Get first name only
                ProfileButtonText.Text = firstName;
                
                // Show avatar instead of icon
                ProfileButtonIcon.Visibility = Visibility.Collapsed;
                ProfileButtonAvatar.Visibility = Visibility.Visible;
                
                // Set initials
                var initials = GetInitials(user.Username ?? user.Email ?? "U");
                ProfileButtonInitials.Text = initials;
                
                // Fetch and display profile picture and game count
                await UpdateProfileButton(user.Id);
                await UpdateGamesStatistics(user.Id.ToString());
                await UpdateGameCount(user.Id);
                
                System.Diagnostics.Debug.WriteLine($"🎮 UI Updated for authenticated user: {user.Username}, Games: {user.GamesRemaining}");
                
                // If we're on the profile page, refresh it to show profile instead of QR
                if (_currentPage == "Profile")
                {
                    ShowProfilePage();
                }
            }
        }

        private async Task UpdateProfileButton(Guid userId)
        {
            try
            {
                var client = Services.SupabaseService.Instance.Client;
                if (client != null)
                {
                    var profileResponse = await client
                        .From<Profile>()
                        .Where(x => x.Id == userId)
                        .Get();
                    
                    if (profileResponse?.Models != null && profileResponse.Models.Count > 0)
                    {
                        var profile = profileResponse.Models[0];
                        
                        // Load profile picture if available
                        if (!string.IsNullOrEmpty(profile.AvatarUrl))
                        {
                            try
                            {
                                var bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(profile.AvatarUrl, UriKind.Absolute);
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                
                                ProfileButtonImageBrush.ImageSource = bitmap;
                                ProfileButtonInitials.Visibility = Visibility.Collapsed;
                                
                                System.Diagnostics.Debug.WriteLine($"✅ Profile button picture loaded");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"⚠️ Failed to load profile button picture: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error updating profile button: {ex.Message}");
            }
        }

        private async Task UpdateGameCount(Guid userId)
        {
            try
            {
                var client = Services.SupabaseService.Instance.Client;
                if (client != null)
                {
                    var registrationResponse = await client
                        .From<EventRegistration>()
                        .Where(x => x.UserId == userId)
                        .Where(x => x.PaymentStatus == "paid")
                        .Get();
                    
                    if (registrationResponse?.Models != null && registrationResponse.Models.Count > 0)
                    {
                        // Sum all games from paid registrations only
                        var totalGames = registrationResponse.Models.Sum(r => r.GamesRemaining);
                        
                        // Find and update badge in the button template
                        var template = ProfileNavButton.Template;
                        if (template != null)
                        {
                            var badge = template.FindName("GameCountBadge", ProfileNavButton) as Border;
                            var countText = template.FindName("ProfileGameCount", ProfileNavButton) as TextBlock;
                            
                            if (badge != null && countText != null)
                            {
                                countText.Text = totalGames.ToString();
                                badge.Visibility = totalGames > 0 ? Visibility.Visible : Visibility.Collapsed;
                            }
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"🎮 Total games remaining: {totalGames}");
                    }
                    else
                    {
                        // Hide badge if no games
                        var template = ProfileNavButton.Template;
                        if (template != null)
                        {
                            var badge = template.FindName("GameCountBadge", ProfileNavButton) as Border;
                            if (badge != null)
                            {
                                badge.Visibility = Visibility.Collapsed;
                            }
                        }
                        System.Diagnostics.Debug.WriteLine($"⚠️ No registrations found for badge update");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error fetching game count: {ex.Message}");
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadApplications();
        }


        #region Home Video Methods
        
        private void InitializeHomeVideo()
        {
            // Initialize home particle animation timer
            _homeParticleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20)
            };
            _homeParticleTimer.Tick += HomeParticleTimer_Tick;
            _homeParticleTimer.Start();
            
            // Create home particles
            CreateHomeParticles();
            
            System.Diagnostics.Debug.WriteLine("✅ Home video particles initialized");
        }
        
        private void CreateHomeParticles()
        {
            // Create white connecting star particles for home page with random flow
            for (int i = 0; i < 150; i++)
            {
                var size = _random.Next(2, 5);
                var particle = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Fill = new SolidColorBrush(Color.FromArgb(
                        180,  // Semi-transparent
                        255,  // White color
                        255,
                        255
                    )),
                    // Random velocity in all directions for natural flow
                    Tag = new Point(
                        (_random.NextDouble() - 0.5) * 3.0, // Random X velocity between -1.5 and 1.5
                        (_random.NextDouble() - 0.5) * 3.0  // Random Y velocity between -1.5 and 1.5
                    )
                };

                // Start particles at completely random positions across the entire screen
                Canvas.SetLeft(particle, _random.NextDouble() * 1920); // Full screen width
                Canvas.SetTop(particle, _random.NextDouble() * 1080);  // Full screen height
                
                HomeParticleCanvas.Children.Add(particle);
                _homeParticles.Add(particle);
            }
        }
        
        private void HomeParticleTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                // Clear old lines efficiently
                for (int i = _homeParticleLines.Count - 1; i >= 0; i--)
                {
                    HomeParticleCanvas.Children.Remove(_homeParticleLines[i]);
                }
                _homeParticleLines.Clear();

                // Move particles
                foreach (var particle in _homeParticles)
                {
                    var velocity = (Point)particle.Tag;
                    var left = Canvas.GetLeft(particle) + velocity.X;
                    var top = Canvas.GetTop(particle) + velocity.Y;

                    // Wrap around edges (full screen like screensaver)
                    if (left < 0) left = ActualWidth;
                    if (left > ActualWidth) left = 0;
                    if (top < 0) top = ActualHeight;
                    if (top > ActualHeight) top = 0;

                    Canvas.SetLeft(particle, left);
                    Canvas.SetTop(particle, top);
                }

                // Draw connecting lines between nearby particles (same as screensaver)
                for (int i = 0; i < _homeParticles.Count; i++)
                {
                    var p1 = _homeParticles[i];
                    var x1 = Canvas.GetLeft(p1) + p1.Width / 2;
                    var y1 = Canvas.GetTop(p1) + p1.Height / 2;

                    for (int j = i + 1; j < _homeParticles.Count; j++)
                    {
                        var p2 = _homeParticles[j];
                        var x2 = Canvas.GetLeft(p2) + p2.Width / 2;
                        var y2 = Canvas.GetTop(p2) + p2.Height / 2;

                        var distance = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));

                        // Draw line if particles are close (within 200px same as screensaver)
                        if (distance < 200)
                        {
                            var opacity = (byte)(100 * (1 - distance / 200)); // Same opacity calculation as screensaver
                            var line = new System.Windows.Shapes.Line
                            {
                                X1 = x1,
                                Y1 = y1,
                                X2 = x2,
                                Y2 = y2,
                                Stroke = new SolidColorBrush(Color.FromArgb(
                                    opacity,
                                    255, 255, 255 // White color
                                )),
                                StrokeThickness = 1.5, // Same thickness as screensaver
                                IsHitTestVisible = false
                            };
                            HomeParticleCanvas.Children.Add(line);
                            _homeParticleLines.Add(line);
                        }
                    }
                }
            }
            catch
            {
                // Silently handle any particle rendering errors
            }
        }
        
        private void HomeVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Loop the home video
            HomeVideo.Position = TimeSpan.Zero;
            HomeVideo.Play();
        }
        
        #endregion

        #region Screensaver Methods
        
        private void InitializeScreensaver()
        {
            // Initialize screensaver timer (15 seconds for testing)
            _screensaverTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(15)
            };
            _screensaverTimer.Tick += ScreensaverTimer_Tick;
            _screensaverTimer.Start();
            
            // Initialize screensaver particle animation
            _screensaverParticleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20)
            };
            _screensaverParticleTimer.Tick += ScreensaverParticleTimer_Tick;
            
            // Add event handlers for user activity detection
            this.MouseMove += OnUserActivity;
            this.KeyDown += OnUserActivity;
            this.MouseDown += OnUserActivity;
            this.MouseWheel += OnUserActivity;
            
            System.Diagnostics.Debug.WriteLine("✅ Screensaver initialized with 15 second timeout (fullscreen only)");
        }
        
        private void OnUserActivity(object sender, EventArgs e)
        {
            // Don't process activity if we're currently showing/hiding screensaver
            if (_isScreensaverActive && ScreensaverOverlay.Opacity < 1.0)
                return;
                
            _lastActivityTime = DateTime.Now;
            
            if (_isScreensaverActive)
            {
                HideScreensaver();
            }
        }
        
        private void ScreensaverTimer_Tick(object? sender, EventArgs e)
        {
            // Only activate screensaver if in fullscreen mode and enough time has passed
            if (!_isScreensaverActive && 
                WindowState == WindowState.Maximized && 
                DateTime.Now - _lastActivityTime >= TimeSpan.FromSeconds(15))
            {
                ShowScreensaver();
            }
        }
        
        private void ShowScreensaver()
        {
            if (_isScreensaverActive) return;
            
            _isScreensaverActive = true;
            
            // Temporarily disable user activity detection during setup
            this.MouseMove -= OnUserActivity;
            this.KeyDown -= OnUserActivity;
            this.MouseDown -= OnUserActivity;
            this.MouseWheel -= OnUserActivity;
            
            // Create screensaver particles
            CreateScreensaverParticles();
            
            // Show screensaver overlay
            ScreensaverOverlay.Visibility = Visibility.Visible;
            ScreensaverOverlay.Opacity = 0;
            
            // Start video playback
            try
            {
                System.Diagnostics.Debug.WriteLine($"🎬 Starting video playback...");
                
                // Try multiple paths for video file
                var possiblePaths = new[]
                {
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media", "blotic-video-compressed.mp4"),
                    System.IO.Path.Combine(Environment.CurrentDirectory, "Media", "blotic-video-compressed.mp4"),
                    @"d:\GitHub\Blotic Arena\Blotic_Arena\Media\blotic-video-compressed.mp4"
                };
                
                string? workingPath = null;
                foreach (var path in possiblePaths)
                {
                    System.Diagnostics.Debug.WriteLine($"📁 Checking path: {path}");
                    if (System.IO.File.Exists(path))
                    {
                        workingPath = path;
                        System.Diagnostics.Debug.WriteLine($"✅ Found video at: {path}");
                        break;
                    }
                }
                
                if (workingPath != null)
                {
                    // Set absolute path as source
                    ScreensaverVideo.Source = new Uri(workingPath, UriKind.Absolute);
                    
                    // Ensure video is visible and reset position
                    ScreensaverVideo.Visibility = Visibility.Visible;
                    ScreensaverVideo.Position = TimeSpan.Zero;
                    
                    // Force load the media
                    ScreensaverVideo.LoadedBehavior = MediaState.Manual;
                    ScreensaverVideo.UnloadedBehavior = MediaState.Manual;
                    
                    // Try to play
                    ScreensaverVideo.Play();
                    
                    System.Diagnostics.Debug.WriteLine($"▶️ Video play command sent");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Video file not found in any location");
                    ScreensaverVideo.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Video playback error: {ex.Message}");
                ScreensaverVideo.Visibility = Visibility.Collapsed;
            }
            
            // Start screensaver particle animation
            _screensaverParticleTimer.Start();
            
            // Smooth fade in animation (2 seconds)
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(2000),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            
            fadeIn.Completed += (s, e) =>
            {
                // Re-enable user activity detection after fade-in completes
                this.MouseMove += OnUserActivity;
                this.KeyDown += OnUserActivity;
                this.MouseDown += OnUserActivity;
                this.MouseWheel += OnUserActivity;
            };
            
            ScreensaverOverlay.BeginAnimation(OpacityProperty, fadeIn);
            
            System.Diagnostics.Debug.WriteLine("🌟 Screensaver activated with smooth fade-in");
        }
        
        private void HideScreensaver()
        {
            if (!_isScreensaverActive) return;
            
            _isScreensaverActive = false;
            
            // Smooth fade out animation (1.5 seconds)
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(1500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            
            fadeOut.Completed += (s, e) =>
            {
                // Stop video playback after fade out
                ScreensaverVideo.Stop();
                
                // Stop screensaver particle animation
                _screensaverParticleTimer.Stop();
                
                // Clear screensaver particles
                ScreensaverParticleCanvas.Children.Clear();
                _screensaverParticles.Clear();
                _screensaverParticleLines.Clear();
                
                // Hide overlay
                ScreensaverOverlay.Visibility = Visibility.Collapsed;
                
                System.Diagnostics.Debug.WriteLine("🌟 Screensaver deactivated with smooth fade-out");
            };
            
            ScreensaverOverlay.BeginAnimation(OpacityProperty, fadeOut);
            
            System.Diagnostics.Debug.WriteLine("🌟 Screensaver fade-out started");
        }
        
        private void CreateScreensaverParticles()
        {
            // Create white connecting star particles for screensaver with random flow
            for (int i = 0; i < 150; i++)
            {
                var size = _random.Next(2, 5);
                var particle = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Fill = new SolidColorBrush(Color.FromArgb(
                        180,  // Semi-transparent
                        255,  // White color
                        255,
                        255
                    )),
                    // Random velocity in all directions for natural flow
                    Tag = new Point(
                        (_random.NextDouble() - 0.5) * 3.0, // Random X velocity between -1.5 and 1.5
                        (_random.NextDouble() - 0.5) * 3.0  // Random Y velocity between -1.5 and 1.5
                    )
                };

                // Start particles at completely random positions across the entire screen
                Canvas.SetLeft(particle, _random.NextDouble() * 1920); // Full screen width
                Canvas.SetTop(particle, _random.NextDouble() * 1080);  // Full screen height
                
                ScreensaverParticleCanvas.Children.Add(particle);
                _screensaverParticles.Add(particle);
            }
        }
        
        private void ScreensaverParticleTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                // Clear old lines efficiently
                for (int i = _screensaverParticleLines.Count - 1; i >= 0; i--)
                {
                    ScreensaverParticleCanvas.Children.Remove(_screensaverParticleLines[i]);
                }
                _screensaverParticleLines.Clear();

                // Move particles
                foreach (var particle in _screensaverParticles)
                {
                    var velocity = (Point)particle.Tag;
                    var left = Canvas.GetLeft(particle) + velocity.X;
                    var top = Canvas.GetTop(particle) + velocity.Y;

                    // Wrap around edges
                    if (left < 0) left = ActualWidth;
                    if (left > ActualWidth) left = 0;
                    if (top < 0) top = ActualHeight;
                    if (top > ActualHeight) top = 0;

                    Canvas.SetLeft(particle, left);
                    Canvas.SetTop(particle, top);
                }

                // Draw connecting lines between nearby particles
                for (int i = 0; i < _screensaverParticles.Count; i++)
                {
                    var p1 = _screensaverParticles[i];
                    var x1 = Canvas.GetLeft(p1) + p1.Width / 2;
                    var y1 = Canvas.GetTop(p1) + p1.Height / 2;

                    for (int j = i + 1; j < _screensaverParticles.Count; j++)
                    {
                        var p2 = _screensaverParticles[j];
                        var x2 = Canvas.GetLeft(p2) + p2.Width / 2;
                        var y2 = Canvas.GetTop(p2) + p2.Height / 2;

                        var distance = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));

                        // Draw line if particles are close (within 200px for screensaver)
                        if (distance < 200)
                        {
                            var opacity = (byte)(100 * (1 - distance / 200)); // White connecting lines
                            var line = new System.Windows.Shapes.Line
                            {
                                X1 = x1,
                                Y1 = y1,
                                X2 = x2,
                                Y2 = y2,
                                Stroke = new SolidColorBrush(Color.FromArgb(
                                    opacity,
                                    255, 255, 255 // White color
                                )),
                                StrokeThickness = 1.5,
                                IsHitTestVisible = false
                            };
                            ScreensaverParticleCanvas.Children.Add(line);
                            _screensaverParticleLines.Add(line);
                        }
                    }
                }
            }
            catch
            {
                // Silently handle any particle rendering errors
            }
        }
        
        private void ScreensaverVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Loop the video
            ScreensaverVideo.Position = TimeSpan.Zero;
            ScreensaverVideo.Play();
        }
        
        private void ScreensaverVideo_MediaOpened(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"✅ Video loaded successfully");
            System.Diagnostics.Debug.WriteLine($"📹 Video dimensions: {ScreensaverVideo.NaturalVideoWidth}x{ScreensaverVideo.NaturalVideoHeight}");
            System.Diagnostics.Debug.WriteLine($"⏱️ Video duration: {ScreensaverVideo.NaturalDuration}");
            
            // Ensure video is visible
            ScreensaverVideo.Visibility = Visibility.Visible;
        }
        
        private void ScreensaverVideo_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Video failed to load: {e.ErrorException?.Message}");
            System.Diagnostics.Debug.WriteLine($"🔍 Current working directory: {Environment.CurrentDirectory}");
            System.Diagnostics.Debug.WriteLine($"📁 Looking for video file...");
            
            // Try to find the video file
            var videoPath = System.IO.Path.Combine(Environment.CurrentDirectory, "Media", "blotic-video-compressed.mp4");
            System.Diagnostics.Debug.WriteLine($"📍 Full video path: {videoPath}");
            System.Diagnostics.Debug.WriteLine($"📄 File exists: {System.IO.File.Exists(videoPath)}");
            
            // Hide video if it fails
            ScreensaverVideo.Visibility = Visibility.Collapsed;
        }
        
        #endregion

        private async Task LoadApplications()
        {
            AppsItemsControl.ItemsSource = null;

            await Task.Run(() =>
            {
                var allApps = _scanner.ScanInstalledApps();
                // Filter to only show games
                _allApps = allApps.Where(app => app.Category == "Games").ToList();
            });

            LoadFavorites();
            ShowHomePage(); // Start on Home page
        }


        private void AppCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border && border.Tag is string path)
            {
                LaunchApplication(path);
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is string path)
            {
                // Check if user is authenticated
                if (!Services.AuthService.Instance.IsAuthenticated)
                {
                    Controls.CustomAlert.Show(AlertContainer, "Login Required", 
                        "Please login to play games.", 
                        Controls.CustomAlert.AlertType.Info);
                    return;
                }

                // Check and decrement game count
                var canPlay = await CheckAndDecrementGameCount();
                if (canPlay)
                {
                    LaunchApplication(path);
                }
            }
        }

        private async Task<bool> CheckAndDecrementGameCount()
        {
            try
            {
                var user = Services.AuthService.Instance.CurrentUser;
                if (user == null) return false;

                var client = Services.SupabaseService.Instance.Client;
                if (client == null) return false;

                // Get paid registrations only
                var registrationResponse = await client
                    .From<EventRegistration>()
                    .Where(x => x.UserId == user.Id)
                    .Where(x => x.PaymentStatus == "paid")
                    .Get();

                if (registrationResponse?.Models == null || registrationResponse.Models.Count == 0)
                {
                    Controls.CustomAlert.Show(AlertContainer, "No Registration", 
                        "No active game registration found. Please register for an event.", 
                        Controls.CustomAlert.AlertType.Warning);
                    return false;
                }

                // Find a registration with games remaining
                var registration = registrationResponse.Models
                    .Where(r => r.GamesRemaining > 0)
                    .OrderByDescending(r => r.GamesRemaining)
                    .FirstOrDefault();

                // Check if any registration has games remaining
                if (registration == null)
                {
                    Controls.CustomAlert.Show(AlertContainer, "No Games Remaining", 
                        "You have no games remaining. Please purchase more games or register for another event.", 
                        Controls.CustomAlert.AlertType.Warning);
                    return false;
                }

                // Decrement games remaining
                var newCount = registration.GamesRemaining - 1;
                registration.GamesRemaining = newCount;
                
                await client
                    .From<EventRegistration>()
                    .Where(x => x.Id == registration.Id)
                    .Set(x => x.GamesRemaining, newCount)
                    .Update();

                System.Diagnostics.Debug.WriteLine($"✅ Game count decremented: {registration.GamesRemaining + 1} → {newCount}");

                // Update UI
                await UpdateGameCount(user.Id);
                await UpdateGamesStatistics(user.Id.ToString());
                
                // Update profile page if visible
                if (_currentPage == "Profile")
                {
                    GamesRemainingCount.Text = newCount.ToString();
                }

                // Show remaining games with toast
                ShowToast("Game Launched!", $"You have {newCount} game{(newCount != 1 ? "s" : "")} remaining.", Controls.ToastNotification.ToastType.Success);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error checking game count: {ex.Message}");
                Controls.CustomAlert.Show(AlertContainer, "Error", 
                    "Failed to verify game count. Please try again.", 
                    Controls.CustomAlert.AlertType.Error);
                return false;
            }
        }

        private void LaunchApplication(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Controls.CustomAlert.Show(AlertContainer, "Error", 
                    "Application path not found.", 
                    Controls.CustomAlert.AlertType.Warning);
                return;
            }

            try
            {
                // Handle protocol URLs (steam://, valorant://, etc.)
                if (path.Contains("://"))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    };
                    Process.Start(startInfo);
                    return;
                }

                // Handle shortcuts (.lnk files)
                if (path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    };
                    Process.Start(startInfo);
                }
                // Handle executables
                else if (path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && IOFile.Exists(path))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true,
                        WorkingDirectory = IOPath.GetDirectoryName(path)
                    };
                    Process.Start(startInfo);
                }
                // Handle directories - find and launch the main executable
                else if (IODirectory.Exists(path))
                {
                    var exePath = FindMainExecutable(path);
                    if (!string.IsNullOrWhiteSpace(exePath) && IOFile.Exists(exePath))
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = exePath,
                            UseShellExecute = true,
                            WorkingDirectory = IOPath.GetDirectoryName(exePath)
                        };
                        Process.Start(startInfo);
                    }
                    else
                    {
                        // Fallback: open directory if no executable found
                        Process.Start("explorer.exe", path);
                    }
                }
                // Try to open as is
                else
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    };
                    Process.Start(startInfo);
                }
            }
            catch (Exception ex)
            {
                Controls.CustomAlert.Show(AlertContainer, "Launch Error", 
                    $"Failed to launch application:\n{ex.Message}", 
                    Controls.CustomAlert.AlertType.Error);
            }
        }

        private string? FindMainExecutable(string directory)
        {
            if (!IODirectory.Exists(directory))
                return null;

            try
            {
                // Get directory name to find matching executable
                var dirName = IOPath.GetFileName(directory.TrimEnd(IOPath.DirectorySeparatorChar));
                
                // Look for executable with same name as directory (e.g., VLC folder -> vlc.exe)
                var exeFiles = IODirectory.GetFiles(directory, "*.exe", System.IO.SearchOption.TopDirectoryOnly);
                
                // First try: exact match with directory name
                var exactMatch = exeFiles.FirstOrDefault(f => 
                    IOPath.GetFileNameWithoutExtension(f).Equals(dirName, StringComparison.OrdinalIgnoreCase));
                if (exactMatch != null) return exactMatch;
                
                // Second try: contains directory name
                var containsMatch = exeFiles.FirstOrDefault(f => 
                    IOPath.GetFileNameWithoutExtension(f).Contains(dirName, StringComparison.OrdinalIgnoreCase));
                if (containsMatch != null) return containsMatch;
                
                // Third try: exclude common non-app executables
                var filtered = exeFiles.Where(f =>
                {
                    var name = IOPath.GetFileNameWithoutExtension(f).ToLower();
                    return !name.Contains("uninstall") &&
                           !name.Contains("setup") &&
                           !name.Contains("install") &&
                           !name.Contains("update") &&
                           !name.Contains("helper") &&
                           !name.Contains("crash") &&
                           !name.Contains("report");
                }).ToList();
                
                // Return first filtered executable
                if (filtered.Any()) return filtered.First();
                
                // Last resort: return first .exe found
                return exeFiles.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        // Window Controls
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeButton_Click(sender, e);
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized 
                ? WindowState.Normal 
                : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Navigation handlers
        private void HomeNav_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = "Home";
            UpdateNavigation();
            ShowHomePage();
        }

        private void MyGamesNav_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = "MyGames";
            UpdateNavigation();
            ShowMyGamesPage();
        }

        private void ProfileNav_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = "Profile";
            UpdateNavigation();
            ShowProfilePage();
        }

        private async void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // If already authenticated, show custom dropdown; otherwise show login
            if (Services.AuthService.Instance.IsAuthenticated)
            {
                ShowCustomProfileDropdown(sender as Button);
            }
            else
            {
                _currentPage = "Profile";
                UpdateNavigation();
                ShowProfilePage();
            }
        }

        private void ShowCustomProfileDropdown(Button targetButton)
        {
            // Remove any existing dropdown
            var existingDropdown = this.FindName("CustomProfileDropdown") as Grid;
            if (existingDropdown != null)
            {
                // Get the main grid (inside the border) and remove existing dropdown
                var contentBorder = this.Content as Border;
                var contentGrid = contentBorder?.Child as Grid;
                if (contentGrid != null && contentGrid.Children.Contains(existingDropdown))
                {
                    contentGrid.Children.Remove(existingDropdown);
                }
                return; // Toggle behavior - close if already open
            }

            // Create custom dropdown
            var dropdown = new Grid
            {
                Name = "CustomProfileDropdown",
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), // Transparent for click-through
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Background overlay to close dropdown when clicked
            var overlay = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), // Transparent
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            overlay.MouseLeftButtonDown += (s, e) => CloseCustomDropdown();
            dropdown.Children.Add(overlay);

            // Dropdown container
            var container = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 35, 50)),
                CornerRadius = new CornerRadius(12),
                BorderBrush = new SolidColorBrush(Color.FromRgb(70, 75, 90)),
                BorderThickness = new Thickness(1),
                MinWidth = 220,
                MaxWidth = 280,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 70, 20, 0), // Position below profile button
                Padding = new Thickness(0),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 270,
                    ShadowDepth = 8,
                    Opacity = 0.3,
                    BlurRadius = 15
                }
            };

            var stackPanel = new StackPanel();

            // User info header
            var user = Services.AuthService.Instance.CurrentUser;
            var userHeader = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(40, 45, 60)),
                Padding = new Thickness(20, 16, 20, 16),
                CornerRadius = new CornerRadius(12, 12, 0, 0)
            };

            var userInfo = new StackPanel { Orientation = Orientation.Horizontal };
            
            // User avatar
            var avatar = new Border
            {
                Width = 45,
                Height = 45,
                CornerRadius = new CornerRadius(22.5),
                Background = new SolidColorBrush(Color.FromRgb(79, 156, 249)),
                Margin = new Thickness(0, 0, 12, 0)
            };
            
            var avatarText = new TextBlock
            {
                Text = GetInitials(user?.Username ?? "U"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            avatar.Child = avatarText;
            
            // User details
            var userDetails = new StackPanel();
            var userName = new TextBlock
            {
                Text = user?.Username ?? "User",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 2)
            };
            
            var userEmail = new TextBlock
            {
                Text = user?.Email ?? "user@example.com",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175)),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            
            userDetails.Children.Add(userName);
            userDetails.Children.Add(userEmail);
            
            userInfo.Children.Add(avatar);
            userInfo.Children.Add(userDetails);
            userHeader.Child = userInfo;
            stackPanel.Children.Add(userHeader);

            // Games count section
            var gamesSection = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(35, 40, 55)),
                Padding = new Thickness(20, 12, 20, 12)
            };
            
            var gamesInfo = new StackPanel { Orientation = Orientation.Horizontal };
            
            var gamesIcon = new TextBlock
            {
                Text = "🎮",
                FontSize = 16,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            
            var gamesText = new TextBlock
            {
                Text = $"{user?.GamesRemaining ?? 0} games remaining",
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(79, 156, 249)),
                VerticalAlignment = VerticalAlignment.Center
            };
            
            gamesInfo.Children.Add(gamesIcon);
            gamesInfo.Children.Add(gamesText);
            gamesSection.Child = gamesInfo;
            stackPanel.Children.Add(gamesSection);

            // Separator
            var separator = new Rectangle
            {
                Height = 1,
                Fill = new SolidColorBrush(Color.FromRgb(60, 65, 80)),
                Margin = new Thickness(12, 0, 12, 0)
            };
            stackPanel.Children.Add(separator);

            // Menu items
            var menuItems = new StackPanel { Margin = new Thickness(0, 8, 0, 8) };

            // View Profile
            var profileBtn = CreateDropdownButton("👤", "View Profile", () =>
            {
                System.Diagnostics.Debug.WriteLine("🔽 View Profile button clicked");
                CloseCustomDropdown();
                
                // Use Dispatcher.BeginInvoke for UI operations
                Dispatcher.BeginInvoke(new Action(async () =>
                {
                    await Task.Delay(100);
                    _currentPage = "Profile";
                    UpdateNavigation();
                    ShowProfilePage();
                }));
            });
            menuItems.Children.Add(profileBtn);

            // Settings (placeholder)
            var settingsBtn = CreateDropdownButton("⚙️", "Settings", () =>
            {
                System.Diagnostics.Debug.WriteLine("🔽 Settings button clicked");
                CloseCustomDropdown();
                
                // Use Dispatcher.BeginInvoke for UI operations
                Dispatcher.BeginInvoke(new Action(async () =>
                {
                    await Task.Delay(100);
                    Controls.CustomAlert.Show(AlertContainer, "Coming Soon", 
                        "Settings feature will be available in a future update.", 
                        Controls.CustomAlert.AlertType.Info);
                }));
            });
            menuItems.Children.Add(settingsBtn);

            // Logout
            var logoutBtn = CreateDropdownButton("⏻", "Logout", () =>
            {
                System.Diagnostics.Debug.WriteLine("🔽 Logout button clicked");
                CloseCustomDropdown();
                
                // Use Dispatcher.BeginInvoke for UI operations
                Dispatcher.BeginInvoke(new Action(async () =>
                {
                    await Task.Delay(200);
                    await LogoutAsync();
                }));
            }, true);
            menuItems.Children.Add(logoutBtn);

            stackPanel.Children.Add(menuItems);
            container.Child = stackPanel;
            dropdown.Children.Add(container);

            // Add to main grid with lower z-index than alerts
            Panel.SetZIndex(dropdown, 2000);
            
            // Get the main grid (inside the border)
            var mainBorder = this.Content as Border;
            var mainGrid = mainBorder?.Child as Grid;
            if (mainGrid != null)
            {
                mainGrid.Children.Add(dropdown);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ Could not find main grid to add dropdown");
                return;
            }

            // Animate in
            dropdown.Opacity = 0;
            container.RenderTransform = new TranslateTransform(0, -10);
            
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            var slideIn = new DoubleAnimation(-10, 0, TimeSpan.FromMilliseconds(200));
            
            dropdown.BeginAnimation(OpacityProperty, fadeIn);
            container.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideIn);
        }

        private Button CreateDropdownButton(string icon, string text, Action onClick, bool isDestructive = false)
        {
            return CreateDropdownButtonInternal(icon, text, () => Task.Run(onClick), isDestructive);
        }

        private Button CreateDropdownButton(string icon, string text, Func<Task> onClickAsync, bool isDestructive = false)
        {
            return CreateDropdownButtonInternal(icon, text, onClickAsync, isDestructive);
        }

        private Button CreateDropdownButtonInternal(string icon, string text, Func<Task> onClickAsync, bool isDestructive = false)
        {
            var button = new Button
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(16, 12, 16, 12),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Cursor = Cursors.Hand
            };

            var content = new StackPanel { Orientation = Orientation.Horizontal };
            
            var iconText = new TextBlock
            {
                Text = icon,
                FontSize = 16,
                Margin = new Thickness(0, 0, 12, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            
            var textBlock = new TextBlock
            {
                Text = text,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Foreground = isDestructive ? new SolidColorBrush(Color.FromRgb(239, 68, 68)) : Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            content.Children.Add(iconText);
            content.Children.Add(textBlock);
            button.Content = content;

            // Hover effect
            button.MouseEnter += (s, e) =>
            {
                button.Background = new SolidColorBrush(isDestructive ? 
                    Color.FromRgb(127, 29, 29) : Color.FromRgb(45, 50, 65));
            };
            
            button.MouseLeave += (s, e) =>
            {
                button.Background = Brushes.Transparent;
            };

            button.Click += async (s, e) => await onClickAsync();
            
            return button;
        }

        private void CloseCustomDropdown()
        {
            // Ensure this runs on the UI thread
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => CloseCustomDropdown());
                return;
            }

            System.Diagnostics.Debug.WriteLine("🔽 CloseCustomDropdown called");
            var dropdown = this.FindName("CustomProfileDropdown") as Grid;
            if (dropdown != null)
            {
                System.Diagnostics.Debug.WriteLine("🔽 Dropdown found, starting fade out animation");
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
                fadeOut.Completed += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("🔽 Fade out completed, removing dropdown");
                    // Get the main grid (inside the border) and remove dropdown
                    var mainBorder = this.Content as Border;
                    var mainGrid = mainBorder?.Child as Grid;
                    if (mainGrid != null && mainGrid.Children.Contains(dropdown))
                    {
                        mainGrid.Children.Remove(dropdown);
                        System.Diagnostics.Debug.WriteLine("🔽 Dropdown removed successfully");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ Could not find main grid or dropdown not in children");
                    }
                };
                dropdown.BeginAnimation(OpacityProperty, fadeOut);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ Dropdown not found in CloseCustomDropdown");
            }
        }

        private void ShowToast(string title, string message, Controls.ToastNotification.ToastType type = Controls.ToastNotification.ToastType.Info, int durationMs = 3000)
        {
            var toast = new Controls.ToastNotification();
            toast.Margin = new Thickness(0, 0, 0, 10);
            ToastContainer.Children.Add(toast);
            toast.Show(title, message, type, durationMs);
        }

        private async Task ShowQRWithAnimation()
        {
            // Reset QR panel state before showing
            QRCodePanel.Opacity = 0;
            QRCodePanel.RenderTransform = null;
            QRCodePanel.Visibility = Visibility.Visible;

            // Fade in panel
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(600),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // Scale up panel
            var scaleUp = new DoubleAnimation
            {
                From = 0.8,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(600),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
            };

            QRCodePanel.BeginAnimation(OpacityProperty, fadeIn);
            QRScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
            QRScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);

            await Task.Delay(200);

            // Animate title
            var titleFade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(400)
            };
            QRTitleText.BeginAnimation(OpacityProperty, titleFade);

            await Task.Delay(150);

            // Animate QR code with rotation
            var qrFade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            var qrRotate = new DoubleAnimation
            {
                From = -10,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(800),
                EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 1, Springiness = 3 }
            };
            QRBorder.BeginAnimation(OpacityProperty, qrFade);
            QRRotateTransform.BeginAnimation(RotateTransform.AngleProperty, qrRotate);

            await Task.Delay(150);

            // Animate status text with pulse
            var statusFade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(400)
            };
            QRStatusText.BeginAnimation(OpacityProperty, statusFade);

            // Start continuous pulse animation on status text
            var pulse = new DoubleAnimation
            {
                From = 0.6,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(1500),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            QRStatusText.BeginAnimation(OpacityProperty, pulse);
        }

        private async Task HideQRWithAnimation()
        {
            // Fast scale down and fade out for smooth transition
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            var scaleDown = new DoubleAnimation
            {
                From = 1.0,
                To = 0.85,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            QRCodePanel.BeginAnimation(OpacityProperty, fadeOut);
            QRScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleDown);
            QRScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleDown);

            await Task.Delay(250);
            QRCodePanel.Visibility = Visibility.Collapsed;
        }

        private async Task LogoutAsync()
        {
            Controls.CustomAlert.Show(AlertContainer, "Logout", 
                "Are you sure you want to logout?", 
                Controls.CustomAlert.AlertType.Info, 
                showCancel: true,
                onOk: async (s, e) => {
                    await PerformLogout();
                },
                okText: "Yes",
                cancelText: "No");
        }

        private async Task PerformLogout()
        {
            System.Diagnostics.Debug.WriteLine("🔄 Starting logout process...");
            
            await Services.AuthService.Instance.LogoutAsync();
            
            // Clear all cached profile data
            ClearProfileCache();
            
            // Reset profile button UI
            ProfileButtonText.Text = "Login";
            ProfileButtonIcon.Visibility = Visibility.Visible;
            ProfileButtonAvatar.Visibility = Visibility.Collapsed;
            ProfileButtonImageBrush.ImageSource = null;
            ProfileButtonInitials.Visibility = Visibility.Visible;
            
            // Hide game count badge
            var template = ProfileNavButton.Template;
            if (template != null)
            {
                var badge = template.FindName("GameCountBadge", ProfileNavButton) as Border;
                if (badge != null)
                {
                    badge.Visibility = Visibility.Collapsed;
                }
            }
            
            // Reset welcome overlay state
            WelcomeOverlay.Visibility = Visibility.Collapsed;
            WelcomeOverlay.Opacity = 1;
            WelcomeText.Opacity = 0;
            UsernameText.Opacity = 0;
            UsernameText.Text = "";
            
            // Reset QR code panel state
            QRCodePanel.Opacity = 1;
            QRCodePanel.Visibility = Visibility.Collapsed;
            QRCodePanel.RenderTransform = null;
            
            ShowToast("Logged Out", "You have been logged out successfully.", Controls.ToastNotification.ToastType.Success);
            
            System.Diagnostics.Debug.WriteLine("✅ Logout completed, cache cleared");
            
            // Show QR code with animation
            await ShowQRCodeForLogin();
        }

        private void ClearProfileCache()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🧹 Clearing profile cache...");
                
                // Clear profile UI fields
                ProfileName.Text = "Demo";
                ProfileRole.Text = "Co-Head";
                ProfileEmail.Text = "kumardeepesh1911@gmail.com";
                ProfilePhone.Text = "5646456456";
                ProfileDepartment.Text = "ECE";
                ProfileYear.Text = "Year 2";
                
                // Clear profile picture
                ProfileAvatarBrush.ImageSource = null;
                ProfileInitials.Visibility = Visibility.Visible;
                ProfileInitials.Text = "U";
                
                // Clear games count
                GamesRemainingCount.Text = "0";
                TotalChancesCount.Text = "0";
                
                // Clear any cached images from memory
                if (ProfileAvatarBrush.ImageSource is BitmapImage bitmap)
                {
                    bitmap.UriSource = null;
                }
                
                // Force garbage collection to clear image cache
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                System.Diagnostics.Debug.WriteLine("✅ Profile cache cleared successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error clearing cache: {ex.Message}");
            }
        }

        private async void ProfileLogout_Click(object sender, RoutedEventArgs e)
        {
            await LogoutAsync();
        }

        private void UpdateNavigation()
        {
            // Update button styles using Tag for state
            if (_currentPage == "Home")
            {
                HomeNavButton.Background = (System.Windows.Media.Brush)FindResource("PrimaryBrush");
                HomeNavButton.Tag = "Active";
                MyGamesNavButton.Background = System.Windows.Media.Brushes.Transparent;
                MyGamesNavButton.Tag = "Inactive";
                ProfileNavButton.Background = System.Windows.Media.Brushes.Transparent;
                ProfileNavButton.Tag = "Inactive";
            }
            else if (_currentPage == "MyGames")
            {
                HomeNavButton.Background = System.Windows.Media.Brushes.Transparent;
                HomeNavButton.Tag = "Inactive";
                MyGamesNavButton.Background = (System.Windows.Media.Brush)FindResource("PrimaryBrush");
                MyGamesNavButton.Tag = "Active";
                ProfileNavButton.Background = System.Windows.Media.Brushes.Transparent;
                ProfileNavButton.Tag = "Inactive";
            }
            else if (_currentPage == "Profile")
            {
                HomeNavButton.Background = System.Windows.Media.Brushes.Transparent;
                HomeNavButton.Tag = "Inactive";
                MyGamesNavButton.Background = System.Windows.Media.Brushes.Transparent;
                MyGamesNavButton.Tag = "Inactive";
                ProfileNavButton.Background = (System.Windows.Media.Brush)FindResource("PrimaryBrush");
                ProfileNavButton.Tag = "Active";
            }
        }

        private void ShowHomePage()
        {
            PageTitle.Text = "HOME";
            GameCountHeader.Text = $"{_favoriteGames.Count} favorite game{(_favoriteGames.Count != 1 ? "s" : "")}";
            AppsItemsControl.ItemsSource = _favoriteGames;
            
            // Show apps grid, hide other panels
            AppsScrollViewer.Visibility = Visibility.Visible;
            QRCodePanel.Visibility = Visibility.Collapsed;
            ProfilePanel.Visibility = Visibility.Collapsed;
            
            // Delay to allow containers to be generated
            Dispatcher.InvokeAsync(() => 
            {
                HideAddToHomeButtons();
                HideAppTypeLabels(); // Hide "Shortcut" and "Installed App" labels on home page
            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void HideAddToHomeButtons()
        {
            for (int i = 0; i < AppsItemsControl.Items.Count; i++)
            {
                var container = AppsItemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (container != null)
                {
                    var button = FindVisualChild<System.Windows.Controls.Button>(container, "AddToHomeButton");
                    if (button != null)
                    {
                        button.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void HideAppTypeLabels()
        {
            for (int i = 0; i < AppsItemsControl.Items.Count; i++)
            {
                var container = AppsItemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (container != null)
                {
                    // Find the TextBlock that displays the app type (Shortcut, Installed App)
                    var typeTextBlocks = FindVisualChildren<TextBlock>(container);
                    foreach (var textBlock in typeTextBlocks)
                    {
                        // Check if this TextBlock is bound to the Type property
                        var binding = textBlock.GetBindingExpression(TextBlock.TextProperty);
                        if (binding?.ParentBinding?.Path?.Path == "Type")
                        {
                            textBlock.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    yield return typedChild;
                }

                foreach (var descendant in FindVisualChildren<T>(child))
                {
                    yield return descendant;
                }
            }
        }

        private void UpdateAddToHomeButtons()
        {
            for (int i = 0; i < AppsItemsControl.Items.Count; i++)
            {
                var container = AppsItemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (container != null)
                {
                    var button = FindVisualChild<System.Windows.Controls.Button>(container, "AddToHomeButton");
                    if (button != null && button.Tag is string gameName)
                    {
                        button.Visibility = Visibility.Visible;
                        
                        var isFavorite = _favoriteGames.Any(g => g.Name == gameName);
                        if (isFavorite)
                        {
                            button.Content = "✓ On Home";
                            button.Foreground = System.Windows.Media.Brushes.LimeGreen;
                        }
                        else
                        {
                            button.Content = "+ Add to Home";
                            button.Foreground = (System.Windows.Media.Brush)FindResource("AccentBrush");
                        }
                    }
                }
            }
        }

        private void ShowMyGamesPage()
        {
            PageTitle.Text = "MY GAMES";
            GameCountHeader.Text = $"{_allApps.Count} game{(_allApps.Count != 1 ? "s" : "")} found";
            AppsItemsControl.ItemsSource = _allApps;
            
            // Show apps grid, hide other panels
            AppsScrollViewer.Visibility = Visibility.Visible;
            QRCodePanel.Visibility = Visibility.Collapsed;
            ProfilePanel.Visibility = Visibility.Collapsed;
            
            // Delay to allow containers to be generated
            Dispatcher.InvokeAsync(() => UpdateAddToHomeButtons(), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private T? FindVisualChild<T>(DependencyObject parent, string? name = null) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is T typedChild)
                {
                    if (name == null || (child is FrameworkElement fe && fe.Name == name))
                        return typedChild;
                }

                var result = FindVisualChild<T>(child, name);
                if (result != null) return result;
            }

            return null;
        }

        private async void ShowProfilePage()
        {
            PageTitle.Text = "PROFILE";
            
            // Check if user is authenticated
            if (Services.AuthService.Instance.IsAuthenticated)
            {
                // Show user profile
                var user = Services.AuthService.Instance.CurrentUser;
                if (user != null)
                {
                    GameCountHeader.Text = $"Welcome, {user.Username}!";
                    
                    // Hide other panels, show profile panel
                    QRCodePanel.Visibility = Visibility.Collapsed;
                    AppsScrollViewer.Visibility = Visibility.Collapsed;
                    ProfilePanel.Visibility = Visibility.Visible;
                    
                    // Populate profile data
                    await PopulateProfileData(user);
                    
                    // Force refresh games statistics
                    await UpdateGamesStatistics(user.Id.ToString());
                    
                    System.Diagnostics.Debug.WriteLine($"🎮 Profile page: Games statistics refreshed");
                }
            }
            else
            {
                // Show QR code for login with animation
                await ShowQRCodeForLogin();
            }
        }

        private async Task ShowQRCodeForLogin()
        {
            GameCountHeader.Text = "Scan QR code to login";
            
            // Hide other panels
            AppsScrollViewer.Visibility = Visibility.Collapsed;
            ProfilePanel.Visibility = Visibility.Collapsed;
            
            // Generate QR code
            GenerateQRCode();
            
            // Show with animation
            await ShowQRWithAnimation();
        }

        private async Task PopulateProfileData(User user)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 Populating profile data for user: {user.Id}");
                
                // Fetch complete profile data from event_registrations and profiles
                var client = Services.SupabaseService.Instance.Client;
                if (client != null)
                {
                    // Get data from event_registrations (has most current info)
                    var registrationResponse = await client
                        .From<EventRegistration>()
                        .Where(x => x.UserId == user.Id)
                        .Where(x => x.PaymentStatus == "paid")
                        .Get();

                    // Get profile data
                    var profileResponse = await client
                        .From<Profile>()
                        .Where(x => x.Id == user.Id)
                        .Get();

                    // Initialize with default values
                    string fullName = user.Username ?? "User";
                    string email = user.Email ?? "No email";
                    string phone = user.PhoneNumber ?? "No phone";
                    string role = "student";
                    string department = "";
                    int? year = null;
                    string avatarUrl = null;

                    // Get profile data first (has role and avatar)
                    if (profileResponse?.Models != null && profileResponse.Models.Count > 0)
                    {
                        var profile = profileResponse.Models[0];
                        
                        role = profile.Role ?? "student";
                        avatarUrl = profile.AvatarUrl;
                        
                        // Use profile name if available
                        if (!string.IsNullOrEmpty(profile.FirstName))
                        {
                            fullName = $"{profile.FirstName} {profile.LastName}".Trim();
                        }
                        else if (!string.IsNullOrEmpty(profile.FullName))
                        {
                            fullName = profile.FullName;
                        }
                        
                        // Use profile contact info if available
                        email = profile.Email ?? email;
                        phone = profile.Phone ?? phone;
                        
                        System.Diagnostics.Debug.WriteLine($"👤 Profile data: Name={fullName}, Role={role}, Avatar={!string.IsNullOrEmpty(avatarUrl)}");
                    }

                    // Override with registration data if available (more current)
                    if (registrationResponse?.Models != null && registrationResponse.Models.Count > 0)
                    {
                        var registration = registrationResponse.Models[0];
                        
                        // Use registration data (prioritize over profile data)
                        fullName = registration.FullName ?? fullName;
                        email = registration.Email ?? email;
                        phone = registration.Phone ?? phone;
                        department = registration.Branch ?? department;
                        year = registration.Year ?? year;
                        
                        System.Diagnostics.Debug.WriteLine($"📋 Registration data: Name={fullName}, Email={email}, Dept={department}, Year={year}");
                    }

                    System.Diagnostics.Debug.WriteLine($"🎯 Final values: Name={fullName}, Role={role}, Dept={department}, Year={year}");

                    // Set all profile fields with actual backend data
                    ProfileName.Text = fullName;
                    ProfileEmail.Text = email;
                    ProfilePhone.Text = phone;
                    ProfileDepartment.Text = string.IsNullOrEmpty(department) ? "Department not set" : department;
                    ProfileYear.Text = year.HasValue ? $"Year {year}" : "Year not set";
                    
                    System.Diagnostics.Debug.WriteLine($"🔄 UI Updated: Name={ProfileName.Text}, Dept={ProfileDepartment.Text}, Year={ProfileYear.Text}");
                    
                    // Format role for display
                    ProfileRole.Text = FormatRole(role);
                    
                    // Set initials
                    var initials = GetInitials(fullName);
                    ProfileInitials.Text = initials;
                    
                    // Load profile picture if available
                    if (!string.IsNullOrEmpty(avatarUrl))
                    {
                        try
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(avatarUrl, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            
                            ProfileAvatarBrush.ImageSource = bitmap;
                            ProfileInitials.Visibility = Visibility.Collapsed;
                            
                            System.Diagnostics.Debug.WriteLine($"✅ Profile picture loaded from: {avatarUrl}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ Failed to load profile picture: {ex.Message}");
                            ProfileInitials.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        ProfileInitials.Visibility = Visibility.Visible;
                        ProfileAvatarBrush.ImageSource = null;
                    }
                    
                    // Fetch games data using completely rewritten logic
                    System.Diagnostics.Debug.WriteLine($"🎮 About to call UpdateGamesStatistics for user: {user.Id}");
                    await UpdateGamesStatistics(user.Id.ToString());
                    
                    // Force refresh AuthService games count
                    await Services.AuthService.Instance.SyncGameCountAsync();
                    
                    // Also ensure AuthService games count is reflected in UI
                    var authUser = Services.AuthService.Instance.CurrentUser;
                    if (authUser != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"🎮 AuthService reports {authUser.GamesRemaining} games remaining");
                        Dispatcher.Invoke(() =>
                        {
                            GamesRemainingCount.Text = authUser.GamesRemaining.ToString();
                            System.Diagnostics.Debug.WriteLine($"🔄 Updated UI from AuthService - Games: {GamesRemainingCount.Text}");
                        });
                    }
                    
                    // Force UI update on main thread
                    Dispatcher.Invoke(() =>
                    {
                        System.Diagnostics.Debug.WriteLine($"🔄 Current UI values - Games: {GamesRemainingCount.Text}, Total: {TotalChancesCount.Text}");
                    });
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ Profile populated successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error populating profile: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                
                // Set fallback values
                ProfileName.Text = user.Username ?? "User";
                ProfileEmail.Text = user.Email ?? "No email";
                ProfilePhone.Text = user.PhoneNumber ?? "No phone";
                ProfileRole.Text = "Student";
                ProfileDepartment.Text = "";
                ProfileYear.Text = "Year -";
                GamesRemainingCount.Text = "0";
                TotalChancesCount.Text = "0";
            }
        }

        private string FormatRole(string role)
        {
            if (string.IsNullOrEmpty(role))
                return "Student";
                
            // Convert role to display format
            switch (role.ToLower())
            {
                case "student":
                    return "Student";
                case "co-head":
                case "cohead":
                    return "Co-Head";
                case "head":
                    return "Head";
                case "admin":
                    return "Admin";
                case "faculty":
                    return "Faculty";
                default:
                    return role; // Return as-is for custom roles
            }
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "U";
                
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, 1).ToUpper();
            else
                return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        /// <summary>
        /// Completely rewritten method to fetch and update games statistics
        /// </summary>
        private async Task UpdateGamesStatistics(string userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🎮 [NEW] Fetching games statistics for user: {userId}");
                
                var client = Services.SupabaseService.Instance.Client;
                if (client == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [NEW] Supabase client is null");
                    GamesRemainingCount.Text = "0";
                    TotalChancesCount.Text = "0";
                    return;
                }

                // Step 1: Get all paid event registrations for the user
                System.Diagnostics.Debug.WriteLine($"📊 [NEW] Step 1: Fetching paid registrations...");
                
                var userGuid = Guid.Parse(userId);
                var registrationResponse = await client
                    .From<EventRegistration>()
                    .Where(x => x.UserId == userGuid)
                    .Where(x => x.PaymentStatus == "paid")
                    .Get();

                System.Diagnostics.Debug.WriteLine($"📊 [NEW] Found {registrationResponse?.Models?.Count ?? 0} paid registrations");

                if (registrationResponse?.Models == null || registrationResponse.Models.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ [NEW] No paid registrations found");
                    GamesRemainingCount.Text = "0";
                    TotalChancesCount.Text = "0";
                    return;
                }

                // Step 2: Calculate games remaining (sum from all paid registrations)
                var gamesRemaining = registrationResponse.Models.Sum(r => r.GamesRemaining);
                System.Diagnostics.Debug.WriteLine($"🎯 [NEW] Games Remaining: {gamesRemaining}");

                // Step 3: Get event details to calculate total chances
                System.Diagnostics.Debug.WriteLine($"📊 [NEW] Step 2: Fetching event details...");
                
                var eventIds = registrationResponse.Models.Select(r => r.EventId).ToList();
                System.Diagnostics.Debug.WriteLine($"📊 [NEW] Event IDs: {string.Join(", ", eventIds)}");

                var eventsResponse = await client
                    .From<Event>()
                    .Where(x => eventIds.Contains(x.Id))
                    .Get();

                System.Diagnostics.Debug.WriteLine($"📊 [NEW] Found {eventsResponse?.Models?.Count ?? 0} events");

                // Step 4: Calculate total chances (sum of number_of_games from all events)
                var totalChances = 0;
                if (eventsResponse?.Models != null)
                {
                    foreach (var evt in eventsResponse.Models)
                    {
                        var eventGames = evt.NumberOfGames ?? 0;
                        totalChances += eventGames;
                        System.Diagnostics.Debug.WriteLine($"📊 [NEW] Event '{evt.Title}': {eventGames} games");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"🎯 [NEW] Total Chances: {totalChances}");

                // Step 5: Update UI
                Dispatcher.Invoke(() =>
                {
                    GamesRemainingCount.Text = gamesRemaining.ToString();
                    TotalChancesCount.Text = totalChances.ToString();
                    
                    System.Diagnostics.Debug.WriteLine($"✅ [NEW] UI Updated - Games: {gamesRemaining}, Chances: {totalChances}");
                });

                // Step 6: Update AuthService user object
                if (Services.AuthService.Instance.CurrentUser != null)
                {
                    Services.AuthService.Instance.CurrentUser.GamesRemaining = gamesRemaining;
                    System.Diagnostics.Debug.WriteLine($"✅ [NEW] AuthService updated with {gamesRemaining} games");
                }

                // Debug: Print detailed registration info
                foreach (var reg in registrationResponse.Models)
                {
                    System.Diagnostics.Debug.WriteLine($"📋 [NEW] Registration: Event={reg.EventId}, Games={reg.GamesRemaining}, Status={reg.PaymentStatus}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [NEW] Error in UpdateGamesStatistics: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ [NEW] Stack trace: {ex.StackTrace}");
                
                Dispatcher.Invoke(() =>
                {
                    GamesRemainingCount.Text = "0";
                    TotalChancesCount.Text = "0";
                });
            }
        }

        private void GenerateQRCode_Click(object sender, RoutedEventArgs e)
        {
            GenerateQRCode();
        }

        private async void GenerateQRCode()
        {
            try
            {
                // Check if Supabase is initialized
                if (!Services.SupabaseService.Instance.IsInitialized)
                {
                    MessageBox.Show("Database connection not initialized. Please restart the application.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create QR session in Supabase
                var sessionId = await Services.AuthService.Instance.CreateQRSessionAsync();
                
                if (string.IsNullOrEmpty(sessionId))
                {
                    MessageBox.Show("Failed to create login session. Please check your internet connection and try again.\n\nIf the problem persists, check the debug output for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Generate login URL with session ID
                var loginUrl = $"https://blotic.com/login?session={sessionId}";
                
                // Generate QR code
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(loginUrl, QRCodeGenerator.ECCLevel.Q);
                    using (QRCode qrCode = new QRCode(qrCodeData))
                    {
                        using (DrawingBitmap qrBitmap = qrCode.GetGraphic(20, DrawingColor.Black, DrawingColor.White, true))
                        {
                            // Convert Bitmap to BitmapImage for WPF
                            using (MemoryStream memory = new MemoryStream())
                            {
                                qrBitmap.Save(memory, ImageFormat.Png);
                                memory.Position = 0;
                                
                                BitmapImage bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.StreamSource = memory;
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.EndInit();
                                bitmapImage.Freeze();
                                
                                QRCodeImage.Source = bitmapImage;
                            }
                        }
                    }
                }

                // Start polling for authentication
                Services.AuthService.Instance.StartPolling();
                System.Diagnostics.Debug.WriteLine($"🔄 Started polling for session: {sessionId}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating QR code: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is string gameName)
            {
                var game = _allApps.FirstOrDefault(g => g.Name == gameName);
                if (game != null)
                {
                    if (_favoriteGames.Any(g => g.Name == gameName))
                    {
                        // Remove from favorites
                        _favoriteGames.RemoveAll(g => g.Name == gameName);
                        button.Content = "+ Add to Home";
                        button.Foreground = (System.Windows.Media.Brush)FindResource("AccentBrush");
                    }
                    else
                    {
                        // Add to favorites
                        _favoriteGames.Add(game);
                        button.Content = "✓ On Home";
                        button.Foreground = System.Windows.Media.Brushes.LimeGreen;
                    }
                    SaveFavorites();
                    
                    // Refresh if on home page
                    if (_currentPage == "Home")
                    {
                        ShowHomePage();
                    }
                }
            }
        }

        private void LoadFavorites()
        {
            try
            {
                var favoritesPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BloticArena", "favorites.txt");
                if (IOFile.Exists(favoritesPath))
                {
                    var favoriteNames = IOFile.ReadAllLines(favoritesPath).ToList();
                    _favoriteGames = _allApps.Where(app => favoriteNames.Contains(app.Name)).ToList();
                }
            }
            catch { }
        }

        private void SaveFavorites()
        {
            try
            {
                var appDataPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BloticArena");
                IODirectory.CreateDirectory(appDataPath);
                var favoritesPath = IOPath.Combine(appDataPath, "favorites.txt");
                IOFile.WriteAllLines(favoritesPath, _favoriteGames.Select(g => g.Name));
            }
            catch { }
        }
    }
}
