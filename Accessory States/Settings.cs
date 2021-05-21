using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using System;
using System.Collections;

namespace Accessory_States
{
    [BepInPlugin(GUID, "Accessory States", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public class Settings : BaseUnityPlugin
    {
        public const string GUID = "Accessory_States";
        public const string Version = "1.2";
        internal static Settings Instance;
        internal static new ManualLogSource Logger;
        public static ConfigEntry<string> NamingID { get; private set; }
        public static ConfigEntry<bool> Enable { get; private set; }

        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;
            Hooks.Init();
            CharacterApi.RegisterExtraBehaviour<CharaEvent>(GUID);

            StartCoroutine(Wait());
            IEnumerator Wait()
            {
                yield return null;
                if (!TryfindPluginInstance("madevil.kk.ass"))
                {
                    CharacterApi.RegisterExtraBehaviour<Dummy>("madevil.kk.ass");
                }
            }
            GameAPI.RegisterExtraBehaviour<GameEvent>(GUID);
            NamingID = Config.Bind("Grouping ID", "Grouping ID", "2", "Requires restarting maker");
            Enable = Config.Bind("Setting", "Enable", true, "Requires restarting maker");
            MakerAPI.MakerStartedLoading += (s, e) => CharaEvent.Maker_started();
            MakerAPI.RegisterCustomSubCategories += CharaEvent.MakerAPI_RegisterCustomSubCategories;
        }

        private bool TryfindPluginInstance(string pluginName, Version minimumVersion = null)
        {
            BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(pluginName, out PluginInfo target);
            if (null != target)
            {
                if (target.Metadata.Version >= minimumVersion)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
