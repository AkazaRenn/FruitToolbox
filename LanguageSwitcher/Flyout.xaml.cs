using System.Globalization;

using H.NotifyIcon;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Win32;

using Windows.Foundation;
using Windows.UI.ViewManagement;

using WindowsDesktop;

using WindowsDisplayAPI;

using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FruitToolbox.LanguageSwitcher;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
internal sealed partial class Flyout: WindowEx, IDisposable {
    private const double WindowMarginFromBottom = 38;
    private static double ScaleFactor = 1;
    private static Point MainDisplay = new(0, 0);
    private static Point MainDisplayOffset = new(0, 0);

    private readonly DispatcherQueueTimer HideFlyoutTimer;
    private static readonly UISettings UISettings = new();


    public Flyout() {
        InitializeComponent();
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

        VirtualDesktop.PinApplication(Constants.AppID);
        HwndExtensions.SetExtendedWindowStyle(this.GetWindowHandle(),
            ExtendedWindowStyle.Transparent | ExtendedWindowStyle.NoActivate | ExtendedWindowStyle.ToolWindow);
        Interop.Utils.SetBorderlessWindow(this.GetWindowHandle());
        MoveToDestination();
        ToggleExternalHooks(true);
    }

    ~Flyout() {
        Dispose();
    }

    private void ToggleExternalHooks(bool enable) {
        if (enable) {
            Core.NewLanguageEvent += OnNewLanguage;
            Hotkey.Core.CapsLockOnEvent += OnCapsLockOn;
            Hotkey.Core.CapsLockOffEvent += OnCapsLockOff;
        } else {
            Core.NewLanguageEvent -= OnNewLanguage;
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

    public void OnNewLanguage(object sender, Constants.LanguageEvent e) {
        UpdateFlyout(new CultureInfo(e.LCID).NativeName);
    }

    public void OnCapsLockOn(object sender, EventArgs e) {
        UpdateFlyout("Caps Lock ON");
    }

    public void OnCapsLockOff(object sender, EventArgs e) {
        UpdateFlyout("Caps Lock OFF");
    }

    private void UpdateFlyout(string newText) {
        DispatcherQueue.TryEnqueue(() => {
            FlyoutText.Text = newText;

            this.Show();
            FlyoutBase.ShowAttachedFlyout(FlyoutAnchor);
            HideFlyoutTimer.Start();
        });
    }

    public void Reload() {
        MoveToDestination();
    }

    private void MoveToDestination() {
        foreach (var display in Display.GetDisplays()) {
            if (display.IsGDIPrimary) {
                MainDisplayOffset = new(display.CurrentSetting.Position.X, display.CurrentSetting.Position.Y);
                MainDisplay = new(display.CurrentSetting.Resolution.Width, display.CurrentSetting.Resolution.Height);
                break;
            }
        }

        this.Move((int)(MainDisplayOffset.X + (MainDisplay.X - AppWindow.Size.Width) / 2),
             (int)(MainDisplayOffset.Y + MainDisplay.Y - AppWindow.Size.Height - ScaleFactor * WindowMarginFromBottom));
    }

    private void UpdateScaleFactor() {
        var currentWidth = Width;
        Width = 1000;
        ScaleFactor = AppWindow.Size.Width / Width;
        Width = currentWidth;
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
