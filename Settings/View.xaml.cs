using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using WinUIEx;

namespace FruitToolbox.Settings;

internal sealed partial class View: WindowEx
{
    public View()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
    }

    private void StartUp_Toggle(object sender, RoutedEventArgs e)
    {
        FruitToolbox.Settings.Core.StartUp = (sender as ToggleSwitch).IsOn;
        (sender as ToggleSwitch).IsOn = FruitToolbox.Settings.Core.StartUp;
    }

    private void ShowFlyout_Toggle(object sender, RoutedEventArgs e)
    {
        FruitToolbox.Settings.Core.FlyoutEnabled = (sender as ToggleSwitch).IsOn;
    }

    private void RemapLGui_Toggle(object sender, RoutedEventArgs e)
    {
        FruitToolbox.Settings.Core.LGuiRemapEnabled = (sender as ToggleSwitch).IsOn;
    }

    private void DesktopToHome_Toggle(object sender, RoutedEventArgs e)
    {
        FruitToolbox.Settings.Core.DesktopToHomeEnabled = (sender as ToggleSwitch).IsOn;
    }

    private void ReverseScroll_Toggle(object sender, RoutedEventArgs e)
    {
        FruitToolbox.Settings.Core.ReverseMouseWheelEnabled = (sender as ToggleSwitch).IsOn;
    }

}
