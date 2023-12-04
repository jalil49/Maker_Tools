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

                if (baseFile.accessory.TryGetExtendedDataById(Settings.Guid, out var baseData))
                {
                    control.nowCoordinate.accessory.SetExtendedDataById(Settings.Guid, baseData);
                }

                var baseParts = baseFile.accessory.parts;
                var nowParts = control.nowCoordinate.accessory.parts;
                for (var i = 0; i < baseParts.Length; i++)
                {
                    if (baseParts[i].TryGetExtendedDataById(Settings.Guid, out baseData))
                    {
                        nowParts[i].SetExtendedDataById(Settings.Guid, baseData);
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
                    coordinateData[i].hairAcc = temp[i];
                }
            }

            if (aciData.data.TryGetValue("AccKeep", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<int>[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].accKeep = temp[i];
                }
            }

            if (aciData.data.TryGetValue("MakeUpKeep", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    if (temp[i])
                    {
                        coordinateData[i].makeUpKeep = true;
                    }
                }
            }

            if (aciData.data.TryGetValue("CoordinateSaveBools", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].coordinateSaveBools = temp[i];
                }
            }

            if (aciData.data.TryGetValue("PersonalityType_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].restrictionData.PersonalityTypeRestriction = temp[i];
                }
            }

            if (aciData.data.TryGetValue("Interest_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].restrictionData.InterestRestriction = temp[i];
                }
            }

            if (aciData.data.TryGetValue("TraitType_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].restrictionData.TraitTypeRestriction = temp[i];
                }
            }

            if (aciData.data.TryGetValue("HstateType_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].restrictionData.hStateTypeRestriction = temp[i];
                }
            }

            if (aciData.data.TryGetValue("ClubType_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].restrictionData.clubTypeRestriction = temp[i];
                }
            }

            if (aciData.data.TryGetValue("Height_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].restrictionData.heightRestriction = temp[i];
                }
            }

            if (aciData.data.TryGetValue("Breastsize_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].restrictionData.breastSizeRestriction = temp[i];
                }
            }

            if (aciData.data.TryGetValue("CoordinateType", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].restrictionData.coordinateType = temp[i] + 1;
                }
            }

            if (aciData.data.TryGetValue("CoordinateSubType", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].restrictionData.coordinateSubType = temp[i] + 1;
                }
            }

            if (aciData.data.TryGetValue("Creator", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<string[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    var nameref = coordinateData[i].creatorNames = new List<string>();
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
                    coordinateData[i].setNames = temp[i];
                }
            }

            if (aciData.data.TryGetValue("SubSetNames", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<string[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].subSetNames = temp[i];
                }
            }

            if (aciData.data.TryGetValue("ClothNot", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].clothNotData = temp[i];
                }
            }

            if (aciData.data.TryGetValue("GenderType", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinateData[i].restrictionData.genderType = temp[i];
                }
            }

            if (aciData.data.TryGetValue("Cosplay_Academy_Ready", out byteData) && byteData != null)
            {
                cardInfo.cosplayReady = MessagePackSerializer.Deserialize<bool>((byte[])byteData);
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
            foreach (var item in infoV1.accKeep)
            {
                slotDict[item] = new SlotData { keepState = KeepState.NonHairKeep };
            }

            foreach (var item in infoV1.hairAcc)
            {
                if (slotDict.TryGetValue(item, out var slotData))
                {
                    slotData.keepState = KeepState.HairKeep;
                    continue;
                }

                slotDict[item] = new SlotData { keepState = KeepState.HairKeep };
            }

            var parts = file.accessory.parts;
            foreach (var item in slotDict)
            {
                if (item.Key >= parts.Length)
                {
                    continue;
                }

                pluginData.data[Constants.AccessoryKey] = item.Value.Serialize();
                parts[item.Key].SetExtendedDataById(Settings.Guid, pluginData);
            }

            pluginData.data.Clear();

            file.accessory.SetExtendedDataById(Settings.Guid, new CoordinateInfo
            {
                clothNotData = infoV1.clothNotData,
                coordinateSaveBools = infoV1.coordinateSaveBools,
                creatorNames = infoV1.creatorNames,
                makeUpKeep = infoV1.makeUpKeep,
                restrictionData = infoV1.restrictionData,
                setNames = infoV1.setNames,
                subSetNames = infoV1.subSetNames,
                advancedFolder = advancedDirect
            }.Serialize());
        }

        private static CoordinateInfoV1 StandardCoordinateMigrateV0(PluginData aciData)
        {
            var coordinateInfoV1 = new CoordinateInfoV1();
            var restrictionInfo = coordinateInfoV1.restrictionData;

            foreach (var valuePair in aciData.data)
            {
                var byteData = valuePair.Value as byte[];
                switch (valuePair.Key)
                {
                    case "HairAcc":
                        coordinateInfoV1.hairAcc = MessagePackSerializer.Deserialize<List<int>>(byteData);
                        break;
                    case "CoordinateSaveBools":
                        var boolRef = MessagePackSerializer.Deserialize<bool[]>(byteData);
                        if (boolRef == null || boolRef.Length != 9)
                        {
                            boolRef = new bool[9];
                        }

                        coordinateInfoV1.coordinateSaveBools = boolRef;
                        break;
                    case "AccKeep":
                        coordinateInfoV1.accKeep = MessagePackSerializer.Deserialize<List<int>>(byteData);
                        break;
                    case "CoordinateType":
                        restrictionInfo.coordinateType = MessagePackSerializer.Deserialize<int>(byteData) + 1;
                        break;
                    case "CoordinateSubType":
                        restrictionInfo.coordinateSubType = MessagePackSerializer.Deserialize<int>(byteData) + 1;
                        break;
                    case "Set_Name":
                        coordinateInfoV1.setNames = MessagePackSerializer.Deserialize<string>(byteData) ?? string.Empty;
                        break;
                    case "SubSetNames":
                        coordinateInfoV1.subSetNames =
                            MessagePackSerializer.Deserialize<string>(byteData) ?? string.Empty;
                        break;
                    case "ClothNot":
                        var notRef = MessagePackSerializer.Deserialize<bool[]>(byteData);
                        if (notRef == null || notRef.Length != 3)
                        {
                            notRef = new bool[3];
                        }

                        coordinateInfoV1.clothNotData = notRef;
                        break;
                    case "GenderType":
                        restrictionInfo.genderType = MessagePackSerializer.Deserialize<int>(byteData);
                        break;
                    case "Creator":
                        var list = coordinateInfoV1.creatorNames = new List<string>();
                        var temp = MessagePackSerializer.Deserialize<string>(byteData);
                        if (!temp.IsNullOrEmpty())
                        {
                            list.Add(temp);
                        }

                        break;
                    case "Interest_Restriction":
                        restrictionInfo.InterestRestriction =
                            MessagePackSerializer.Deserialize<Dictionary<int, int>>(byteData) ??
                            new Dictionary<int, int>();
                        break;
                    case "PersonalityType_Restriction":
                        restrictionInfo.PersonalityTypeRestriction =
                            MessagePackSerializer.Deserialize<Dictionary<int, int>>(byteData) ??
                            new Dictionary<int, int>();
                        break;
                    case "TraitType_Restriction":
                        restrictionInfo.TraitTypeRestriction =
                            MessagePackSerializer.Deserialize<Dictionary<int, int>>(byteData) ??
                            new Dictionary<int, int>();
                        break;
                    case "HstateType_Restriction":
                        restrictionInfo.hStateTypeRestriction =
                            MessagePackSerializer.Deserialize<int>(byteData);
                        break;
                    case "ClubType_Restriction":
                        restrictionInfo.clubTypeRestriction = MessagePackSerializer.Deserialize<int>(byteData);
                        break;
                    case "Height_Restriction":
                        boolRef = MessagePackSerializer.Deserialize<bool[]>(byteData);
                        if (boolRef == null || boolRef.Length != 3)
                        {
                            boolRef = new bool[3];
                        }

                        restrictionInfo.heightRestriction = boolRef;
                        break;
                    case "Breastsize_Restriction":
                        boolRef = MessagePackSerializer.Deserialize<bool[]>(byteData);
                        if (boolRef == null || boolRef.Length != 3)
                        {
                            boolRef = new bool[3];
                        }

                        restrictionInfo.breastSizeRestriction = boolRef;
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
            [FormerlySerializedAs("CosplayReady")] [Key("_cosplayready")]
            public bool cosplayReady;

            [FormerlySerializedAs("AdvancedDirectory")] [Key("_advdir")]
            public bool advancedDirectory;

            [FormerlySerializedAs("PersonalClothingBools")] [Key("_personalsavebool")]
            public bool[] personalClothingBools;

            [FormerlySerializedAs("SimpleFolderDirectory")] [Key("_simpledirectory")]
            public string simpleFolderDirectory;

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
                simpleFolderDirectory = simpleFolderDirectory ?? string.Empty;
                AdvancedFolderDirectory = AdvancedFolderDirectory ?? new Dictionary<string, string>();
                personalClothingBools = personalClothingBools ?? new bool[9];
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
                clothNotData = clothNotData ?? new bool[3];
                coordinateSaveBools = coordinateSaveBools ?? new bool[9];
                accKeep = accKeep ?? new List<int>();
                hairAcc = hairAcc ?? new List<int>();
                creatorNames = creatorNames ?? new List<string>();
                setNames = setNames ?? string.Empty;
                subSetNames = subSetNames ?? string.Empty;
                restrictionData = restrictionData ?? new RestrictionData();
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

            [FormerlySerializedAs("MakeUpKeep")] [Key("_makeup")]
            public bool makeUpKeep;

            [FormerlySerializedAs("ClothNotData")] [Key("_clothnot")]
            public bool[] clothNotData;

            [FormerlySerializedAs("CoordinateSaveBools")] [Key("_coordsavebool")]
            public bool[] coordinateSaveBools;

            [FormerlySerializedAs("AccKeep")] [Key("_acckeep")]
            public List<int> accKeep;

            [FormerlySerializedAs("HairAcc")] [Key("_hairkeep")]
            public List<int> hairAcc;

            [FormerlySerializedAs("CreatorNames")] [Key("_creatornames")]
            public List<string> creatorNames;

            [FormerlySerializedAs("SetNames")] [Key("_set")]
            public string setNames;

            [FormerlySerializedAs("SubSetNames")] [Key("_subset")]
            public string subSetNames;

            [FormerlySerializedAs("RestrictionData")]
            [FormerlySerializedAs("RestrictionInfo")]
            [Key("_restrictioninfo")]
            public RestrictionData restrictionData;

            #endregion
        }
    }
}