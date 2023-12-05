using System.Collections.Generic;
using ExtensibleSaveFormat;
using MessagePack;

namespace Additional_Card_Info
{
    public static class Migrator
    {
        public static void MigrateV0(PluginData aciData, ref DataStruct data)
        {
            for (var i = 0; i < 7; i++)
                if (!data.CoordinateInfo.TryGetValue(i, out var _))
                    data.CreateOutfit(i);

            if (aciData.data.TryGetValue("HairAcc", out var byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<int>[]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].hairAcc = temp[i];
            }

            if (aciData.data.TryGetValue("AccKeep", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<List<int>[]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].accKeep = temp[i];
            }

            if (aciData.data.TryGetValue("MakeUpKeep", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                    if (temp[i])
                        data.CoordinateInfo[i].makeUpKeep = true;
            }

            if (aciData.data.TryGetValue("CoordinateSaveBools", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].coordinateSaveBools = temp[i];
            }

            if (aciData.data.TryGetValue("PersonalityType_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].restrictionInfo.PersonalityTypeRestriction = temp[i];
            }

            if (aciData.data.TryGetValue("Interest_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].restrictionInfo.InterestRestriction = temp[i];
            }

            if (aciData.data.TryGetValue("TraitType_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].restrictionInfo.TraitTypeRestriction = temp[i];
            }

            if (aciData.data.TryGetValue("HstateType_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].restrictionInfo.hStateTypeRestriction = temp[i];
            }

            if (aciData.data.TryGetValue("ClubType_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].restrictionInfo.clubTypeRestriction = temp[i];
            }

            if (aciData.data.TryGetValue("Height_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].restrictionInfo.heightRestriction = temp[i];
            }

            // ReSharper disable once StringLiteralTypo
            if (aciData.data.TryGetValue("Breastsize_Restriction", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].restrictionInfo.breastsizeRestriction = temp[i];
            }

            if (aciData.data.TryGetValue("CoordinateType", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].restrictionInfo.coordinateType = temp[i] + 1;
            }

            if (aciData.data.TryGetValue("CoordinateSubType", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].restrictionInfo.coordinateSubType = temp[i] + 1;
            }

            if (aciData.data.TryGetValue("Creator", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<string[]>((byte[])byteData);
                for (var i = 0; i < 7; i++)
                {
                    var nameref = data.CoordinateInfo[i].creatorNames = new List<string>();
                    if (temp[i] != "") nameref.Add(temp[i]);
                }
            }

            if (aciData.data.TryGetValue("Set_Name", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<string[]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].setNames = temp[i];
            }

            if (aciData.data.TryGetValue("SubSetNames", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<string[]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].subSetNames = temp[i];
            }

            if (aciData.data.TryGetValue("ClothNot", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<bool[][]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].clothNotData = temp[i];
            }

            if (aciData.data.TryGetValue("GenderType", out byteData) && byteData != null)
            {
                var temp = MessagePackSerializer.Deserialize<int[]>((byte[])byteData);
                for (var i = 0; i < 7; i++) data.CoordinateInfo[i].restrictionInfo.genderType = temp[i];
            }

            if (aciData.data.TryGetValue("Cosplay_Academy_Ready", out byteData) && byteData != null)
                data.CardInfo.cosplayReady = MessagePackSerializer.Deserialize<bool>((byte[])byteData);
        }

        public static CoordinateInfo CoordinateMigrateV0(PluginData aciData)
        {
            var coordinateInfo = new CoordinateInfo();
            var restrictionInfo = coordinateInfo.restrictionInfo;
            if (aciData.data.TryGetValue("HairAcc", out var byteData) && byteData != null)
                coordinateInfo.hairAcc = MessagePackSerializer.Deserialize<List<int>>((byte[])byteData);
            if (aciData.data.TryGetValue("CoordinateSaveBools", out byteData) && byteData != null)
            {
                var boolref = MessagePackSerializer.Deserialize<bool[]>((byte[])byteData);
                if (boolref == null || boolref.Length != 9) boolref = new bool[9];
                coordinateInfo.coordinateSaveBools = boolref;
            }

            if (aciData.data.TryGetValue("AccKeep", out byteData) && byteData != null)
                coordinateInfo.accKeep = MessagePackSerializer.Deserialize<List<int>>((byte[])byteData);
            if (aciData.data.TryGetValue("CoordinateType", out byteData) && byteData != null)
                restrictionInfo.coordinateType = MessagePackSerializer.Deserialize<int>((byte[])byteData) + 1;
            if (aciData.data.TryGetValue("CoordinateSubType", out byteData) && byteData != null)
                restrictionInfo.coordinateSubType = MessagePackSerializer.Deserialize<int>((byte[])byteData) + 1;
            if (aciData.data.TryGetValue("Set_Name", out byteData) && byteData != null)
            {
                var stringref = MessagePackSerializer.Deserialize<string>((byte[])byteData);
                if (stringref == null) stringref = "";
                coordinateInfo.setNames = stringref;
            }

            if (aciData.data.TryGetValue("SubSetNames", out byteData) && byteData != null)
            {
                var stringref = MessagePackSerializer.Deserialize<string>((byte[])byteData);
                coordinateInfo.subSetNames = stringref;
            }

            if (aciData.data.TryGetValue("ClothNot", out byteData) && byteData != null)
            {
                var notref = MessagePackSerializer.Deserialize<bool[]>((byte[])byteData);
                if (notref == null || notref.Length != 3) notref = new bool[3];
                coordinateInfo.clothNotData = notref;
            }

            if (aciData.data.TryGetValue("GenderType", out byteData) && byteData != null)
                restrictionInfo.genderType = MessagePackSerializer.Deserialize<int>((byte[])byteData);
            if (aciData.data.TryGetValue("Creator", out byteData) && byteData != null)
            {
                var list = coordinateInfo.creatorNames = new List<string>();
                var temp = MessagePackSerializer.Deserialize<string>((byte[])byteData);
                if (temp != "") list.Add(temp);
            }

            if (aciData.data.TryGetValue("Interest_Restriction", out byteData) && byteData != null)
            {
                var dictRef = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])byteData);
                if (dictRef == null) dictRef = new Dictionary<int, int>();
                restrictionInfo.InterestRestriction = dictRef;
            }

            if (aciData.data.TryGetValue("PersonalityType_Restriction", out byteData) && byteData != null)
            {
                var dictRef = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])byteData);
                if (dictRef == null) dictRef = new Dictionary<int, int>();

                restrictionInfo.PersonalityTypeRestriction = dictRef;
            }

            if (aciData.data.TryGetValue("TraitType_Restriction", out byteData) && byteData != null)
            {
                var dictRef = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])byteData);
                if (dictRef == null) dictRef = new Dictionary<int, int>();
                restrictionInfo.TraitTypeRestriction = dictRef;
            }

            // ReSharper disable once StringLiteralTypo
            if (aciData.data.TryGetValue("HstateType_Restriction", out byteData) && byteData != null)
                restrictionInfo.hStateTypeRestriction = MessagePackSerializer.Deserialize<int>((byte[])byteData);
            if (aciData.data.TryGetValue("ClubType_Restriction", out byteData) && byteData != null)
                restrictionInfo.clubTypeRestriction = MessagePackSerializer.Deserialize<int>((byte[])byteData);
            if (aciData.data.TryGetValue("Height_Restriction", out byteData) && byteData != null)
            {
                var boolRef = MessagePackSerializer.Deserialize<bool[]>((byte[])byteData);
                if (boolRef == null || boolRef.Length != 3) boolRef = new bool[3];
                restrictionInfo.heightRestriction = boolRef;
            }

            // ReSharper disable once StringLiteralTypo
            if (aciData.data.TryGetValue("Breastsize_Restriction", out byteData) && byteData != null)
            {
                var boolRef = MessagePackSerializer.Deserialize<bool[]>((byte[])byteData);
                if (boolRef == null || boolRef.Length != 3) boolRef = new bool[3];
                restrictionInfo.breastsizeRestriction = boolRef;
            }

            return coordinateInfo;
        }
    }
}