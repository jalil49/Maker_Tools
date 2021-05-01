using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI.Chara;

namespace Accessory_Parents
{
    [BepInPlugin(GUID, "Accessory Parents", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public class Settings : BaseUnityPlugin
    {
        public const string GUID = "Accessory_Parents";
        public const string Version = "1.0";
        internal static Settings Instance;
        internal static new ManualLogSource Logger;
        public static ConfigEntry<string> NamingID { get; private set; }

        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;
            Hooks.Init();
            CharacterApi.RegisterExtraBehaviour<CharaEvent>(GUID);
            NamingID = Config.Bind("Grouping ID", "Grouping ID", "1", "Requires restarting maker");
        }
    }
}
