using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Generic.Core;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using KKAPI.Studio;

namespace Accessory_States
{
    [BepInIncompatibility("madevil.kk.ass")]
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", "2.0.0")]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInPlugin(Guid, "Accessory States", Version)]
    public partial class Settings : BaseUnityPlugin
    {
        private const string Guid = "Accessory_States";
        public const string Version = "1.6";
        internal static Settings Instance;
        internal new static ManualLogSource Logger;

        public static ConfigEntry<string> NamingID { get; private set; }
        public static ConfigEntry<bool> Enable { get; private set; }
        public static ConfigEntry<bool> AssSave { get; private set; }

        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;
            Hooks.Init();
            CharacterApi.RegisterExtraBehaviour<CharaEvent>(Guid);
            StartCoroutine(DelayedInit());
            StartCoroutine(Wait());

            GameAPI.RegisterExtraBehaviour<GameEvent>(Guid);

            NamingID = Config.Bind("Grouping ID", "Grouping ID", "2", "Requires restarting maker");
            Enable = Config.Bind("Setting", "Enable", true, "Requires restarting maker");
            AssSave = Config.Bind("Setting", "Accessory State Sync Save", true, "Save ASS format as well.");
            MakerAPI.MakerStartedLoading += (s, e) => CharaEvent.Maker_started();
            MakerAPI.RegisterCustomSubCategories += CharaEvent.MakerAPI_RegisterCustomSubCategories;

            if (StudioAPI.InsideStudio) CreateStudioControls();

            GameUnique();
            return;

            IEnumerator<int> Wait()
            {
                yield return 0;
                var assExists = CharaEvent.AssExists = TryFindPluginInstance("madevil.kk.ass", new Version("4.1.0.0"));
                if (!assExists) CharacterApi.RegisterExtraBehaviour<Dummy>("madevil.kk.ass");
            }
        }

        private bool TryFindPluginInstance(string pluginName, Version minimumVersion = null)
        {
            Chainloader.PluginInfos.TryGetValue(pluginName, out var target);
            if (null != target)
                if (target.Metadata.Version >= minimumVersion)
                    return true;
            return false;
        }

        private static IEnumerator<int> DelayedInit()
        {
            yield return 0;
            Hooks.Init();
        }
    }
}