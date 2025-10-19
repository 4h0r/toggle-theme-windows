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
            Console.WriteLine("=== ToggleTheme() called ===");
            try
            {
                // Read current theme state
                int currentTheme = GetCurrentTheme();
                Console.WriteLine($"Current theme: {currentTheme} ({(currentTheme == 1 ? "Light" : "Dark")})");
                
                // Toggle: 1 = Light, 0 = Dark
                int newTheme = currentTheme == 1 ? 0 : 1;
                Console.WriteLine($"Switching to: {newTheme} ({(newTheme == 1 ? "Light" : "Dark")})");
                
                // Apply new theme
                SetTheme(newTheme);

                // Show notification
                string themeName = newTheme == 1 ? "Light Mode" : "Dark Mode";
                ShowNotification($"Switched to {themeName}");
                Console.WriteLine($"Theme toggle completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in ToggleTheme: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ShowNotification($"Error: {ex.Message}");
                Debug.WriteLine($"Theme toggle error: {ex}");
            }
        }

        private int GetCurrentTheme()
        {
            Console.WriteLine($"Reading theme from registry: {RegistryKeyPath}");
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
                {
                    if (key != null)
                    {
                        object value = key.GetValue(AppsUseLightThemeValue);
                        Console.WriteLine($"Registry value for {AppsUseLightThemeValue}: {value}");
                        if (value != null)
                        {
                            return Convert.ToInt32(value);
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Registry key is null!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR reading theme: {ex.Message}");
                Debug.WriteLine($"Failed to read theme: {ex.Message}");
            }
            
            // Default to light theme if unable to read
            Console.WriteLine("Defaulting to Light theme (1)");
            return 1;
        }

        private void SetTheme(int themeValue)
        {
            Console.WriteLine($"Setting theme to: {themeValue}");
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (key != null)
                    {
                        // Set both app and system theme
                        Console.WriteLine($"Writing {AppsUseLightThemeValue} = {themeValue}");
                        key.SetValue(AppsUseLightThemeValue, themeValue, RegistryValueKind.DWord);
                        Console.WriteLine($"Writing {SystemUsesLightThemeValue} = {themeValue}");
                        key.SetValue(SystemUsesLightThemeValue, themeValue, RegistryValueKind.DWord);
                        Console.WriteLine("Registry values written successfully");
                    }
                    else
                    {
                        throw new Exception("Unable to access theme registry key");
                    }
                }

                // Notify Windows of the theme change
                Console.WriteLine("Notifying Windows of theme change...");
                NotifyThemeChange();
                
                // Note: Explorer restart removed for faster response
                // The theme will apply to new windows immediately
                // Existing windows will update on next refresh
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in SetTheme: {ex.Message}");
                Debug.WriteLine($"Failed to set theme: {ex.Message}");
                throw;
            }
        }

        private void NotifyThemeChange()
        {
            // Broadcast WM_SETTINGCHANGE to notify applications of theme change
            const int HWND_BROADCAST = 0xffff;
            const uint WM_SETTINGCHANGE = 0x001A;
            
            // Use SendMessageTimeout instead of SendNotifyMessage for more reliable delivery
            NativeMethods.SendMessageTimeout(
                (IntPtr)HWND_BROADCAST,
                WM_SETTINGCHANGE,
                (IntPtr)0,
                "ImmersiveColorSet",
                SendMessageTimeoutFlags.SMTO_ABORTIFHUNG,
                5000,
                out IntPtr result);
        }

        private void RefreshExplorer()
        {
            try
            {
                // Kill and restart explorer to apply theme immediately
                Process[] explorerProcesses = Process.GetProcessesByName("explorer");
                Console.WriteLine($"Found {explorerProcesses.Length} explorer processes");
                foreach (Process process in explorerProcesses)
                {
                    Console.WriteLine($"Killing explorer process {process.Id}");
                    process.Kill();
                    process.WaitForExit();
                }
                
                // Start explorer again
                Console.WriteLine("Starting explorer.exe");
                Process.Start("explorer.exe");
                Console.WriteLine("Explorer restarted successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR refreshing explorer: {ex.Message}");
                Debug.WriteLine($"Failed to refresh explorer: {ex.Message}");
            }
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

    [System.Flags]
    internal enum SendMessageTimeoutFlags : uint
    {
        SMTO_NORMAL = 0x0,
        SMTO_BLOCK = 0x1,
        SMTO_ABORTIFHUNG = 0x2,
        SMTO_NOTIMEOUTIFNOTHUNG = 0x8
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            string lParam,
            SendMessageTimeoutFlags fuFlags,
            uint uTimeout,
            out IntPtr lpdwResult);
    }
}
