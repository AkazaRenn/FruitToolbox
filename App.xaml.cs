using System;
using System.ComponentModel;

using FruitLanguageSwitcher.Core;

using H.NotifyIcon;

using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using Windows.ApplicationModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FruitLanguageSwitcher
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App: Application
    {
        #region Properties

        public static TaskbarIcon TrayIcon { get; private set; }
        public static Window Window { get; set; }

        private static LanguageSwitcher Switcher;
        private static Hotkey Hotkey;
        private static Views.Settings SettingsWindow = null;
        private static Views.Flyout NewLangFlyout;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            InitializeFunction();
            InitializeTrayIcon();
            InitializeFlyout();
        }

        private static void InitializeFlyout()
        {
            NewLangFlyout = new();

            LanguageSwitcher.NewLanguageEvent += NewLangFlyout.UpdateText;
            NewLangFlyout.Activate();
        }

        private void InitializeTrayIcon()
        {
            var OpenSettingsCommand = (XamlUICommand)Resources["OpenSettingsCommand"];
            OpenSettingsCommand.ExecuteRequested += OpenSettingsCommand_ExecuteRequested;

            var ReloadCommand = (XamlUICommand)Resources["ReloadCommand"];
            ReloadCommand.ExecuteRequested += ReloadCommand_ExecuteRequested;

            var ExitApplicationCommand = (XamlUICommand)Resources["ExitApplicationCommand"];
            ExitApplicationCommand.ExecuteRequested += ExitApplicationCommand_ExecuteRequested;

            TrayIcon = (TaskbarIcon)Resources["TrayIcon"];
            TrayIcon.ForceCreate();
        }

        private static void InitializeFunction()
        {
            Switcher = new LanguageSwitcher();
            Hotkey = new Hotkey(Switcher.SwapCategoryNoReturn,
                                Switcher.UpdateInputLanguageByKeyboard,
                                Switcher.OnRaltUp);
            Settings.SettingsChangedEventHandler += Hotkey.SettingsUpdateHandler;
            Settings.SettingsChangedEventHandler += Views.Flyout.SettingsUpdateHandler;

            if(!Switcher.Ready())
            {
                Settings.LanguageSwitcherEnabled = false;
                new ToastContentBuilder()
                    .AddText("Unable to enable language switcher")
                    .AddText("Please make sure you have both keyboard languages and IME languages installed")
                    .Show();
            }

        }

        private void OpenSettingsCommand_ExecuteRequested(object _, ExecuteRequestedEventArgs args)
        {
            if(SettingsWindow == null)
            {
                SettingsWindow = new();
                SettingsWindow.Closed += (e, s) => SettingsWindow = null;
            }

            SettingsWindow.Activate();
        }

        private void ReloadCommand_ExecuteRequested(object _, ExecuteRequestedEventArgs args)
        {
            ReloadComponents();
        }

        public static void ReloadComponents()
        {
            Switcher.Reload();
            NewLangFlyout.Reload();
        }

        private void ExitApplicationCommand_ExecuteRequested(object _, ExecuteRequestedEventArgs args)
        {
            TrayIcon?.Dispose();
            Window?.Close();
        }

        #endregion
    }
}