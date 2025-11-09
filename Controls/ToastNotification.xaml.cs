using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BloticArena.Controls
{
    public partial class ToastNotification : UserControl
    {
        public enum ToastType
        {
            Success,
            Error,
            Warning,
            Info
        }

        public ToastNotification()
        {
            InitializeComponent();
        }

        public void Show(string title, string message, ToastType type = ToastType.Info, int durationMs = 3000)
        {
            TitleText.Text = title;
            MessageText.Text = message;

            // Set colors based on type
            switch (type)
            {
                case ToastType.Success:
                    IconBorder.Background = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Green
                    IconText.Text = "✓";
                    break;
                case ToastType.Error:
                    IconBorder.Background = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
                    IconText.Text = "✕";
                    break;
                case ToastType.Warning:
                    IconBorder.Background = new SolidColorBrush(Color.FromRgb(245, 158, 11)); // Orange
                    IconText.Text = "⚠";
                    break;
                case ToastType.Info:
                default:
                    IconBorder.Background = new SolidColorBrush(Color.FromRgb(59, 130, 246)); // Blue
                    IconText.Text = "ℹ";
                    break;
            }

            // Fade in animation
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // Slide in from right
            var slideIn = new ThicknessAnimation
            {
                From = new Thickness(400, 0, -400, 0),
                To = new Thickness(0),
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            ToastBorder.BeginAnimation(OpacityProperty, fadeIn);
            this.BeginAnimation(MarginProperty, slideIn);

            // Auto-hide after duration
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(durationMs)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                Hide();
            };
            timer.Start();
        }

        public void Hide()
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            fadeOut.Completed += (s, e) =>
            {
                if (Parent is Panel panel)
                {
                    panel.Children.Remove(this);
                }
            };

            ToastBorder.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
