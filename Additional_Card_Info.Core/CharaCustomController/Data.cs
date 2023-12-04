using System.Collections.Generic;
using Additional_Card_Info.Classes.Migration;
using KKAPI.Chara;
using KKAPI.Maker;
using UnityEngine.Serialization;
using static ExtensibleSaveFormat.Extensions;

namespace Additional_Card_Info
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        [FormerlySerializedAs("CardData")] public CardData cardData = new CardData();

        [FormerlySerializedAs("NowCoordinateInfo")] public CoordinateInfo nowCoordinateInfo = new CoordinateInfo();

        public readonly Dictionary<int, SlotData> SlotData = new Dictionary<int, SlotData>();

        #region Properties

        public ChaFileAccessory.PartsInfo[] Parts => ChaControl.nowCoordinate.accessory.parts;

        public RestrictionData NowRestrictionData
        {
            get => nowCoordinateInfo.restrictionData;
            set => nowCoordinateInfo.restrictionData = value;
        }

        #region Cardinfo

        public bool CosplayReady
        {
            get => cardData.cosplayReady;
            set => cardData.cosplayReady = value;
        }

        public bool[] PersonalClothingBools
        {
            get => cardData.personalClothingBools;
            set => cardData.personalClothingBools = value;
        }

        public Dictionary<string, string> ReferenceAdvDirectory
        {
            get => cardData.AdvancedFolderDirectory;
            set => cardData.AdvancedFolderDirectory = value;
        }

        public bool AdvancedDirectory
        {
            get => cardData.advancedDirectory;
            set => cardData.advancedDirectory = value;
        }

        public string SimpleFolderDirectory
        {
            get => cardData.simpleFolderDirectory;
            set => cardData.simpleFolderDirectory = value;
        }

        #endregion

        #region Coordinate

        public bool MakeUpKeep
        {
            get => nowCoordinateInfo.makeUpKeep;
            set => nowCoordinateInfo.makeUpKeep = value;
        }

        public bool[] CoordinateSaveBools
        {
            get => nowCoordinateInfo.coordinateSaveBools;
            set => nowCoordinateInfo.coordinateSaveBools = value;
        }

        /// <summary>
        ///     Pull correct Not data, since ClothingUnlocker will modify the normal output
        /// </summary>
        public bool[] ClothNotData
        {
            get => nowCoordinateInfo.clothNotData;
            set => nowCoordinateInfo.clothNotData = value;
        }

        public List<string> CreatorNames
        {
            get => nowCoordinateInfo.creatorNames;
            set => nowCoordinateInfo.creatorNames = value;
        }

        public string SetNames
        {
            get => nowCoordinateInfo.setNames;
            set => nowCoordinateInfo.setNames = value;
        }

        public string SubSetNames
        {
            get => nowCoordinateInfo.subSetNames;
            set => nowCoordinateInfo.subSetNames = value;
        }

        #endregion

        #region Restriction

        public int CoordinateType
        {
            get => NowRestrictionData.coordinateType;
            set => NowRestrictionData.coordinateType = value;
        }

        public int CoordinateSubType
        {
            get => NowRestrictionData.coordinateSubType;
            set => NowRestrictionData.coordinateSubType = value;
        }

        public Dictionary<int, int> PersonalityTypeRestriction
        {
            get => NowRestrictionData.PersonalityTypeRestriction;
            set => NowRestrictionData.PersonalityTypeRestriction = value;
        }

        public Dictionary<int, int> TraitTypeRestriction
        {
            get => NowRestrictionData.TraitTypeRestriction;
            set => NowRestrictionData.TraitTypeRestriction = value;
        }

        public Dictionary<int, int> InterestRestriction
        {
            get => NowRestrictionData.InterestRestriction;
            set => NowRestrictionData.InterestRestriction = value;
        }

        public bool[] HeightRestriction
        {
            get => NowRestrictionData.heightRestriction;
            set => NowRestrictionData.heightRestriction = value;
        }

        public bool[] BreastsizeRestriction
        {
            get => NowRestrictionData.breastSizeRestriction;
            set => NowRestrictionData.breastSizeRestriction = value;
        }

        public int HstateTypeRestriction
        {
            get => NowRestrictionData.hStateTypeRestriction;
            set => NowRestrictionData.hStateTypeRestriction = value;
        }

        public int ClubTypeRestriction
        {
            get => NowRestrictionData.clubTypeRestriction;
            set => NowRestrictionData.clubTypeRestriction = value;
        }

        public int GenderType
        {
            get => NowRestrictionData.genderType;
            set => NowRestrictionData.genderType = value;
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
                Parts[slot].SetExtendedDataById(Settings.Guid, null);
                return;
            }

            Parts[slot].SetExtendedDataById(Settings.Guid, slotData.Serialize());
        }

        internal void LoadSlot(int slot)
        {
            if (slot >= Parts.Length)
            {
                SlotData.Remove(slot);
                return;
            }

            if (Parts[slot].TryGetExtendedDataById(Settings.Guid, out var pluginData))
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
            ChaControl.nowCoordinate.accessory.SetExtendedDataById(Settings.Guid,
                nowCoordinateInfo.Serialize(Settings.CreatorName.Value));
        }

        private void LoadCoordinate()
        {
            if (ChaControl.nowCoordinate.accessory.TryGetExtendedDataById(Settings.Guid, out var pluginData))
            {
                var newInfo = Migrator.CoordinateInfoMigrate(pluginData);
                if (newInfo != null)
                {
                    nowCoordinateInfo = newInfo;
                    return;
                }
            }

            nowCoordinateInfo.Clear();
        }

        private void SaveCard()
        {
            ChaControl.fileParam.SetExtendedDataById(Settings.Guid, cardData.Serialize());
        }

        private void LoadCard()
        {
            if (ChaControl.fileParam.TryGetExtendedDataById(Settings.Guid, out var plugin))
            {
                var card = Migrator.CardInfoMigrate(plugin);
                if (card != null)
                {
                    cardData = card;
                    return;
                }
            }

            cardData.Clear();
        }

        #endregion
    }
}