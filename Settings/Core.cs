using Windows.ApplicationModel;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FruitToolbox.Settings;

[Serializable]
internal class Entries {
    public bool LanguageSwitcherEnabled { get; set; } = false;
    public bool FlyoutEnabled { get; set; } = true;
    public bool DisableFlyoutInFullscreen { get; set; } = true;
    public bool ScrollLockForImeLanguage { get; set; } = true;
    public bool DisableCapsLockOnLanguageChange { get; set; } = false;
    public bool RAltModifierEnabled { get; set; } = true;

    public bool MaxToDesktopEnabled { get; set; } = false;
    public bool DisableSwapInFullscreen { get; set; } = true;
    public bool SwapVirtualDesktopHotkeysEnabled { get; set; } = false;
    public uint ReorgnizeDesktopIntervalMs { get; set; } = 5000;

    public bool LGuiRemapEnabled { get; set; } = false;
    public bool ReverseMouseWheelEnabled { get; set; } = false;
}

public static class Core {
    private static readonly string SaveFileDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string SaveFilePath = Path.Combine(SaveFileDir, "settings.yaml");

    public static event EventHandler SettingsChangedEventHandler;
    private static Entries SettingsEntries;
    private static bool Loaded = false;

    private static void EnsureLoaded() {
        if (!Loaded)
            Load();
    }

    public static bool StartUp {
        get {
            StartupTask startupTask = StartupTask.GetAsync("MyStartupId").GetAwaiter().GetResult();
            return startupTask.State == StartupTaskState.Enabled || startupTask.State == StartupTaskState.EnabledByPolicy;
        }
        set {
            StartupTask startupTask = StartupTask.GetAsync("MyStartupId").GetAwaiter().GetResult();
            if (value == true && startupTask.State == StartupTaskState.Disabled) {
                startupTask.RequestEnableAsync().GetAwaiter().GetResult();
            } else if (value == false && startupTask.State == StartupTaskState.Enabled) {
                startupTask.Disable();
            }
        }
    }

    public static bool LanguageSwitcherEnabled {
        get {
            EnsureLoaded();
            return SettingsEntries.LanguageSwitcherEnabled;
        }
        set {
            EnsureLoaded();
            SettingsEntries.LanguageSwitcherEnabled = value;
            OnSettingsUpdate();
        }
    }

    public static bool FlyoutEnabled {
        get {
            return LanguageSwitcherEnabled &&
                SettingsEntries.FlyoutEnabled;
        }
        set {
            EnsureLoaded();
            SettingsEntries.FlyoutEnabled = value;
            OnSettingsUpdate();
        }
    }

    public static bool DisableFlyoutInFullscreen {
        get {
            return LanguageSwitcherEnabled &&
                FlyoutEnabled &&
                SettingsEntries.DisableFlyoutInFullscreen;
        }
        set {
            EnsureLoaded();
            SettingsEntries.DisableFlyoutInFullscreen = value;
            OnSettingsUpdate();
        }
    }

    public static bool ScrollLockForImeLanguage {
        get {
            return LanguageSwitcherEnabled &&
                SettingsEntries.ScrollLockForImeLanguage;
        }
        set {
            EnsureLoaded();
            SettingsEntries.ScrollLockForImeLanguage = value;
            OnSettingsUpdate();
        }
    }

    public static bool DisableCapsLockOnLanguageChange {
        get {
            return LanguageSwitcherEnabled &&
                SettingsEntries.DisableCapsLockOnLanguageChange;
        }
        set {
            EnsureLoaded();
            SettingsEntries.DisableCapsLockOnLanguageChange = value;
            OnSettingsUpdate();
        }
    }

    public static bool RAltModifierEnabled {
        get {
            return LanguageSwitcherEnabled &&
                SettingsEntries.RAltModifierEnabled;
        }
        set {
            EnsureLoaded();
            SettingsEntries.RAltModifierEnabled = value;
            OnSettingsUpdate();
        }
    }

    public static bool LGuiRemapEnabled {
        get {
            EnsureLoaded();
            return SettingsEntries.LGuiRemapEnabled;
        }
        set {
            EnsureLoaded();
            SettingsEntries.LGuiRemapEnabled = value;
            OnSettingsUpdate();
        }
    }

    public static bool ReverseMouseWheelEnabled {
        get {
            EnsureLoaded();
            return SettingsEntries.ReverseMouseWheelEnabled;
        }
        set {
            EnsureLoaded();
            SettingsEntries.ReverseMouseWheelEnabled = value;
            OnSettingsUpdate();
        }
    }

    public static bool MaxToDesktopEnabled {
        get {
            EnsureLoaded();
            return SettingsEntries.MaxToDesktopEnabled;
        }
        set {
            EnsureLoaded();
            SettingsEntries.MaxToDesktopEnabled = value;
            OnSettingsUpdate();
        }
    }

    public static bool DisableSwapInFullscreen {
        get {
            return MaxToDesktopEnabled &&
                SettingsEntries.DisableSwapInFullscreen;
        }
        set {
            EnsureLoaded();
            SettingsEntries.DisableSwapInFullscreen = value;
            OnSettingsUpdate();
        }
    }

    public static bool SwapVirtualDesktopHotkeysEnabled {
        get {
            return MaxToDesktopEnabled &&
                SettingsEntries.SwapVirtualDesktopHotkeysEnabled;
        }
        set {
            EnsureLoaded();
            SettingsEntries.SwapVirtualDesktopHotkeysEnabled = value;
            OnSettingsUpdate();
        }
    }

    public static uint ReorgnizeDesktopIntervalMs {
        get {
            EnsureLoaded();
            return SettingsEntries.ReorgnizeDesktopIntervalMs;
        }
        set {
            EnsureLoaded();
            SettingsEntries.ReorgnizeDesktopIntervalMs = value;
            OnSettingsUpdate();
        }
    }

    public static void OnSettingsUpdate() {
        SettingsChangedEventHandler?.Invoke(null, EventArgs.Empty);
        Save();
    }

    private static void Load() {
        try {
            var yaml = File.ReadAllText(SaveFilePath);
            var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
            SettingsEntries = deserializer.Deserialize<Entries>(yaml);
        } catch {
            SettingsEntries = new();
        }

        Loaded = true;
    }

    private static async void Save() {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(SettingsEntries);
        Directory.CreateDirectory(SaveFileDir);
        await File.WriteAllTextAsync(SaveFilePath, yaml);
    }
}
