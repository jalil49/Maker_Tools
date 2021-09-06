using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;
using System.Collections.Generic;

namespace Additional_Card_Info
{
    [BepInPlugin(GUID, "Additional Card Info", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", "2.0.0")]

    public partial class Settings : BaseUnityPlugin
    {
        public const string GUID = "Additional_Card_Info";
        public const string Version = "1.5";
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
            StartCoroutine(DelayedInit());
            NamingID = Config.Bind("Grouping ID", "Grouping ID", "4", "Requires restarting maker");
            CreatorName = Config.Bind("User", "Creator", "", "Default Creator name for those who make a lot of coordinates");
            MakerAPI.MakerStartedLoading += CharaEvent.MakerAPI_MakerStartedLoading;
            MakerAPI.RegisterCustomSubCategories += CharaEvent.RegisterCustomSubCategories;
            EventRegister();
        }

        private IEnumerator<int> DelayedInit()
        {
            yield return 0;
            Hooks.Init(Logger);
        }
    }
}
