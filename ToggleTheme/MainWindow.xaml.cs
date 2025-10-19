using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;

namespace ToggleTheme
{
    public partial class MainWindow : Window
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string AppsUseLightThemeValue = "AppsUseLightTheme";
        private const string SystemUsesLightThemeValue = "SystemUsesLightTheme";

        public MainWindow()
        {
            InitializeComponent();
            SetStartup();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Window is hidden, just needed for WPF app structure
        }

        private void SetStartup()
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                rk.SetValue("ToggleTheme", Process.GetCurrentProcess().MainModule.FileName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to set startup: {ex.Message}");
            }
        }

        public void ToggleTheme()
        {
            try
            {
                // Read current theme state
                int currentTheme = GetCurrentTheme();
                
                // Toggle: 1 = Light, 0 = Dark
                int newTheme = currentTheme == 1 ? 0 : 1;
                
                // Apply new theme
                SetTheme(newTheme);

                // Show notification
                string themeName = newTheme == 1 ? "Light Mode" : "Dark Mode";
                ShowNotification($"Switched to {themeName}");
            }
            catch (Exception ex)
            {
                ShowNotification($"Error: {ex.Message}");
                Debug.WriteLine($"Theme toggle error: {ex}");
            }
        }

        private int GetCurrentTheme()
        {
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
                Debug.WriteLine($"Failed to read theme: {ex.Message}");
            }
            
            // Default to light theme if unable to read
            return 1;
        }

        private void SetTheme(int themeValue)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (key != null)
                    {
                        // Set both app and system theme
                        key.SetValue(AppsUseLightThemeValue, themeValue, RegistryValueKind.DWord);
                        key.SetValue(SystemUsesLightThemeValue, themeValue, RegistryValueKind.DWord);
                    }
                    else
                    {
                        throw new Exception("Unable to access theme registry key");
                    }
                }

                // Notify Windows of the theme change
                NotifyThemeChange();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to set theme: {ex.Message}");
                throw;
            }
        }

        private void NotifyThemeChange()
        {
            // Broadcast WM_SETTINGCHANGE to notify applications of theme change
            const int HWND_BROADCAST = 0xffff;
            const uint WM_SETTINGCHANGE = 0x001A;
            
            NativeMethods.SendNotifyMessage(
                (IntPtr)HWND_BROADCAST,
                WM_SETTINGCHANGE,
                IntPtr.Zero,
                "ImmersiveColorSet");
        }

        private void ShowNotification(string message)
        {
            try
            {
                var app = System.Windows.Application.Current as App;
                if (app != null)
                {
                    // Access the tray icon through reflection since it's private
                    var trayIconField = app.GetType().GetField("_trayIcon", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (trayIconField != null)
                    {
                        var trayIcon = trayIconField.GetValue(app) as System.Windows.Forms.NotifyIcon;
                        if (trayIcon != null)
                        {
                            trayIcon.ShowBalloonTip(2000, "Toggle Theme", message, 
                                System.Windows.Forms.ToolTipIcon.Info);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to show notification: {ex.Message}");
            }
        }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        internal static extern IntPtr SendNotifyMessage(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            string lParam);
    }
}
