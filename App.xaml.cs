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
            Settings = Settings.Load();
            InitializeTrayIcon();
            InitializeFunction();
        }

        private void InitializeTrayIcon() {
            var exitApplicationCommand = (XamlUICommand)Resources["ExitApplicationCommand"];
            exitApplicationCommand.ExecuteRequested += ExitApplicationCommand_ExecuteRequested;

            var enableLWinRemapCommand = (XamlUICommand)Resources["EnableLWinRemapCommand"];
            enableLWinRemapCommand.ExecuteRequested += Settings.ToggleLWinRemapEnabled;
            SetOptionCommandGlyph(enableLWinRemapCommand, Settings.LWinRemapEnabled);

            var enableReverseMouseWheelCommand = (XamlUICommand)Resources["EnableReverseMouseWheelCommand"];
            enableReverseMouseWheelCommand.ExecuteRequested += Settings.ToggleReverseMouseWheelEnabled;
            SetOptionCommandGlyph(enableReverseMouseWheelCommand, Settings.ReverseMouseWheelEnabled);

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
            Switcher = new LanguageSwitcher();
            if(!Switcher.Ready()) {
                SwitcherNotReady();
                return;
            }

            Hotkey = new Hotkey(Switcher.SwapCategoryNoReturn,
                                Switcher.UpdateInputLanguageByKeyboard,
                                Switcher.OnRaltUp);
            Settings.SettingsChangedEventHandler += Hotkey.SettingsUpdateHandler;

            RegisterStartup();
        }

        private void ExitApplicationCommand_ExecuteRequested(object _, ExecuteRequestedEventArgs args)
            => ExitApp();

        private static async void RegisterStartup() {
            StartupTask startupTask = await StartupTask.GetAsync("MyStartupId");
            if(startupTask.State == StartupTaskState.Disabled) {
                await startupTask.RequestEnableAsync();
            }
        }

        private static void SwitcherNotReady() {
            new ToastContentBuilder()
                .AddText("Exiting, you don't really need the app")
                .AddText("It is for those who have both keyboard languages and IME languages installed.")
                .AddText("All your needs can be satisfied by native Windows functions.")
                .Show();

            ExitApp();
        }

        private static void ExitApp() {
            TrayIcon?.Dispose();
            Window?.Close();
        }

        #endregion
    }
}