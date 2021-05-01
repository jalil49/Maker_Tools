using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Additional_Card_Info
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private bool[] PersonalClothingBools = new bool[9];

        private List<int>[] AccKeep = new List<int>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
        private List<int>[] HairAcc = new List<int>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
        private bool[][] CoordinateSaveBools = new bool[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length][];

        private int CoordinateNum = 0;

        private bool Character_Cosplay_Ready = false;

        private bool underwearbool;

        public CharaEvent()
        {
            MakerAPI.MakerStartedLoading += MakerAPI_MakerStartedLoading;
            MakerAPI.MakerFinishedLoading += MakerAPI_MakerFinishedLoading;
            MakerAPI.RegisterCustomSubCategories += RegisterCustomSubCategories;
            MakerAPI.MakerExiting += MakerAPI_MakerExiting;
            for (int i = 0; i < AccKeep.Length; i++)
            {
                AccKeep[i] = new List<int>();
                HairAcc[i] = new List<int>();
                CoordinateSaveBools[i] = new bool[9];
            }
        }

        protected override void OnDestroy()
        {
            MakerAPI.MakerStartedLoading -= MakerAPI_MakerStartedLoading;
            MakerAPI.MakerFinishedLoading -= MakerAPI_MakerFinishedLoading;
            MakerAPI.RegisterCustomSubCategories -= RegisterCustomSubCategories;
            base.OnDestroy();
        }

        private void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {

        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (currentGameMode != GameMode.Maker)
            {
                return;
            }
            for (int i = 0; i < AccKeep.Length; i++)
            {
                AccKeep[i].Clear();
                HairAcc[i].Clear();
                CoordinateSaveBools[i] = new bool[9];
            }
            CurrentCoordinate.Subscribe(delegate (ChaFileDefine.CoordinateType value)
            {
                CoordinateNum = (int)value;
            });

            for (int j = 0; j < 9; j++)
            {
                PersonalClothingBools[j] = false;
            }
            var MyData = GetExtendedData();
            if (MyData != null)
            {
                if (MyData.data.TryGetValue("HairAcc", out var ByteData) && ByteData != null)
                {
                    HairAcc = MessagePackSerializer.Deserialize<List<int>[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("AccKeep", out ByteData) && ByteData != null)
                {
                    AccKeep = MessagePackSerializer.Deserialize<List<int>[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Personal_Clothing_Save", out ByteData) && ByteData != null)
                {
                    PersonalClothingBools = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Cosplay_Academy_Ready", out ByteData) && ByteData != null)
                {
                    Character_Cosplay_Ready = MessagePackSerializer.Deserialize<bool>((byte[])ByteData);
                }
            }
        }

        private void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            if (HairAcc[CoordinateNum].Contains(e.SourceSlotIndex))
            {
                HairAcc[CoordinateNum].Add(e.DestinationSlotIndex);
            }
            if (AccKeep[CoordinateNum].Contains(e.SourceSlotIndex))
            {
                AccKeep[CoordinateNum].Add(e.DestinationSlotIndex);
            }
            VisibiltyToggle();
        }

        private void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            var CopiedSlots = e.CopiedSlotIndexes.ToArray();
            var Source = (int)e.CopySource;
            var Dest = (int)e.CopyDestination;
            Settings.Logger.LogWarning($"Source {Source} Dest {Dest}");
            for (int i = 0; i < CopiedSlots.Length; i++)
            {
                Settings.Logger.LogWarning($"ACCKeep");
                if (AccKeep[Source].Contains(CopiedSlots[i]) && !AccKeep[Dest].Contains(CopiedSlots[i]))
                {
                    AccKeep[Dest].Add(CopiedSlots[i]);
                }
                Settings.Logger.LogWarning($"HairKeep");
                if (HairAcc[Source].Contains(CopiedSlots[i]) && !HairAcc[Dest].Contains(CopiedSlots[i]))
                {
                    HairAcc[Dest].Add(CopiedSlots[i]);
                }
            }
            VisibiltyToggle();
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            PluginData MyData = new PluginData();
            MyData.data.Add("HairAcc", MessagePackSerializer.Serialize(HairAcc));
            MyData.data.Add("AccKeep", MessagePackSerializer.Serialize(AccKeep));
            MyData.data.Add("Personal_Clothing_Save", MessagePackSerializer.Serialize(PersonalClothingBools));
            MyData.data.Add("Cosplay_Academy_Ready", MessagePackSerializer.Serialize(Character_Cosplay_Ready));
            SetExtendedData(MyData);
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            PluginData MyData = new PluginData();
            //clothing to keep
            //Do not keep accessory
            //is colorable
            MyData.data.Add("CoordinateSaveBools", MessagePackSerializer.Serialize(CoordinateSaveBools[CoordinateNum]));
            MyData.data.Add("HairAcc", MessagePackSerializer.Serialize(HairAcc[CoordinateNum]));
            MyData.data.Add("Is_Underwear", MessagePackSerializer.Serialize(underwearbool));
            //MyData.data.Add("ACC_State_2_array", MessagePackSerializer.Serialize(ACC_State_2_array[CoordinateNum]));
            //MyData.data.Add("ACC_State_bool_array", MessagePackSerializer.Serialize(ACC_State_bool_array[CoordinateNum]));

            //Items to not color
            SetCoordinateExtendedData(coordinate, MyData);
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                return;
            }
            var MyData = GetCoordinateExtendedData(coordinate);
            if (MyData != null)
            {
                if (MyData.data.TryGetValue("HairAcc", out var ByteData) && ByteData != null)
                {
                    HairAcc[CoordinateNum] = MessagePackSerializer.Deserialize<List<int>>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("CoordinateSaveBools", out ByteData) && ByteData != null)
                {
                    CoordinateSaveBools[CoordinateNum] = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Is_Underwear", out ByteData) && ByteData != null)
                {
                    IsUnderwear.SetValue(MessagePackSerializer.Deserialize<bool>((byte[])ByteData));
                }
            }
        }
    }
}
