using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;

namespace Accessory_Parents
{
    [BepInPlugin(Guid, "Accessory Parents", Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", "2.0.0")]
    public partial class Settings : BaseUnityPlugin
    {
        public const string Guid = "Accessory_Parents";
        public const string Version = "1.6";
        internal static Settings Instance;
        internal new static ManualLogSource Logger;
        public static ConfigEntry<string> NamingID { get; private set; }
        public static ConfigEntry<bool> Enable { get; private set; }

        public void Awake()
        {
            if (StudioAPI.InsideStudio) return;
            Instance = this;
            Logger = base.Logger;
            StartCoroutine(DelayedInit());
            CharacterApi.RegisterExtraBehaviour<CharaEvent>(Guid);
            NamingID = Config.Bind("Grouping ID", "Grouping ID", "1", "Requires restarting maker");
            Enable = Config.Bind("Setting", "Enable", true, "Requires restarting maker");

            MakerAPI.MakerStartedLoading += CharaEvent.MakerAPI_MakerStartedLoading;
            MakerAPI.MakerExiting += CharaEvent.MakerAPI_MakerExiting;
            MakerAPI.RegisterCustomSubCategories += CharaEvent.MakerAPI_RegisterCustomSubCategories;

            GameUnique();
        }

        private static IEnumerator<int> DelayedInit()
        {
            yield return 0;
            Hooks.Init(Logger);
        }
    }
}