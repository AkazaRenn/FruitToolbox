using Windows.ApplicationModel;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FruitToolbox.Settings;

[Serializable]
internal class Entries
{
    public bool LanguageSwitcherEnabled { get; set; } = true;
    public bool FlyoutEnabled { get; set; } = true;
    public bool RAltModifierEnabled { get; set; } = true;
    public bool LGuiRemapEnabled { get; set; } = false;
    public bool ReverseMouseWheelEnabled { get; set; } = false;
    public bool DesktopToHomeEnabled { get; set; } = false;
    public uint ReorgnizeDesktopDelaySec { get; set; } = 5;
}

public static class Core
{
    private static readonly string SaveFileDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string SaveFilePath = Path.Combine(SaveFileDir, "settings.yaml");

    public static event EventHandler SettingsChangedEventHandler;
    private static Entries SettingsEntries;
    private static bool Loaded = false;

    private static void EnsureLoaded()
    {
        if (!Loaded)
            Load();
    }

    public static bool StartUp
    {
        get
        {
            StartupTask startupTask = StartupTask.GetAsync("MyStartupId").GetAwaiter().GetResult();
            return startupTask.State == StartupTaskState.Enabled || startupTask.State == StartupTaskState.EnabledByPolicy;
        }
        set
        {
            StartupTask startupTask = StartupTask.GetAsync("MyStartupId").GetAwaiter().GetResult();
            if(value == true && startupTask.State == StartupTaskState.Disabled)
            {
                startupTask.RequestEnableAsync().GetAwaiter().GetResult();
            } else if(value == false && startupTask.State == StartupTaskState.Enabled)
            {
                startupTask.Disable();
            }
        }
    }

    public static bool LanguageSwitcherEnabled
    {
        get
        {
            EnsureLoaded();
            return SettingsEntries.LanguageSwitcherEnabled;
        }
        set
        {
            EnsureLoaded();
            SettingsEntries.LanguageSwitcherEnabled = value;
            OnSettingsUpdate();
        }
    }

    public static bool FlyoutEnabled
    {
        get
        {
            EnsureLoaded();
            return SettingsEntries.FlyoutEnabled;
        }
        set
        {
            EnsureLoaded();
            SettingsEntries.FlyoutEnabled = value;
            OnSettingsUpdate();
        }
    }
    public static bool RAltModifierEnabled
    {
        get
        {
            EnsureLoaded();
            return SettingsEntries.RAltModifierEnabled;
        }
        set
        {
            EnsureLoaded();
            SettingsEntries.RAltModifierEnabled = value;
            OnSettingsUpdate();
        }
    }
    public static bool LGuiRemapEnabled
    {
        get
        {
            EnsureLoaded();
            return SettingsEntries.LGuiRemapEnabled;
        }
        set
        {
            EnsureLoaded();
            SettingsEntries.LGuiRemapEnabled = value;
            OnSettingsUpdate();
        }
    }
    public static bool ReverseMouseWheelEnabled
    {
        get
        {
            EnsureLoaded();
            return SettingsEntries.ReverseMouseWheelEnabled;
        }
        set
        {
            EnsureLoaded();
            SettingsEntries.ReverseMouseWheelEnabled = value;
            OnSettingsUpdate();
        }
    }

    public static bool DesktopToHomeEnabled
    {
        get
        {
            EnsureLoaded();
            return SettingsEntries.DesktopToHomeEnabled;
        }
        set
        { 
            EnsureLoaded();
            SettingsEntries.DesktopToHomeEnabled = value;
            OnSettingsUpdate();
        }
    }

    public static uint ReorgnizeDesktopDelaySec
    {
        get
        {
            EnsureLoaded();
            return SettingsEntries.ReorgnizeDesktopDelaySec;
        }
        set { 
            EnsureLoaded();
            SettingsEntries.ReorgnizeDesktopDelaySec = value;
            OnSettingsUpdate();
        }
    }

    public static void OnSettingsUpdate()
    {
        SettingsChangedEventHandler.Invoke(null, EventArgs.Empty);
        Save();
    }

    private static void Load()
    {
        try
        {
            var yaml = File.ReadAllText(SaveFilePath);
            var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
            SettingsEntries = deserializer.Deserialize<Entries>(yaml);
        }
        catch
        {
            SettingsEntries = new();
        }

        Loaded = true;
    }

    private static async void Save()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(SettingsEntries);
        Directory.CreateDirectory(SaveFileDir);
        await File.WriteAllTextAsync(SaveFilePath, yaml);
    }
}
