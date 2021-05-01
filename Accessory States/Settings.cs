using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI.MainGame;

namespace Accessory_States
{
    [BepInPlugin(GUID, "Accessory States", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public class Settings : BaseUnityPlugin
    {
        public const string GUID = "Accessory_States";
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
            CharacterApi.RegisterExtraBehaviour<Dummy>("madevil.kk.ass");
            GameAPI.RegisterExtraBehaviour<GameEvent>(GUID);
            NamingID = Config.Bind("Grouping ID", "Grouping ID", "2", "Requires restarting maker");
        }
    }
}
