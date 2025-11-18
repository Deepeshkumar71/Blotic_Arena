using System;
using System.Windows;
using System.Windows.Threading;
using System.Configuration;
using System.Data;
using System.IO;

namespace BloticArena
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Redirect Debug output to Console for visibility
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());
            
            // Add global exception handlers
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            
            Console.WriteLine("=== Blotic Arena Starting ===");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var ex = args.ExceptionObject as Exception;
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BloticArena_Error.txt");
            File.WriteAllText(logPath, $"Error: {ex?.Message}\n\nStack Trace:\n{ex?.StackTrace}");
            MessageBox.Show($"Application error. Log saved to Desktop.\n\n{ex?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BloticArena_Error.txt");
            File.WriteAllText(logPath, $"Error: {args.Exception.Message}\n\nStack Trace:\n{args.Exception.StackTrace}");
            MessageBox.Show($"Application error. Log saved to Desktop.\n\n{args.Exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        }
    }
}
