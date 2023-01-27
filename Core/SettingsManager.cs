using System;
using System.IO;

using Microsoft.UI.Xaml.Input;

using Windows.ApplicationModel;
using Windows.Management.Core;
using Windows.Storage;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FruitLanguageSwitcher.Core {

    //[Serializable]
    public class SettingsManager {
        public struct Options {
            public bool LanguageSwitcherEnabled { get; set; }
            public bool RAltModifierEnabled { get; set; }
            public bool LWinRemapEnabled { get; set; }
            public bool ReverseMouseWheelEnabled { get; set; }
        }
        //private static readonly string SaveFileDir = Path.Combine(
        //    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        //    "FruitLanguageSwitcher");
        //private static readonly string SaveFilePath = Path.Combine(SaveFileDir, "settings.yaml");
        private static readonly ApplicationDataContainer localSettings = 
            ApplicationDataManager.CreateForPackageFamily(Package.Current.Id.FamilyName).LocalSettings;
        private const string SETTINGS_KEY_STR = "settings";

        //[YamlIgnore]
        public Options Settings { get; set; }
        public event EventHandler SettingsChangedEventHandler;

        public SettingsManager() {
            Settings = new Options {
                LanguageSwitcherEnabled = true,
                RAltModifierEnabled = true,
                LWinRemapEnabled = false,
                ReverseMouseWheelEnabled = false
            };

            Save();
        }

        public void DisableLanguageSwitcher() {
            Settings.LanguageSwitcherEnabled = false;
            OnSettingsUpdate();
        }

        public void ToggleLWinRemapEnabled(object _, ExecuteRequestedEventArgs args) {
            LWinRemapEnabled = !LWinRemapEnabled;
            OnSettingsUpdate();
        }

        public void ToggleReverseMouseWheelEnabled(object _, ExecuteRequestedEventArgs args) {
            ReverseMouseWheelEnabled = !ReverseMouseWheelEnabled;
            OnSettingsUpdate();
        }

        public void OnSettingsUpdate() {
            SettingsChangedEventHandler.Invoke(this, EventArgs.Empty);
            this.Save();
        }

        public static SettingsManager Load() {
            //var yaml = File.ReadAllText(SaveFilePath);
            //var deserializer = new DeserializerBuilder()
            //.WithNamingConvention(CamelCaseNamingConvention.Instance)
            //.Build();
            //return deserializer.Deserialize<SettingsManager>(yaml);
            if(localSettings.Values[SETTINGS_KEY_STR] is SettingsManager loaded) {
                return loaded;
            } else {
                return new SettingsManager();
            }
        }

        public void Save() {
            localSettings.Values[SETTINGS_KEY_STR] = this;
            //var serializer = new SerializerBuilder()
            //    .WithNamingConvention(CamelCaseNamingConvention.Instance)
            //    .Build();
            //var yaml = serializer.Serialize(this);
            //Directory.CreateDirectory(SaveFileDir);
            //await File.WriteAllTextAsync(SaveFilePath, yaml);
        }
    }
}
