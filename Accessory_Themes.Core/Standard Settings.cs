﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;

namespace Accessory_Themes
{
    [BepInPlugin(GUID, "Accessory Themes", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    [BepInDependency(Additional_Card_Info.Settings.GUID, Additional_Card_Info.Settings.Version)]
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", "2.0.0")]
    public partial class Settings : BaseUnityPlugin
    {
        public const string GUID = "Accessory_Themes";
        public const string Version = "2.0";
        internal static Settings Instance;
        internal static new ManualLogSource Logger;
        public static ConfigEntry<string> NamingID { get; private set; }
        public static ConfigEntry<bool> Enable { get; private set; }

        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            if (StudioAPI.InsideStudio) return;

            CharacterApi.RegisterExtraBehaviour<CharaEvent>(GUID);
            Hooks.Init(Logger);

            NamingID = Config.Bind("Grouping ID", "Grouping ID", "3", "Requires restarting maker");
            Enable = Config.Bind("Setting", "Enable", true, "Requires restarting maker");
            MakerAPI.MakerStartedLoading += CharaEvent.MakerAPI_MakerStartedLoading;
            MakerAPI.RegisterCustomSubCategories += CharaEvent.RegisterCustomSubCategories;

            GameUnique();
        }
    }
}
