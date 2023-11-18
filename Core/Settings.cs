using System;
using System.IO;

using Microsoft.UI.Xaml.Input;

using Windows.ApplicationModel;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FruitLanguageSwitcher.Core
{
    [Serializable]
    internal class SettingsProperties
    {
        [YamlIgnore]
        public static bool StartUp
        {
            get
            {
                StartupTask startupTask = StartupTask.GetAsync("MyStartupId").GetAwaiter().GetResult();
                return (startupTask.State == StartupTaskState.Enabled) || (startupTask.State == StartupTaskState.EnabledByPolicy);
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
        public bool LanguageSwitcherEnabled { get; set; } = true;
        public bool FlyoutEnabled { get; set; } = true;
        public bool RAltModifierEnabled { get; set; } = true;
        public bool LGuiRemapEnabled { get; set; } = false;
        public bool ReverseMouseWheelEnabled { get; set; } = false;
    }

    public static class Settings
    {
        private static readonly string SaveFileDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FruitLanguageSwitcher");
        private static readonly string SaveFilePath = Path.Combine(SaveFileDir, "settings.yaml");

        public static event EventHandler SettingsChangedEventHandler;
        private static SettingsProperties SettingsProperties = new();
        private static bool Loaded = false;

        private static void EnsureLoaded() {
            if(!Loaded)
                Load();
        }

        public static bool StartUp
        {
            get
            {
                EnsureLoaded();
                return SettingsProperties.StartUp;
            }
            set
            {
                EnsureLoaded();
                SettingsProperties.StartUp = value;
            }
        }

        public static bool LanguageSwitcherEnabled
        {
            get
            {
                EnsureLoaded();
                return SettingsProperties.LanguageSwitcherEnabled;
            }
            set
            {
                EnsureLoaded();
                SettingsProperties.LanguageSwitcherEnabled = value;
                OnSettingsUpdate();
            }
        }

        public static bool FlyoutEnabled
        {
            get
            {
                EnsureLoaded();
                return SettingsProperties.FlyoutEnabled;
            }
            set
            {
                EnsureLoaded();
                SettingsProperties.FlyoutEnabled = value;
                OnSettingsUpdate();
            }
        }
        public static bool RAltModifierEnabled
        {
            get
            {
                EnsureLoaded();
                return SettingsProperties.RAltModifierEnabled;
            }
            set
            {
                EnsureLoaded();
                SettingsProperties.RAltModifierEnabled = value;
                OnSettingsUpdate();
            }
        }
        public static bool LGuiRemapEnabled
        {
            get
            {
                EnsureLoaded();
                return SettingsProperties.LGuiRemapEnabled;
            }
            set
            {
                EnsureLoaded();
                SettingsProperties.LGuiRemapEnabled = value;
                OnSettingsUpdate();
            }
        }
        public static bool ReverseMouseWheelEnabled
        {
            get
            {
                EnsureLoaded();
                return SettingsProperties.ReverseMouseWheelEnabled;
            }
            set
            {
                EnsureLoaded();
                SettingsProperties.ReverseMouseWheelEnabled = value;
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
                SettingsProperties = deserializer.Deserialize<SettingsProperties>(yaml);
            } catch
            {
                SettingsProperties = new();
            }

            Loaded = true;
        }

        private static async void Save()
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var yaml = serializer.Serialize(SettingsProperties);
            Directory.CreateDirectory(SaveFileDir);
            await File.WriteAllTextAsync(SaveFilePath, yaml);
        }
    }
}
