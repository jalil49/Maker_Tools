using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Generic.Core;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using KKAPI.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Extensions.GUI_Classes.Config;
#if Studio
using KKAPI.Studio;
#endif

namespace Accessory_States
{
    [BepInIncompatibility("madevil.kk.ass")]
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", "2.0.0")]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    [BepInPlugin(GUID, "Accessory States", Version)]
#if Studio
    [BepInProcess("CharaStudio")]
#endif
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

        #region Maker Data Save
        internal static ConfigEntry<int> MakerFontSize { get; set; }
        #endregion

        public static ConfigEntry<KeyboardShortcut> SlotWindowHotKey { get; private set; }
        public static ConfigEntry<KeyboardShortcut> PreviewWindowHotKey { get; private set; }

#if Studio
        #region Studio Data Save
        internal static ConfigEntry<int> StudioFontSize { get; set; }
        #endregion
#endif

        public void Awake()
        {

            Instance = this;
            Logger = base.Logger;
            CharacterApi.RegisterExtraBehaviour<CharaEvent>(GUID);
            StartCoroutine(DelayedInit());
            StartCoroutine(Wait());
            GameAPI.RegisterExtraBehaviour<GameEvent>(GUID);

            WindowConfig.Register();
            NamingID = Config.Bind("Grouping ID", "Grouping ID", "2", "Requires restarting maker");
            Enable = Config.Bind("Setting", "Enable", true, "Requires restarting maker");
            ASS_SAVE = Config.Bind("Setting", "Accessory State Sync Save", true, "Save ASS format as well.");
            PreviewWindowHotKey = Config.Bind("HotKeys", "Preview Window", new KeyboardShortcut(KeyCode.None), "Toggle the visibility of the Preview Window");
            SlotWindowHotKey = Config.Bind("HotKeys", "Slot Window", new KeyboardShortcut(KeyCode.None), "Toggle the visibility of the Slot Window");

            MakerAPI.MakerStartedLoading += (s, e) => Maker.Maker_started();
            MakerAPI.RegisterCustomSubCategories += Maker.MakerAPI_RegisterCustomSubCategories;
            MakerWindowSetup();
            GUISetup(MakerFontSize.Value);
            GameUnique();
#if Studio
            if(!StudioAPI.InsideStudio)
                return;
            StudioWindowSetup();
            GUISetup(StudioFontSize.Value);
            StudioAPI.StudioLoadedChanged += (val, val2) => { if(_studio == null) _studio = new Studio(); };
#endif
        }
        private void GUISetup(int fontsize)
        {
            Extensions.OnGUIExtensions.FontSize = fontsize;
        }

        private void MakerWindowSetup()
        {
            MakerFontSize = Config.Bind("Gui", "Font Size", 24, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
        }

#if Studio
        private void StudioWindowSetup()
        {
            StudioFontSize = Config.Bind("Gui", "Font Size", 24, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
        }
#endif

        private bool TryfindPluginInstance(string pluginName, Version minimumVersion = null)
        {
            BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(pluginName, out var target);
            if(null != target)
            {
                if(target.Metadata.Version >= minimumVersion)
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

        private IEnumerator<int> Wait()
        {
            yield return 0;
            var ASS_Exists = CharaEvent.ASSExists = TryfindPluginInstance("madevil.kk.ass", new Version("4.1.0.0"));
            if(!ASS_Exists)
            {
                //Create Dummy Controller to make data visible outside of maker
                CharacterApi.RegisterExtraBehaviour<Dummy>("madevil.kk.ass");
            }
        }
    }
}
