﻿using BepInEx;
using BepInEx.Logging;
using KKAPI.Maker;
using KKAPI.Studio;

namespace CardUpdateTool
{
    [BepInPlugin(GUID, "Card Update Tool", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    [BepInDependency(Sideloader.Sideloader.GUID, Sideloader.Sideloader.Version)]
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", "2.0.0")]
    public partial class CardUpdateTool : BaseUnityPlugin
    {
        public const string GUID = "Card_Update_Tool";
        public const string Version = "1.2";
        internal static CardUpdateTool Instance;
        internal static new ManualLogSource Logger;

        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            if (StudioAPI.InsideStudio) return;

            Hooks.Init();

            // Saving state in standard configuration file
            ConfigEntries();

            MakerAPI.MakerStartedLoading += (s, e) => Starting();
            MakerAPI.MakerExiting += (s, e) => Exiting();
            MakerAPI.RegisterCustomSubCategories += Register;
        }
    }
}
