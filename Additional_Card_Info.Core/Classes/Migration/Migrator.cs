using System;
using System.Collections.Generic;
using ExtensibleSaveFormat;
using MessagePack;
using UnityEngine.Serialization;

namespace Additional_Card_Info.Classes.Migration
{
    public static class Migrator
    {
        public static SlotData SlotInfoMigrate(PluginData pluginData)
        {
            switch (pluginData.version)
            {
                case 2:
                    if (pluginData.data.TryGetValue(Constants.AccessoryKey, out var bytearray))
                    {
                        return SlotData.Deserialize(bytearray);
                    }

                    break;
            }

            return null;
        }

        public static CoordinateInfo CoordinateInfoMigrate(PluginData pluginData)
        {
            switch (pluginData.version)
            {
                case 2:
                    if (pluginData.data.TryGetValue(Constants.CoordinateKey, out var bytearray) && bytearray != null)
                    {
                        return CoordinateInfo.Deserialize(bytearray);
                    }

                    break;
                default:
                    PrintOutdated();
                    break;
            }

            return null;
        }

        public static CardData CardInfoMigrate(PluginData pluginData)
        {
            switch (pluginData.version)
            {
                case 2:
                    if (pluginData.data.TryGetValue(Constants.CardKey, out var bytearray) && bytearray != null)
                    {
                        return CardData.Deserialize(bytearray);
                    }

                    break;
            }

            return null;
        }

        public static CardData StandardCharaMigrate(ChaControl control, PluginData aciData)
        {
            if (aciData.version == 2)
            {
                if (aciData.data.TryGetValue(Constants.CardKey, out var bytearray) && bytearray != null)
                {
                    return CardData.Deserialize(bytearray);
                }
            }

            if (aciData.version > 2)
            {
                Settings.Logger.LogMessage("New version of Additional Card Info Detected Please Update.");
                return new CardData();
            }

            var oldCoordinateData = new Dictionary<int, CoordinateInfoV1>();
            var oldCardData = new CardInfoV1();

            switch (aciData.version)
            {
                case 0:
                    StandardCharaMigrateV0(aciData, ref oldCoordinateData, ref oldCardData);
                    break;
                case 1:
                {
                    if (aciData.data.TryGetValue("CardInfo", out var byteData) && byteData != null)
                    {
                        oldCardData = CardInfoV1.Deserialize(byteData);
                    }

                    if (aciData.data.TryGetValue("CoordinateInfo", out byteData) && byteData != null)
                    {
                        oldCoordinateData = CoordinateInfoV1.DictDeserialize(byteData);
                    }

                    break;
                }
            }

            foreach (var item in oldCoordinateData)
            {
                if (item.Key >= control.chaFile.coordinate.Length)
                {
                    continue;
                }

                string advString = null;
                if (item.Key < Constants.Coordinates.Length)
                {
                    oldCardData.AdvancedFolderDirectory.TryGetValue(Constants.Coordinates[item.Key], out advString);
                }

                CoordProcess(control.chaFile.coordinate[item.Key], item.Value, advString);

                if (item.Key != control.fileStatus.coordinateType)
                {
                    continue;
                }

                var baseFile = control.chaFile.coordinate[item.Key];

                if (baseFile.accessory.TryGetExtendedDataById(Settings.GUID, out var baseData))
                {
                    control.nowCoordinate.accessory.SetExtendedDataById(Settings.GUID, baseData);
                }

                var baseParts = baseFile.accessory.parts;
                var nowParts = control.nowCoordinate.accessory.parts;
                for (var i = 0; i < baseParts.Length; i++)
                {
                    if (baseParts[i].TryGetExtendedDataById(Settings.GUID, out baseData))
                    {
                        nowParts[i].SetExtendedDataById(Settings.GUID, baseData);
                    }
                }
            }

            var newCardInfo = new CardData(oldCardData);

            aciData.version = 2;
            aciData.data.Clear();
            aciData.data[Constants.CardKey] = MessagePackSerializer.Serialize(newCardInfo);
            return newCardInfo;
        }

        public static void StandardCharaMigrateV0(PluginData aciData,
                                                  ref Dictionary<int, CoordinateInfoV1> coordinateData,
                                                  ref CardInfoV1 cardInfo)
        {
            if (aciData.data.TryGetValue("HairAcc", out var byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<int>[]>((byte[])byteData);
                for (var i = 0; i < 7 && i < coordinateData.Count; i++)
                {
                    coordinateData[i].HairAcc = temp[i];
                }
            }

            if (aciData.data.TryGetValue("AccKeep", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<int>[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].AccKeep = temp[i];
                }
            }

            if (aciData.data.TryGetValue("MakeUpKeep", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    if (temp[i])
                    {
                        coordinateData[i].MakeUpKeep = true;
                    }
                }
            }

            if (aciData.data.TryGetValue("CoordinateSaveBools", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].CoordinateSaveBools = temp[i];
                }
            }

            if (aciData.data.TryGetValue("PersonalityType_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].RestrictionData.PersonalityType_Restriction = temp[i];
                }
            }

            if (aciData.data.TryGetValue("Interest_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].RestrictionData.Interest_Restriction = temp[i];
                }
            }

            if (aciData.data.TryGetValue("TraitType_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].RestrictionData.TraitType_Restriction = temp[i];
                }
            }

            if (aciData.data.TryGetValue("HstateType_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].RestrictionData.HStateType_Restriction = temp[i];
                }
            }

            if (aciData.data.TryGetValue("ClubType_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].RestrictionData.ClubType_Restriction = temp[i];
                }
            }

            if (aciData.data.TryGetValue("Height_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].RestrictionData.Height_Restriction = temp[i];
                }
            }

            if (aciData.data.TryGetValue("Breastsize_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].RestrictionData.BreastSize_Restriction = temp[i];
                }
            }

            if (aciData.data.TryGetValue("CoordinateType", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].RestrictionData.CoordinateType = temp[i] + 1;
                }
            }

            if (aciData.data.TryGetValue("CoordinateSubType", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].RestrictionData.CoordinateSubType = temp[i] + 1;
                }
            }

            if (aciData.data.TryGetValue("Creator", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<string[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    var nameref = coordinateData[i].CreatorNames = new List<string>();
                    if (temp[i] != string.Empty)
                    {
                        nameref.Add(temp[i]);
                    }
                }
            }

            if (aciData.data.TryGetValue("Set_Name", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<string[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].SetNames = temp[i];
                }
            }

            if (aciData.data.TryGetValue("SubSetNames", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<string[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].SubSetNames = temp[i];
                }
            }

            if (aciData.data.TryGetValue("ClothNot", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].ClothNotData = temp[i];
                }
            }

            if (aciData.data.TryGetValue("GenderType", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].RestrictionData.GenderType = temp[i];
                }
            }

            if (aciData.data.TryGetValue("Cosplay_Academy_Ready", out byteData) && byteData != null)
            {
                cardInfo.CosplayReady = MessagePackSerializer.Deserialize<bool>((byte[])byteData);
            }
        }

        public static void StandardCoordinateMigrate(ChaFileCoordinate file, PluginData aciData)
        {
            if (aciData.version == 2)
            {
                return;
            }

            if (aciData.version > 2)
            {
                Settings.Logger.LogWarning("New version of ACI detected please update");
                return;
            }

            var coordInfo = new CoordinateInfoV1();
            switch (aciData.version)
            {
                case 0:
                    coordInfo = StandardCoordinateMigrateV0(aciData);
                    break;
                case 1:
                {
                    if (aciData.data.TryGetValue("CoordinateInfo", out var byteData) && byteData != null)
                    {
                        coordInfo = CoordinateInfoV1.Deserialize(byteData);
                    }

                    break;
                }
            }

            CoordProcess(file, coordInfo);
        }

        private static void CoordProcess(ChaFileCoordinate file, CoordinateInfoV1 infoV1, string advancedDirect = null)
        {
            advancedDirect = advancedDirect ?? string.Empty;

            var pluginData = new PluginData { version = 2 };
            var slotDict = new Dictionary<int, SlotData>();
            foreach (var item in infoV1.AccKeep)
            {
                slotDict[item] = new SlotData { KeepState = KeepState.NonHairKeep };
            }

            foreach (var item in infoV1.HairAcc)
            {
                if (slotDict.TryGetValue(item, out var slotData))
                {
                    slotData.KeepState = KeepState.HairKeep;
                    continue;
                }

                slotDict[item] = new SlotData { KeepState = KeepState.HairKeep };
            }

            var parts = file.accessory.parts;
            foreach (var item in slotDict)
            {
                if (item.Key >= parts.Length)
                {
                    continue;
                }

                pluginData.data[Constants.AccessoryKey] = item.Value.Serialize();
                parts[item.Key].SetExtendedDataById(Settings.GUID, pluginData);
            }

            pluginData.data.Clear();

            file.accessory.SetExtendedDataById(Settings.GUID, new CoordinateInfo
            {
                ClothNotData = infoV1.ClothNotData,
                CoordinateSaveBools = infoV1.CoordinateSaveBools,
                CreatorNames = infoV1.CreatorNames,
                MakeUpKeep = infoV1.MakeUpKeep,
                RestrictionData = infoV1.RestrictionData,
                SetNames = infoV1.SetNames,
                SubSetNames = infoV1.SubSetNames,
                AdvancedFolder = advancedDirect
            }.Serialize());
        }

        private static CoordinateInfoV1 StandardCoordinateMigrateV0(PluginData aciData)
        {
            var coordinateInfoV1 = new CoordinateInfoV1();
            var restrictionInfo = coordinateInfoV1.RestrictionData;

            foreach (var valuePair in aciData.data)
            {
                var byteData = valuePair.Value as byte[];
                switch (valuePair.Key)
                {
                    case "HairAcc":
                        coordinateInfoV1.HairAcc = MessagePackSerializer.Deserialize<List<int>>(byteData);
                        break;
                    case "CoordinateSaveBools":
                        var boolRef = MessagePackSerializer.Deserialize<bool[]>(byteData);
                        if (boolRef == null || boolRef.Length != 9)
                        {
                            boolRef = new bool[9];
                        }

                        coordinateInfoV1.CoordinateSaveBools = boolRef;
                        break;
                    case "AccKeep":
                        coordinateInfoV1.AccKeep = MessagePackSerializer.Deserialize<List<int>>(byteData);
                        break;
                    case "CoordinateType":
                        restrictionInfo.CoordinateType = MessagePackSerializer.Deserialize<int>(byteData) + 1;
                        break;
                    case "CoordinateSubType":
                        restrictionInfo.CoordinateSubType = MessagePackSerializer.Deserialize<int>(byteData) + 1;
                        break;
                    case "Set_Name":
                        coordinateInfoV1.SetNames = MessagePackSerializer.Deserialize<string>(byteData) ?? string.Empty;
                        break;
                    case "SubSetNames":
                        coordinateInfoV1.SubSetNames =
                            MessagePackSerializer.Deserialize<string>(byteData) ?? string.Empty;
                        break;
                    case "ClothNot":
                        var notRef = MessagePackSerializer.Deserialize<bool[]>(byteData);
                        if (notRef == null || notRef.Length != 3)
                        {
                            notRef = new bool[3];
                        }

                        coordinateInfoV1.ClothNotData = notRef;
                        break;
                    case "GenderType":
                        restrictionInfo.GenderType = MessagePackSerializer.Deserialize<int>(byteData);
                        break;
                    case "Creator":
                        var list = coordinateInfoV1.CreatorNames = new List<string>();
                        var temp = MessagePackSerializer.Deserialize<string>(byteData);
                        if (!temp.IsNullOrEmpty())
                        {
                            list.Add(temp);
                        }

                        break;
                    case "Interest_Restriction":
                        restrictionInfo.Interest_Restriction =
                            MessagePackSerializer.Deserialize<Dictionary<int, int>>(byteData) ??
                            new Dictionary<int, int>();
                        break;
                    case "PersonalityType_Restriction":
                        restrictionInfo.PersonalityType_Restriction =
                            MessagePackSerializer.Deserialize<Dictionary<int, int>>(byteData) ??
                            new Dictionary<int, int>();
                        break;
                    case "TraitType_Restriction":
                        restrictionInfo.TraitType_Restriction =
                            MessagePackSerializer.Deserialize<Dictionary<int, int>>(byteData) ??
                            new Dictionary<int, int>();
                        break;
                    case "HstateType_Restriction":
                        restrictionInfo.HStateType_Restriction =
                            MessagePackSerializer.Deserialize<int>(byteData);
                        break;
                    case "ClubType_Restriction":
                        restrictionInfo.ClubType_Restriction = MessagePackSerializer.Deserialize<int>(byteData);
                        break;
                    case "Height_Restriction":
                        boolRef = MessagePackSerializer.Deserialize<bool[]>(byteData);
                        if (boolRef == null || boolRef.Length != 3)
                        {
                            boolRef = new bool[3];
                        }

                        restrictionInfo.Height_Restriction = boolRef;
                        break;
                    case "Breastsize_Restriction":
                        boolRef = MessagePackSerializer.Deserialize<bool[]>(byteData);
                        if (boolRef == null || boolRef.Length != 3)
                        {
                            boolRef = new bool[3];
                        }

                        restrictionInfo.BreastSize_Restriction = boolRef;
                        break;
                }
            }

            return coordinateInfoV1;
        }

        private static void PrintOutdated()
        {
            Settings.Logger.LogWarning("New version of ACI detected please update");
        }

        [Serializable]
        [MessagePackObject]
        public class CardInfoV1 : IMessagePackSerializationCallbackReceiver
        {
            [Key("_cosplayready")]
            public bool CosplayReady;

            [Key("_advdir")]
            public bool AdvancedDirectory;

            [Key("_personalsavebool")]
            public bool[] PersonalClothingBools;

            [Key("_simpledirectory")]
            public string SimpleFolderDirectory;

            [Key("_advdirectory")]
            public Dictionary<string, string> AdvancedFolderDirectory;

            public CardInfoV1() => NullCheck();

            public void OnBeforeSerialize() { }

            public void OnAfterDeserialize()
            {
                NullCheck();
            }

            private void NullCheck()
            {
                SimpleFolderDirectory = SimpleFolderDirectory ?? string.Empty;
                AdvancedFolderDirectory = AdvancedFolderDirectory ?? new Dictionary<string, string>();
                PersonalClothingBools = PersonalClothingBools ?? new bool[9];
            }

            public PluginData Serialize() =>
                new PluginData
                {
                    version = Constants.MasterSaveVersion,
                    data = new Dictionary<string, object>
                        { [Constants.CardKey] = MessagePackSerializer.Serialize(this) }
                };

            public static CardInfoV1 Deserialize(object bytearray) =>
                MessagePackSerializer.Deserialize<CardInfoV1>((byte[])bytearray);
        }

        [Serializable]
        [MessagePackObject]
        public class CoordinateInfoV1 : IMessagePackSerializationCallbackReceiver
        {
            public CoordinateInfoV1() => NullCheck();

            public void OnBeforeSerialize() { }

            public void OnAfterDeserialize() => NullCheck();

            private void NullCheck()
            {
                ClothNotData = ClothNotData ?? new bool[3];
                CoordinateSaveBools = CoordinateSaveBools ?? new bool[9];
                AccKeep = AccKeep ?? new List<int>();
                HairAcc = HairAcc ?? new List<int>();
                CreatorNames = CreatorNames ?? new List<string>();
                SetNames = SetNames ?? string.Empty;
                SubSetNames = SubSetNames ?? string.Empty;
                RestrictionData = RestrictionData ?? new RestrictionData();
            }

            public PluginData Serialize() =>
                new PluginData
                {
                    version = Constants.MasterSaveVersion,
                    data = new Dictionary<string, object>
                        { [Constants.CoordinateKey] = MessagePackSerializer.Serialize(this) }
                };

            public static CoordinateInfoV1 Deserialize(object bytearray) =>
                MessagePackSerializer.Deserialize<CoordinateInfoV1>((byte[])bytearray);

            public static Dictionary<int, CoordinateInfoV1> DictDeserialize(object bytearray) =>
                MessagePackSerializer.Deserialize<Dictionary<int, CoordinateInfoV1>>((byte[])bytearray);

            #region fields

            [Key("_makeup")]
            public bool MakeUpKeep;

            [Key("_clothnot")]
            public bool[] ClothNotData;

            [Key("_coordsavebool")]
            public bool[] CoordinateSaveBools;

            [Key("_acckeep")]
            public List<int> AccKeep;

            [Key("_hairkeep")]
            public List<int> HairAcc;

            [Key("_creatornames")]
            public List<string> CreatorNames;

            [Key("_set")]
            public string SetNames;

            [Key("_subset")]
            public string SubSetNames;

            [FormerlySerializedAs("RestrictionInfo")]
            [Key("_restrictioninfo")]
            public RestrictionData RestrictionData;

            #endregion
        }
    }
}