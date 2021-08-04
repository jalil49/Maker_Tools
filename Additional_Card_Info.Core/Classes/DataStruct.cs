using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Additional_Card_Info
{
    public class DataStruct
    {
        public Dictionary<int, CoordinateInfo> CoordinateInfo = new Dictionary<int, CoordinateInfo>();

        public Cardinfo CardInfo = new Cardinfo();

        public CoordinateInfo NowCoordinateInfo = new CoordinateInfo();

        public void CleanUp()
        {
            CardInfo.CleanUp();
            foreach (var item in CoordinateInfo)
            {
                item.Value.CleanUp();
            }
        }

        public void Clearoutfit(int key)
        {
            if (!CoordinateInfo.ContainsKey(key))
                Createoutfit(key);
            CoordinateInfo[key].Clear();
        }

        public void Createoutfit(int key)
        {
            if (!CoordinateInfo.ContainsKey(key))
                CoordinateInfo[key] = new CoordinateInfo();
        }

        public void Moveoutfit(int dest, int src)
        {
            CoordinateInfo[dest] = new CoordinateInfo(CoordinateInfo[src]);
        }

        public void Removeoutfit(int key)
        {
            CoordinateInfo.Remove(key);
        }
    }

    [Serializable]
    [MessagePackObject]
    public class Cardinfo
    {
        [Key("_makeup")]
        public List<int> MakeUpKeep = new List<int>();

        [Key("_cosplayready")]
        public bool CosplayReady = false;

        [Key("_personalsavebool")]
        public bool[] PersonalClothingBools = new bool[9];

        [Key("_folderdirectory")]
        public Dictionary<string, string> FolderDirectory = new Dictionary<string, string>();

        internal void CleanUp()
        {
            NullCheck();
            var invalidpath = System.IO.Path.GetInvalidPathChars().Select(x => x.ToString());
            var folderkeys = FolderDirectory.Keys.ToList();
            foreach (var key in folderkeys)
            {
                var path = FolderDirectory[key] = FolderDirectory[key].Trim();

                if (path.Length == 0 || path.ContainsAny(invalidpath))
                {
                    FolderDirectory.Remove(key);
                }
            }
        }

        public void Clear()
        {
            NullCheck();
            MakeUpKeep.Clear();
            CosplayReady = false;
            PersonalClothingBools = new bool[9];
            FolderDirectory.Clear();
        }

        private void NullCheck()
        {
            if (FolderDirectory == null) FolderDirectory = new Dictionary<string, string>();
            if (MakeUpKeep == null) MakeUpKeep = new List<int>();
            if (PersonalClothingBools == null) PersonalClothingBools = new bool[9];
        }
    }

    [Serializable]
    [MessagePackObject]
    public class CoordinateInfo
    {
        #region fields
        [Key("_clothnot")]
        public bool[] ClothNotData = new bool[3];

        [Key("_coordsavebool")]
        public bool[] CoordinateSaveBools = new bool[9];

        [Key("_acckeep")]
        public List<int> AccKeep = new List<int>();

        [Key("_hairkeep")]
        public List<int> HairAcc = new List<int>();

        [Key("_creatornames")]
        public List<string> CreatorNames = new List<string>();

        [Key("_set")]
        public string SetNames = "";

        [Key("_subset")]
        public string SubSetNames = "";

        [Key("_restrictioninfo")]
        public RestrictionInfo RestrictionInfo = new RestrictionInfo();
        #endregion

        public CoordinateInfo() { }

        public CoordinateInfo(CoordinateInfo _copy) => CopyData(_copy);

        public void CopyData(CoordinateInfo _copy) => CopyData(_copy.ClothNotData, _copy.CoordinateSaveBools, _copy.AccKeep, _copy.HairAcc, _copy.CreatorNames, _copy.SetNames, _copy.SubSetNames, _copy.RestrictionInfo);

        public void CopyData(bool[] _clothnot, bool[] _coordsavebool, List<int> _acckeep, List<int> _hairkeep, List<string> _creatornames, string _set, string _subset, RestrictionInfo _restrictioninfo)
        {
            ClothNotReset(_clothnot);

            CoordinateSaveBoolsReset(_coordsavebool);

            AccKeep.Clear();
            AccKeep = new List<int>(_acckeep);
            HairAcc.Clear();
            HairAcc = new List<int>(_hairkeep);
            CreatorNames.Clear();
            CreatorNames = new List<string>(_creatornames);
            SetNames = "";
            SetNames = _set;
            SubSetNames = "";
            SubSetNames = _subset;
            RestrictionInfo.Clear();
            RestrictionInfo = _restrictioninfo;
            NullCheck();
        }

        public void Clear()
        {
            NullCheck();
            for (int i = 0; i < ClothNotData.Length; i++)
            {
                ClothNotData[i] = false;
            }
            for (int i = 0; i < CoordinateSaveBools.Length; i++)
            {
                CoordinateSaveBools[i] = false;
            }
            AccKeep.Clear();
            HairAcc.Clear();
            CreatorNames.Clear();
            SetNames = "";
            SubSetNames = "";
            RestrictionInfo.Clear();
        }

        private void ClothNotReset(bool[] _clothnot)
        {
            if (_clothnot != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    ClothNotData[i] = _clothnot[i];
                }
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                ClothNotData[i] = false;
            }
        }

        private void CoordinateSaveBoolsReset(bool[] _coordsavebool)
        {
            if (_coordsavebool != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    CoordinateSaveBools[i] = _coordsavebool[i];
                }
            }

            for (int i = 0; i < 9; i++)
            {
                CoordinateSaveBools[i] = false;
            }
        }

        internal void CleanUp()
        {
            NullCheck();
            var invalidpath = System.IO.Path.GetInvalidPathChars();

            foreach (var item in invalidpath)
            {
                SetNames.Replace(item, '_');
                SubSetNames.Replace(item, '_');
            }
            RestrictionInfo.CleanUp();
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
    }

    [Serializable]
    [MessagePackObject]
    public class RestrictionInfo
    {
        #region fields
        [Key("_personality")]
        public Dictionary<int, int> PersonalityType_Restriction = new Dictionary<int, int>();

        [Key("_trait")]
        public Dictionary<int, int> TraitType_Restriction = new Dictionary<int, int>();

        [Key("_interest")]
        public Dictionary<int, int> Interest_Restriction = new Dictionary<int, int>();

        [Key("_height")]
        public bool[] Height_Restriction = new bool[3];

        [Key("_breast")]
        public bool[] Breastsize_Restriction = new bool[3];

        [Key("_htype")]
        public int HstateType_Restriction = 0;

        [Key("_club")]
        public int ClubType_Restriction = 0;

        [Key("_gender")]
        public int GenderType = 0;

        [Key("_coordtype")]
        public int CoordinateType = 0;

        [Key("_coordsubtype")]
        public int CoordinateSubType = 0;
        #endregion

        public RestrictionInfo() { }

        public RestrictionInfo(RestrictionInfo _copy) => CopyData(_copy);

        public void CopyData(RestrictionInfo _copy) => CopyData(_copy.PersonalityType_Restriction, _copy.TraitType_Restriction, _copy.Interest_Restriction, _copy.HstateType_Restriction, _copy.ClubType_Restriction, _copy.GenderType, _copy.CoordinateType, _copy.CoordinateSubType, _copy.Height_Restriction, _copy.Breastsize_Restriction);

        public void CopyData(Dictionary<int, int> _personality, Dictionary<int, int> _trait, Dictionary<int, int> _interest, int _htype, int _club, int _gender, int _coordtype, int _coordsubtype, bool[] _height, bool[] _breast)
        {
            PersonalityType_Restriction = new Dictionary<int, int>(_personality);
            TraitType_Restriction = new Dictionary<int, int>(_trait);
            Interest_Restriction = new Dictionary<int, int>(_interest);
            HstateType_Restriction = _htype;
            ClubType_Restriction = _club;
            GenderType = _gender;
            CoordinateType = _coordtype;
            CoordinateSubType = _coordsubtype;
            Height_Restriction = _height;
            Breastsize_Restriction = _breast;
        }

        public void Clear()
        {
            PersonalityType_Restriction.Clear();

            TraitType_Restriction.Clear();

            Interest_Restriction.Clear();

            CoordinateType = 0;

            CoordinateSubType = 0;

            HstateType_Restriction = 0;

            ClubType_Restriction = 0;

            GenderType = 0;

            for (int i = 0; i < 3; i++)
            {
                Height_Restriction[i] = Breastsize_Restriction[i] = false;
            }
        }

        internal void CleanUp()
        {
            var personclean = PersonalityType_Restriction.Where(x => x.Value == 0).ToList();
            foreach (var item in personclean)
            {
                PersonalityType_Restriction.Remove(item.Key);
            }
            var traitclean = TraitType_Restriction.Where(x => x.Value == 0).ToList();
            foreach (var item in traitclean)
            {
                TraitType_Restriction.Remove(item.Key);
            }
            var interestclean = Interest_Restriction.Where(x => x.Value == 0).ToList();
            foreach (var item in interestclean)
            {
                Interest_Restriction.Remove(item.Key);
            }
        }
    }
}
