using BepInEx;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;

namespace Accessory_Shortcuts
{
    [BepInPlugin(GUID, "Accessory Shortcuts", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class Settings : BaseUnityPlugin
    {
        public const string GUID = "Accessory_Shortcuts";
        public const string Version = "1.3";
        internal static Settings Instance;
        internal static new ManualLogSource Logger;
        //public static ConfigEntry<string> NamingID { get; private set; }

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
            MakerAPI.MakerFinishedLoading += CharaEvent.MakerAPI_MakerFinishedLoading;
            //NamingID = Config.Bind("Grouping ID", "Grouping ID", "99", "Requires restarting maker");
        }
    }
}
