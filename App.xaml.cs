using System;

using FruitLanguageSwitcher.Core;
using FruitLanguageSwitcher.Views;

using H.NotifyIcon;
using H.NotifyIcon.Core;

using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using Windows.ApplicationModel;

using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FruitLanguageSwitcher {
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App: Application {
        #region Properties
        public static Window Window { get; set; }
        public static Settings Settings { get; private set; }

        private static LanguageSwitcher Switcher { get; set; }
        private static Hotkey Hotkey { get; set; }

        private static Tooltip aaa = new();

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
            aaa.Activate();
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

        private static async void RegisterStartup() {
            StartupTask startupTask = await StartupTask.GetAsync("MyStartupId");
            if(startupTask.State == StartupTaskState.Disabled) {
                await startupTask.RequestEnableAsync();
            }
        }

        #endregion
    }
}