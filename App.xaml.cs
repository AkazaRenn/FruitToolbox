using System;

using FruitLanguageSwitcher.Core;

using H.NotifyIcon;

using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
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

        public static TaskbarIcon TrayIcon { get; private set; }
        public static Window Window { get; set; }
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
            InitializeTrayIcon();
            InitializeFunction();
        }

        private void InitializeTrayIcon() {
            var exitApplicationCommand = (XamlUICommand)Resources["ExitApplicationCommand"];
            exitApplicationCommand.ExecuteRequested += ExitApplicationCommand_ExecuteRequested;

            var reloadApplicationCommand = (XamlUICommand)Resources["ReloadApplicationCommand"];
            reloadApplicationCommand.ExecuteRequested += ReloadApplicationCommand_ExecuteRequested;

            TrayIcon = (TaskbarIcon)Resources["TrayIcon"];
            TrayIcon.ForceCreate();
        }

        private void InitializeFunction() {
            Switcher = new LanguageSwitcher();
            if(!Switcher.Ready()) {
                SwitcherNotReady();
                return;
            }

            Hotkey = new Hotkey(Switcher.SwapCategoryNoReturn, Switcher.UpdateInputLanguage,
                                Switcher.OnRaltDown, Switcher.OnRaltUp);

            RegisterStartup();
        }

        private static async void RegisterStartup() {
            StartupTask startupTask = await StartupTask.GetAsync("MyStartupId");
            if(startupTask.State == StartupTaskState.Disabled) {
                // Task is disabled but can be enabled
                await startupTask.RequestEnableAsync();
            }
        }

        private void SwitcherNotReady() {
            new ToastContentBuilder()
                .AddText("Exiting, you don't really need the app")
                .AddText("It is for those who have both keyboard languages and IME languages installed.")
                .AddText("All your needs can be satisfied by native Windows functions.")
                .Show();

            ExitApp();
        }

        private void ExitApplicationCommand_ExecuteRequested(object _, ExecuteRequestedEventArgs args) {
            ExitApp();
        }

        private static void ExitApp() {
            TrayIcon?.Dispose();
            Window?.Close();
        }

        private void ReloadApplicationCommand_ExecuteRequested(object _, ExecuteRequestedEventArgs args) {
            Switcher.Reload();
            // Not working, don't know why, comment out for now
            //Hotkey.Reload();
        }

        #endregion
    }
}