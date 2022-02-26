//
// Configuration Entries
//

using BepInEx.Configuration;

using KKAPI.Utilities;

using UnityEngine;

using static GUIDrawer;

namespace CardUpdateTool
{
    public partial class CardUpdateTool
    {
        internal void ConfigEntries()
        {
            var sectionKeys = "Window Settings";

            Window.FontSize = Config.Bind(
                section: sectionKeys,
                key: "Font Size",
                defaultValue: -1,
                configDescription: new ConfigDescription(
                    description: "Font size to use in window.",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 20 }));

            Window.FontSize.SettingChanged += (_sender, _args) =>
            {
                if (labelstyle != null)
                {
                    if (labelstyle.fontSize != Window.FontSize.Value)
                    {
                        if (Window.FontSize.Value < 0)
                        {
                            SetFontSize(Window.Defaults.fontSize);
                        }
                        else
                        {
                            SetFontSize(Window.FontSize.Value);
                        }
                    }
                }
            };

            Window.Y = Config.Bind(
                section: sectionKeys,
                key: "Top",
                defaultValue: -1,
                configDescription: new ConfigDescription(
                    description: "Window top position.",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 18 }));
            Window.Y.SettingChanged += (_sender, _args) =>
            {
                if (screenRect != null)
                {
                    if (screenRect.y != Window.Y.Value)
                    {
                        if (Window.Y.Value < 0)
                        {
                            screenRect.y = Window.Defaults.y;
                        }
                        else 
                        { 
                            screenRect.y = Window.Y.Value; 
                        }
                    }
                }
            };

            Window.X = Config.Bind(
                section: sectionKeys,
                key: "Top Right",
                defaultValue: -1,
                configDescription: new ConfigDescription(
                    description: "Widow top left margin.",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 16 }));
            Window.X.SettingChanged += (_sender, _args) =>
            {
                if (screenRect != null)
                {
                    if (screenRect.x != Window.X.Value)
                    {
                        if (Window.X.Value < 0)
                        {
                            screenRect.x = Window.Defaults.x;
                        }
                        else
                        {
                            screenRect.x = Window.X.Value;
                        }
                    }
                }
            };


            Window.Height = Config.Bind(
                section: sectionKeys,
                key: "Height",
                defaultValue: -1,
                configDescription: new ConfigDescription(
                    description: "Window height.",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 16 }));
            Window.Height.SettingChanged += (_sender, _args) =>
            {
                if (screenRect != null)
                {
                    if (screenRect.height != Window.Height.Value)
                    {
                        if (Window.Height.Value < 0)
                        {
                            screenRect.height = Window.Defaults.height;
                        }
                        else
                        {
                            screenRect.height = Window.Height.Value;
                        }
                    }
                }
            };

            Window.Width = Config.Bind(
                section: sectionKeys,
                key: "Width",
                defaultValue: -1,
                configDescription: new ConfigDescription(
                    description: "Window width.",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 16 }));
            Window.Width.SettingChanged += (_sender, _args) =>
            {
                if (screenRect != null)
                {
                    if (screenRect.width != Window.Width.Value)
                    {
                        if (Window.Width.Value < 0)
                        {
                            screenRect.width = Window.Defaults.width;
                        }
                        else
                        {
                            screenRect.width = Window.Width.Value;
                        }
                    }
                }
            };
        }
    }
}
