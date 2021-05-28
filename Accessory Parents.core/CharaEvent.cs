using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using MessagePack;
using MoreAccessoriesKOI;
using System;
using System.Collections.Generic;
using ToolBox;
using UniRx;
using UnityEngine;

namespace Accessory_Parents.Core
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        public readonly Dictionary<int, int>[] Child = new Dictionary<int, int>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
        public Dictionary<string, int>[] Custom_Names = new Dictionary<string, int>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
        public Dictionary<int, Vector3[,]>[] Relative_Data = new Dictionary<int, Vector3[,]>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];

        private readonly Dictionary<int, string>[] Old_Parent = new Dictionary<int, string>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
        private Dictionary<int, List<int>>[] Bindings = new Dictionary<int, List<int>>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];

        internal List<ChaFileAccessory.PartsInfo> Accessorys_Parts = new List<ChaFileAccessory.PartsInfo>();

        private int CoordinateNum = 0;

        public CharaEvent()
        {
            for (int i = 0; i < Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length; i++)
            {
                Bindings[i] = new Dictionary<int, List<int>>();
                Custom_Names[i] = new Dictionary<string, int>();
                Relative_Data[i] = new Dictionary<int, Vector3[,]>();
                Child[i] = new Dictionary<int, int>();
                Old_Parent[i] = new Dictionary<int, string>();
            }
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (currentGameMode != GameMode.Maker || !MakerEnabled)
            {
                return;
            }
            for (int i = 0; i < Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length; i++)
            {
                Bindings[i].Clear();
                Custom_Names[i].Clear();
                Relative_Data[i].Clear();
                Child[i].Clear();
            }
            CurrentCoordinate.Subscribe(X => { CoordinateNum = (int)X; ; Update_DropBox(); });
            var Data = GetExtendedData();
            if (Data != null)
            {
                if (Data.data.TryGetValue("Parenting_Data", out var ByteData) && ByteData != null)
                {
                    Bindings = MessagePackSerializer.Deserialize<Dictionary<int, List<int>>[]>((byte[])ByteData);
                }
                if (Data.data.TryGetValue("Parenting_Names", out ByteData) && ByteData != null)
                {
                    Custom_Names = MessagePackSerializer.Deserialize<Dictionary<string, int>[]>((byte[])ByteData);
                }
                if (Data.data.TryGetValue("Relative_Data", out ByteData) && ByteData != null)
                {
                    Relative_Data = MessagePackSerializer.Deserialize<Dictionary<int, Vector3[,]>[]>((byte[])ByteData);
                }
                for (int outfitnum = 0, n = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length; outfitnum < n; outfitnum++)
                {
                    Parent_To_Child(outfitnum);
                }
            }
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker || !MakerEnabled)
            {
                return;
            }
            var Data = GetCoordinateExtendedData(coordinate);
            if (Data != null)
            {
                if (Data.data.TryGetValue("Parenting_Data", out var DataBytes) && DataBytes != null)
                {
                    Bindings[CoordinateNum] = MessagePackSerializer.Deserialize<Dictionary<int, List<int>>>((byte[])DataBytes);
                }
                if (Data.data.TryGetValue("Parenting_Names", out DataBytes) && DataBytes != null)
                {
                    Custom_Names[CoordinateNum] = MessagePackSerializer.Deserialize<Dictionary<string, int>>((byte[])DataBytes);
                }
                if (Data.data.TryGetValue("Relative_Data", out DataBytes) && DataBytes != null)
                {
                    Relative_Data[CoordinateNum] = MessagePackSerializer.Deserialize<Dictionary<int, Vector3[,]>>((byte[])DataBytes);
                }
                Parent_To_Child(CoordinateNum);
            }
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker || !MakerEnabled)
            {
                return;
            }
            PluginData data = new PluginData();
            data.data.Add("Parenting_Data", MessagePackSerializer.Serialize(Bindings[CoordinateNum]));
            data.data.Add("Parenting_Names", MessagePackSerializer.Serialize(Custom_Names[CoordinateNum]));
            data.data.Add("Relative_Data", MessagePackSerializer.Serialize(Relative_Data[CoordinateNum]));
            SetCoordinateExtendedData(coordinate, data);
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker || !MakerEnabled)
            {
                return;
            }
            Update_Parenting();
            PluginData data = new PluginData();
            data.data.Add("Parenting_Data", MessagePackSerializer.Serialize(Bindings));
            data.data.Add("Parenting_Names", MessagePackSerializer.Serialize(Custom_Names));
            data.data.Add("Relative_Data", MessagePackSerializer.Serialize(Relative_Data));
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

        private void Parent_To_Child(int outfitnum)
        {
            foreach (var ParentPair in Bindings[outfitnum])
            {
                foreach (var Slot in ParentPair.Value)
                {
                    Child[outfitnum][Slot] = ParentPair.Key;
                }
            }
        }
    }
}
