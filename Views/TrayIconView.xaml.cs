using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FruitLanguageSwitcher.Views
{
    [ObservableObject]
    public sealed partial class TrayIconView: UserControl
    {
        public TrayIconView()
        {
            this.InitializeComponent();
        }

        [RelayCommand]
        private void OpenSettings() { }

        [RelayCommand]
        private void ExitApplication()
        {
            Environment.Exit(0);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
