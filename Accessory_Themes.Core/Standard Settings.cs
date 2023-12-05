using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;

namespace Accessory_Themes
{
    [BepInPlugin(Guid, "Accessory Themes", Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency("Additional_Card_Info")]
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", "2.0.0")]
    public partial class Settings : BaseUnityPlugin
    {
        private const string Guid = "Accessory_Themes";
        public const string Version = "1.6";
        internal static Settings Instance;
        internal new static ManualLogSource Logger;
        public static ConfigEntry<string> NamingID { get; private set; }
        public static ConfigEntry<bool> Enable { get; private set; }

        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            if (StudioAPI.InsideStudio) return;

            CharacterApi.RegisterExtraBehaviour<CharaEvent>(Guid);
            Hooks.Init();

            NamingID = Config.Bind("Grouping ID", "Grouping ID", "3", "Requires restarting maker");
            Enable = Config.Bind("Setting", "Enable", true, "Requires restarting maker");
            MakerAPI.MakerStartedLoading += CharaEvent.MakerAPI_MakerStartedLoading;
            MakerAPI.RegisterCustomSubCategories += CharaEvent.RegisterCustomSubCategories;

            GameUnique();
        }
    }
}