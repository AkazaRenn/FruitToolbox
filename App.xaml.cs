using System;

using FruitLanguageSwitcher.Core;

using H.NotifyIcon;

using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using Windows.ApplicationModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FruitLanguageSwitcher {
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App: Application {
        #region Properties
        public const char ICON_CHECKBOX_GLYPH = (char)0xF16B;
        public const char ICON_CHECKBOX_COMPOSITE_GLYPH = (char)0xF16C;

        public static TaskbarIcon TrayIcon { get; private set; }
        public static Window Window { get; set; }
        public static Settings Settings { get; private set; }

        private static LanguageSwitcher Switcher { get; set; }
        private static Hotkey Hotkey { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App() {
            this.InitializeComponent();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args) {
            InitializeFunction();
            InitializeTrayIcon();
        }

        private void InitializeTrayIcon() {
            var exitApplicationCommand = (XamlUICommand)Resources["ExitApplicationCommand"];
            exitApplicationCommand.ExecuteRequested += ExitApplicationCommand_ExecuteRequested;

            var lwinRemapCommand = (XamlUICommand)Resources["LWinRemapCommand"];
            lwinRemapCommand.ExecuteRequested += Settings.ToggleLWinRemapEnabled;
            SetOptionCommandGlyph(lwinRemapCommand, Settings.LWinRemapEnabled);

            var reverseMouseWheelCommand = (XamlUICommand)Resources["ReverseMouseWheelCommand"];
            reverseMouseWheelCommand.ExecuteRequested += Settings.ToggleReverseMouseWheelEnabled;
            SetOptionCommandGlyph(reverseMouseWheelCommand, Settings.ReverseMouseWheelEnabled);

            TrayIcon = (TaskbarIcon)Resources["TrayIcon"];
            TrayIcon.ForceCreate();
        }

        private static void SetOptionCommandGlyph(XamlUICommand command, bool enabled) {
            ((FontIconSource)command.IconSource).Glyph =
                enabled
                ? ICON_CHECKBOX_COMPOSITE_GLYPH.ToString()
                : ICON_CHECKBOX_GLYPH.ToString();
        }

        private static void InitializeFunction() {
            Settings = Settings.Load();
            Switcher = new LanguageSwitcher();
            Hotkey = new Hotkey(Switcher.SwapCategoryNoReturn,
                                Switcher.UpdateInputLanguageByKeyboard,
                                Switcher.OnRaltUp);
            Settings.SettingsChangedEventHandler += Hotkey.SettingsUpdateHandler;

            if(!Switcher.Ready()) {
                Settings.DisableLanguageSwitcher();
                new ToastContentBuilder()
                    .AddText("Unable to enable language switcher")
                    .AddText("Please make sure you have both keyboard languages and IME languages installed")
                    .Show();
            } else {
                RegisterStartup();
            }

        }

        private void ExitApplicationCommand_ExecuteRequested(object _, ExecuteRequestedEventArgs args) {
            TrayIcon?.Dispose();
            Window?.Close();
        }

        private static async void RegisterStartup() {
            StartupTask startupTask = await StartupTask.GetAsync("MyStartupId");
            if(startupTask.State == StartupTaskState.Disabled) {
                await startupTask.RequestEnableAsync();
            }
        }

        #endregion
    }
}