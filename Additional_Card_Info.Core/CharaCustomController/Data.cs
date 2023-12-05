using System.Collections.Generic;
using System.Linq;
using KKAPI.Chara;

namespace Additional_Card_Info
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        public DataStruct Data = new DataStruct();

        #region Properties

        public int MaxKey => CoordinateInfo.Count();

        public Dictionary<int, CoordinateInfo> CoordinateInfo
        {
            get => Data.CoordinateInfo;
            set => Data.CoordinateInfo = value;
        }

        public CardInfo CardInfo
        {
            get => Data.CardInfo;
            set => Data.CardInfo = value;
        }

        public CoordinateInfo NowCoordinateInfo
        {
            get => Data.NowCoordinateInfo;
            set => Data.NowCoordinateInfo = value;
        }

        public RestrictionInfo NowRestrictionInfo
        {
            get => NowCoordinateInfo.restrictionInfo;
            set => NowCoordinateInfo.restrictionInfo = value;
        }

        #region Cardinfo

        public bool CosplayReady
        {
            get => CardInfo.cosplayReady;
            set => CardInfo.cosplayReady = value;
        }

        public bool[] PersonalClothingBools
        {
            get => CardInfo.personalClothingBools;
            set => CardInfo.personalClothingBools = value;
        }

        public Dictionary<string, string> AdvancedFolderDirectory
        {
            get => CardInfo.AdvancedFolderDirectory;
            set => CardInfo.AdvancedFolderDirectory = value;
        }

        public bool AdvancedDirectory
        {
            get => CardInfo.advancedDirectory;
            set => CardInfo.advancedDirectory = value;
        }

        public string SimpleFolderDirectory
        {
            get => CardInfo.simpleFolderDirectory;
            set => CardInfo.simpleFolderDirectory = value;
        }

        #endregion

        #region Coordinate

        public bool MakeUpKeep
        {
            get => NowCoordinateInfo.makeUpKeep;
            set => NowCoordinateInfo.makeUpKeep = value;
        }

        public List<int> AccKeep
        {
            get => NowCoordinateInfo.accKeep;
            set => NowCoordinateInfo.accKeep = value;
        }

        public List<int> HairAcc
        {
            get => NowCoordinateInfo.hairAcc;
            set => NowCoordinateInfo.hairAcc = value;
        }

        public bool[] CoordinateSaveBools
        {
            get => NowCoordinateInfo.coordinateSaveBools;
            set => NowCoordinateInfo.coordinateSaveBools = value;
        }

        public bool[] ClothNotData
        {
            get => NowCoordinateInfo.clothNotData;
            set => NowCoordinateInfo.clothNotData = value;
        }

        public List<string> CreatorNames
        {
            get => NowCoordinateInfo.creatorNames;
            set => NowCoordinateInfo.creatorNames = value;
        }

        public string SetNames
        {
            get => NowCoordinateInfo.setNames;
            set => NowCoordinateInfo.setNames = value;
        }

        public string SubSetNames
        {
            get => NowCoordinateInfo.subSetNames;
            set => NowCoordinateInfo.subSetNames = value;
        }

        #endregion

        #region Restriction

        public int CoordinateType
        {
            get => NowRestrictionInfo.coordinateType;
            set => NowRestrictionInfo.coordinateType = value;
        }

        public int CoordinateSubType
        {
            get => NowRestrictionInfo.coordinateSubType;
            set => NowRestrictionInfo.coordinateSubType = value;
        }

        public Dictionary<int, int> PersonalityTypeRestriction
        {
            get => NowRestrictionInfo.PersonalityTypeRestriction;
            set => NowRestrictionInfo.PersonalityTypeRestriction = value;
        }

        public Dictionary<int, int> TraitTypeRestriction
        {
            get => NowRestrictionInfo.TraitTypeRestriction;
            set => NowRestrictionInfo.TraitTypeRestriction = value;
        }

        public Dictionary<int, int> InterestRestriction
        {
            get => NowRestrictionInfo.InterestRestriction;
            set => NowRestrictionInfo.InterestRestriction = value;
        }

        public bool[] HeightRestriction
        {
            get => NowRestrictionInfo.heightRestriction;
            set => NowRestrictionInfo.heightRestriction = value;
        }

        public bool[] BreastsizeRestriction
        {
            get => NowRestrictionInfo.breastsizeRestriction;
            set => NowRestrictionInfo.breastsizeRestriction = value;
        }

        public int HStateTypeRestriction
        {
            get => NowRestrictionInfo.hStateTypeRestriction;
            set => NowRestrictionInfo.hStateTypeRestriction = value;
        }

        public int ClubTypeRestriction
        {
            get => NowRestrictionInfo.clubTypeRestriction;
            set => NowRestrictionInfo.clubTypeRestriction = value;
        }

        public int GenderType
        {
            get => NowRestrictionInfo.genderType;
            set => NowRestrictionInfo.genderType = value;
        }

        #endregion

        #endregion
    }
}