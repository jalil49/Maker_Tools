using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Studio;
using System;
namespace Accessory_Themes_and_Info
{
    [BepInPlugin(GUID, "Accessory Themes and Info", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class Settings : BaseUnityPlugin
    {
        public const string GUID = "Accessory_Themes_and_Info";
        public const string Version = "1.0";
        internal static Settings Instance;
        internal static new ManualLogSource Logger;
        public static ConfigEntry<string> NamingID { get; private set; }


        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            if (StudioAPI.InsideStudio) return;

            CharacterApi.RegisterExtraBehaviour<Required_ACC_Controller>(GUID);
            Harmony.CreateAndPatchAll(typeof(Hooks));

            NamingID = Config.Bind("Grouping ID", "Grouping ID", "3", "Requires restarting maker");
        }

        private static void ShowTypeInfo(Type t)
        {
            Logger.LogWarning($"Name: {t.Name}");
            Logger.LogWarning($"Full Name: {t.FullName}");
            Logger.LogWarning($"ToString:  {t}");
            Logger.LogWarning($"Assembly Qualified Name: {t.AssemblyQualifiedName}");
            Logger.LogWarning("");
        }
    }
}
