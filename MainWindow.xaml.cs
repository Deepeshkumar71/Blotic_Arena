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
        private readonly List<Ellipse> _particles = new();
        private readonly List<System.Windows.Shapes.Line> _particleLines = new();
        private readonly Random _random = new();
        private readonly DispatcherTimer _particleTimer;
        private string _currentPage = "Home";

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            
            // Initialize particle animation
            _particleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20) // Even faster update
            };
            _particleTimer.Tick += ParticleTimer_Tick;
            _particleTimer.Start();
            
            CreateParticles();
            LoadFavorites();
            InitializeSupabase();
            LoadApplications();
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
                MessageBox.Show($"Authentication failed: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        private void OnSessionExpired(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("⏰ QR session expired");
                MessageBox.Show("QR code expired. Please generate a new one.", "Session Expired", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                await UpdateGameCount(user.Id);
                
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
                        .Get();
                    
                    if (registrationResponse?.Models != null && registrationResponse.Models.Count > 0)
                    {
                        // Sum all games from all registrations
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

        private void CreateParticles()
        {
            // Create particles with pitch white color
            for (int i = 0; i < 100; i++)
            {
                var size = _random.Next(3, 6); // Bigger particles
                var particle = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Fill = new SolidColorBrush(Color.FromArgb(
                        255,  // Full opacity - pitch white
                        255,  // Full white
                        255,  // Full white  
                        255   // Full white
                    )),
                    Tag = new Point(_random.NextDouble() * 5.1 - 2.55, _random.NextDouble() * 5.1 - 2.55) // 70% faster: 3 * 1.7 = 5.1
                };

                Canvas.SetLeft(particle, _random.Next(0, (int)Width));
                Canvas.SetTop(particle, _random.Next(0, (int)Height));
                
                ParticleCanvas.Children.Add(particle);
                _particles.Add(particle);
            }
        }

        private void ParticleTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                // Clear old lines efficiently
                for (int i = _particleLines.Count - 1; i >= 0; i--)
                {
                    ParticleCanvas.Children.Remove(_particleLines[i]);
                }
                _particleLines.Clear();

                // Move particles
                foreach (var particle in _particles)
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

                // Draw lines between nearby particles (website style)
                for (int i = 0; i < _particles.Count; i++)
                {
                    var p1 = _particles[i];
                    var x1 = Canvas.GetLeft(p1) + p1.Width / 2;
                    var y1 = Canvas.GetTop(p1) + p1.Height / 2;

                    for (int j = i + 1; j < _particles.Count; j++)
                    {
                        var p2 = _particles[j];
                        var x2 = Canvas.GetLeft(p2) + p2.Width / 2;
                        var y2 = Canvas.GetTop(p2) + p2.Height / 2;

                        var distance = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));

                        // Draw line if particles are close (within 180px for more connections)
                        if (distance < 180)
                        {
                            var opacity = (byte)(100 * (1 - distance / 180)); // More visible lines
                            var line = new System.Windows.Shapes.Line
                            {
                                X1 = x1,
                                Y1 = y1,
                                X2 = x2,
                                Y2 = y2,
                                Stroke = new SolidColorBrush(Color.FromArgb(
                                    opacity,
                                    255, 255, 255 // Bright white
                                )),
                                StrokeThickness = 1.0, // Thicker lines
                                IsHitTestVisible = false // Improve performance
                            };
                            ParticleCanvas.Children.Add(line);
                            _particleLines.Add(line);
                        }
                    }
                }
            }
            catch
            {
                // Silently handle any particle rendering errors
            }
        }

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
                    MessageBox.Show("Please login to play games.", "Login Required", MessageBoxButton.OK, MessageBoxImage.Information);
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

                // Get all registrations
                var registrationResponse = await client
                    .From<EventRegistration>()
                    .Where(x => x.UserId == user.Id)
                    .Get();

                if (registrationResponse?.Models == null || registrationResponse.Models.Count == 0)
                {
                    MessageBox.Show("No active game registration found. Please register for an event.", "No Registration", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    MessageBox.Show("You have no games remaining. Please purchase more games or register for another event.", "No Games Remaining", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Failed to verify game count. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void LaunchApplication(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Application path not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show($"Failed to launch application:\n{ex.Message}", 
                    "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            // If already authenticated, show profile; otherwise show login
            if (Services.AuthService.Instance.IsAuthenticated)
            {
                // Show modern context menu with options
                var contextMenu = new System.Windows.Controls.ContextMenu
                {
                    Background = new SolidColorBrush(Color.FromArgb(240, 20, 24, 36)),
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(8),
                    HasDropShadow = true
                };
                
                // Add blur effect
                contextMenu.Effect = new System.Windows.Media.Effects.BlurEffect
                {
                    Radius = 0,
                    KernelType = System.Windows.Media.Effects.KernelType.Gaussian
                };
                
                // View Profile Item
                var profileItem = new System.Windows.Controls.MenuItem 
                { 
                    Header = "👤   View Profile",
                    Foreground = Brushes.White,
                    Background = new SolidColorBrush(Color.FromRgb(20, 24, 36)),
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    FontSize = 14,
                    FontWeight = FontWeights.Medium,
                    Padding = new Thickness(16, 12, 16, 12),
                    Height = 44
                };
                
                // Style for profile item
                var profileItemStyle = new Style(typeof(System.Windows.Controls.MenuItem));
                profileItemStyle.Setters.Add(new Setter(System.Windows.Controls.MenuItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(20, 24, 36))));
                profileItemStyle.Setters.Add(new Setter(System.Windows.Controls.MenuItem.BorderBrushProperty, Brushes.Transparent));
                profileItemStyle.Setters.Add(new Setter(System.Windows.Controls.MenuItem.BorderThicknessProperty, new Thickness(0)));
                profileItemStyle.Triggers.Add(new Trigger 
                { 
                    Property = System.Windows.Controls.MenuItem.IsHighlightedProperty, 
                    Value = true,
                    Setters = { 
                        new Setter(System.Windows.Controls.MenuItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(45, 50, 65))),
                        new Setter(System.Windows.Controls.MenuItem.BorderBrushProperty, Brushes.Transparent)
                    }
                });
                profileItem.Style = profileItemStyle;
                
                profileItem.Click += (s, args) =>
                {
                    _currentPage = "Profile";
                    UpdateNavigation();
                    ShowProfilePage();
                };
                
                // Logout Item with power icon
                var logoutItem = new System.Windows.Controls.MenuItem 
                { 
                    Header = "⏻   Logout",
                    Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                    Background = new SolidColorBrush(Color.FromRgb(20, 24, 36)),
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    FontSize = 14,
                    FontWeight = FontWeights.Medium,
                    Padding = new Thickness(16, 12, 16, 12),
                    Height = 44
                };
                
                // Style for logout item
                var logoutItemStyle = new Style(typeof(System.Windows.Controls.MenuItem));
                logoutItemStyle.Setters.Add(new Setter(System.Windows.Controls.MenuItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(20, 24, 36))));
                logoutItemStyle.Setters.Add(new Setter(System.Windows.Controls.MenuItem.BorderBrushProperty, Brushes.Transparent));
                logoutItemStyle.Setters.Add(new Setter(System.Windows.Controls.MenuItem.BorderThicknessProperty, new Thickness(0)));
                var logoutTrigger = new Trigger 
                { 
                    Property = System.Windows.Controls.MenuItem.IsHighlightedProperty, 
                    Value = true
                };
                logoutTrigger.Setters.Add(new Setter(System.Windows.Controls.MenuItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(127, 29, 29))));
                logoutTrigger.Setters.Add(new Setter(System.Windows.Controls.MenuItem.ForegroundProperty, Brushes.White));
                logoutTrigger.Setters.Add(new Setter(System.Windows.Controls.MenuItem.BorderBrushProperty, Brushes.Transparent));
                logoutItemStyle.Triggers.Add(logoutTrigger);
                logoutItem.Style = logoutItemStyle;
                
                logoutItem.Click += async (s, args) => await LogoutAsync();
                
                contextMenu.Items.Add(profileItem);
                contextMenu.Items.Add(new System.Windows.Controls.Separator 
                { 
                    Background = new SolidColorBrush(Color.FromRgb(60, 65, 80)),
                    Margin = new Thickness(8, 4, 8, 4),
                    Height = 1
                });
                contextMenu.Items.Add(logoutItem);
                
                contextMenu.PlacementTarget = sender as System.Windows.Controls.Button;
                contextMenu.IsOpen = true;
            }
            else
            {
                _currentPage = "Profile";
                UpdateNavigation();
                ShowProfilePage();
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
            var result = MessageBox.Show("Are you sure you want to logout?", "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                await Services.AuthService.Instance.LogoutAsync();
                
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
                
                // Show QR code with animation
                await ShowQRCodeForLogin();
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
            Dispatcher.InvokeAsync(() => HideAddToHomeButtons(), System.Windows.Threading.DispatcherPriority.Loaded);
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
                // Set basic user info
                ProfileName.Text = user.Username ?? "User";
                ProfileEmail.Text = user.Email ?? "No email";
                ProfilePhone.Text = user.PhoneNumber ?? "No phone";
                
                // Set initials
                var initials = GetInitials(user.Username ?? user.Email ?? "U");
                ProfileInitials.Text = initials;
                
                // Fetch profile from database to get role and avatar
                var client = Services.SupabaseService.Instance.Client;
                if (client != null)
                {
                    var profileResponse = await client
                        .From<Profile>()
                        .Where(x => x.Id == user.Id)
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
                                
                                ProfileAvatarBrush.ImageSource = bitmap;
                                ProfileInitials.Visibility = Visibility.Collapsed;
                                
                                System.Diagnostics.Debug.WriteLine($"✅ Profile picture loaded from: {profile.AvatarUrl}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"⚠️ Failed to load profile picture: {ex.Message}");
                                // Keep showing initials on error
                            }
                        }
                    }
                    
                    // Fetch event registrations to get games remaining
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"📊 Fetching registrations for user: {user.Id}");
                        
                        var registrationResponse = await client
                            .From<EventRegistration>()
                            .Where(x => x.UserId == user.Id)
                            .Get();
                        
                        System.Diagnostics.Debug.WriteLine($"📊 Query returned {registrationResponse?.Models?.Count ?? 0} registrations");
                        
                        if (registrationResponse?.Models != null && registrationResponse.Models.Count > 0)
                        {
                            // Sum up all games remaining from all registrations
                            var totalGames = registrationResponse.Models.Sum(r => r.GamesRemaining);
                            GamesRemainingCount.Text = totalGames.ToString();
                            System.Diagnostics.Debug.WriteLine($"✅ Total games remaining across {registrationResponse.Models.Count} registrations: {totalGames}");
                            
                            foreach (var reg in registrationResponse.Models)
                            {
                                System.Diagnostics.Debug.WriteLine($"  - Event: {reg.EventId}, Games: {reg.GamesRemaining}");
                            }
                        }
                        else
                        {
                            GamesRemainingCount.Text = "0";
                            System.Diagnostics.Debug.WriteLine($"⚠️ No registrations found for user: {user.Id}");
                        }
                    }
                    catch (Exception regEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Error fetching games: {regEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {regEx.StackTrace}");
                        GamesRemainingCount.Text = "0";
                    }
                    
                    // TODO: Fetch total played from game_history
                    TotalPlayedCount.Text = "0";
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ Profile populated for: {user.Username}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error populating profile: {ex.Message}");
            }
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "U";
            
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "U";
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
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
