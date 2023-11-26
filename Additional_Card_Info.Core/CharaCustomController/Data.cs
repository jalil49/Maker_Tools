using System.Collections.Generic;
using Additional_Card_Info.Classes.Migration;
using KKAPI.Chara;
using KKAPI.Maker;
using static ExtensibleSaveFormat.Extensions;

namespace Additional_Card_Info
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        public CardData CardData = new CardData();

        public CoordinateInfo NowCoordinateInfo = new CoordinateInfo();

        public readonly Dictionary<int, SlotData> SlotData = new Dictionary<int, SlotData>();

        #region Properties

        public ChaFileAccessory.PartsInfo[] Parts => ChaControl.nowCoordinate.accessory.parts;

        public RestrictionData NowRestrictionData
        {
            get => NowCoordinateInfo.RestrictionData;
            set => NowCoordinateInfo.RestrictionData = value;
        }

        #region Cardinfo

        public bool CosplayReady
        {
            get => CardData.CosplayReady;
            set => CardData.CosplayReady = value;
        }

        public bool[] PersonalClothingBools
        {
            get => CardData.PersonalClothingBools;
            set => CardData.PersonalClothingBools = value;
        }

        public Dictionary<string, string> ReferenceAdvDirectory
        {
            get => CardData.AdvancedFolderDirectory;
            set => CardData.AdvancedFolderDirectory = value;
        }

        public bool AdvancedDirectory
        {
            get => CardData.AdvancedDirectory;
            set => CardData.AdvancedDirectory = value;
        }

        public string SimpleFolderDirectory
        {
            get => CardData.SimpleFolderDirectory;
            set => CardData.SimpleFolderDirectory = value;
        }

        #endregion

        #region Coordinate

        public bool MakeUpKeep
        {
            get => NowCoordinateInfo.MakeUpKeep;
            set => NowCoordinateInfo.MakeUpKeep = value;
        }

        public bool[] CoordinateSaveBools
        {
            get => NowCoordinateInfo.CoordinateSaveBools;
            set => NowCoordinateInfo.CoordinateSaveBools = value;
        }

        /// <summary>
        ///     Pull correct Not data, since ClothingUnlocker will modify the normal output
        /// </summary>
        public bool[] ClothNotData
        {
            get => NowCoordinateInfo.ClothNotData;
            set => NowCoordinateInfo.ClothNotData = value;
        }

        public List<string> CreatorNames
        {
            get => NowCoordinateInfo.CreatorNames;
            set => NowCoordinateInfo.CreatorNames = value;
        }

        public string SetNames
        {
            get => NowCoordinateInfo.SetNames;
            set => NowCoordinateInfo.SetNames = value;
        }

        public string SubSetNames
        {
            get => NowCoordinateInfo.SubSetNames;
            set => NowCoordinateInfo.SubSetNames = value;
        }

        #endregion

        #region Restriction

        public int CoordinateType
        {
            get => NowRestrictionData.CoordinateType;
            set => NowRestrictionData.CoordinateType = value;
        }

        public int CoordinateSubType
        {
            get => NowRestrictionData.CoordinateSubType;
            set => NowRestrictionData.CoordinateSubType = value;
        }

        public Dictionary<int, int> PersonalityTypeRestriction
        {
            get => NowRestrictionData.PersonalityType_Restriction;
            set => NowRestrictionData.PersonalityType_Restriction = value;
        }

        public Dictionary<int, int> TraitTypeRestriction
        {
            get => NowRestrictionData.TraitType_Restriction;
            set => NowRestrictionData.TraitType_Restriction = value;
        }

        public Dictionary<int, int> InterestRestriction
        {
            get => NowRestrictionData.Interest_Restriction;
            set => NowRestrictionData.Interest_Restriction = value;
        }

        public bool[] HeightRestriction
        {
            get => NowRestrictionData.Height_Restriction;
            set => NowRestrictionData.Height_Restriction = value;
        }

        public bool[] BreastsizeRestriction
        {
            get => NowRestrictionData.BreastSize_Restriction;
            set => NowRestrictionData.BreastSize_Restriction = value;
        }

        public int HstateTypeRestriction
        {
            get => NowRestrictionData.HStateType_Restriction;
            set => NowRestrictionData.HStateType_Restriction = value;
        }

        public int ClubTypeRestriction
        {
            get => NowRestrictionData.ClubType_Restriction;
            set => NowRestrictionData.ClubType_Restriction = value;
        }

        public int GenderType
        {
            get => NowRestrictionData.GenderType;
            set => NowRestrictionData.GenderType = value;
        }

        #endregion

        private SlotData SelectedSlotInfo
        {
            get
            {
                var slot = AccessoriesApi.SelectedMakerAccSlot;
                if (slot < 0 || slot >= Parts.Length)
                {
                    return null;
                }

                SlotData.TryGetValue(slot, out var result);
                return result;
            }
            set
            {
                var slot = AccessoriesApi.SelectedMakerAccSlot;
                if (slot < 0 || slot >= Parts.Length)
                {
                    return;
                }

                if (value == null)
                {
                    SlotData.Remove(slot);
                    return;
                }

                SlotData[slot] = value;
            }
        }

        #endregion

        #region Save or Load Data

        private void SaveSlot()
        {
            var slot = AccessoriesApi.SelectedMakerAccSlot;
            if (slot < 0 || slot >= Parts.Length)
            {
                return;
            }

            SaveSlot(slot);
        }

        internal void SaveSlot(int slot)
        {
            if (!SlotData.TryGetValue(slot, out var slotData))
            {
                Parts[slot].SetExtendedDataById(Settings.GUID, null);
                return;
            }

            Parts[slot].SetExtendedDataById(Settings.GUID, slotData.Serialize());
        }

        internal void LoadSlot(int slot)
        {
            if (slot >= Parts.Length)
            {
                SlotData.Remove(slot);
                return;
            }

            if (Parts[slot].TryGetExtendedDataById(Settings.GUID, out var pluginData))
            {
                var slotData = Migrator.SlotInfoMigrate(pluginData);
                if (slotData != null)
                {
                    SlotData[slot] = slotData;
                    return;
                }
            }

            SlotData.Remove(slot);
        }

        private void SaveCoordinate()
        {
            ChaControl.nowCoordinate.accessory.SetExtendedDataById(Settings.GUID,
                NowCoordinateInfo.Serialize(CreatorName));
        }

        private void LoadCoordinate()
        {
            if (ChaControl.nowCoordinate.accessory.TryGetExtendedDataById(Settings.GUID, out var pluginData))
            {
                var newInfo = Migrator.CoordinateInfoMigrate(pluginData);
                if (newInfo != null)
                {
                    NowCoordinateInfo = newInfo;
                    return;
                }
            }

            NowCoordinateInfo.Clear();
        }

        private void SaveCard()
        {
            ChaControl.fileParam.SetExtendedDataById(Settings.GUID, CardData.Serialize());
        }

        private void LoadCard()
        {
            if (ChaControl.fileParam.TryGetExtendedDataById(Settings.GUID, out var plugin))
            {
                var card = Migrator.CardInfoMigrate(plugin);
                if (card != null)
                {
                    CardData = card;
                    return;
                }
            }

            CardData.Clear();
        }

        #endregion
    }
}