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
            // First, remove any existing trace listeners that might be writing to the desktop
            var desktopListeners = System.Diagnostics.Trace.Listeners
                .OfType<System.Diagnostics.TextWriterTraceListener>()
                .Where(l => l.Writer != null && l.Writer.ToString().Contains("Desktop"))
                .ToList();

            foreach (var listener in desktopListeners)
            {
                System.Diagnostics.Trace.Listeners.Remove(listener);
                listener.Close();
            }

            // Add Console trace listener for debug output
            if (!System.Diagnostics.Trace.Listeners.OfType<System.Diagnostics.ConsoleTraceListener>().Any())
            {
                System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());
            }
            
            // Set up file trace listener that writes to application directory
            string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BloticArena_Debug.txt");
            
            // Remove any existing file listener to prevent duplicates
            var existingFileListeners = System.Diagnostics.Trace.Listeners
                .OfType<System.Diagnostics.TextWriterTraceListener>()
                .Where(l => l.Writer != null && l.Writer.ToString().Contains("BloticArena_Debug.txt"))
                .ToList();

            foreach (var listener in existingFileListeners)
            {
                System.Diagnostics.Trace.Listeners.Remove(listener);
                listener.Close();
            }

            // Add our new file listener
            try
            {
                var fileListener = new System.Diagnostics.TextWriterTraceListener(logPath);
                System.Diagnostics.Trace.Listeners.Add(fileListener);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create file trace listener: {ex.Message}");
            }
            
            // Add global exception handlers
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            
            // Force flush the trace to ensure it's written to the file
            System.Diagnostics.Trace.Flush();
            
            Console.WriteLine("=== Blotic Arena Starting ===");
            System.Diagnostics.Debug.WriteLine($"Debug log file location: {logPath}");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var ex = args.ExceptionObject as Exception;
            // Log to debug output instead of file
            System.Diagnostics.Debug.WriteLine($"Unhandled Exception: {ex?.Message}\n{ex?.StackTrace}");
            MessageBox.Show($"An unexpected error occurred.\n\n{ex?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            // Log to debug output instead of file
            System.Diagnostics.Debug.WriteLine($"Dispatcher Unhandled Exception: {args.Exception.Message}\n{args.Exception.StackTrace}");
            MessageBox.Show($"An unexpected error occurred.\n\n{args.Exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        }
    }
}
