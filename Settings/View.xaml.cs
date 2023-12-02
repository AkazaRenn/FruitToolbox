using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using WinUIEx;

namespace FruitToolbox.Settings;

internal sealed partial class View: WindowEx {
    public View() {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.Title = "Fruit Toolbox Settings";
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/durian.ico"));

        ToggleEnabledStatesBySettings();
        Core.SettingsChangedEventHandler += OnSettingsUpdate;
    }

    private void OnSettingsUpdate(object sender, EventArgs e) =>
        ToggleEnabledStatesBySettings();

    private void ToggleEnabledStatesBySettings() {
        ShowFlyoutCard.IsEnabled = Core.LanguageSwitcherEnabled;
        RAltModifierCard.IsEnabled = Core.LanguageSwitcherEnabled;
        ScrollLockOnImeLanguageCard.IsEnabled = Core.LanguageSwitcherEnabled;
        DisableCapsLockOnLanguageChangeCard.IsEnabled = Core.LanguageSwitcherEnabled;

        DesktopToHomeCard.IsEnabled = Core.MaximizerEnabled;
        SwapVirtualDesktopHotkeysCard.IsEnabled = Core.MaximizerEnabled;
        ReorderIntervalCard.IsEnabled = Core.MaximizerEnabled;
    }

    private void StartUp_Toggle(ToggleSwitch sender, RoutedEventArgs e) {
        Core.StartUp = sender.IsOn;
        if (sender.IsOn == true &&
            Core.StartUp == false) {
            sender.IsOn = false;
            new ToastContentBuilder()
                .AddText("Failed enabling startup")
                .AddText("Please check if it is disabled by Windows Settings or Group Policy")
                .Show();
        }
    }
}
