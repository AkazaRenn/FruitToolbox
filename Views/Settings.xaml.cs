using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using WinUIEx;

namespace FruitLanguageSwitcher.Views;

internal sealed partial class Settings: WindowEx
{
    public Settings()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
    }

    private void StartUp_Toggle(object sender, RoutedEventArgs e)
    {
        Core.Settings.StartUp = (sender as ToggleSwitch).IsOn;
    }

    private void ReloadButton_Click(object sender, RoutedEventArgs e)
    {
        App.ReloadComponents();
    }
    private void ShowFlyout_Toggle(object sender, RoutedEventArgs e)
    {
        Core.Settings.FlyoutEnabled = (sender as ToggleSwitch).IsOn;
    }
    private void RemapLGui_Toggle(object sender, RoutedEventArgs e)
    {
        Core.Settings.LGuiRemapEnabled = (sender as ToggleSwitch).IsOn;
    }
    private void ReverseScroll_Toggle(object sender, RoutedEventArgs e)
    {
        Core.Settings.ReverseMouseWheelEnabled = (sender as ToggleSwitch).IsOn;
    }

}
