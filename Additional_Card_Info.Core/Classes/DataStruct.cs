using Extensions;
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

        public Cardinfo() { NullCheck(); }

        public Cardinfo(bool _advdir, bool _cosplayready, bool[] _personalsavebool, string _simpledirectory, Dictionary<string, string> _advdirectory)
        {
            CosplayReady = _cosplayready;
            SimpleFolderDirectory = _simpledirectory;
            AdvancedDirectory = _advdir;
            PersonalClothingBools = _personalsavebool.ToNewArray(9);

            AdvancedFolderDirectory = _advdirectory.ToNewDictionary();
            NullCheck();
        }

        internal void CleanUp()
        {
            var invalidpath = System.IO.Path.GetInvalidPathChars().Select(x => x.ToString());
            var folderkeys = AdvancedFolderDirectory.Keys.ToList();
            foreach (var key in folderkeys)
            {
                var path = AdvancedFolderDirectory[key] = AdvancedFolderDirectory[key].Trim();

                if (path.Length == 0 || path.ContainsAny(invalidpath))
                {
                    AdvancedFolderDirectory.Remove(key);
                }
            }
        }

        public void Clear()
        {
            CosplayReady = false;
            PersonalClothingBools = new bool[9];
            AdvancedFolderDirectory.Clear();
        }

        private void NullCheck()
        {
            if (SimpleFolderDirectory == null) SimpleFolderDirectory = "";
            if (AdvancedFolderDirectory == null) AdvancedFolderDirectory = new Dictionary<string, string>();
            if (PersonalClothingBools == null) PersonalClothingBools = new bool[9];
        }
    }

    [Serializable]
    [MessagePackObject]
    public class CoordinateInfo
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

        public CoordinateInfo() { NullCheck(); }

        public CoordinateInfo(CoordinateInfo _copy) => CopyData(_copy);

        public void CopyData(CoordinateInfo _copy) => CopyData(_copy.ClothNotData, _copy.CoordinateSaveBools, _copy.AccKeep, _copy.HairAcc, _copy.CreatorNames, _copy.SetNames, _copy.SubSetNames, _copy.RestrictionInfo);

        public void CopyData(bool[] _clothnot, bool[] _coordsavebool, List<int> _acckeep, List<int> _hairkeep, List<string> _creatornames, string _set, string _subset, RestrictionInfo _restrictioninfo)
        {
            SetNames = _set;
            SubSetNames = _subset;

            CoordinateSaveBools = _coordsavebool.ToNewArray(9);
            AccKeep = _acckeep.ToNewList();
            HairAcc = _hairkeep.ToNewList();
            CreatorNames = _creatornames.ToNewList();
            ClothNotData = _clothnot.ToNewArray(3);

            if (_restrictioninfo != null) RestrictionInfo = new RestrictionInfo(_restrictioninfo);
            else RestrictionInfo = null;
            NullCheck();
        }

        public void Clear()
        {
            ClothNotData = new bool[3];
            CoordinateSaveBools = new bool[9];
            AccKeep.Clear();
            HairAcc.Clear();
            CreatorNames.Clear();
            SetNames = "";
            SubSetNames = "";
            RestrictionInfo.Clear();
        }

        internal void CleanUp()
        {
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
        public Dictionary<int, int> PersonalityType_Restriction;

        [Key("_trait")]
        public Dictionary<int, int> TraitType_Restriction;

        [Key("_interest")]
        public Dictionary<int, int> Interest_Restriction;

        [Key("_height")]
        public bool[] Height_Restriction;

        [Key("_breast")]
        public bool[] Breastsize_Restriction;

        [Key("_htype")]
        public int HstateType_Restriction;

        [Key("_club")]
        public int ClubType_Restriction;

        [Key("_gender")]
        public int GenderType;

        [Key("_coordtype")]
        public int CoordinateType;

        [Key("_coordsubtype")]
        public int CoordinateSubType;
        #endregion

        public RestrictionInfo() { NullCheck(); }

        public RestrictionInfo(RestrictionInfo _copy) => CopyData(_copy);

        public void CopyData(RestrictionInfo _copy) => CopyData(_copy.PersonalityType_Restriction, _copy.TraitType_Restriction, _copy.Interest_Restriction, _copy.HstateType_Restriction, _copy.ClubType_Restriction, _copy.GenderType, _copy.CoordinateType, _copy.CoordinateSubType, _copy.Height_Restriction, _copy.Breastsize_Restriction);

        public void CopyData(Dictionary<int, int> _personality, Dictionary<int, int> _trait, Dictionary<int, int> _interest, int _htype, int _club, int _gender, int _coordtype, int _coordsubtype, bool[] _height, bool[] _breast)
        {
            HstateType_Restriction = _htype;
            ClubType_Restriction = _club;
            GenderType = _gender;
            CoordinateType = _coordtype;
            CoordinateSubType = _coordsubtype;

            Height_Restriction = _height.ToNewArray(3);
            Breastsize_Restriction = _breast.ToNewArray(3);

            PersonalityType_Restriction = _personality.ToNewDictionary();
            TraitType_Restriction = _trait.ToNewDictionary();
            Interest_Restriction = _interest.ToNewDictionary();
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
            if (PersonalityType_Restriction == null) PersonalityType_Restriction = new Dictionary<int, int>();
            if (Interest_Restriction == null) Interest_Restriction = new Dictionary<int, int>();
            if (TraitType_Restriction == null) TraitType_Restriction = new Dictionary<int, int>();
            if (Height_Restriction == null) Height_Restriction = new bool[3];
            if (Breastsize_Restriction == null) Breastsize_Restriction = new bool[3];
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
            var clean = PersonalityType_Restriction.Where(x => x.Value == 0).Select(x => x.Key).ToList();
            foreach (var item in clean)
            {
                PersonalityType_Restriction.Remove(item);
            }
            clean = TraitType_Restriction.Where(x => x.Value == 0).Select(x => x.Key).ToList();
            foreach (var item in clean)
            {
                TraitType_Restriction.Remove(item);
            }
            clean = Interest_Restriction.Where(x => x.Value == 0).Select(x => x.Key).ToList();
            foreach (var item in clean)
            {
                Interest_Restriction.Remove(item);
            }
        }
    }
}
