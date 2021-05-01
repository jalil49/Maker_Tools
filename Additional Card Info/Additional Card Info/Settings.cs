using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Studio;

namespace Additional_Card_Info
{
    [BepInPlugin(GUID, "Additional Card Info", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class Settings : BaseUnityPlugin
    {
        public const string GUID = "Additional_Card_Info";
        public const string Version = "1.0";
        internal static Settings Instance;
        internal static new ManualLogSource Logger;
        public static ConfigEntry<string> NamingID { get; private set; }


        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            if (StudioAPI.InsideStudio) return;

            CharacterApi.RegisterExtraBehaviour<CharaEvent>(GUID);
            Harmony.CreateAndPatchAll(typeof(Hooks));
            //ShowTypeInfo(typeof(Required_ACC_Controller));
            NamingID = Config.Bind("Grouping ID", "Grouping ID", "4", "Requires restarting maker");
        }
    }
}
