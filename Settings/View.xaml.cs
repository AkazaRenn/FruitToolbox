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
    }

    private void StartUp_Toggle(object sender, RoutedEventArgs e) {
        ToggleSwitch toggle = (ToggleSwitch)sender;

        Core.StartUp = toggle.IsOn;
        if (toggle.IsOn == true &&
            Core.StartUp == false) {
            toggle.IsOn = false;
            new ToastContentBuilder()
                .AddText("Failed enabling startup")
                .AddText("Please check if it is disabled by Windows Settings or Group Policy")
                .Show();
        }
    }
}
