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
            base.OnStartup(e);

            // Get the main window reference
            _mainWindow = Current.MainWindow as MainWindow;

            // Create system tray icon
            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            _trayIcon.Visible = true;
            _trayIcon.Text = "Toggle Theme - Click to toggle Light/Dark mode";

            // Handle left-click to toggle theme
            _trayIcon.Click += (s, args) =>
            {
                if (args is MouseEventArgs mouseArgs && mouseArgs.Button == MouseButtons.Left)
                {
                    _mainWindow?.ToggleTheme();
                }
            };

            // Create context menu for right-click
            var menu = new ContextMenuStrip();

            var toggleItem = new ToolStripMenuItem("Toggle Theme");
            toggleItem.Click += (s, a) => _mainWindow?.ToggleTheme();
            menu.Items.Add(toggleItem);

            menu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, a) => ExitApplication();
            menu.Items.Add(exitItem);

            _trayIcon.ContextMenuStrip = menu;
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
