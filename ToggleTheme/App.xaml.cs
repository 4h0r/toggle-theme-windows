using System;
using System.Windows;
using System.Windows.Forms;

namespace ToggleTheme
{
    public partial class App : System.Windows.Application
    {
        private NotifyIcon _trayIcon;
        private MainWindow _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            Console.WriteLine("=== Application Starting ===");
            base.OnStartup(e);

            // Create the main window explicitly
            _mainWindow = new MainWindow();
            Console.WriteLine($"MainWindow created: {(_mainWindow != null ? "OK" : "NULL")}");

            // Create system tray icon
            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            _trayIcon.Visible = true;
            _trayIcon.Text = "Toggle Theme - Click to toggle Light/Dark mode";
            Console.WriteLine("Tray icon created and visible");

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
    }
}
