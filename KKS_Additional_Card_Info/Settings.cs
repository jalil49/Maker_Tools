using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ExtensibleSaveFormat;
using Hook_Space;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;
using MessagePack;
using System.Collections.Generic;

namespace Additional_Card_Info
{
    [BepInPlugin(GUID, "Additional Card Info", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class Settings : BaseUnityPlugin
    {
        public const string GUID = "Additional_Card_Info";
        public const string Version = "1.2";
        internal static Settings Instance;
        internal static new ManualLogSource Logger;
        public static ConfigEntry<string> NamingID { get; private set; }
        public static ConfigEntry<string> CreatorName { get; private set; }

        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            if (StudioAPI.InsideStudio) return;

            CharacterApi.RegisterExtraBehaviour<CharaEvent>(GUID);
            Hooks.Init(Logger);
            NamingID = Config.Bind("Grouping ID", "Grouping ID", "4", "Requires restarting maker");
            CreatorName = Config.Bind("User", "Creator", "", "Default Creator name for those who make a lot of coordinates");
            MakerAPI.MakerStartedLoading += CharaEvent.MakerAPI_MakerStartedLoading;
            MakerAPI.RegisterCustomSubCategories += CharaEvent.RegisterCustomSubCategories;
            ExtendedSave.CardBeingImported += ExtendedSave_CardBeingImported;
        }

        private void ExtendedSave_CardBeingImported(Dictionary<string, PluginData> importedExtendedData)
        {
            importedExtendedData.TryGetValue(GUID, out var ACI_Data);
            if (ACI_Data != null)
            {
                Logger.LogWarning("ACI Data Found");
                Dictionary<int, int> ImportDict = new Dictionary<int, int>() { { 0, 3 }, { 1, -1 }, { 2, -1 }, { 3, 1 }, { 4, -1 }, { 5, 0 }, { 6, 2 } };

                //bool[] PersonalClothingBools = new bool[9];

                //bool CosplayReady = false;

                if (ACI_Data.data.TryGetValue("HairAcc", out var ByteData) && ByteData != null)
                {
                    List<int>[] HairAcc = new List<int>[Constants.CoordinateLength];
                    var temp = MessagePackSerializer.Deserialize<List<int>[]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        HairAcc[Value] = temp[i];
                    }
                    ACI_Data.data["HairAcc"] = MessagePackSerializer.Serialize(HairAcc);
                }

                if (ACI_Data.data.TryGetValue("AccKeep", out ByteData) && ByteData != null)
                {
                    List<int>[] AccKeep = new List<int>[Constants.CoordinateLength];
                    var temp = MessagePackSerializer.Deserialize<List<int>[]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        AccKeep[Value] = temp[i];
                    }
                    ACI_Data.data["AccKeep"] = MessagePackSerializer.Serialize(AccKeep);
                }

                //if (ACI_Data.data.TryGetValue("Personal_Clothing_Save", out ByteData) && ByteData != null)
                //{
                //    PersonalClothingBools = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                //}
                //if (ACI_Data.data.TryGetValue("Cosplay_Academy_Ready", out ByteData) && ByteData != null)
                //{
                //    CosplayReady = MessagePackSerializer.Deserialize<bool>((byte[])ByteData);
                //}
                if (ACI_Data.data.TryGetValue("MakeUpKeep", out ByteData) && ByteData != null)
                {
                    bool[] MakeUpKeep = new bool[Constants.CoordinateLength];
                    var temp = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        MakeUpKeep[Value] = temp[i];
                    }
                    ACI_Data.data["MakeupKeep"] = MessagePackSerializer.Serialize(MakeUpKeep);
                }

                if (ACI_Data.data.TryGetValue("CoordinateSaveBools", out ByteData) && ByteData != null)
                {
                    bool[][] CoordinateSaveBools = new bool[Constants.CoordinateLength][];
                    var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        CoordinateSaveBools[Value] = temp[i];
                    }
                    ACI_Data.data["CoordinateSaveBools"] = MessagePackSerializer.Serialize(CoordinateSaveBools);
                }

                if (ACI_Data.data.TryGetValue("PersonalityType_Restriction", out ByteData) && ByteData != null)
                {
                    Dictionary<int, int>[] PersonalityType_Restriction = new Dictionary<int, int>[Constants.CoordinateLength];
                    var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        PersonalityType_Restriction[Value] = temp[i];
                    }
                    ACI_Data.data["PersonalityType_Restriction"] = MessagePackSerializer.Serialize(PersonalityType_Restriction);
                }

                if (ACI_Data.data.TryGetValue("Interest_Restriction", out ByteData) && ByteData != null)
                {
                    Dictionary<int, int>[] Interest_Restriction = new Dictionary<int, int>[Constants.CoordinateLength];
                    var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        Interest_Restriction[Value] = temp[i];
                    }
                    ACI_Data.data["Interest_Restriction"] = MessagePackSerializer.Serialize(Interest_Restriction);

                }

                if (ACI_Data.data.TryGetValue("TraitType_Restriction", out ByteData) && ByteData != null)
                {
                    //special
                    Dictionary<int, int>[] TraitType_Restriction = new Dictionary<int, int>[Constants.CoordinateLength];
                    var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        foreach (var item in temp[i])
                        {
                            TraitType_Restriction[i][Constants.Koi_to_Sun_Traits[item.Key]] = item.Value;
                        }
                    }
                    ACI_Data.data["TraitType_Restriction"] = MessagePackSerializer.Serialize(TraitType_Restriction);
                }

                if (ACI_Data.data.TryGetValue("HstateType_Restriction", out ByteData) && ByteData != null)
                {
                    int[] HstateType_Restriction = new int[Constants.CoordinateLength];
                    var temp = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        HstateType_Restriction[Value] = temp[i];
                    }
                    ACI_Data.data["HstateType_Restriction"] = MessagePackSerializer.Serialize(HstateType_Restriction);
                }

                if (ACI_Data.data.TryGetValue("ClubType_Restriction", out ByteData) && ByteData != null)
                {
                    int[] ClubType_Restriction = new int[Constants.CoordinateLength];
                    var temp = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        ClubType_Restriction[Value] = temp[i];
                    }
                    ACI_Data.data["ClubType_Restriction"] = MessagePackSerializer.Serialize(ClubType_Restriction);
                }

                if (ACI_Data.data.TryGetValue("Height_Restriction", out ByteData) && ByteData != null)
                {
                    bool[][] Height_Restriction = new bool[Constants.CoordinateLength][];
                    var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        Height_Restriction[Value] = temp[i];
                    }
                    ACI_Data.data["Height_Restriction"] = MessagePackSerializer.Serialize(Height_Restriction);
                }

                if (ACI_Data.data.TryGetValue("Breastsize_Restriction", out ByteData) && ByteData != null)
                {
                    bool[][] Breastsize_Restriction = new bool[Constants.CoordinateLength][];
                    var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        Breastsize_Restriction[Value] = temp[i];
                    }
                    ACI_Data.data["Breastsize_Restriction"] = MessagePackSerializer.Serialize(Breastsize_Restriction);

                }

                if (ACI_Data.data.TryGetValue("CoordinateType", out ByteData) && ByteData != null)
                {
                    int[] CoordinateType = new int[Constants.CoordinateLength];
                    var temp = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        CoordinateType[Value] = temp[i];
                    }
                    ACI_Data.data["CoordinateSaveBools"] = MessagePackSerializer.Serialize(CoordinateType);

                }

                if (ACI_Data.data.TryGetValue("CoordinateSubType", out ByteData) && ByteData != null)
                {
                    int[] CoordinateSubType = new int[Constants.CoordinateLength];
                    var temp = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        CoordinateSubType[Value] = temp[i];
                    }
                    ACI_Data.data["CoordinateSubType"] = MessagePackSerializer.Serialize(CoordinateSubType);
                }

                if (ACI_Data.data.TryGetValue("Creator", out ByteData) && ByteData != null)
                {
                    string[] CreatorNames = new string[Constants.CoordinateLength];
                    var temp = MessagePackSerializer.Deserialize<string[]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        CreatorNames[Value] = temp[i];
                    }
                    ACI_Data.data["Creator"] = MessagePackSerializer.Serialize(CreatorNames);
                }

                if (ACI_Data.data.TryGetValue("Set_Name", out ByteData) && ByteData != null)
                {
                    var SetNames = new string[Constants.CoordinateLength];
                    var temp = MessagePackSerializer.Deserialize<string[]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        SetNames[Value] = temp[i];
                    }
                    ACI_Data.data["Set_Name"] = MessagePackSerializer.Serialize(SetNames);
                }

                if (ACI_Data.data.TryGetValue("SubSetNames", out ByteData) && ByteData != null)
                {
                    string[] SubSetNames = new string[Constants.CoordinateLength];
                    var temp = MessagePackSerializer.Deserialize<string[]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        SubSetNames[Value] = temp[i];
                    }
                    ACI_Data.data["SubSetNames"] = MessagePackSerializer.Serialize(SubSetNames);
                }

                if (ACI_Data.data.TryGetValue("ClothNot", out ByteData) && ByteData != null)
                {
                    bool[][] ClothNotData = new bool[Constants.CoordinateLength][];
                    var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        ClothNotData[Value] = temp[i];
                    }
                    ACI_Data.data["ClothNot"] = MessagePackSerializer.Serialize(ClothNotData);
                }

                if (ACI_Data.data.TryGetValue("GenderType", out ByteData) && ByteData != null)
                {
                    int[] GenderType = new int[Constants.CoordinateLength];
                    var temp = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                    for (int i = 0; i < ImportDict.Count; i++)
                    {
                        var Value = ImportDict[i];
                        if (Value == -1)
                            continue;
                        GenderType[Value] = temp[i];
                    }
                    ACI_Data.data["GenderType"] = MessagePackSerializer.Serialize(GenderType);
                }

                importedExtendedData[GUID] = ACI_Data;
            }
        }
    }
}
