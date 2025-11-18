using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BloticArena
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Screensaver related fields
        private DispatcherTimer _inactivityTimer;
        private Point _lastMousePosition;
        private bool _isScreensaverActive = false;
        private const int SCREENSAVER_TIMEOUT_SECONDS = 30; // 30 seconds

        public MainWindow()
        {
            InitializeComponent();
            InitializeScreensaver();
        }

        private void InitializeScreensaver()
        {
            // Set up inactivity timer
            _inactivityTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(SCREENSAVER_TIMEOUT_SECONDS)
            };
            _inactivityTimer.Tick += InactivityTimer_Tick;
            ResetInactivityTimer();

            // Track mouse movement
            this.MouseMove += OnMouseMove;
            this.MouseDown += OnMouseDown;
            this.MouseWheel += OnMouseWheel;
            this.KeyDown += OnKeyDown;

            // Set up screensaver video
            ScreensaverVideo.MediaEnded += ScreensaverVideo_MediaEnded;
            ScreensaverVideo.MediaOpened += ScreensaverVideo_MediaOpened;
            ScreensaverVideo.MediaFailed += ScreensaverVideo_MediaFailed;
        }

        private void ResetInactivityTimer()
        {
            if (_isScreensaverActive) return;
            _inactivityTimer.Stop();
            _inactivityTimer.Start();
        }

        private void InactivityTimer_Tick(object sender, EventArgs e)
        {
            if (!_isScreensaverActive)
            {
                StartScreensaver();
            }
        }

        private void StartScreensaver()
        {
            _isScreensaverActive = true;
            _inactivityTimer.Stop();
            ScreensaverOverlay.Visibility = Visibility.Visible;
            ScreensaverVideo.Position = TimeSpan.Zero;
            ScreensaverVideo.Play();
            
            // Start particle animation if needed
            StartParticleAnimation();
        }

        private void StopScreensaver()
        {
            _isScreensaverActive = false;
            ScreensaverOverlay.Visibility = Visibility.Collapsed;
            ScreensaverVideo.Stop();
            ResetInactivityTimer();
            
            // Stop particle animation if needed
            StopParticleAnimation();
        }

        private void StartParticleAnimation()
        {
            // Add your particle animation logic here
            // This is a placeholder for the particle effect
            // You can implement your own particle system or use an existing one
        }

        private void StopParticleAnimation()
        {
            // Stop and clear particle animation
            ScreensaverParticleCanvas.Children.Clear();
        }

        #region Event Handlers

        private void ScreensaverVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Loop the video
            ScreensaverVideo.Position = TimeSpan.Zero;
            ScreensaverVideo.Play();
        }

        private void ScreensaverVideo_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Video opened successfully
        }

        private void ScreensaverVideo_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // Handle video load failure
            Console.WriteLine($"Failed to load video: {e.ErrorException?.Message}");
        }

        #endregion

        #region Input Event Handlers

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var currentPosition = e.GetPosition(this);
            
            if (_isScreensaverActive)
            {
                // If mouse moves while screensaver is active, deactivate it
                StopScreensaver();
            }
            else
            {
                // Update last mouse position and reset timer
                _lastMousePosition = currentPosition;
                ResetInactivityTimer();
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isScreensaverActive)
            {
                StopScreensaver();
                e.Handled = true;
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_isScreensaverActive)
            {
                StopScreensaver();
                e.Handled = true;
            }
            else
            {
                ResetInactivityTimer();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_isScreensaverActive)
            {
                StopScreensaver();
                e.Handled = true;
            }
            else
            {
                ResetInactivityTimer();
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        // Add other existing event handlers and methods from your code-behind
        private void HomeVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Loop the home video
            HomeVideo.Position = TimeSpan.Zero;
            HomeVideo.Play();
        }
        
        private void HomeNav_Click(object sender, RoutedEventArgs e) { /* Implementation */ }
        private void MyGamesNav_Click(object sender, RoutedEventArgs e) { /* Implementation */ }
        private void ProfileNav_Click(object sender, RoutedEventArgs e) { /* Implementation */ }
        private void CloseButton_Click(object sender, RoutedEventArgs e) { /* Implementation */ }
        private void ProfileLogout_Click(object sender, RoutedEventArgs e) { /* Implementation */ }
        private void FavoriteButton_Click(object sender, RoutedEventArgs e) { /* Implementation */ }
    }
}
