using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Hook_Space;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;

namespace Additional_Card_Info
{
    [BepInPlugin(GUID, "Additional Card Info", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class Settings : BaseUnityPlugin
    {
        public const string GUID = "Additional_Card_Info";
        public const string Version = "1.2";
        internal static Settings Instance;
        internal static new ManualLogSource Logger;
        public static ConfigEntry<string> NamingID { get; private set; }
        public static ConfigEntry<string> CreatorName { get; private set; }

        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            if (StudioAPI.InsideStudio) return;

            CharacterApi.RegisterExtraBehaviour<CharaEvent>(GUID);
            Hooks.Init(Logger);
            NamingID = Config.Bind("Grouping ID", "Grouping ID", "4", "Requires restarting maker");
            CreatorName = Config.Bind("User", "Creator", "", "Default Creator name for those who make a lot of coordinates");
            MakerAPI.MakerStartedLoading += CharaEvent.MakerAPI_MakerStartedLoading;
            MakerAPI.RegisterCustomSubCategories += CharaEvent.RegisterCustomSubCategories;
        }
    }
}
