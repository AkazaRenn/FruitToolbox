using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using H.NotifyIcon;
using H.NotifyIcon.Core;

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Runtime.InteropServices;

using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using System.Timers;

using WindowsDisplayAPI;
using WinRT.Interop;

using WinUIEx;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using System.Text;
using Microsoft.Win32;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;
using WindowsDisplayAPI.Native.Structures;
using System.Globalization;
using FruitLanguageSwitcher.Views.Win32Helper;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FruitLanguageSwitcher.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Flyout: WindowEx
    {

        private const double WindowMarginFromBottom = 38;
        private double ScaleFactor = 1;
        private Point MainDisplay = new(0, 0);
        private Point MainDisplayOffset = new(0, 0);
        private readonly DispatcherQueueTimer HideFlyoutTimer;
        private readonly UISettings UISettings = new();


        public Flyout()
        {
            InitializeComponent();
            UpdateMainDisplayOffset();
            UpdateScaleFactor();
            UpdateTheme();

            HideFlyoutTimer = DispatcherQueue.CreateTimer();
            HideFlyoutTimer.Interval = TimeSpan.FromSeconds(2);
            HideFlyoutTimer.Tick += HideFlyout;
            FlyoutControl.Closed += (_, e) => {
                Thread.Sleep(100);
                this.Hide();
            };
            UISettings.ColorValuesChanged += (_, _) => UpdateTheme();

            //Hide();
            IsShownInSwitchers = false;
            IsMinimizable = false;
            IsMaximizable = false;
            IsResizable = false;
            IsTitleBarVisible = false;
            IsAlwaysOnTop = true;

            VirtualDesktop.PinApp(Constants.AppID);
            HwndExtensions.SetExtendedWindowStyle(this.GetWindowHandle(), 
                ExtendedWindowStyle.Transparent | ExtendedWindowStyle.NoActivate | ExtendedWindowStyle.ToolWindow);
            DesktopWindowManager.DisableRoundCorners(this.GetWindowHandle());
            MoveToDestination();
        }

        void HideFlyout(object t, object s)
        {
            FlyoutControl.Hide();
            HideFlyoutTimer.Stop();
        }

        public void UpdateText(int lcid)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                FlyoutText.Text = new CultureInfo(lcid).NativeName;

                this.Show();
                FlyoutBase.ShowAttachedFlyout(FlyoutAnchor);
                HideFlyoutTimer.Start();
            });
        }

        private void MoveToDestination()
        {
            this.Move((int)(MainDisplayOffset.X + (MainDisplay.X - AppWindow.Size.Width) / 2),
                 (int)(MainDisplayOffset.Y + MainDisplay.Y - AppWindow.Size.Height - ScaleFactor * WindowMarginFromBottom));
        }

        private void UpdateScaleFactor()
        {
            var currentWidth = Width;
            Width = 1000;
            ScaleFactor = AppWindow.Size.Width / Width;
            Width = currentWidth;
        }

        private void UpdateMainDisplayOffset()
        {
            foreach(var display in Display.GetDisplays())
            {
                if(display.IsGDIPrimary)
                {
                    MainDisplayOffset = new(display.CurrentSetting.Position.X, display.CurrentSetting.Position.Y);
                    MainDisplay = new(display.CurrentSetting.Resolution.Width, display.CurrentSetting.Resolution.Height);
                    return;
                }
            }
        }

        private void UpdateTheme()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if(key != null)
            {
                bool useLightTheme = (int)key.GetValue("SystemUsesLightTheme") != 0;
                if(useLightTheme)
                {
                    DispatcherQueue.TryEnqueue(() => ((FrameworkElement)Content).RequestedTheme = ElementTheme.Light);
                } else
                {
                    DispatcherQueue.TryEnqueue(() => ((FrameworkElement)Content).RequestedTheme = ElementTheme.Dark);
                }
            }
        }
    }
}
