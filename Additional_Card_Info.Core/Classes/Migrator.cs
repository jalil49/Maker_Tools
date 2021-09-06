using ExtensibleSaveFormat;
using MessagePack;
using System.Collections.Generic;

namespace Additional_Card_Info
{
    public static class Migrator
    {
        public static void MigrateV0(PluginData ACI_Data, ref DataStruct data)
        {
            if (ACI_Data.data.TryGetValue("HairAcc", out var ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<int>[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    data.CoordinateInfo[i].HairAcc = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("AccKeep", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<int>[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    data.CoordinateInfo[i].AccKeep = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("MakeUpKeep", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    if (temp[i])
                        data.CoordinateInfo[i].MakeUpKeep = true;
                }
            }
            if (ACI_Data.data.TryGetValue("CoordinateSaveBools", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    data.CoordinateInfo[i].CoordinateSaveBools = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("PersonalityType_Restriction", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    data.CoordinateInfo[i].RestrictionInfo.PersonalityType_Restriction = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("Interest_Restriction", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    data.CoordinateInfo[i].RestrictionInfo.Interest_Restriction = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("TraitType_Restriction", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    data.CoordinateInfo[i].RestrictionInfo.TraitType_Restriction = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("HstateType_Restriction", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    data.CoordinateInfo[i].RestrictionInfo.HstateType_Restriction = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("ClubType_Restriction", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    data.CoordinateInfo[i].RestrictionInfo.ClubType_Restriction = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("Height_Restriction", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    data.CoordinateInfo[i].RestrictionInfo.Height_Restriction = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("Breastsize_Restriction", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    data.CoordinateInfo[i].RestrictionInfo.Breastsize_Restriction = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("CoordinateType", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    data.CoordinateInfo[i].RestrictionInfo.CoordinateType = temp[i] + 1;
                }
            }
            if (ACI_Data.data.TryGetValue("CoordinateSubType", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    data.CoordinateInfo[i].RestrictionInfo.CoordinateSubType = temp[i] + 1;
                }
            }
            if (ACI_Data.data.TryGetValue("Creator", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<string[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    var nameref = data.CoordinateInfo[i].CreatorNames = new List<string>();
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
                    data.CoordinateInfo[i].SetNames = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("SubSetNames", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<string[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    data.CoordinateInfo[i].SubSetNames = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("ClothNot", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    data.CoordinateInfo[i].ClothNotData = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("GenderType", out ByteData) && ByteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])ByteData);
                for (var i = 0; i < 7; i++)
                {
                    data.CoordinateInfo[i].RestrictionInfo.GenderType = temp[i];
                }
            }
            if (ACI_Data.data.TryGetValue("Cosplay_Academy_Ready", out ByteData) && ByteData != null)
            {
                data.CardInfo.CosplayReady = MessagePackSerializer.Deserialize<bool>((byte[])ByteData);
            }
        }

        public static CoordinateInfo CoordinateMigrateV0(PluginData ACI_Data)
        {
            var coordinateInfo = new CoordinateInfo();
            var restrictionInfo = coordinateInfo.RestrictionInfo;
            if (ACI_Data.data.TryGetValue("HairAcc", out var ByteData) && ByteData != null)
            {
                coordinateInfo.HairAcc = MessagePackSerializer.Deserialize<List<int>>((byte[])ByteData);
            }
            if (ACI_Data.data.TryGetValue("CoordinateSaveBools", out ByteData) && ByteData != null)
            {
                var boolref = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                if (boolref == null || boolref.Length != 9)
                {
                    boolref = new bool[9];
                }
                coordinateInfo.CoordinateSaveBools = boolref;
            }
            if (ACI_Data.data.TryGetValue("AccKeep", out ByteData) && ByteData != null)
            {
                coordinateInfo.AccKeep = MessagePackSerializer.Deserialize<List<int>>((byte[])ByteData);
            }
            if (ACI_Data.data.TryGetValue("CoordinateType", out ByteData) && ByteData != null)
            {
                restrictionInfo.CoordinateType = MessagePackSerializer.Deserialize<int>((byte[])ByteData) + 1;
            }
            if (ACI_Data.data.TryGetValue("CoordinateSubType", out ByteData) && ByteData != null)
            {
                restrictionInfo.CoordinateSubType = MessagePackSerializer.Deserialize<int>((byte[])ByteData) + 1;
            }
            if (ACI_Data.data.TryGetValue("Set_Name", out ByteData) && ByteData != null)
            {
                var stringref = MessagePackSerializer.Deserialize<string>((byte[])ByteData);
                if (stringref == null)
                {
                    stringref = "";
                }
                coordinateInfo.SetNames = stringref;
            }
            if (ACI_Data.data.TryGetValue("SubSetNames", out ByteData) && ByteData != null)
            {
                var stringref = MessagePackSerializer.Deserialize<string>((byte[])ByteData);
                coordinateInfo.SubSetNames = stringref;
            }
            if (ACI_Data.data.TryGetValue("ClothNot", out ByteData) && ByteData != null)
            {
                var notref = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                if (notref == null || notref.Length != 3)
                {
                    notref = new bool[3];
                }
                coordinateInfo.ClothNotData = notref;
            }
            if (ACI_Data.data.TryGetValue("GenderType", out ByteData) && ByteData != null)
            {
                restrictionInfo.GenderType = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
            }
            if (ACI_Data.data.TryGetValue("Creator", out ByteData) && ByteData != null)
            {
                var list = coordinateInfo.CreatorNames = new List<string>();
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
                restrictionInfo.Interest_Restriction = dictref;
            }
            if (ACI_Data.data.TryGetValue("PersonalityType_Restriction", out ByteData) && ByteData != null)
            {
                var dictref = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                if (dictref == null)
                {
                    dictref = new Dictionary<int, int>();
                }

                restrictionInfo.PersonalityType_Restriction = dictref;
            }
            if (ACI_Data.data.TryGetValue("TraitType_Restriction", out ByteData) && ByteData != null)
            {
                var dictref = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                if (dictref == null)
                {
                    dictref = new Dictionary<int, int>();
                }
                restrictionInfo.TraitType_Restriction = dictref;
            }

            if (ACI_Data.data.TryGetValue("HstateType_Restriction", out ByteData) && ByteData != null)
            {
                restrictionInfo.HstateType_Restriction = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
            }
            if (ACI_Data.data.TryGetValue("ClubType_Restriction", out ByteData) && ByteData != null)
            {
                restrictionInfo.ClubType_Restriction = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
            }
            if (ACI_Data.data.TryGetValue("Height_Restriction", out ByteData) && ByteData != null)
            {
                var boolref = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                if (boolref == null || boolref.Length != 3)
                {
                    boolref = new bool[3];
                }
                restrictionInfo.Height_Restriction = boolref;
            }
            if (ACI_Data.data.TryGetValue("Breastsize_Restriction", out ByteData) && ByteData != null)
            {
                var boolref = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                if (boolref == null || boolref.Length != 3)
                {
                    boolref = new bool[3];
                }
                restrictionInfo.Breastsize_Restriction = boolref;
            }

            return coordinateInfo;
        }
    }
}
