using ExtensibleSaveFormat;
using Extensions;
using MessagePack;
using System;
using System.Collections.Generic;
using static ExtensibleSaveFormat.Extensions;

namespace Additional_Card_Info
{
    public static class Migrator
    {
        public static SlotInfo SlotInfoMigrate(PluginData pluginData)
        {
            switch (pluginData.version)
            {
                case 2:
                    if (pluginData.data.TryGetValue(Constants.AccessoryKey, out var bytearray))
                    {
                        return SlotInfo.Deserialize(bytearray);
                    }
                    break;
                default:
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

        public static CardInfo CardInfoMigrate(PluginData pluginData)
        {
            switch (pluginData.version)
            {
                case 2:
                    if (pluginData.data.TryGetValue(Constants.CardKey, out var bytearray) && bytearray != null)
                    {
                        return CardInfo.Deserialize(bytearray);
                    }
                    break;
                default:
                    break;
            }
            return null;
        }

        public static CardInfo StandardCharaMigrate(ChaControl control, PluginData ACI_Data)
        {
            if (ACI_Data.version == 2)
            {
                if (ACI_Data.data.TryGetValue(Constants.CardKey, out var bytearray) && bytearray != null)
                {
                    return CardInfo.Deserialize(bytearray);
                }
            }

            if (ACI_Data.version > 2)
            {

            }

            var oldcoordinatedata = new Dictionary<int, CoordinateInfoV1>();
            var oldcardinfo = new CardInfoV1();

            if (ACI_Data.version == 0)
            {
                StandardCharaMigrateV0(ACI_Data, ref oldcoordinatedata, ref oldcardinfo);
            }

            if (ACI_Data.version == 1)
            {
                if (ACI_Data.data.TryGetValue("CardInfo", out var ByteData) && ByteData != null)
                {
                    oldcardinfo = CardInfoV1.Deserialize(ByteData);
                }
                if (ACI_Data.data.TryGetValue("CoordinateInfo", out ByteData) && ByteData != null)
                {
                    oldcoordinatedata = CoordinateInfoV1.DictDeserialize(ByteData);
                }
            }

            foreach (var item in oldcoordinatedata)
            {
                if (item.Key >= control.chaFile.coordinate.Length)
                {
                    continue;
                }
                string advstring = null;
                if (item.Key < Constants.Coordinates.Length)
                {
                    oldcardinfo.AdvancedFolderDirectory.TryGetValue(Constants.Coordinates[item.Key], out advstring);
                }

                CoordProcess(control.chaFile.coordinate[item.Key], item.Value, advstring);

                if (item.Key == control.fileStatus.coordinateType)
                {
                    var basefile = control.chaFile.coordinate[item.Key];

                    if (basefile.accessory.TryGetExtendedDataById(Settings.GUID, out var basedata))
                    {
                        control.nowCoordinate.accessory.SetExtendedDataById(Settings.GUID, basedata);
                    }
                    var baseparts = basefile.accessory.parts;
                    var nowparts = control.nowCoordinate.accessory.parts;
                    for (var i = 0; i < baseparts.Length; i++)
                    {
                        if (baseparts[i].TryGetExtendedDataById(Settings.GUID, out basedata))
                        {
                            nowparts[i].SetExtendedDataById(Settings.GUID, basedata);
                        }
                    }
                }
            }

            var newcardinfo = new CardInfo(oldcardinfo);

            ACI_Data.version = 2;
            ACI_Data.data.Clear();
            ACI_Data.data[Constants.CardKey] = MessagePackSerializer.Serialize(newcardinfo);
            return newcardinfo;
        }

        public static void StandardCharaMigrateV0(PluginData ACI_Data, ref Dictionary<int, CoordinateInfoV1> coordinatedata, ref CardInfoV1 cardinfo)
        {
            if (ACI_Data.data.TryGetValue("HairAcc", out var ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<int>[]>((byte[])ByteData);
                for (var i = 0; i < 7 && i < coordinatedata.Count; i++)
                {
                    coordinatedata[i].HairAcc = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("AccKeep", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<int>[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinatedata[i].AccKeep = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("MakeUpKeep", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    if (temp[i])
                        coordinatedata[i].MakeUpKeep = true;
                }
            }
            if (ACI_Data.data.TryGetValue("CoordinateSaveBools", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinatedata[i].CoordinateSaveBools = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("PersonalityType_Restriction", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinatedata[i].RestrictionInfo.PersonalityType_Restriction = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("Interest_Restriction", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinatedata[i].RestrictionInfo.Interest_Restriction = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("TraitType_Restriction", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinatedata[i].RestrictionInfo.TraitType_Restriction = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("HstateType_Restriction", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinatedata[i].RestrictionInfo.HstateType_Restriction = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("ClubType_Restriction", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinatedata[i].RestrictionInfo.ClubType_Restriction = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("Height_Restriction", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinatedata[i].RestrictionInfo.Height_Restriction = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("Breastsize_Restriction", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinatedata[i].RestrictionInfo.Breastsize_Restriction = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("CoordinateType", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinatedata[i].RestrictionInfo.CoordinateType = temp[i] + 1;
                }
            }
            if (ACI_Data.data.TryGetValue("CoordinateSubType", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinatedata[i].RestrictionInfo.CoordinateSubType = temp[i] + 1;
                }
            }
            if (ACI_Data.data.TryGetValue("Creator", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<string[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    var nameref = coordinatedata[i].CreatorNames = new List<string>();
                    if (temp[i] != "")
                    {
                        nameref.Add(temp[i]);
                    }
                }
            }
            if (ACI_Data.data.TryGetValue("Set_Name", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<string[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinatedata[i].SetNames = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("SubSetNames", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<string[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinatedata[i].SubSetNames = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("ClothNot", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinatedata[i].ClothNotData = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("GenderType", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    coordinatedata[i].RestrictionInfo.GenderType = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("Cosplay_Academy_Ready", out ByteData) && ByteData != null)
            {
                cardinfo.CosplayReady = MessagePackSerializer.Deserialize<bool>((byte[])ByteData);
            }
        }

        public static void StandardCoordinateMigrate(ChaFileCoordinate file, PluginData ACI_Data)
        {
            if (ACI_Data.version == 2)
            {
                return;
            }
            if (ACI_Data.version > 2)
            {
                Settings.Logger.LogWarning("New version of ACI detected please update");
                return;
            }
            var coordinfo = new CoordinateInfoV1();
            if (ACI_Data.version == 0)
            {
                coordinfo = StandardCoordinateMigrateV0(ACI_Data);
            }

            if (ACI_Data.version == 1)
            {
                if (ACI_Data.data.TryGetValue("CoordinateInfo", out var ByteData) && ByteData != null)
                {
                    coordinfo = CoordinateInfoV1.Deserialize(ByteData);
                }
            }

            CoordProcess(file, coordinfo);
        }

        private static void CoordProcess(ChaFileCoordinate file, CoordinateInfoV1 infoV1, string advanceddirect = null)
        {
            if (advanceddirect == null)
            {
                advanceddirect = "";
            }

            var plugindata = new PluginData() { version = 2 };
            var slotdict = new Dictionary<int, SlotInfo>();
            foreach (var item in infoV1.AccKeep)
            {
                slotdict[item] = new SlotInfo() { KeepState = KeepState.NonHairKeep };
            }

            foreach (var item in infoV1.HairAcc)
            {
                if (slotdict.TryGetValue(item, out var slotInfo))
                {
                    slotInfo.KeepState = KeepState.HairKeep;
                    continue;
                }
                slotdict[item] = new SlotInfo() { KeepState = KeepState.HairKeep };
            }

            var parts = file.accessory.parts;
            foreach (var item in slotdict)
            {
                if (item.Key >= parts.Length)
                {
                    continue;
                }
                plugindata.data[Constants.AccessoryKey] = item.Value.Serialize();
                parts[item.Key].SetExtendedDataById(Settings.GUID, plugindata);
            }
            plugindata.data.Clear();

            var newcoordata = new CoordinateInfo()
            {
                ClothNotData = infoV1.ClothNotData,
                CoordinateSaveBools = infoV1.CoordinateSaveBools,
                CreatorNames = infoV1.CreatorNames,
                MakeUpKeep = infoV1.MakeUpKeep,
                RestrictionInfo = infoV1.RestrictionInfo,
                SetNames = infoV1.SetNames,
                SubSetNames = infoV1.SubSetNames,
                AdvancedFolder = advanceddirect
            };
            file.accessory.SetExtendedDataById(Settings.GUID, newcoordata.Serialize());
        }

        public static CoordinateInfoV1 StandardCoordinateMigrateV0(PluginData ACI_Data)
        {
            var CoordinateInfoV1 = new CoordinateInfoV1();
            var RestrictionInfo = CoordinateInfoV1.RestrictionInfo;
            if (ACI_Data.data.TryGetValue("HairAcc", out var ByteData) && ByteData != null)
            {
                CoordinateInfoV1.HairAcc = MessagePackSerializer.Deserialize<List<int>>((byte[])ByteData);
            }
            if (ACI_Data.data.TryGetValue("CoordinateSaveBools", out ByteData) && ByteData != null)
            {
                var boolref = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                if (boolref == null || boolref.Length != 9)
                {
                    boolref = new bool[9];
                }
                CoordinateInfoV1.CoordinateSaveBools = boolref;
            }
            if (ACI_Data.data.TryGetValue("AccKeep", out ByteData) && ByteData != null)
            {
                CoordinateInfoV1.AccKeep = MessagePackSerializer.Deserialize<List<int>>((byte[])ByteData);
            }
            if (ACI_Data.data.TryGetValue("CoordinateType", out ByteData) && ByteData != null)
            {
                RestrictionInfo.CoordinateType = MessagePackSerializer.Deserialize<int>((byte[])ByteData) + 1;
            }
            if (ACI_Data.data.TryGetValue("CoordinateSubType", out ByteData) && ByteData != null)
            {
                RestrictionInfo.CoordinateSubType = MessagePackSerializer.Deserialize<int>((byte[])ByteData) + 1;
            }
            if (ACI_Data.data.TryGetValue("Set_Name", out ByteData) && ByteData != null)
            {
                var stringref = MessagePackSerializer.Deserialize<string>((byte[])ByteData);
                if (stringref == null)
                {
                    stringref = "";
                }
                CoordinateInfoV1.SetNames = stringref;
            }
            if (ACI_Data.data.TryGetValue("SubSetNames", out ByteData) && ByteData != null)
            {
                var stringref = MessagePackSerializer.Deserialize<string>((byte[])ByteData);
                CoordinateInfoV1.SubSetNames = stringref;
            }
            if (ACI_Data.data.TryGetValue("ClothNot", out ByteData) && ByteData != null)
            {
                var notref = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                if (notref == null || notref.Length != 3)
                {
                    notref = new bool[3];
                }
                CoordinateInfoV1.ClothNotData = notref;
            }
            if (ACI_Data.data.TryGetValue("GenderType", out ByteData) && ByteData != null)
            {
                RestrictionInfo.GenderType = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
            }
            if (ACI_Data.data.TryGetValue("Creator", out ByteData) && ByteData != null)
            {
                var list = CoordinateInfoV1.CreatorNames = new List<string>();
                var temp = MessagePackSerializer.Deserialize<string>((byte[])ByteData);
                if (temp != "")
                {
                    list.Add(temp);
                }
            }

            if (ACI_Data.data.TryGetValue("Interest_Restriction", out ByteData) && ByteData != null)
            {
                var dictref = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                if (dictref == null)
                {
                    dictref = new Dictionary<int, int>();
                }
                RestrictionInfo.Interest_Restriction = dictref;
            }
            if (ACI_Data.data.TryGetValue("PersonalityType_Restriction", out ByteData) && ByteData != null)
            {
                var dictref = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                if (dictref == null)
                {
                    dictref = new Dictionary<int, int>();
                }

                RestrictionInfo.PersonalityType_Restriction = dictref;
            }
            if (ACI_Data.data.TryGetValue("TraitType_Restriction", out ByteData) && ByteData != null)
            {
                var dictref = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                if (dictref == null)
                {
                    dictref = new Dictionary<int, int>();
                }
                RestrictionInfo.TraitType_Restriction = dictref;
            }

            if (ACI_Data.data.TryGetValue("HstateType_Restriction", out ByteData) && ByteData != null)
            {
                RestrictionInfo.HstateType_Restriction = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
            }
            if (ACI_Data.data.TryGetValue("ClubType_Restriction", out ByteData) && ByteData != null)
            {
                RestrictionInfo.ClubType_Restriction = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
            }
            if (ACI_Data.data.TryGetValue("Height_Restriction", out ByteData) && ByteData != null)
            {
                var boolref = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                if (boolref == null || boolref.Length != 3)
                {
                    boolref = new bool[3];
                }
                RestrictionInfo.Height_Restriction = boolref;
            }
            if (ACI_Data.data.TryGetValue("Breastsize_Restriction", out ByteData) && ByteData != null)
            {
                var boolref = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                if (boolref == null || boolref.Length != 3)
                {
                    boolref = new bool[3];
                }
                RestrictionInfo.Breastsize_Restriction = boolref;
            }

            return CoordinateInfoV1;
        }

        private static void PrintOutdated() => Settings.Logger.LogWarning("New version of ACI detected please update");

        [Serializable]
        [MessagePackObject]
        public class CardInfoV1
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

            public CardInfoV1() { NullCheck(); }

            public CardInfoV1(bool _advdir, bool _cosplayready, bool[] _personalsavebool, string _simpledirectory, Dictionary<string, string> _advdirectory)
            {
                CosplayReady = _cosplayready;
                SimpleFolderDirectory = _simpledirectory;
                AdvancedDirectory = _advdir;
                PersonalClothingBools = _personalsavebool.ToNewArray(9);

                AdvancedFolderDirectory = _advdirectory.ToNewDictionary();
                NullCheck();
            }

            private void NullCheck()
            {
                if (SimpleFolderDirectory == null) SimpleFolderDirectory = "";
                if (AdvancedFolderDirectory == null) AdvancedFolderDirectory = new Dictionary<string, string>();
                if (PersonalClothingBools == null) PersonalClothingBools = new bool[9];
            }

            public PluginData Serialize() => new PluginData { version = Constants.MasterSaveVersion, data = new Dictionary<string, object>() { [Constants.CardKey] = MessagePackSerializer.Serialize(this) } };

            public static CardInfoV1 Deserialize(object bytearray) => MessagePackSerializer.Deserialize<CardInfoV1>((byte[])bytearray);
        }

        [Serializable]
        [MessagePackObject]
        public class CoordinateInfoV1
        {
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

            [Key("_restrictioninfo")]
            public RestrictionInfo RestrictionInfo;
            #endregion

            public CoordinateInfoV1() { NullCheck(); }

            public CoordinateInfoV1(CoordinateInfoV1 _copy) => CopyData(_copy);

            public void CopyData(CoordinateInfoV1 _copy) => CopyData(_copy.ClothNotData, _copy.CoordinateSaveBools, _copy.AccKeep, _copy.HairAcc, _copy.CreatorNames, _copy.SetNames, _copy.SubSetNames, _copy.RestrictionInfo);

            public void CopyData(bool[] _clothnot, bool[] _coordsavebool, List<int> _acckeep, List<int> _hairkeep, List<string> _creatornames, string _set, string _subset, RestrictionInfo _RestrictionInfo)
            {
                SetNames = _set;
                SubSetNames = _subset;

                CoordinateSaveBools = _coordsavebool.ToNewArray(9);
                AccKeep = _acckeep.ToNewList();
                HairAcc = _hairkeep.ToNewList();
                CreatorNames = _creatornames.ToNewList();
                ClothNotData = _clothnot.ToNewArray(3);

                if (_RestrictionInfo != null) RestrictionInfo = new RestrictionInfo(_RestrictionInfo);
                else RestrictionInfo = null;
                NullCheck();
            }
            private void NullCheck()
            {
                if (ClothNotData == null) ClothNotData = new bool[3];
                if (CoordinateSaveBools == null) CoordinateSaveBools = new bool[9];
                if (AccKeep == null) AccKeep = new List<int>();
                if (HairAcc == null) HairAcc = new List<int>();
                if (CreatorNames == null) CreatorNames = new List<string>();
                if (SetNames == null) SetNames = "";
                if (SubSetNames == null) SubSetNames = "";
                if (RestrictionInfo == null) RestrictionInfo = new RestrictionInfo();
            }

            public PluginData Serialize() => new PluginData { version = Constants.MasterSaveVersion, data = new Dictionary<string, object>() { [Constants.CoordinateKey] = MessagePackSerializer.Serialize(this) } };

            public static CoordinateInfoV1 Deserialize(object bytearray) => MessagePackSerializer.Deserialize<CoordinateInfoV1>((byte[])bytearray);
            public static Dictionary<int, CoordinateInfoV1> DictDeserialize(object bytearray) => MessagePackSerializer.Deserialize<Dictionary<int, CoordinateInfoV1>>((byte[])bytearray);
        }
    }
}
