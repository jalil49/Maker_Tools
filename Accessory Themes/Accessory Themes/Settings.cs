using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Studio;

namespace Accessory_Themes
{
    [BepInPlugin(GUID, "Accessory Themes", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    [BepInDependency("Additional_Card_Info", BepInDependency.DependencyFlags.HardDependency)]
    public partial class Settings : BaseUnityPlugin
    {
        public const string GUID = "Accessory_Themes";
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

            NamingID = Config.Bind("Grouping ID", "Grouping ID", "3", "Requires restarting maker");
        }

        //private static void ShowTypeInfo(Type t)
        //{
        //    Logger.LogWarning($"Name: {t.Name}");
        //    Logger.LogWarning($"Full Name: {t.FullName}");
        //    Logger.LogWarning($"ToString:  {t}");
        //    Logger.LogWarning($"Assembly Qualified Name: {t.AssemblyQualifiedName}");
        //    Logger.LogWarning("");
        //}
    }
}
