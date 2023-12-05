using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions;
using MessagePack;
using UnityEngine.Serialization;

namespace Additional_Card_Info
{
    public class DataStruct
    {
        public CardInfo CardInfo = new CardInfo();
        public Dictionary<int, CoordinateInfo> CoordinateInfo = new Dictionary<int, CoordinateInfo>();

        public CoordinateInfo NowCoordinateInfo = new CoordinateInfo();

        public void CleanUp()
        {
            CardInfo.CleanUp();
            foreach (var item in CoordinateInfo) item.Value.CleanUp();
        }

        public void ClearOutfit(int key)
        {
            if (!CoordinateInfo.ContainsKey(key))
                CreateOutfit(key);
            CoordinateInfo[key].Clear();
        }

        public void CreateOutfit(int key)
        {
            if (!CoordinateInfo.ContainsKey(key))
                CoordinateInfo[key] = new CoordinateInfo();
        }

        public void MoveOutfit(int dest, int src)
        {
            CoordinateInfo[dest] = new CoordinateInfo(CoordinateInfo[src]);
        }

        public void RemoveOutfit(int key)
        {
            CoordinateInfo.Remove(key);
        }
    }

    [Serializable]
    [MessagePackObject]
    public class CardInfo
    {
        // ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("CosplayReady")] [Key("_cosplayready")]
        public bool cosplayReady;

        // ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("AdvancedDirectory")] [Key("_advdir")]
        public bool advancedDirectory;

        [FormerlySerializedAs("PersonalClothingBools")] [Key("_personalsavebool")]
        public bool[] personalClothingBools;

        [FormerlySerializedAs("SimpleFolderDirectory")] [Key("_simpledirectory")]
        public string simpleFolderDirectory;

        [Key("_advdirectory")] public Dictionary<string, string> AdvancedFolderDirectory;

        public CardInfo()
        {
            NullCheck();
        }

        public CardInfo(bool advDir, bool cosplayReady, bool[] personalsavebool, string simpledirectory,
            Dictionary<string, string> advdirectory)
        {
            this.cosplayReady = cosplayReady;
            simpleFolderDirectory = simpledirectory;
            advancedDirectory = advDir;
            personalClothingBools = personalsavebool.ToNewArray(9);

            AdvancedFolderDirectory = advdirectory.ToNewDictionary();
            if (AdvancedFolderDirectory == null) return;
            NullCheck();
        }

        internal void CleanUp()
        {
            var invalidPath = Path.GetInvalidPathChars().Select(x => x.ToString());
            var folderKeys = AdvancedFolderDirectory.Keys.ToList();
            foreach (var key in folderKeys)
            {
                var path = AdvancedFolderDirectory[key] = AdvancedFolderDirectory[key].Trim();
                var enumerable = invalidPath.ToList();
                if (path.Length == 0 || path.ContainsAny(enumerable)) AdvancedFolderDirectory.Remove(key);
            }
        }

        public void Clear()
        {
            cosplayReady = false;
            personalClothingBools = new bool[9];
            AdvancedFolderDirectory.Clear();
        }

        private void NullCheck()
        {
            if (simpleFolderDirectory == null) simpleFolderDirectory = "";
            if (AdvancedFolderDirectory == null) AdvancedFolderDirectory = new Dictionary<string, string>();
            if (personalClothingBools == null) personalClothingBools = new bool[9];
        }
    }

    [Serializable]
    [MessagePackObject]
    public class CoordinateInfo
    {
        public CoordinateInfo()
        {
            NullCheck();
        }

        public CoordinateInfo(CoordinateInfo copy)
        {
            CopyData(copy);
        }

        public void CopyData(CoordinateInfo copy)
        {
            CopyData(copy.clothNotData, copy.coordinateSaveBools, copy.accKeep, copy.hairAcc, copy.creatorNames,
                copy.setNames, copy.subSetNames, copy.restrictionInfo);
        }

        public void CopyData(bool[] clothNot, bool[] coordSaveBool, List<int> copyAccKeep, List<int> copyHairKeep,
            List<string> copyCreatorNames, string set, string subset, RestrictionInfo copyRestrictionInfo)
        {
            setNames = set;
            subSetNames = subset;

            coordinateSaveBools = coordSaveBool.ToNewArray(9);
            accKeep = copyAccKeep.ToNewList();
            hairAcc = copyHairKeep.ToNewList();
            creatorNames = copyCreatorNames.ToNewList();
            clothNotData = clothNot.ToNewArray(3);

            restrictionInfo = copyRestrictionInfo != null ? new RestrictionInfo(copyRestrictionInfo) : null;
            NullCheck();
        }

        public void Clear()
        {
            clothNotData = new bool[3];
            coordinateSaveBools = new bool[9];
            accKeep.Clear();
            hairAcc.Clear();
            creatorNames.Clear();
            setNames = "";
            subSetNames = "";
            restrictionInfo.Clear();
        }

        internal void CleanUp()
        {
            var invalidPath = Path.GetInvalidPathChars();
            foreach (var item in invalidPath)
            {
                setNames = setNames.Replace(item, '_');
                subSetNames = subSetNames.Replace(item, '_');
            }

            restrictionInfo.CleanUp();
        }

        private void NullCheck()
        {
            if (clothNotData == null) clothNotData = new bool[3];
            if (coordinateSaveBools == null) coordinateSaveBools = new bool[9];
            if (accKeep == null) accKeep = new List<int>();
            if (hairAcc == null) hairAcc = new List<int>();
            if (creatorNames == null) creatorNames = new List<string>();
            if (setNames == null) setNames = "";
            if (subSetNames == null) subSetNames = "";
            if (restrictionInfo == null) restrictionInfo = new RestrictionInfo();
        }

        #region fields

        [FormerlySerializedAs("MakeUpKeep")] [Key("_makeup")]
        public bool makeUpKeep;

// ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("ClothNotData")] [Key("_clothnot")]
        public bool[] clothNotData;

// ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("CoordinateSaveBools")] [Key("_coordsavebool")]
        public bool[] coordinateSaveBools;

// ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("AccKeep")] [Key("_acckeep")]
        public List<int> accKeep;

// ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("HairAcc")] [Key("_hairkeep")]
        public List<int> hairAcc;

// ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("CreatorNames")] [Key("_creatornames")]
        public List<string> creatorNames;

        [FormerlySerializedAs("SetNames")] [Key("_set")]
        public string setNames;

        [FormerlySerializedAs("SubSetNames")] [Key("_subset")]
        public string subSetNames;

// ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("RestrictionInfo")] [Key("_restrictioninfo")]
        public RestrictionInfo restrictionInfo;

        #endregion
    }

    [Serializable]
    [MessagePackObject]
    public class RestrictionInfo
    {
        public RestrictionInfo()
        {
            NullCheck();
        }

        public RestrictionInfo(RestrictionInfo copy)
        {
            CopyData(copy);
        }

        public void CopyData(RestrictionInfo copy)
        {
            CopyData(copy.PersonalityTypeRestriction, copy.TraitTypeRestriction, copy.InterestRestriction,
                copy.hStateTypeRestriction, copy.clubTypeRestriction, copy.genderType, copy.coordinateType,
                copy.coordinateSubType, copy.heightRestriction, copy.breastsizeRestriction);
        }

        public void CopyData(Dictionary<int, int> personality, Dictionary<int, int> trait,
            Dictionary<int, int> interest, int copyHType, int club, int gender, int copyCoordType, int coordSubType,
            bool[] height, bool[] breast)
        {
            hStateTypeRestriction = copyHType;
            clubTypeRestriction = club;
            genderType = gender;
            coordinateType = copyCoordType;
            coordinateSubType = coordSubType;

            heightRestriction = height.ToNewArray(3);
            breastsizeRestriction = breast.ToNewArray(3);

            PersonalityTypeRestriction = personality.ToNewDictionary();
            TraitTypeRestriction = trait.ToNewDictionary();
            InterestRestriction = interest.ToNewDictionary();
            //if (_personality != null) PersonalityType_Restriction = new Dictionary<int, int>(_personality);
            //else PersonalityType_Restriction = null;
            //if (_trait != null) TraitType_Restriction = new Dictionary<int, int>(_personality);
            //else TraitType_Restriction = null;
            //if (_interest != null) Interest_Restriction = new Dictionary<int, int>(_personality);
            //else Interest_Restriction = null;

            //NullCheck();
        }

        private void NullCheck()
        {
            if (PersonalityTypeRestriction == null) PersonalityTypeRestriction = new Dictionary<int, int>();
            if (InterestRestriction == null) InterestRestriction = new Dictionary<int, int>();
            if (TraitTypeRestriction == null) TraitTypeRestriction = new Dictionary<int, int>();
            if (heightRestriction == null) heightRestriction = new bool[3];
            if (breastsizeRestriction == null) breastsizeRestriction = new bool[3];
        }

        public void Clear()
        {
            PersonalityTypeRestriction.Clear();

            TraitTypeRestriction.Clear();

            InterestRestriction.Clear();

            coordinateType = 0;

            coordinateSubType = 0;

            hStateTypeRestriction = 0;

            clubTypeRestriction = 0;

            genderType = 0;

            for (var i = 0; i < 3; i++) heightRestriction[i] = breastsizeRestriction[i] = false;
        }

        internal void CleanUp()
        {
            var clean = PersonalityTypeRestriction.Where(x => x.Value == 0).Select(x => x.Key).ToList();
            foreach (var item in clean) PersonalityTypeRestriction.Remove(item);
            clean = TraitTypeRestriction.Where(x => x.Value == 0).Select(x => x.Key).ToList();
            foreach (var item in clean) TraitTypeRestriction.Remove(item);
            clean = InterestRestriction.Where(x => x.Value == 0).Select(x => x.Key).ToList();
            foreach (var item in clean) InterestRestriction.Remove(item);
        }

        #region fields

        [Key("_personality")] public Dictionary<int, int> PersonalityTypeRestriction;

        [Key("_trait")] public Dictionary<int, int> TraitTypeRestriction;

        [Key("_interest")] public Dictionary<int, int> InterestRestriction;

        [FormerlySerializedAs("Height_Restriction")] [Key("_height")]
        public bool[] heightRestriction;

// ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("Breastsize_Restriction")] [Key("_breast")]
        public bool[] breastsizeRestriction;

// ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("hstateTypeRestriction")]
        [FormerlySerializedAs("HstateType_Restriction")]
        // ReSharper disable once StringLiteralTypo
        [Key("_htype")]
        public int hStateTypeRestriction;

        [FormerlySerializedAs("ClubType_Restriction")] [Key("_club")]
        public int clubTypeRestriction;

        [FormerlySerializedAs("GenderType")] [Key("_gender")]
        public int genderType;

// ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("CoordinateType")] [Key("_coordtype")]
        public int coordinateType;

// ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("CoordinateSubType")] [Key("_coordsubtype")]
        public int coordinateSubType;

        #endregion
    }
}