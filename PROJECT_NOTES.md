# Toggle Theme - Project Notes

## Project Structure

This project follows the same structure as Windows-RoundedScreen:

```
ToggleTheme/
├── .gitignore                      # Git ignore file
├── README.md                       # User documentation
├── ToggleTheme.sln                 # Visual Studio solution file
├── build.bat                       # Build script
├── install.bat                     # Install prerequisites script
├── run.bat                         # Run the built executable
└── ToggleTheme/                    # Main project folder
    ├── App.config                  # Application configuration
    ├── App.xaml                    # Application XAML definition
    ├── App.xaml.cs                 # Application code-behind (tray icon setup)
    ├── MainWindow.xaml             # Main window XAML (hidden window)
    ├── MainWindow.xaml.cs          # Main window code (theme toggle logic)
    ├── ToggleTheme.csproj          # Project file
    ├── Properties/                 # Assembly properties
    │   ├── AssemblyInfo.cs
    │   ├── Resources.Designer.cs
    │   ├── Resources.resx
    │   ├── Settings.Designer.cs
    │   └── Settings.settings
    └── Resources/                  # Resource files
        └── Icon.ico.txt            # Placeholder (needs actual .ico file)
```

## Key Features

### 1. System Tray Integration
- **App.xaml.cs**: Creates a NotifyIcon in the system tray
- **Left-click**: Toggles theme immediately
- **Right-click**: Shows context menu with Toggle and Exit options

### 2. Theme Toggle Functionality
- **MainWindow.xaml.cs**: Contains the theme toggle logic
- Modifies Windows registry keys:
  - `AppsUseLightTheme` (controls app theme)
  - `SystemUsesLightTheme` (controls system theme)
- Broadcasts `WM_SETTINGCHANGE` message to notify Windows of the change
- Shows balloon notification when theme changes

### 3. Auto-Start
- Automatically adds itself to Windows startup registry
- Runs on system boot

## How It Works

1. **Startup**: App.xaml.cs creates a hidden window and system tray icon
2. **User clicks icon**: Triggers ToggleTheme() method
3. **Read current theme**: Checks registry for current theme value (0=Dark, 1=Light)
4. **Toggle**: Switches to opposite value
5. **Write to registry**: Updates both app and system theme keys
6. **Notify Windows**: Broadcasts setting change message
7. **Show feedback**: Displays balloon notification

## Building the Project

### Prerequisites
- Visual Studio 2022 Build Tools
- .NET Framework 4.7.2 Targeting Pack

### Steps
1. Run `install.bat` to install prerequisites (if needed)
2. Run `build.bat` to compile the project
3. Run `run.bat` to execute the program

## Important Notes

### Icon File
The project references `Resources\Icon.ico` but currently only has a placeholder text file. 
To complete the project:
1. Create or obtain a .ico file
2. Save it as `ToggleTheme\Resources\Icon.ico`
3. Delete the `Icon.ico.txt` placeholder

The program will still work without a custom icon (it will use the default executable icon).

### Registry Keys Modified
The program modifies:
```
HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize
- AppsUseLightTheme (DWORD: 0 or 1)
- SystemUsesLightTheme (DWORD: 0 or 1)
```

### Startup Registry
The program adds itself to:
```
HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
- ToggleTheme = [path to executable]
```

## Differences from Windows-RoundedScreen

1. **Simpler UI**: No visible window, just tray icon
2. **Different functionality**: Toggles system theme instead of screen corners
3. **Click behavior**: Left-click performs action (RoundedScreen uses right-click menu)
4. **No settings persistence**: Theme state is read from Windows registry each time

## Future Enhancements (Optional)

- Add keyboard shortcut support
- Add custom icon with light/dark variants
- Add option to toggle only apps or only system theme
- Add startup delay option
- Add theme scheduling (auto-switch at certain times)
