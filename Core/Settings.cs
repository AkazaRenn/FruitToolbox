using System;
using System.IO;

using Microsoft.UI.Xaml.Input;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FruitLanguageSwitcher.Core {
    [Serializable]
    public class Settings {
        private static readonly string SaveFileDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FruitLanguageSwitcher");
        private static readonly string SaveFilePath = Path.Combine(SaveFileDir, "settings.yaml");

        public bool LanguageSwitcherEnabled  { get; private set; }
        public bool RAltModifierEnabled      { get; private set; }
        public bool LWinRemapEnabled         { get; private set; }
        public bool ReverseMouseWheelEnabled { get; private set; }

        public event EventHandler SettingsChangedEventHandler;

        public Settings() {
            LanguageSwitcherEnabled = true;
            RAltModifierEnabled = true;
            LWinRemapEnabled = false;
            ReverseMouseWheelEnabled = false;
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

        public static Settings Load() {
            try {
                var yaml = File.ReadAllText(SaveFilePath);
                var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
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
            Directory.CreateDirectory(SaveFileDir);
            await File.WriteAllTextAsync(SaveFilePath, yaml);
        }
    }
}
