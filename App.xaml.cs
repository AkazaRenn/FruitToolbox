using FruitLanguageSwitcher.Core;

using H.NotifyIcon;

using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

using System;
using System.Diagnostics;

using Windows.ApplicationModel;
using Windows.Foundation.Collections;
using Windows.UI.Popups;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FruitLanguageSwitcher {
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App: Application {
        #region Properties

        public static TaskbarIcon? TrayIcon { get; private set; }
        public static Window? Window { get; set; }
        private static LanguageSwitcher? switcher { get; set; }

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
            CheckStartup();
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
            switcher = new LanguageSwitcher(false);
        }

        private void ShowHideWindowCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args) {
            if(Window == null) {
                Window = new Window();
                Window.Show();
                return;
            }

            if(Window.Visible) {
                Window.Hide();
            } else {
                Window.Show();
            }
        }

        private void ExitApplicationCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args) {
            TrayIcon?.Dispose();
            Window?.Close();
        }

        private void ReloadApplicationCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args) {
            switcher.reload();
        }

        #endregion

        #region Startup Checker

        private async void CheckStartup() {
            StartupTask startupTask = await StartupTask.GetAsync("MyStartupId");

            ToastNotificationManagerCompat.OnActivated += async toastArgs => {
                // Obtain the arguments from the notification
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
                if(args.Contains("TOAST_REGISTER_STARTUP")) {
                    StartupTaskState newState = await startupTask.RequestEnableAsync();
                }
            };

            if(startupTask.State == StartupTaskState.Disabled) {
                // Task is disabled but can be enabled
                new ToastContentBuilder()
                        .AddText("Would you like the app to start with Windows logging?")
                        .AddText("You can disable this notification if you want, or turn off startup in settings.")
                        .AddButton("Yes", ToastActivationType.Background, "TOAST_REGISTER_STARTUP")
                        .AddButton("No", ToastActivationType.Background, "TOAST_DISABLE_STARTUP")
                        .Show();
            }
        }

        #endregion
    }
}