using System;
using System.IO;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FruitLanguageSwitcher {
    [Serializable]
    public class Settings {
        public static string SaveFileDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FruitLanguageSwitcher");
        public static string SaveFilePath = Path.Combine(SaveFileDir, "settings.yaml");

        public bool LanguageSwitcherEnabled = true;
        public bool RAltModifierEnabled = true;
        public bool LWinRemapEnabled = false;

        //public event EventHandler SettingsChangedEventHandler;

        public Settings() { }

        //public void NotifySettingsUpdate() {
        //    SettingsChangedEventHandler.Invoke(this, EventArgs.Empty);
        //}

        public static Settings Load() {
            try {
                var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)  // see height_in_inches in sample yml 
                .Build();
                string yaml = System.IO.File.ReadAllText(SaveFilePath);
                return deserializer.Deserialize<Settings>(yaml);
            } catch {
                return new Settings();
            }
        }

        public async void Save() {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var yaml = serializer.Serialize(this);
            System.IO.Directory.CreateDirectory(SaveFileDir);
            await File.WriteAllTextAsync(SaveFilePath, yaml);
        }
    }
}
