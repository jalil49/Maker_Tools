using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Additional_Card_Info
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private bool[] PersonalClothingBools = new bool[9];

        public List<int>[] AccKeep = new List<int>[Constants.CoordinateLength];
        public List<int>[] HairAcc = new List<int>[Constants.CoordinateLength];

        private Dictionary<int, int>[] PersonalityType_Restriction = new Dictionary<int, int>[Constants.CoordinateLength];
        private Dictionary<int, int>[] TraitType_Restriction = new Dictionary<int, int>[Constants.CoordinateLength];

        private bool[][] CoordinateSaveBools = new bool[Constants.CoordinateLength][];
        private bool[][] Height_Restriction = new bool[Constants.CoordinateLength][];
        private bool[][] Breastsize_Restriction = new bool[Constants.CoordinateLength][];

        private int[] HstateType_Restriction = new int[Constants.CoordinateLength];
        private int[] ClubType_Restriction = new int[Constants.CoordinateLength];
        private int[] CoordinateType = new int[Constants.CoordinateLength];
        private int[] CoordinateSubType = new int[Constants.CoordinateLength];

        private string[] CreatorNames = new string[Constants.CoordinateLength];
        private string[] SetNames = new string[Constants.CoordinateLength];
        private string[] SubSetNames = new string[Constants.CoordinateLength];

        private int CoordinateNum = 0;

        private bool Character_Cosplay_Ready = false;

        public CharaEvent()
        {
            MakerAPI.MakerStartedLoading += MakerAPI_MakerStartedLoading;
            MakerAPI.RegisterCustomSubCategories += RegisterCustomSubCategories;

            for (int i = 0; i < AccKeep.Length; i++)
            {
                AccKeep[i] = new List<int>();
                HairAcc[i] = new List<int>();
                CoordinateSaveBools[i] = new bool[Enum.GetNames(typeof(Constants.ClothingTypes)).Length];
                CreatorNames[i] = Settings.CreatorName.Value;
                SetNames[i] = "";
                PersonalityType_Restriction[i] = new Dictionary<int, int>();
                TraitType_Restriction[i] = new Dictionary<int, int>();
                Height_Restriction[i] = new bool[Constants.HeightLength];
                Breastsize_Restriction[i] = new bool[Constants.BreastsizeLength];
                HstateType_Restriction[i] = 0;
                CoordinateType[i] = 0;
                SubSetNames[i] = "";
            }
        }

        protected override void OnDestroy()
        {
            MakerAPI.MakerStartedLoading -= MakerAPI_MakerStartedLoading;
            MakerAPI.RegisterCustomSubCategories -= RegisterCustomSubCategories;

            base.OnDestroy();
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
                CoordinateSaveBools[i] = new bool[Enum.GetNames(typeof(Constants.ClothingTypes)).Length];
                CreatorNames[i] = Settings.CreatorName.Value;
                SetNames[i] = "";
                SubSetNames[i] = "";
                PersonalityType_Restriction[i].Clear();
                TraitType_Restriction[i].Clear();
                HstateType_Restriction[i] = 0;
                CoordinateType[i] = 0;
                Height_Restriction[i] = new bool[Constants.HeightLength];
                Breastsize_Restriction[i] = new bool[Constants.BreastsizeLength];
            }

            CurrentCoordinate.Subscribe(delegate (ChaFileDefine.CoordinateType value)
            {
                CoordinateNum = (int)value;
                if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
                {
                    StartCoroutine(UpdateSlots());
                }
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


                if (MyData.data.TryGetValue("CoordinateSaveBools", out ByteData) && ByteData != null)
                {
                    CoordinateSaveBools = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("AccKeep", out ByteData) && ByteData != null)
                {
                    AccKeep = MessagePackSerializer.Deserialize<List<int>[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("PersonalityType_Restriction", out ByteData) && ByteData != null)
                {
                    PersonalityType_Restriction = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("TraitType_Restriction", out ByteData) && ByteData != null)
                {
                    TraitType_Restriction = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("HstateType_Restriction", out ByteData) && ByteData != null)
                {
                    HstateType_Restriction = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("ClubType_Restriction", out ByteData) && ByteData != null)
                {
                    ClubType_Restriction = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Height_Restriction", out ByteData) && ByteData != null)
                {
                    Height_Restriction = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Breastsize_Restriction", out ByteData) && ByteData != null)
                {
                    Breastsize_Restriction = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("CoordinateType", out ByteData) && ByteData != null)
                {
                    CoordinateType = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("CoordinateSubType", out ByteData) && ByteData != null)
                {
                    CoordinateSubType = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Creator", out ByteData) && ByteData != null)
                {
                    CreatorNames = MessagePackSerializer.Deserialize<string[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Set_Name", out ByteData) && ByteData != null)
                {
                    SetNames = MessagePackSerializer.Deserialize<string[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("SubSetNames", out ByteData) && ByteData != null)
                {
                    SubSetNames = MessagePackSerializer.Deserialize<string[]>((byte[])ByteData);
                }
            }
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

            MyData.data.Add("CoordinateSaveBools", MessagePackSerializer.Serialize(CoordinateSaveBools[CoordinateNum]));
            MyData.data.Add("HairAcc", MessagePackSerializer.Serialize(HairAcc[CoordinateNum]));
            MyData.data.Add("AccKeep", MessagePackSerializer.Serialize(AccKeep[CoordinateNum]));
            MyData.data.Add("PersonalityType_Restriction", MessagePackSerializer.Serialize(PersonalityType_Restriction[CoordinateNum]));
            MyData.data.Add("TraitType_Restriction", MessagePackSerializer.Serialize(TraitType_Restriction[CoordinateNum]));
            MyData.data.Add("HstateType_Restriction", MessagePackSerializer.Serialize(HstateType_Restriction[CoordinateNum]));
            MyData.data.Add("ClubType_Restriction", MessagePackSerializer.Serialize(ClubType_Restriction[CoordinateNum]));
            MyData.data.Add("Height_Restriction", MessagePackSerializer.Serialize(Height_Restriction[CoordinateNum]));
            MyData.data.Add("Breastsize_Restriction", MessagePackSerializer.Serialize(Breastsize_Restriction[CoordinateNum]));
            MyData.data.Add("CoordinateType", MessagePackSerializer.Serialize(CoordinateType[CoordinateNum]));
            MyData.data.Add("CoordinateSubType", MessagePackSerializer.Serialize(CoordinateSubType[CoordinateNum]));
            MyData.data.Add("Creator", MessagePackSerializer.Serialize(CreatorNames[CoordinateNum]));
            MyData.data.Add("Set_Name", MessagePackSerializer.Serialize(SetNames[CoordinateNum]));
            MyData.data.Add("SubSetNames", MessagePackSerializer.Serialize(SubSetNames[CoordinateNum]));

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
                if (MyData.data.TryGetValue("AccKeep", out ByteData) && ByteData != null)
                {
                    AccKeep[CoordinateNum] = MessagePackSerializer.Deserialize<List<int>>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("PersonalityType_Restriction", out ByteData) && ByteData != null)
                {
                    PersonalityType_Restriction[CoordinateNum] = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("TraitType_Restriction", out ByteData) && ByteData != null)
                {
                    TraitType_Restriction[CoordinateNum] = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("HstateType_Restriction", out ByteData) && ByteData != null)
                {
                    HstateType_Restriction[CoordinateNum] = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("ClubType_Restriction", out ByteData) && ByteData != null)
                {
                    ClubType_Restriction[CoordinateNum] = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Height_Restriction", out ByteData) && ByteData != null)
                {
                    Height_Restriction[CoordinateNum] = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Breastsize_Restriction", out ByteData) && ByteData != null)
                {
                    Breastsize_Restriction[CoordinateNum] = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("CoordinateType", out ByteData) && ByteData != null)
                {
                    CoordinateType[CoordinateNum] = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("CoordinateSubType", out ByteData) && ByteData != null)
                {
                    CoordinateSubType[CoordinateNum] = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Creator", out ByteData) && ByteData != null)
                {
                    CreatorNames[CoordinateNum] = MessagePackSerializer.Deserialize<string>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Set_Name", out ByteData) && ByteData != null)
                {
                    SetNames[CoordinateNum] = MessagePackSerializer.Deserialize<string>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("SubSetNames", out ByteData) && ByteData != null)
                {
                    SubSetNames[CoordinateNum] = MessagePackSerializer.Deserialize<string>((byte[])ByteData);
                }
            }
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            {
                StartCoroutine(UpdateSlots());
            }
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}
