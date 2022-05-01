using KKAPI.Chara;
using System.Collections.Generic;
using static ExtensibleSaveFormat.Extensions;

namespace Additional_Card_Info
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        public CardInfo CardInfo = new CardInfo();

        public CoordinateInfo NowCoordinateInfo = new CoordinateInfo();

        public Dictionary<int, SlotInfo> SlotInfo = new Dictionary<int, SlotInfo>();

        #region Properties
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

        public Dictionary<string, string> ReferenceADVDirectory
        {
            get { return CardInfo.AdvancedFolderDirectory; }
            set { CardInfo.AdvancedFolderDirectory = value; }
        }

        public bool AdvancedDirectory
        {
            get { return CardInfo.AdvancedDirectory; }
            set { CardInfo.AdvancedDirectory = value; }
        }
        public string SimpleFolderDirectory
        {
            get { return CardInfo.SimpleFolderDirectory; }
            set { CardInfo.SimpleFolderDirectory = value; }
        }

        #endregion 

        #region Coordinate

        public bool MakeUpKeep
        {
            get { return NowCoordinateInfo.MakeUpKeep; }
            set { NowCoordinateInfo.MakeUpKeep = value; }
        }

        public bool[] CoordinateSaveBools
        {
            get { return NowCoordinateInfo.CoordinateSaveBools; }
            set { NowCoordinateInfo.CoordinateSaveBools = value; }
        }

        /// <summary>
        /// Pull correct Not data, since ClothingUnlocker will modify the normal output
        /// </summary>
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
        #endregion

        private SlotInfo SelectedSlotInfo
        {
            get
            {
                var slot = KKAPI.Maker.AccessoriesApi.SelectedMakerAccSlot;
                if (slot < 0 || slot >= Parts.Length)
                {
                    return null;
                }
                SlotInfo.TryGetValue(slot, out var result);
                return result;
            }
            set
            {
                var slot = KKAPI.Maker.AccessoriesApi.SelectedMakerAccSlot;
                if (slot < 0 || slot >= Parts.Length)
                {
                    return;
                }
                if (value == null)
                {
                    SlotInfo.Remove(slot);
                    return;
                }
                SlotInfo[slot] = value;
            }
        }
        #endregion

        #region Save or Load Data
        private void SaveSlot()
        {
            var slot = KKAPI.Maker.AccessoriesApi.SelectedMakerAccSlot;
            if (slot < 0 || slot >= Parts.Length)
            {
                return;
            }
            SaveSlot(slot);
        }
        private void SaveSlot(int slot)
        {
            if (!SlotInfo.TryGetValue(slot, out var slotInfo))
            {
                Parts[slot].SetExtendedDataById(Settings.GUID, null);
                return;
            }
            Parts[slot].SetExtendedDataById(Settings.GUID, slotInfo.Serialize());
        }
        private void LoadSlot(int slot)
        {
            if (slot >= Parts.Length)
            {
                SlotInfo.Remove(slot);
                return;
            }

            if (Parts[slot].TryGetExtendedDataById(Settings.GUID, out var pluginData))
            {
                var slotinfo = Migrator.SlotInfoMigrate(pluginData);
                if (slotinfo != null)
                {
                    SlotInfo[slot] = slotinfo;
                    return;
                }
            }
            SlotInfo.Remove(slot);
        }

        private void SaveCoordinate() => ChaControl.nowCoordinate.accessory.SetExtendedDataById(Settings.GUID, NowCoordinateInfo.Serialize(Creatorname));
        private void LoadCoordinate()
        {
            if (ChaControl.nowCoordinate.accessory.TryGetExtendedDataById(Settings.GUID, out var pluginData))
            {
                var newinfo = Migrator.CoordinateInfoMigrate(pluginData);
                if (newinfo != null)
                {
                    NowCoordinateInfo = newinfo;
                    return;
                }
            }
            NowCoordinateInfo.Clear();
        }

        private void SaveCard() => ChaControl.fileParam.SetExtendedDataById(Settings.GUID, CardInfo.Serialize());
        private void LoadCard()
        {
            if (ChaControl.fileParam.TryGetExtendedDataById(Settings.GUID, out var plugin))
            {
                var card = Migrator.CardInfoMigrate(plugin);
                if (card != null)
                {
                    CardInfo = card;
                    return;
                }
            }
            CardInfo.Clear();
        }
        #endregion
    }
}
