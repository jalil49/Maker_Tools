using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;

namespace Accessory_Parents
{
    [BepInPlugin(GUID, "Accessory Parents", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public class Settings : BaseUnityPlugin
    {
        public const string GUID = "Accessory_Parents";
        public const string Version = "1.1";
        internal static Settings Instance;
        internal static new ManualLogSource Logger;
        public static ConfigEntry<string> NamingID { get; private set; }
        public static ConfigEntry<bool> Enable { get; private set; }

        public void Awake()
        {
            if (StudioAPI.InsideStudio)
            {
                return;
            }
            Instance = this;
            Logger = base.Logger;
            Hooks.Init();
            CharacterApi.RegisterExtraBehaviour<CharaEvent>(GUID);
            NamingID = Config.Bind("Grouping ID", "Grouping ID", "1", "Requires restarting maker");
            Enable = Config.Bind("Setting", "Enable", true, "Requires restarting maker");

            MakerAPI.MakerStartedLoading += CharaEvent.MakerAPI_MakerStartedLoading;
            MakerAPI.RegisterCustomSubCategories += CharaEvent.MakerAPI_RegisterCustomSubCategories;
            MakerAPI.ReloadCustomInterface += CharaEvent.MakerAPI_ReloadCustomInterface;
        }
    }
}
