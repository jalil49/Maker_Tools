using System.Collections.Generic;
using BepInEx.Configuration;
using KKAPI.Utilities;
using UnityEngine;

namespace Extensions.GUI_Classes.Config
{
    public class WindowConfig
    {
        private static readonly Dictionary<KeyValuePair<string, string>, ConfigEntry<WindowConfig>> ConfigDictionary =
            new Dictionary<KeyValuePair<string, string>, ConfigEntry<WindowConfig>>();

        public float Transparency = 1f;
        public Rect WindowRect;

        public WindowConfig(Rect windowRect)
        {
            WindowRect = windowRect;
            Transparency = 1f;
        }

        public WindowConfig(Rect windowRect, float transparency)
        {
            WindowRect = windowRect;
            Transparency = transparency;
        }

        public static void Register()
        {
            TomlTypeConverter.AddConverter(typeof(WindowConfig), new TypeConverter
            {
                ConvertToString = (obj, type) => JsonUtility.ToJson(obj),
                ConvertToObject = (str, type) => JsonUtility.FromJson(type: type, json: str)
            });
        }

        public static ConfigEntry<WindowConfig> GetConfigEntry(ConfigFile Config, string section, string key,
                                                               WindowConfig DefaultWindow)
        {
            var keyval = new KeyValuePair<string, string>(section, key);
            if (ConfigDictionary.TryGetValue(keyval, out var configEntry))
            {
                return configEntry;
            }

            return ConfigDictionary[keyval] = Config.Bind(new ConfigDefinition(section, key), DefaultWindow,
                new ConfigDescription(string.Empty, null, new ConfigurationManagerAttributes { Browsable = false }));
        }

        public static void Clear()
        {
            ConfigDictionary.Clear();
        }
    }
}