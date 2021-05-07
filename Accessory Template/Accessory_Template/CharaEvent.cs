using BepInEx.Logging;
using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
using MoreAccessoriesKOI;
using System;
using System.Collections.Generic;
using ToolBox;
using UniRx;
using UnityEngine;

namespace Template_Accessories
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private int CoordinateNum = 0;
        public List<ChaFileAccessory.PartsInfo> Accessorys_Parts = new List<ChaFileAccessory.PartsInfo>();
        readonly ManualLogSource Logger;

        public CharaEvent()
        {
            for (int i = 0; i < Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length; i++)
            {
            }
            MakerAPI.MakerStartedLoading += MakerAPI_MakerStartedLoading;
            MakerAPI.MakerExiting += MakerAPI_MakerExiting;
            MakerAPI.RegisterCustomSubCategories += MakerAPI_RegisterCustomSubCategories;
            Logger = Settings.Logger;
        }

        protected override void OnDestroy()
        {
            MakerAPI.MakerStartedLoading -= MakerAPI_MakerStartedLoading;
            MakerAPI.MakerExiting -= MakerAPI_MakerExiting;
            MakerAPI.RegisterCustomSubCategories -= MakerAPI_RegisterCustomSubCategories;

            base.OnDestroy();
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (currentGameMode != GameMode.Maker)
            {
                return;
            }
            for (int i = 0; i < Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length; i++)
            {
            }
            CurrentCoordinate.Subscribe(X => { CoordinateNum = (int)X; ; Update_DropBox(); Update_More_Accessories(); });
            var Data = GetExtendedData();
            if (Data != null)
            {

            }
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                return;
            }
            var Data = GetCoordinateExtendedData(coordinate);
            if (Data != null)
            {
            }
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                return;
            }
            PluginData data = new PluginData();
            SetCoordinateExtendedData(coordinate, data);
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                return;
            }
            PluginData data = new PluginData();
            SetExtendedData(data);
        }

        private void Update_More_Accessories()
        {
            WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();
            if (_accessoriesByChar.TryGetValue(ChaFileControl, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
            }
            Accessorys_Parts = data.nowAccessories;
        }
    }
}
