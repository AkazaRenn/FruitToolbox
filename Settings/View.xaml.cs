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

        Core.SettingsChangedEventHandler += SettingsUpdateHandler;
    }

    public void SettingsUpdateHandler(object sender, EventArgs e) {
        ShowFlyoutCard.IsEnabled = Core.LanguageSwitcherEnabled;
        DesktopToHomeCard.IsEnabled = Core.MaximizerEnabled;
        SwapVirtualDesktopHotkeysCard.IsEnabled = Core.MaximizerEnabled;
    }

    private void StartUp_Toggle(object sender, RoutedEventArgs e) {
        Core.StartUp = (sender as ToggleSwitch).IsOn;
        (sender as ToggleSwitch).IsOn = Core.StartUp;
    }


    private void LanguageSwitcherEnabled_Toggle(object sender, RoutedEventArgs e) {
        Core.LanguageSwitcherEnabled = (sender as ToggleSwitch).IsOn;
    }

    private void ShowFlyout_Toggle(object sender, RoutedEventArgs e) {
        Core.FlyoutEnabled = (sender as ToggleSwitch).IsOn;
    }


    private void MaximizerEnabled_Toggle(object sender, RoutedEventArgs e) {
        Core.MaximizerEnabled = (sender as ToggleSwitch).IsOn;
    }

    private void DesktopToHome_Toggle(object sender, RoutedEventArgs e) {
        Core.DesktopToHomeEnabled = (sender as ToggleSwitch).IsOn;
    }

    private void SwapVirtualDesktopHotkeys_Toggle(object sender, RoutedEventArgs e) {
        Core.SwapVirtualDesktopHotkeysEnabled = (sender as ToggleSwitch).IsOn;
    }


    private void RemapLGui_Toggle(object sender, RoutedEventArgs e) {
        Core.LGuiRemapEnabled = (sender as ToggleSwitch).IsOn;
    }

    private void ReverseScroll_Toggle(object sender, RoutedEventArgs e) {
        Core.ReverseMouseWheelEnabled = (sender as ToggleSwitch).IsOn;
    }

}
