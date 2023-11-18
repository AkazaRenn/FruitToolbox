using System;

using H.NotifyIcon;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;

using Windows.Foundation;
using Windows.UI.ViewManagement;

using WindowsDisplayAPI;

using WinUIEx;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.Win32;
using System.Globalization;
using FruitLanguageSwitcher.Views.UnmanagedHelper;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FruitLanguageSwitcher.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    internal sealed partial class Flyout: WindowEx
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
            UpdateScaleFactor();
            UpdateTheme();

            HideFlyoutTimer = DispatcherQueue.CreateTimer();
            HideFlyoutTimer.Interval = TimeSpan.FromSeconds(2);
            HideFlyoutTimer.Tick += HideFlyout;
            FlyoutControl.Closed += (_, e) =>
            {
                Thread.Sleep(100);
                this.Hide();
            };
            UISettings.ColorValuesChanged += (_, _) => UpdateTheme();

            this.Hide();
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

        public void UpdateText(object sender, Constants.LanguageEvent e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                FlyoutText.Text = new CultureInfo(e.LCID).NativeName;

                this.Show();
                FlyoutBase.ShowAttachedFlyout(FlyoutAnchor);
                HideFlyoutTimer.Start();
            });
        }

        public void Reload()
        {
            MoveToDestination();
        }

        private void MoveToDestination()
        {
            foreach(var display in Display.GetDisplays())
            {
                if(display.IsGDIPrimary)
                {
                    MainDisplayOffset = new(display.CurrentSetting.Position.X, display.CurrentSetting.Position.Y);
                    MainDisplay = new(display.CurrentSetting.Resolution.Width, display.CurrentSetting.Resolution.Height);
                    break;
                }
            }

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
