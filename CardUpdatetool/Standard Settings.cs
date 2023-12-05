using BepInEx;
using BepInEx.Logging;
using KKAPI;
using KKAPI.Maker;
using KKAPI.Studio;

namespace CardUpdateTool
{
    [BepInPlugin(Guid, "Card Update Tool", Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency(Sideloader.Sideloader.GUID, Sideloader.Sideloader.Version)]
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", "2.0.0")]
    public partial class CardUpdateTool : BaseUnityPlugin
    {
        private const string Guid = "Card_Update_Tool";
        public const string Version = "1.2";
        private static CardUpdateTool _instance;
        private static ManualLogSource _logger;

        public void Awake()
        {
            _instance = this;
            _logger = Logger;

            if (StudioAPI.InsideStudio) return;

            Hooks.Init();

            MakerAPI.MakerStartedLoading += (s, e) => Starting();
            MakerAPI.MakerExiting += (s, e) => Exiting();
            MakerAPI.RegisterCustomSubCategories += Register;
        }
    }
}