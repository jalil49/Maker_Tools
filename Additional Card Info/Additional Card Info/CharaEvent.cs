using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using MessagePack;
using System;
using System.Collections.Generic;
using UniRx;

namespace Additional_Card_Info
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private bool[] PersonalClothingBools = new bool[9];

        private bool[] MakeUpKeep = new bool[9];

        public List<int>[] AccKeep = new List<int>[Constants.CoordinateLength];
        public List<int>[] HairAcc = new List<int>[Constants.CoordinateLength];

        private Dictionary<int, int>[] PersonalityType_Restriction = new Dictionary<int, int>[Constants.CoordinateLength];
        private Dictionary<int, int>[] TraitType_Restriction = new Dictionary<int, int>[Constants.CoordinateLength];

        private bool[][] CoordinateSaveBools = new bool[Constants.CoordinateLength][];
        private bool[][] Height_Restriction = new bool[Constants.CoordinateLength][];
        private bool[][] Breastsize_Restriction = new bool[Constants.CoordinateLength][];
        private bool[][] ClothNotData = new bool[Constants.CoordinateLength][];

        private int[] HstateType_Restriction = new int[Constants.CoordinateLength];
        private int[] ClubType_Restriction = new int[Constants.CoordinateLength];
        private int[] CoordinateType = new int[Constants.CoordinateLength];
        private int[] CoordinateSubType = new int[Constants.CoordinateLength];

        private string[] CreatorNames = new string[Constants.CoordinateLength];
        private string[] SetNames = new string[Constants.CoordinateLength];
        private string[] SubSetNames = new string[Constants.CoordinateLength];

        private int[] GenderType = new int[Constants.CoordinateLength];

        private int CoordinateNum = 0;

        public CharaEvent()
        {
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
                ClothNotData[i] = new bool[3];
                GenderType[i] = 0;
            }
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
                ClothNotData[i] = new bool[3];
                GenderType[i] = 0;
            }
            if (Character_Cosplay_Ready != null)
            {
                Character_Cosplay_Ready.SetValue(false);
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

            var ACI_Data = GetExtendedData();

            if (ACI_Data != null)
            {
                if (ACI_Data.data.TryGetValue("HairAcc", out var ByteData) && ByteData != null)
                {
                    HairAcc = MessagePackSerializer.Deserialize<List<int>[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("AccKeep", out ByteData) && ByteData != null)
                {
                    AccKeep = MessagePackSerializer.Deserialize<List<int>[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("Personal_Clothing_Save", out ByteData) && ByteData != null)
                {
                    PersonalClothingBools = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("Cosplay_Academy_Ready", out ByteData) && ByteData != null)
                {
                    Character_Cosplay_Ready.SetValue(MessagePackSerializer.Deserialize<bool>((byte[])ByteData));
                }
                if (ACI_Data.data.TryGetValue("MakeUpKeep", out ByteData) && ByteData != null)
                {
                    MakeUpKeep = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                }


                if (ACI_Data.data.TryGetValue("CoordinateSaveBools", out ByteData) && ByteData != null)
                {
                    CoordinateSaveBools = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("AccKeep", out ByteData) && ByteData != null)
                {
                    AccKeep = MessagePackSerializer.Deserialize<List<int>[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("PersonalityType_Restriction", out ByteData) && ByteData != null)
                {
                    PersonalityType_Restriction = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("TraitType_Restriction", out ByteData) && ByteData != null)
                {
                    TraitType_Restriction = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("HstateType_Restriction", out ByteData) && ByteData != null)
                {
                    HstateType_Restriction = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("ClubType_Restriction", out ByteData) && ByteData != null)
                {
                    ClubType_Restriction = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("Height_Restriction", out ByteData) && ByteData != null)
                {
                    Height_Restriction = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("Breastsize_Restriction", out ByteData) && ByteData != null)
                {
                    Breastsize_Restriction = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("CoordinateType", out ByteData) && ByteData != null)
                {
                    CoordinateType = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("CoordinateSubType", out ByteData) && ByteData != null)
                {
                    CoordinateSubType = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("Creator", out ByteData) && ByteData != null)
                {
                    CreatorNames = MessagePackSerializer.Deserialize<string[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("Set_Name", out ByteData) && ByteData != null)
                {
                    SetNames = MessagePackSerializer.Deserialize<string[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("SubSetNames", out ByteData) && ByteData != null)
                {
                    SubSetNames = MessagePackSerializer.Deserialize<string[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("ClothNot", out ByteData) && ByteData != null)
                {
                    ClothNotData = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("GenderType", out ByteData) && ByteData != null)
                {
                    GenderType = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                }
            }
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            PluginData ACI_Data = new PluginData();
            ACI_Data.data.Add("HairAcc", MessagePackSerializer.Serialize(HairAcc));
            ACI_Data.data.Add("AccKeep", MessagePackSerializer.Serialize(AccKeep));
            ACI_Data.data.Add("Personal_Clothing_Save", MessagePackSerializer.Serialize(PersonalClothingBools));
            ACI_Data.data.Add("Cosplay_Academy_Ready", MessagePackSerializer.Serialize(Character_Cosplay_Ready.Value));
            ACI_Data.data.Add("MakeUpKeep", MessagePackSerializer.Serialize(MakeUpKeep));
            SetExtendedData(ACI_Data);
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            PluginData ACI_Data = new PluginData();

            ACI_Data.data.Add("CoordinateSaveBools", MessagePackSerializer.Serialize(CoordinateSaveBools[CoordinateNum]));
            ACI_Data.data.Add("HairAcc", MessagePackSerializer.Serialize(HairAcc[CoordinateNum]));
            ACI_Data.data.Add("AccKeep", MessagePackSerializer.Serialize(AccKeep[CoordinateNum]));
            ACI_Data.data.Add("PersonalityType_Restriction", MessagePackSerializer.Serialize(PersonalityType_Restriction[CoordinateNum]));
            ACI_Data.data.Add("TraitType_Restriction", MessagePackSerializer.Serialize(TraitType_Restriction[CoordinateNum]));
            ACI_Data.data.Add("HstateType_Restriction", MessagePackSerializer.Serialize(HstateType_Restriction[CoordinateNum]));
            ACI_Data.data.Add("ClubType_Restriction", MessagePackSerializer.Serialize(ClubType_Restriction[CoordinateNum]));
            ACI_Data.data.Add("Height_Restriction", MessagePackSerializer.Serialize(Height_Restriction[CoordinateNum]));
            ACI_Data.data.Add("Breastsize_Restriction", MessagePackSerializer.Serialize(Breastsize_Restriction[CoordinateNum]));
            ACI_Data.data.Add("CoordinateType", MessagePackSerializer.Serialize(CoordinateType[CoordinateNum]));
            ACI_Data.data.Add("CoordinateSubType", MessagePackSerializer.Serialize(CoordinateSubType[CoordinateNum]));
            ACI_Data.data.Add("Creator", MessagePackSerializer.Serialize(CreatorNames[CoordinateNum]));
            ACI_Data.data.Add("Set_Name", MessagePackSerializer.Serialize(SetNames[CoordinateNum]));
            ACI_Data.data.Add("SubSetNames", MessagePackSerializer.Serialize(SubSetNames[CoordinateNum]));
            ACI_Data.data.Add("ClothNot", MessagePackSerializer.Serialize(ClothNotData[CoordinateNum]));
            ACI_Data.data.Add("GenderType", MessagePackSerializer.Serialize(GenderType[CoordinateNum]));

            SetCoordinateExtendedData(coordinate, ACI_Data);
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                return;
            }
            var ACI_Data = GetCoordinateExtendedData(coordinate);
            if (ACI_Data != null)
            {
                if (ACI_Data.data.TryGetValue("HairAcc", out var ByteData) && ByteData != null)
                {
                    HairAcc[CoordinateNum] = MessagePackSerializer.Deserialize<List<int>>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("CoordinateSaveBools", out ByteData) && ByteData != null)
                {
                    CoordinateSaveBools[CoordinateNum] = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("AccKeep", out ByteData) && ByteData != null)
                {
                    AccKeep[CoordinateNum] = MessagePackSerializer.Deserialize<List<int>>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("PersonalityType_Restriction", out ByteData) && ByteData != null)
                {
                    PersonalityType_Restriction[CoordinateNum] = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("TraitType_Restriction", out ByteData) && ByteData != null)
                {
                    TraitType_Restriction[CoordinateNum] = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("HstateType_Restriction", out ByteData) && ByteData != null)
                {
                    HstateType_Restriction[CoordinateNum] = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("ClubType_Restriction", out ByteData) && ByteData != null)
                {
                    ClubType_Restriction[CoordinateNum] = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("Height_Restriction", out ByteData) && ByteData != null)
                {
                    Height_Restriction[CoordinateNum] = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("Breastsize_Restriction", out ByteData) && ByteData != null)
                {
                    Breastsize_Restriction[CoordinateNum] = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("CoordinateType", out ByteData) && ByteData != null)
                {
                    CoordinateType[CoordinateNum] = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("CoordinateSubType", out ByteData) && ByteData != null)
                {
                    CoordinateSubType[CoordinateNum] = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("Creator", out ByteData) && ByteData != null)
                {
                    CreatorNames[CoordinateNum] = MessagePackSerializer.Deserialize<string>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("Set_Name", out ByteData) && ByteData != null)
                {
                    SetNames[CoordinateNum] = MessagePackSerializer.Deserialize<string>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("SubSetNames", out ByteData) && ByteData != null)
                {
                    SubSetNames[CoordinateNum] = MessagePackSerializer.Deserialize<string>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("ClothNot", out ByteData) && ByteData != null)
                {
                    ClothNotData[CoordinateNum] = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                }
                if (ACI_Data.data.TryGetValue("GenderType", out ByteData) && ByteData != null)
                {
                    GenderType[CoordinateNum] = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
                }
            }
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            {
                StartCoroutine(UpdateSlots());
            }
        }
    }
}
