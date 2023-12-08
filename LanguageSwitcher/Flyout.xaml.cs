using System.Globalization;

using H.NotifyIcon;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Win32;

using Windows.UI.ViewManagement;

using WindowsDesktop;

using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FruitToolbox.LanguageSwitcher;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
internal sealed partial class Flyout: WindowEx, IDisposable {
    private const double WindowMarginFromBottom = 38;

    private readonly DispatcherQueueTimer HideFlyoutTimer;
    private static readonly UISettings UISettings = new();

    public Flyout() {
        InitializeComponent();
        UpdateTheme();

        HideFlyoutTimer = DispatcherQueue.CreateTimer();
        HideFlyoutTimer.Interval = TimeSpan.FromSeconds(2);
        HideFlyoutTimer.Tick += HideFlyout;
        FlyoutControl.Closed += (_, e) => {
            Thread.Sleep(100);
            this.Hide();
        };
        UISettings.ColorValuesChanged += (_, _) => UpdateTheme();
        Activated += (_, _) => {
            DispatcherQueue.TryEnqueue(() => {
                this.Hide();
            });
        };

        IsShownInSwitchers = false;
        IsMinimizable = false;
        IsMaximizable = false;
        IsResizable = false;
        IsTitleBarVisible = false;
        IsAlwaysOnTop = true;

        VirtualDesktop.PinApplication(Utils.AppID);
        HwndExtensions.SetExtendedWindowStyle(this.GetWindowHandle(),
            ExtendedWindowStyle.Transparent | ExtendedWindowStyle.NoActivate | ExtendedWindowStyle.ToolWindow);
        Interop.Utils.SetBorderlessWindow(this.GetWindowHandle());
        ToggleExternalHooks(true);
    }

    ~Flyout() {
        Dispose();
    }

    private void ToggleExternalHooks(bool enable) {
        if (enable) {
            Core.SwapCategoryEvent += OnSwapCategory;
            Hotkey.Core.CapsLockOnEvent += OnCapsLockOn;
            Hotkey.Core.CapsLockOffEvent += OnCapsLockOff;
        } else {
            Core.SwapCategoryEvent -= OnSwapCategory;
            Hotkey.Core.CapsLockOnEvent -= OnCapsLockOn;
            Hotkey.Core.CapsLockOffEvent -= OnCapsLockOff;
        }
    }

    public void Dispose() {
        ToggleExternalHooks(false);
        Close();
        GC.SuppressFinalize(this);
    }

    void HideFlyout(object t, object s) {
        FlyoutControl.Hide();
        HideFlyoutTimer.Stop();
    }

    public void OnSwapCategory(object sender, Utils.LanguageEvent e) {
        ShowFlyout(new CultureInfo(e.LCID).NativeName);
    }

    public void OnCapsLockOn(object sender, EventArgs e) {
        ShowFlyout("Caps Lock ON");
    }

    public void OnCapsLockOff(object sender, EventArgs e) {
        ShowFlyout("Caps Lock OFF");
    }

    private void ShowFlyout(string newText) {
        if (Settings.Core.DisableFlyoutInFullscreen &&
            Interop.Utils.InFullScreen()) {
            return;
        }

        DispatcherQueue.TryEnqueue(() => {
            MoveToDestination();
            FlyoutText.Text = newText;

            this.Show();
            FlyoutBase.ShowAttachedFlyout(FlyoutAnchor);
            HideFlyoutTimer.Start();
        });
    }

    private void MoveToDestination() {
        var monitorInfo = Interop.Utils.GetActiveMonitorInfo();

        this.Move(
            (int)(monitorInfo.X + (monitorInfo.Width - AppWindow.Size.Width) / 2),
            (int)(monitorInfo.Y + monitorInfo.Height - AppWindow.Size.Height - monitorInfo.Scaling * WindowMarginFromBottom));
    }

    private void UpdateTheme() {
        var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        if (key != null) {
            bool useLightTheme = (int)key.GetValue("SystemUsesLightTheme") != 0;
            if (useLightTheme) {
                DispatcherQueue.TryEnqueue(() => ((FrameworkElement)Content).RequestedTheme = ElementTheme.Light);
            } else {
                DispatcherQueue.TryEnqueue(() => ((FrameworkElement)Content).RequestedTheme = ElementTheme.Dark);
            }
        }
    }
}
