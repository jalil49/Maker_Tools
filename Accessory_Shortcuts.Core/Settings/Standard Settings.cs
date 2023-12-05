using BepInEx;
using BepInEx.Logging;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Studio;

namespace Accessory_Shortcuts
{
    [BepInPlugin(Guid, "Accessory Shortcuts", Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", "2.0.0")]
    public partial class Settings : BaseUnityPlugin
    {
        private const string Guid = "Accessory_Shortcuts";
        public const string Version = "1.6";
        internal static Settings Instance;
        internal new static ManualLogSource Logger;

        public void Awake()
        {
            if (StudioAPI.InsideStudio) return;
            Instance = this;
            Logger = base.Logger;
            Hooks.Init();
            CharacterApi.RegisterExtraBehaviour<CharaEvent>(Guid);
        }
    }
}