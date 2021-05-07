using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI.Chara;

namespace Template_Accessories
{
    [BepInPlugin(GUID, "Accessory Template", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public class Settings : BaseUnityPlugin
    {
        public const string GUID = "Accessory_Template";
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
            NamingID = Config.Bind("Grouping ID", "Grouping ID", "99", "Requires restarting maker");
        }
    }
}
