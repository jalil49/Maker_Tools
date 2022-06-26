using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Generic.Core;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using KKAPI.Studio;
using System;
using System.Collections.Generic;

namespace Accessory_States
{
    [BepInIncompatibility("madevil.kk.ass")]
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", "2.0.0")]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    [BepInPlugin(GUID, "Accessory States", Version)]

    public partial class Settings : BaseUnityPlugin
    {
        public const string GUID = "Accessory_States";
        public const string Version = "2.0";
        internal static Settings Instance;
        internal static new ManualLogSource Logger;
        internal static bool showstacktrace = false;

        public static ConfigEntry<string> NamingID { get; private set; }
        public static ConfigEntry<bool> Enable { get; private set; }
        public static ConfigEntry<bool> ASS_SAVE { get; private set; }

        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;
            CharacterApi.RegisterExtraBehaviour<CharaEvent>(GUID);
            StartCoroutine(DelayedInit());
            StartCoroutine(Wait());
            IEnumerator<int> Wait()
            {
                yield return 0;
                var ASS_Exists = CharaEvent.ASSExists = TryfindPluginInstance("madevil.kk.ass", new Version("4.1.0.0"));
                if (!ASS_Exists)
                {
                    //Create Dummy Controller to make data visible outside of maker
                    CharacterApi.RegisterExtraBehaviour<Dummy>("madevil.kk.ass");
                }
            }
            GameAPI.RegisterExtraBehaviour<GameEvent>(GUID);

            NamingID = Config.Bind("Grouping ID", "Grouping ID", "2", "Requires restarting maker");
            Enable = Config.Bind("Setting", "Enable", true, "Requires restarting maker");
            ASS_SAVE = Config.Bind("Setting", "Accessory State Sync Save", true, "Save ASS format as well.");
            MakerAPI.MakerStartedLoading += (s, e) => CharaEvent.Maker_started();
            MakerAPI.RegisterCustomSubCategories += CharaEvent.MakerAPI_RegisterCustomSubCategories;

            if (StudioAPI.InsideStudio)
            {
                //CreateStudioControls();
            }
            GameUnique();
        }

        private bool TryfindPluginInstance(string pluginName, Version minimumVersion = null)
        {
            BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(pluginName, out var target);
            if (null != target)
            {
                if (target.Metadata.Version >= minimumVersion)
                {
                    return true;
                }
            }
            return false;
        }

        private IEnumerator<int> DelayedInit()
        {
            yield return 0;
            Hooks.Init(Logger);
        }
    }
}
