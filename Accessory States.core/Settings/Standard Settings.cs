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
        internal static ConfigEntry<Rect> SlotWindowRectMaker { get; set; }
        internal static ConfigEntry<Rect> GroupWindowRectMaker { get; set; }
        internal static ConfigEntry<Rect> PresetWindowRectMaker { get; set; }
        internal static ConfigEntry<Rect> AddBindingWindowRectMaker { get; set; }
        internal static ConfigEntry<Rect> PreviewWindowRectMaker { get; set; }
        internal static ConfigEntry<Rect> SettingWindowRectMaker { get; set; }

        internal static ConfigEntry<int> SlotWindowTransparencyMaker { get; set; }
        internal static ConfigEntry<int> GroupWindowTransparencyMaker { get; set; }
        internal static ConfigEntry<int> PresetWindowTransparencyMaker { get; set; }
        internal static ConfigEntry<int> AddBindingWindowTransparencyMaker { get; set; }
        internal static ConfigEntry<int> PreviewWindowTransparencyMaker { get; set; }
        internal static ConfigEntry<int> SettingWindowTransparencyMaker { get; set; }
        #endregion

        public static ConfigEntry<KeyCode> SlotWindowShortcut { get; private set; }
        public static ConfigEntry<KeyCode> PreviewWindowShortcut { get; private set; }

#if Studio
        #region Studio Data Save
        internal static ConfigEntry<int> StudioFontSize { get; set; }
        internal static ConfigEntry<Rect> SlotWindowRectStudio { get; set; }
        internal static ConfigEntry<Rect> GroupWindowRectStudio { get; set; }
        internal static ConfigEntry<Rect> PresetWindowRectStudio { get; set; }
        internal static ConfigEntry<Rect> AddBindingWindowRectStudio { get; set; }
        internal static ConfigEntry<Rect> PreviewWindowRectStudio { get; set; }
        internal static ConfigEntry<Rect> SettingWindowRectStudio { get; set; }
        internal static ConfigEntry<Rect> CharaSelectWindowRectStudio { get; set; }
        internal static ConfigEntry<Rect> AccessorySelectWindowRectStudio { get; set; }

        internal static ConfigEntry<int> SlotWindowTransparencyStudio { get; set; }
        internal static ConfigEntry<int> GroupWindowTransparencyStudio { get; set; }
        internal static ConfigEntry<int> PresetWindowTransparencyStudio { get; set; }
        internal static ConfigEntry<int> AddBindingWindowTransparencyStudio { get; set; }
        internal static ConfigEntry<int> PreviewWindowTransparencyStudio { get; set; }
        internal static ConfigEntry<int> SettingWindowTransparencyStudio { get; set; }
        internal static ConfigEntry<int> CharaSelectWindowTransparencyStudio { get; set; }
        internal static ConfigEntry<int> AccessorySelectWindowTransparencyStudio { get; set; }
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

            NamingID = Config.Bind("Grouping ID", "Grouping ID", "2", "Requires restarting maker");
            Enable = Config.Bind("Setting", "Enable", true, "Requires restarting maker");
            ASS_SAVE = Config.Bind("Setting", "Accessory State Sync Save", true, "Save ASS format as well.");
            PreviewWindowShortcut = Config.Bind("Toggles", "Preview Window", KeyCode.None, "Toggle the visibility of the Preview Window");
            SlotWindowShortcut = Config.Bind("Toggles", "Slot Window", KeyCode.None, "Toggle the visibility of the Slot Window");

            MakerAPI.MakerStartedLoading += (s, e) => Maker.Maker_started();
            MakerAPI.RegisterCustomSubCategories += Maker.MakerAPI_RegisterCustomSubCategories;
            MakerWindowSetup();
            GUISetup(MakerFontSize.Value);
            GameUnique();
#if Studio
            if (!StudioAPI.InsideStudio)
                return;
            StudioWindowSetup();
            GUISetup(StudioFontSize.Value);
            StudioAPI.StudioLoadedChanged += (val, val2) => { if (_studio == null) _studio = new Studio(); };
#endif
        }
        private void GUISetup(int fontsize)
        {
            Extensions.OnGUIExtensions.FontSize = fontsize;
        }

        private void MakerWindowSetup()
        {
            MakerFontSize = Config.Bind("Gui", "Font Size", 24, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            SlotWindowRectMaker = Config.Bind(new ConfigDefinition("Maker Windows", "SlotWindowRectMaker"), new Rect(Screen.width * 0.33f, Screen.height * 0.1f, Screen.width * 0.14f, Screen.height * 0.2f), new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            GroupWindowRectMaker = Config.Bind(new ConfigDefinition("Maker Windows", "GroupWindowRectMaker"), new Rect(Screen.width * 0.475f, Screen.height * 0.3f, Screen.width * 0.128f, Screen.height * 0.2f), new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            PresetWindowRectMaker = Config.Bind(new ConfigDefinition("Maker Windows", "PresetWindowRectMaker"), new Rect(Screen.width * 0.33f, Screen.height * 0.3f, Screen.width * 0.14f, Screen.height * 0.2f), new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            AddBindingWindowRectMaker = Config.Bind(new ConfigDefinition("Maker Windows", "AddBindingWindowRectMaker"), new Rect(Screen.width * 0.475f, Screen.height * 0.1f, Screen.width * 0.075f, Screen.height * 0.2f), new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            PreviewWindowRectMaker = Config.Bind(new ConfigDefinition("Maker Windows", "PreviewWindowRectMaker"), new Rect(Screen.width * 0.80f, Screen.height * 0.2f, Screen.width * 0.076f, Screen.height * 0.75f), new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            SettingWindowRectMaker = Config.Bind(new ConfigDefinition("Maker Windows", "SettingWindowRectMaker"), new Rect(Screen.width * 0.57f, Screen.height * 0.1f, Screen.width * 0.14f, Screen.height * 0.2f), new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));

            SlotWindowTransparencyMaker = Config.Bind(new ConfigDefinition("Maker Windows", "SlotWindowTransparencyMaker"), 100, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            GroupWindowTransparencyMaker = Config.Bind(new ConfigDefinition("Maker Windows", "GroupWindowTransparencyMaker"), 100, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            PresetWindowTransparencyMaker = Config.Bind(new ConfigDefinition("Maker Windows", "PresetWindowTransparencyMaker"), 100, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            AddBindingWindowTransparencyMaker = Config.Bind(new ConfigDefinition("Maker Windows", "AddBindingWindowTransparencyMaker"), 100, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            PreviewWindowTransparencyMaker = Config.Bind(new ConfigDefinition("Maker Windows", "PreviewWindowTransparencyMaker"), 60, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            SettingWindowTransparencyMaker = Config.Bind(new ConfigDefinition("Maker Windows", "SettingWindowTransparencyMaker"), 100, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
        }

#if Studio
        private void StudioWindowSetup()
        {
            StudioFontSize = Config.Bind("Gui", "Font Size", 24, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            SlotWindowRectStudio = Config.Bind(new ConfigDefinition("Studio Windows", "SlotWindowRectStudio"), new Rect(Screen.width * 0.33f, Screen.height * 0.1f, Screen.width * 0.14f, Screen.height * 0.2f), new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            GroupWindowRectStudio = Config.Bind(new ConfigDefinition("Studio Windows", "GroupWindowRectStudio"), new Rect(Screen.width * 0.475f, Screen.height * 0.3f, Screen.width * 0.128f, Screen.height * 0.2f), new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            PresetWindowRectStudio = Config.Bind(new ConfigDefinition("Studio Windows", "PresetWindowRectStudio"), new Rect(Screen.width * 0.33f, Screen.height * 0.3f, Screen.width * 0.14f, Screen.height * 0.2f), new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            AddBindingWindowRectStudio = Config.Bind(new ConfigDefinition("Studio Windows", "AddBindingWindowRectStudio"), new Rect(Screen.width * 0.475f, Screen.height * 0.1f, Screen.width * 0.075f, Screen.height * 0.2f), new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            PreviewWindowRectStudio = Config.Bind(new ConfigDefinition("Studio Windows", "PreviewWindowRectStudio"), new Rect(Screen.width * 0.80f, Screen.height * 0.2f, Screen.width * 0.076f, Screen.height * 0.75f), new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            SettingWindowRectStudio = Config.Bind(new ConfigDefinition("Studio Windows", "SettingWindowRectStudio"), new Rect(Screen.width * 0.57f, Screen.height * 0.1f, Screen.width * 0.14f, Screen.height * 0.2f), new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            CharaSelectWindowRectStudio = Config.Bind(new ConfigDefinition("Studio Windows", "CharaSelectWindowRectStudio"), new Rect(Screen.width * 0.57f, Screen.height * 0.1f, Screen.width * 0.14f, Screen.height * 0.2f), new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            AccessorySelectWindowRectStudio = Config.Bind(new ConfigDefinition("Studio Windows", "AccessorySelectWindowRectStudio"), new Rect(Screen.width * 0.57f, Screen.height * 0.1f, Screen.width * 0.14f, Screen.height * 0.2f), new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));


            SlotWindowTransparencyStudio = Config.Bind(new ConfigDefinition("Studio Windows", "SlotWindowTransparencyStudio"), 100, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            GroupWindowTransparencyStudio = Config.Bind(new ConfigDefinition("Studio Windows", "GroupWindowTransparencyStudio"), 100, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            PresetWindowTransparencyStudio = Config.Bind(new ConfigDefinition("Studio Windows", "PresetWindowTransparencyStudio"), 100, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            AddBindingWindowTransparencyStudio = Config.Bind(new ConfigDefinition("Studio Windows", "AddBindingWindowTransparencyStudio"), 100, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            PreviewWindowTransparencyStudio = Config.Bind(new ConfigDefinition("Studio Windows", "PreviewWindowTransparencyStudio"), 60, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            SettingWindowTransparencyStudio = Config.Bind(new ConfigDefinition("Studio Windows", "SettingWindowTransparencyStudio"), 100, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            CharaSelectWindowTransparencyStudio = Config.Bind(new ConfigDefinition("Studio Windows", "CharaSelectWindowTransparencyStudio"), 100, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
            AccessorySelectWindowTransparencyStudio = Config.Bind(new ConfigDefinition("Studio Windows", "AccessorySelectWindowTransparencyStudio"), 100, new ConfigDescription("", null, new ConfigurationManagerAttributes() { Browsable = false }));
        }
#endif

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

        private IEnumerator<int> Wait()
        {
            yield return 0;
            var ASS_Exists = CharaEvent.ASSExists = TryfindPluginInstance("madevil.kk.ass", new Version("4.1.0.0"));
            if (!ASS_Exists)
            {
                //Create Dummy Controller to make data visible outside of maker
                CharacterApi.RegisterExtraBehaviour<Dummy>("madevil.kk.ass");
            }
        }
    }
}
