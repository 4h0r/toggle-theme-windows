using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ToggleTheme
{
    public partial class App : System.Windows.Application
    {
        private NotifyIcon _trayIcon;
        private MainWindow _mainWindow;

        // Public property to access tray icon without reflection
        public NotifyIcon TrayIcon => _trayIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            Console.WriteLine("=== Application Starting ===");
            base.OnStartup(e);

            // Create the main window explicitly
            _mainWindow = new MainWindow();
            Console.WriteLine($"MainWindow created: {(_mainWindow != null ? "OK" : "NULL")}");

            // Create system tray icon
            _trayIcon = new NotifyIcon();
            _trayIcon.Visible = true;
            _trayIcon.Text = "Toggle Theme - Click to toggle Light/Dark mode";
            Console.WriteLine("Tray icon created and visible");
            
            // Set initial icon based on current theme
            UpdateTrayIcon();

            // Handle left-click to toggle theme
            _trayIcon.Click += (s, args) =>
            {
                Console.WriteLine($"Tray icon clicked! Button: {(args as MouseEventArgs)?.Button}");
                if (args is MouseEventArgs mouseArgs && mouseArgs.Button == MouseButtons.Left)
                {
                    Console.WriteLine("Left click detected, calling ToggleTheme()");
                    if (_mainWindow != null)
                    {
                        _mainWindow.ToggleTheme();
                        UpdateTrayIcon();
                    }
                    else
                    {
                        Console.WriteLine("ERROR: MainWindow is null!");
                    }
                }
            };

            // Create context menu for right-click
            var menu = new ContextMenuStrip();

            var toggleItem = new ToolStripMenuItem("Toggle Theme");
            toggleItem.Click += (s, a) =>
            {
                Console.WriteLine("Context menu 'Toggle Theme' clicked");
                if (_mainWindow != null)
                {
                    _mainWindow.ToggleTheme();
                    UpdateTrayIcon();
                }
                else
                {
                    Console.WriteLine("ERROR: MainWindow is null!");
                }
            };
            menu.Items.Add(toggleItem);

            menu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, a) => ExitApplication();
            menu.Items.Add(exitItem);

            _trayIcon.ContextMenuStrip = menu;
            Console.WriteLine("Application startup complete");
        }

        private void ExitApplication()
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            base.OnExit(e);
        }

        private System.Drawing.Icon LoadIconFromResource(string resourceName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fullResourceName = $"ToggleTheme.Resources.{resourceName}";
                
                using (Stream stream = assembly.GetManifestResourceStream(fullResourceName))
                {
                    if (stream != null)
                    {
                        return new System.Drawing.Icon(stream);
                    }
                    else
                    {
                        Console.WriteLine($"ERROR: Could not find resource: {fullResourceName}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR loading icon from resource: {ex.Message}");
                return null;
            }
        }

        public void UpdateTrayIcon()
        {
            try
            {
                // Read current theme: 1 = Light, 0 = Dark
                int currentTheme = GetCurrentTheme();
                
                // Light mode (1) = use dark icon, Dark mode (0) = use light icon
                string iconName = currentTheme == 1 ? "dark.ico" : "light.ico";
                Console.WriteLine($"Updating tray icon to: {iconName} (theme value: {currentTheme})");
                
                var newIcon = LoadIconFromResource(iconName);
                if (newIcon != null)
                {
                    _trayIcon.Icon = newIcon;
                    Console.WriteLine("Tray icon updated successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR updating tray icon: {ex.Message}");
            }
        }

        private int GetCurrentTheme()
        {
            const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            const string AppsUseLightThemeValue = "AppsUseLightTheme";
            
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
                {
                    if (key != null)
                    {
                        object value = key.GetValue(AppsUseLightThemeValue);
                        if (value != null)
                        {
                            return Convert.ToInt32(value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR reading theme: {ex.Message}");
            }
            
            return 1; // Default to light theme
        }
    }
}
