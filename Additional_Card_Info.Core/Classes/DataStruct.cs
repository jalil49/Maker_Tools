using ExtensibleSaveFormat;
using Extensions;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Additional_Card_Info
{
    [Serializable]
    [MessagePackObject(true)]
    public class CardInfo
    {
        public bool CosplayReady;

        public bool AdvancedDirectory;

        public bool[] PersonalClothingBools;

        public string SimpleFolderDirectory;

        public Dictionary<string, string> AdvancedFolderDirectory;//for folder reference to external card directory not saved on coordinates

        public CardInfo() { NullCheck(); }

        public CardInfo(bool _advdir, bool _cosplayready, bool[] _personalsavebool, string _simpledirectory)
        {
            CosplayReady = _cosplayready;
            SimpleFolderDirectory = _simpledirectory;
            AdvancedDirectory = _advdir;
            PersonalClothingBools = _personalsavebool.ToNewArray(9);
            NullCheck();
        }

        internal CardInfo(Migrator.CardInfoV1 oldinfo)
        {
            CosplayReady = oldinfo.CosplayReady;
            AdvancedDirectory = oldinfo.AdvancedDirectory;
            PersonalClothingBools = oldinfo.PersonalClothingBools;
            SimpleFolderDirectory = oldinfo.SimpleFolderDirectory;
            AdvancedFolderDirectory = oldinfo.AdvancedFolderDirectory;
            foreach (var item in Constants.Coordinates)
            {
                AdvancedFolderDirectory.Remove(item);
            }
        }

        public void Clear()
        {
            CosplayReady = false;
            PersonalClothingBools = new bool[9];
        }

        private void NullCheck()
        {
            if (SimpleFolderDirectory == null) SimpleFolderDirectory = "";
            if (PersonalClothingBools == null) PersonalClothingBools = new bool[9];
            if (AdvancedFolderDirectory == null) AdvancedFolderDirectory = new Dictionary<string, string>();
        }

        public PluginData Serialize() => new PluginData { version = Constants.MasterSaveVersion, data = new Dictionary<string, object>() { [Constants.CardKey] = MessagePackSerializer.Serialize(this) } };

        public static CardInfo Deserialize(object bytearray) => MessagePackSerializer.Deserialize<CardInfo>((byte[])bytearray);
    }

    [Serializable]
    [MessagePackObject(true)]
    public class CoordinateInfo
    {
        #region fields
        public bool MakeUpKeep;

        public bool[] ClothNotData;

        public bool[] CoordinateSaveBools;

        public List<string> CreatorNames;

        public string SetNames;

        public string SubSetNames;

        public string AdvancedFolder;

        public RestrictionInfo RestrictionInfo;
        #endregion

        public CoordinateInfo() { NullCheck(); }

        public CoordinateInfo(CoordinateInfo _copy) => CopyData(_copy);

        public void CopyData(CoordinateInfo _copy) => CopyData(_copy.ClothNotData, _copy.CoordinateSaveBools, _copy.CreatorNames, _copy.SetNames, _copy.SubSetNames, _copy.RestrictionInfo, _copy.AdvancedFolder);

        public void CopyData(bool[] _clothnot, bool[] _coordsavebool, List<string> _creatornames, string _set, string _subset, RestrictionInfo _restrictioninfo, string _advancedfolder)
        {
            SetNames = _set;
            SubSetNames = _subset;

            CoordinateSaveBools = _coordsavebool.ToNewArray(9);
            CreatorNames = _creatornames.ToNewList();
            ClothNotData = _clothnot.ToNewArray(3);
            AdvancedFolder = _advancedfolder;
            if (_restrictioninfo != null) RestrictionInfo = new RestrictionInfo(_restrictioninfo);
            else RestrictionInfo = null;
            NullCheck();
        }

        public void Clear()
        {
            ClothNotData = new bool[3];
            CoordinateSaveBools = new bool[9];
            AdvancedFolder = "";
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
            if (AdvancedFolder == null) AdvancedFolder = "";
            if (CreatorNames == null) CreatorNames = new List<string>();
            if (SetNames == null) SetNames = "";
            if (SubSetNames == null) SubSetNames = "";
            if (RestrictionInfo == null) RestrictionInfo = new RestrictionInfo();
        }

        public PluginData Serialize(string creatorname = null)
        {
            if (!creatorname.IsNullOrEmpty() && (CreatorNames.Count == 0 || CreatorNames.Last() != creatorname))
            {
                CreatorNames.Add(creatorname);
            }

            return new PluginData { version = Constants.MasterSaveVersion, data = new Dictionary<string, object>() { [Constants.CoordinateKey] = MessagePackSerializer.Serialize(this) } };
        }

        public static CoordinateInfo Deserialize(object bytearray) => MessagePackSerializer.Deserialize<CoordinateInfo>((byte[])bytearray);
    }

    [Serializable]
    [MessagePackObject]
    public class RestrictionInfo
    {
        #region Fields
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

            NullCheck();
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

            for (var i = 0; i < 3; i++)
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

    [Serializable]
    [MessagePackObject(true)]
    public class SlotInfo
    {
        /// <summary>
        /// Accessories to suggest keeping when modifying coordinates
        /// Example: when an appendage is made of accessories like Mecha Chika (granted this card broke at somepoint due to mod updates)
        /// </summary>
        public KeepState KeepState = KeepState.DontKeep;

        public void Clear()
        {
            KeepState = KeepState.DontKeep;
        }

        public PluginData Serialize()
        {
            if (KeepState != KeepState.DontKeep)
            {
                return new PluginData { version = Constants.MasterSaveVersion, data = new Dictionary<string, object>() { [Constants.AccessoryKey] = MessagePackSerializer.Serialize(this) } };
            }
            return null;
        }

        public static SlotInfo Deserialize(object bytearray) => MessagePackSerializer.Deserialize<SlotInfo>((byte[])bytearray);
    }
    public enum KeepState
    {
        DontKeep = -1,
        NonHairKeep = 0,
        HairKeep = 1
    }
}
