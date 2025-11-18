using System;
using System.Windows;
using System.Windows.Controls;

namespace BloticArena.Controls
{
    public partial class CustomAlert : UserControl
    {
        public event EventHandler? OkClicked;
        public event EventHandler? CancelClicked;

        public CustomAlert()
        {
            InitializeComponent();
        }

        public enum AlertType
        {
            Info,
            Warning,
            Error,
            Success
        }

        public void ShowAlert(string title, string message, AlertType type = AlertType.Info, bool showCancel = false, string okText = "OK", string cancelText = "Cancel")
        {
            AlertTitle.Text = title;
            AlertMessage.Text = message;
            
            // Set button text
            OkButton.Content = okText;
            CancelButton.Content = cancelText;
            
            // Set icon and colors based on type
            switch (type)
            {
                case AlertType.Info:
                    AlertIcon.Text = "ℹ";
                    ((Border)AlertIcon.Parent).Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(59, 130, 246)); // Blue
                    break;
                case AlertType.Warning:
                    AlertIcon.Text = "⚠";
                    ((Border)AlertIcon.Parent).Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 158, 11)); // Yellow
                    break;
                case AlertType.Error:
                    AlertIcon.Text = "✕";
                    ((Border)AlertIcon.Parent).Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(239, 68, 68)); // Red
                    break;
                case AlertType.Success:
                    AlertIcon.Text = "✓";
                    ((Border)AlertIcon.Parent).Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 197, 94)); // Green
                    break;
            }

            // Show/hide cancel button
            CancelButton.Visibility = showCancel ? Visibility.Visible : Visibility.Collapsed;
            
            // Show the alert
            Visibility = Visibility.Visible;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            OkClicked?.Invoke(this, EventArgs.Empty);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            CancelClicked?.Invoke(this, EventArgs.Empty);
        }

        public static void Show(Panel parent, string title, string message, AlertType type = AlertType.Info, bool showCancel = false, EventHandler? onOk = null, EventHandler? onCancel = null, string okText = "OK", string cancelText = "Cancel")
        {
            var alert = new CustomAlert();
            
            if (onOk != null)
                alert.OkClicked += onOk;
            if (onCancel != null)
                alert.CancelClicked += onCancel;
            
            // Remove any existing alerts
            for (int i = parent.Children.Count - 1; i >= 0; i--)
            {
                if (parent.Children[i] is CustomAlert)
                {
                    parent.Children.RemoveAt(i);
                }
            }
            
            // Add new alert
            parent.Children.Add(alert);
            Panel.SetZIndex(alert, 1000);
            
            alert.ShowAlert(title, message, type, showCancel, okText, cancelText);
        }
    }
}
