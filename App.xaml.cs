using H.NotifyIcon;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FruitToolbox;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public sealed partial class App: Application {
    #region Properties

    private static readonly LanguageSwitcher.Core LanguageSwitcher =
        FruitToolbox.LanguageSwitcher.Core.Instance;
    private static readonly Maximizer.Core Maximizer =
        FruitToolbox.Maximizer.Core.Instance;
    private static readonly Hotkey.Core Hotkey =
        FruitToolbox.Hotkey.Core.Instance;

    private static TaskbarIcon TrayIcon;
    private static Settings.View SettingsWindow = null;


    #endregion

    #region Constructors

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App() {
        InitializeComponent();
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args) {
        InitializeTrayIcon();
    }

    private void InitializeTrayIcon() {
        var OpenSettingsCommand = (XamlUICommand)Resources["OpenSettingsCommand"];
        OpenSettingsCommand.ExecuteRequested += OpenSettingsCommand_ExecuteRequested;

        var ExitApplicationCommand = (XamlUICommand)Resources["ExitApplicationCommand"];
        ExitApplicationCommand.ExecuteRequested += ExitApplicationCommand_ExecuteRequested;

        TrayIcon = (TaskbarIcon)Resources["TrayIcon"];
        TrayIcon.ForceCreate();
    }

    private void OpenSettingsCommand_ExecuteRequested(object _, ExecuteRequestedEventArgs args) {
        if (SettingsWindow == null) {
            SettingsWindow = new();
            SettingsWindow.Closed += (e, s) => {
                s.Handled = true;
                SettingsWindow.Hide();
                SettingsWindow = null;
            };
            SettingsWindow.Activate();
        }

        SettingsWindow.Show();
    }

    private void ExitApplicationCommand_ExecuteRequested(object _, ExecuteRequestedEventArgs args) {
        LanguageSwitcher?.Dispose();
        Maximizer?.Dispose();
        Hotkey?.Dispose();
        TrayIcon?.Dispose();
        Environment.Exit(0);
    }

    #endregion
}
