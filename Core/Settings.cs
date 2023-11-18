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
        public bool StartUp { get; set; } = true;
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

        public static bool StartUp
        {
            get
            {
                if(!Loaded)
                    Load();

                StartupTask startupTask = StartupTask.GetAsync("MyStartupId").GetAwaiter().GetResult();
                return (startupTask.State == StartupTaskState.Enabled) || (startupTask.State == StartupTaskState.EnabledByPolicy);
            }
            set
            {
                if(!Loaded)
                    Load();

                StartupTask startupTask = StartupTask.GetAsync("MyStartupId").GetAwaiter().GetResult();
                if(value == true && (startupTask.State == StartupTaskState.Disabled || startupTask.State== StartupTaskState.DisabledByUser))
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
                if(!Loaded)
                    Load();
                return SettingsProperties.LanguageSwitcherEnabled;
            }
            set
            {
                if(!Loaded)
                    Load();
                SettingsProperties.LanguageSwitcherEnabled = value;
                OnSettingsUpdate();
            }
        }

        public static bool FlyoutEnabled
        {
            get
            {
                if(!Loaded)
                    Load();
                return SettingsProperties.FlyoutEnabled;
            }
            set
            {
                if(!Loaded)
                    Load();
                SettingsProperties.FlyoutEnabled = value;
                OnSettingsUpdate();
            }
        }
        public static bool RAltModifierEnabled
        {
            get
            {
                if(!Loaded)
                    Load();
                return SettingsProperties.RAltModifierEnabled;
            }
            set
            {
                if(!Loaded)
                    Load();
                SettingsProperties.RAltModifierEnabled = value;
                OnSettingsUpdate();
            }
        }
        public static bool LGuiRemapEnabled
        {
            get
            {
                if(!Loaded)
                    Load();
                return SettingsProperties.LGuiRemapEnabled;
            }
            set
            {
                if(!Loaded)
                    Load();
                SettingsProperties.LGuiRemapEnabled = value;
                OnSettingsUpdate();
            }
        }
        public static bool ReverseMouseWheelEnabled
        {
            get
            {
                if(!Loaded)
                    Load();
                return SettingsProperties.ReverseMouseWheelEnabled;
            }
            set
            {
                if(!Loaded)
                    Load();
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
