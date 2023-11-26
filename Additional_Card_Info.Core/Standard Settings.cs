using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;

namespace Additional_Card_Info
{
    [BepInPlugin(GUID, "Additional Card Info", Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", "2.0.0")]
    public partial class Settings : BaseUnityPlugin
    {
        public const string GUID = "Additional_Card_Info";
        public const string Version = "2.0";
        internal static Settings Instance;
        internal new static ManualLogSource Logger;
        public static ConfigEntry<string> NamingID { get; private set; }
        public static ConfigEntry<string> CreatorName { get; private set; }

        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            if (StudioAPI.InsideStudio)
            {
                return;
            }

            CharacterApi.RegisterExtraBehaviour<CharaEvent>(GUID);
            StartCoroutine(DelayedInit());
            NamingID = Config.Bind("Grouping ID", "Grouping ID", "4", "Requires restarting maker");
            CreatorName = Config.Bind("User", "Creator", string.Empty,
                "Default Creator name for those who make a lot of coordinates");
            MakerAPI.MakerStartedLoading += Maker.MakerAPI_MakerStartedLoading;
            MakerAPI.RegisterCustomSubCategories += Maker.RegisterCustomSubCategories;
            GameUnique();
        }

        private IEnumerator<int> DelayedInit()
        {
            yield return 0;
            Hooks.Init(Logger);
        }
    }
}