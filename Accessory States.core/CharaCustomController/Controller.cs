using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Accessory_States
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        // TODO: Implement ASS support
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            var insidermaker = currentGameMode == GameMode.Maker;

            var chafile = (currentGameMode == GameMode.Maker) ? MakerAPI.LastLoadedChaFile : ChaFileControl;

            var Extended_Data = GetExtendedData();
            if (Extended_Data != null)
            {
                if (Extended_Data.version < 2)
                {
                    Migration.Migrator.StandardCharaMigrator(ChaControl, Extended_Data);
                }
            }

            CurrentCoordinate.Subscribe(x =>
            {
                StartCoroutine(ChangeOutfitCoroutine());
                Settings.UpdateGUI(this);
            });
        }

        // TODO: Confirm ASS support
        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            var States_Data = new PluginData() { version = Constants.SaveVersion };
            SetExtendedData(States_Data);

            if (!Settings.ASS_SAVE.Value || ASSExists)
            {
                return;
            }

            var triggers = new List<AccStateSync.TriggerProperty>();
            var groups = new List<AccStateSync.TriggerGroup>();
            AssCardSave(ref triggers, ref groups);
            if (triggers.Count == 0)
                return;
            var pluginData = new PluginData() { version = Constants.SaveVersion };
            pluginData.data.Add("Modified", MessagePackSerializer.Serialize(new HashSet<string>() { "Accesory States" }));
            pluginData.data.Add(AccStateSync.TriggerProperty.SerializeKey, MessagePackSerializer.Serialize(triggers));
            pluginData.data.Add(AccStateSync.TriggerGroup.SerializeKey, MessagePackSerializer.Serialize(groups));
            ExtendedSave.SetExtendedDataById(ChaFileControl, "madevil.kk.ass", pluginData);
        }

        // TODO: Confirm ASS support
        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            if (!Settings.ASS_SAVE.Value || ASSExists)
            {
                return;
            }
            var triggers = new List<AccStateSync.TriggerProperty>();
            var groups = new List<AccStateSync.TriggerGroup>();
            ConvertCoordinateToAss(-1, coordinate, ref triggers, ref groups);
            if (triggers.Count == 0)
                return;
            var pluginData = new PluginData() { version = Constants.SaveVersion };
            pluginData.data.Add("Modified", MessagePackSerializer.Serialize(new HashSet<string>() { "Accesory States" }));
            pluginData.data.Add(AccStateSync.TriggerProperty.SerializeKey, MessagePackSerializer.Serialize(triggers));
            pluginData.data.Add(AccStateSync.TriggerGroup.SerializeKey, MessagePackSerializer.Serialize(groups));
            ExtendedSave.SetExtendedDataById(coordinate, "madevil.kk.ass", pluginData);
        }

        // TODO: Implement ASS support
        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            var Extended_Data = GetCoordinateExtendedData(coordinate);
            if (Extended_Data != null)
            {
                if (Extended_Data.version < 2)
                {
                    Migration.Migrator.StandardCoordMigrator(coordinate, Extended_Data);
                }
            }

            UpdatePluginData();
        }

        private IEnumerator<int> ChangeOutfitCoroutine()
        {
            yield return 0;
            UpdatePluginData();
        }

        internal void AccessoryCategoryChange(int category, bool show)
        {
            switch (category)
            {
                case 0:
                    ShowMain = show;
                    break;
                case 1:
                    ShowSub = show;
                    break;
                default:
                    Settings.Logger.LogWarning($"Unknown Accessory Category [{category}] set to {show}");
                    break;
            }
        }

        // TODO: do a selective hide
        internal void ChangeBindingSub(int hidesetting)
        {
            var coordinateaccessory = ChaFileControl.coordinate[(int)CurrentCoordinate.Value].accessory.parts;
            var nowcoodaccessory = ChaControl.nowCoordinate.accessory.parts;
            var slotslist = SlotBindingData.Where(x => x.Value.bindingDatas.Any(y => y.GetBinding() == -2)).Select(x => x.Key);
            foreach (var slot in slotslist)
            {
                coordinateaccessory[slot].hideCategory = nowcoodaccessory[slot].hideCategory = hidesetting;
            }
        }

        /// <summary>
        /// Iterrate over whole list to deal with potentially cascading effects of one state affecting another which affects another
        /// </summary>
        public void RefreshSlots()
        {
            foreach (var slotData in SlotBindingData)
            {
                if (slotData.Key >= PartsArray.Length)
                    return;

                var show = ShouldShow(slotData.Value);
                var partsInfo = PartsArray[slotData.Key];
                if (slotData.Value.Parented && ParentedNameDictionary.TryGetValue(partsInfo.parentKey, out var parentingState))
                {
                    show &= parentingState;
                }
                var isSub = partsInfo.hideCategory == 1;
                show &= !isSub && ShowMain || isSub && ShowSub; //respect the hide/show main and sub buttons
                ChaControl.SetAccessoryState(slotData.Key, show);
            }
        }

        /// <summary>
        /// Check bindings of slotdata, respect 
        /// 
        /// </summary>
        /// <param name="slotdata"></param>
        /// <returns></returns>
        public bool ShouldShow(SlotData slotdata)
        {
            var shoeType = ChaControl.fileStatus.shoesType;
            var result = true;
            var currentPriority = 0;

            foreach (var item in slotdata.bindingDatas)
            {
                if (item.NameData == null)
                    continue;

                var stateInfo = item.GetStateInfo(item.NameData.CurrentState, shoeType);
                if (stateInfo == null || stateInfo.Binding < 0 || stateInfo.Priority < currentPriority)
                    continue;


                if (stateInfo.Priority == currentPriority)
                {
                    result &= stateInfo.Show;
                    continue;
                }

                currentPriority = stateInfo.Priority;
                result = stateInfo.Show;
            }
            return result;
        }

        internal void SetClothesState(int clothesKind, byte state)
        {
            var nameData = Names.FirstOrDefault(x => x.Binding == clothesKind);
            if (nameData == null)
                return;
            nameData.CurrentState = state;
            RefreshSlots();
        }
    }
}
