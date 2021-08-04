using BepInEx;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;

namespace CardUpdateTool
{
    [BepInPlugin(GUID, "Card Update Tool", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    [BepInDependency(Sideloader.Sideloader.GUID, Sideloader.Sideloader.Version)]
    public partial class Settings : BaseUnityPlugin
    {
        public const string GUID = "Card_Update_Tool";
        public const string Version = "1.0";
        internal static Settings Instance;
        internal static new ManualLogSource Logger;

        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            if (StudioAPI.InsideStudio) return;

            Hooks.Init();
            CharacterApi.RegisterExtraBehaviour<CharaEvent>(GUID, 3000);

            MakerAPI.MakerStartedLoading += (s, e) => CharaEvent.Starting();
            MakerAPI.MakerExiting += (s, e) => CharaEvent.Exiting();
            MakerAPI.RegisterCustomSubCategories += CharaEvent.Register;
        }
    }
}
