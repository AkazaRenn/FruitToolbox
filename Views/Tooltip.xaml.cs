using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;

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

using WindowsDisplayAPI;
using WinRT.Interop;

using WinUIEx;
using Microsoft.UI.Windowing;
using System.Text;
using Microsoft.Win32;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FruitLanguageSwitcher.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Tooltip: WindowEx
    {


        private const int WindowMarginFromBottom = 61;

        private double ScaleFactor = 1;
        private Point MainDisplay = new(0, 0);
        private Point MainDisplayOffset = new(0, 0);

        public Tooltip()
        {
            InitializeComponent();
            UpdateScaleFactor();
            UpdateMainDisplayOffset();

            //Hide();
            //IsShownInSwitchers = false;
            IsMinimizable = false;
            IsMaximizable = false;
            IsResizable = false;
            IsTitleBarVisible = false;
            IsAlwaysOnTop = true;

            VirtualDesktop.PinApp(Constants.AppID);
            MoveToDestination();
            Interop.DisableRoundCorners(this.GetWindowHandle());
        }

        private void MoveToDestination()
        {
            this.Move((int)(MainDisplayOffset.X + (MainDisplay.X - AppWindow.Size.Width) / 2),
                 (int)(MainDisplayOffset.Y + MainDisplay.Y - AppWindow.Size.Height - ScaleFactor * WindowMarginFromBottom));
        }

        private void UpdateScaleFactor()
        {
            Width = 1000;
            ScaleFactor = AppWindow.Size.Width / Width;
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
    }
}
