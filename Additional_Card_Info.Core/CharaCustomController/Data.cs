using KKAPI.Chara;
using System.Collections.Generic;
using System.Linq;

namespace Additional_Card_Info
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        public DataStruct data = new DataStruct();

        #region Properties

        public int MaxKey => CoordinateInfo.Count();

        public Dictionary<int, CoordinateInfo> CoordinateInfo
        {
            get { return data.CoordinateInfo; }
            set { data.CoordinateInfo = value; }
        }

        public Cardinfo CardInfo
        {
            get { return data.CardInfo; }
            set { data.CardInfo = value; }
        }

        public CoordinateInfo NowCoordinateInfo
        {
            get { return data.NowCoordinateInfo; }
            set { data.NowCoordinateInfo = value; }
        }

        public RestrictionInfo NowRestrictionInfo
        {
            get { return NowCoordinateInfo.RestrictionInfo; }
            set { NowCoordinateInfo.RestrictionInfo = value; }
        }

        #region Cardinfo
        public bool CosplayReady
        {
            get { return CardInfo.CosplayReady; }
            set { CardInfo.CosplayReady = value; }
        }

        public bool[] PersonalClothingBools
        {
            get { return CardInfo.PersonalClothingBools; }
            set { CardInfo.PersonalClothingBools = value; }
        }
        #endregion 

        #region Coordinate

        public List<int> MakeUpKeep
        {
            get { return CardInfo.MakeUpKeep; }
            set { CardInfo.MakeUpKeep = value; }
        }

        public List<int> AccKeep
        {
            get { return NowCoordinateInfo.AccKeep; }
            set { NowCoordinateInfo.AccKeep = value; }
        }

        public List<int> HairAcc
        {
            get { return NowCoordinateInfo.HairAcc; }
            set { NowCoordinateInfo.HairAcc = value; }
        }

        public bool[] CoordinateSaveBools
        {
            get { return NowCoordinateInfo.CoordinateSaveBools; }
            set { NowCoordinateInfo.CoordinateSaveBools = value; }
        }

        public bool[] ClothNotData
        {
            get { return NowCoordinateInfo.ClothNotData; }
            set { NowCoordinateInfo.ClothNotData = value; }
        }

        public List<string> CreatorNames
        {
            get { return NowCoordinateInfo.CreatorNames; }
            set { NowCoordinateInfo.CreatorNames = value; }
        }

        public string SetNames
        {
            get { return NowCoordinateInfo.SetNames; }
            set { NowCoordinateInfo.SetNames = value; }
        }

        public string SubSetNames
        {
            get { return NowCoordinateInfo.SubSetNames; }
            set { NowCoordinateInfo.SubSetNames = value; }
        }
        #endregion

        #region Restriction

        public int CoordinateType
        {
            get { return NowRestrictionInfo.CoordinateType; }
            set { NowRestrictionInfo.CoordinateType = value; }
        }

        public int CoordinateSubType
        {
            get { return NowRestrictionInfo.CoordinateSubType; }
            set { NowRestrictionInfo.CoordinateSubType = value; }
        }

        public Dictionary<int, int> PersonalityType_Restriction
        {
            get { return NowRestrictionInfo.PersonalityType_Restriction; }
            set { NowRestrictionInfo.PersonalityType_Restriction = value; }
        }

        public Dictionary<int, int> TraitType_Restriction
        {
            get { return NowRestrictionInfo.TraitType_Restriction; }
            set { NowRestrictionInfo.TraitType_Restriction = value; }
        }

        public Dictionary<int, int> Interest_Restriction
        {
            get { return NowRestrictionInfo.Interest_Restriction; }
            set { NowRestrictionInfo.Interest_Restriction = value; }
        }

        public bool[] Height_Restriction
        {
            get { return NowRestrictionInfo.Height_Restriction; }
            set { NowRestrictionInfo.Height_Restriction = value; }
        }

        public bool[] Breastsize_Restriction
        {
            get { return NowRestrictionInfo.Breastsize_Restriction; }
            set { NowRestrictionInfo.Breastsize_Restriction = value; }
        }

        public int HstateType_Restriction
        {
            get { return NowRestrictionInfo.HstateType_Restriction; }
            set { NowRestrictionInfo.HstateType_Restriction = value; }
        }

        public int ClubType_Restriction
        {
            get { return NowRestrictionInfo.ClubType_Restriction; }
            set { NowRestrictionInfo.ClubType_Restriction = value; }
        }

        public int GenderType
        {
            get { return NowRestrictionInfo.GenderType; }
            set { NowRestrictionInfo.GenderType = value; }
        }

        public Dictionary<string, string> FolderDirectory
        {
            get { return CardInfo.FolderDirectory; }
            set { CardInfo.FolderDirectory = value; }
        }
        #endregion

        #endregion
    }
}
