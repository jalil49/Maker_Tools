using System;
using System.Collections.Generic;
using System.Linq;
using Accessory_States.Migration;
using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
using UniRx;

namespace Accessory_States
{
    public partial class CharaEvent
    {
        // TODO: Confirm ASS support
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            var chafile = currentGameMode == GameMode.Maker ? MakerAPI.LastLoadedChaFile : ChaFileControl;

            var extendedData = GetExtendedData();
            if (extendedData != null)
            {
                if (extendedData.version < 2) Migrator.StandardCharaMigrator(ChaControl, extendedData);
            }
            else if ((extendedData = ExtendedSave.GetExtendedDataById(chafile, "madevil.kk.ass")) != null)
            {
                if (extendedData.version > 7)
                {
                    Settings.Logger.LogWarning(
                        "New version of AccessoryStateSync found, accessory states needs update for compatibility");
                }
                else
                {
                    var triggerPropertyList = new List<AccStateSync.TriggerProperty>();
                    var triggerGroupList = new List<AccStateSync.TriggerGroup>();

                    if (extendedData.version < 6)
                    {
                        AccStateSync.Migration.ConvertCharaPluginData(extendedData, ref triggerPropertyList,
                            ref triggerGroupList);
                    }
                    else
                    {
                        if (extendedData.data.TryGetValue("TriggerPropertyList", out var loadedTriggerProperty) &&
                            loadedTriggerProperty != null)
                        {
                            var tempTriggerProperty =
                                MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>(
                                    (byte[])loadedTriggerProperty);
                            if (tempTriggerProperty?.Count > 0)
                                triggerPropertyList.AddRange(tempTriggerProperty);

                            if (extendedData.data.TryGetValue("TriggerGroupList", out var loadedTriggerGroup) &&
                                loadedTriggerGroup != null)
                            {
                                var tempTriggerGroup =
                                    MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>(
                                        (byte[])loadedTriggerGroup);
                                if (tempTriggerGroup?.Count > 0)
                                {
                                    foreach (var group in tempTriggerGroup)
                                        if (group.Guid.IsNullOrEmpty())
                                            group.Guid = Guid.NewGuid().ToString("D").ToUpper();

                                    triggerGroupList.AddRange(tempTriggerGroup);
                                }
                            }
                        }
                    }

                    triggerPropertyList = triggerPropertyList ?? new List<AccStateSync.TriggerProperty>();
                    triggerGroupList = triggerGroupList ?? new List<AccStateSync.TriggerGroup>();

                    AccStateSync.FullAssCardLoad(chafile.coordinate, ChaControl.nowCoordinate,
                        (int)CurrentCoordinate.Value, triggerPropertyList, triggerGroupList);
                }
            }

            CurrentCoordinate.Subscribe(x =>
            {
                StartCoroutine(ChangeOutfitCoroutine());
                // Settings.UpdateGUI(this);
            });
        }

        // TODO: Confirm ASS support
        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            var statesData = new PluginData { version = Constants.SaveVersion };
            SetExtendedData(statesData);

            if (!Settings.AssSave.Value || AssExists) return;

            var triggers = new List<AccStateSync.TriggerProperty>();
            var groups = new List<AccStateSync.TriggerGroup>();
            AccStateSync.FullAssCardSave(ChaFileControl.coordinate, AssShoePreference, ref triggers, ref groups);
            if (triggers.Count == 0)
                return;
            var pluginData = new PluginData { version = 7 };
            pluginData.data.Add("Modified", MessagePackSerializer.Serialize(new HashSet<string> { "Accesory States" }));
            pluginData.data.Add(AccStateSync.TriggerProperty.SerializeKey, MessagePackSerializer.Serialize(triggers));
            pluginData.data.Add(AccStateSync.TriggerGroup.SerializeKey, MessagePackSerializer.Serialize(groups));
            ExtendedSave.SetExtendedDataById(ChaFileControl, "madevil.kk.ass", pluginData);
        }

        // TODO: Confirm ASS support
        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            if (!Settings.AssSave.Value || AssExists) return;

            var triggers = new List<AccStateSync.TriggerProperty>();
            var groups = new List<AccStateSync.TriggerGroup>();
            AccStateSync.ConvertCoordinateToAss(-1, AssShoePreference, coordinate, ref triggers, ref groups);
            if (triggers.Count == 0)
                return;
            var pluginData = new PluginData { version = 7 };
            pluginData.data.Add("Modified", MessagePackSerializer.Serialize(new HashSet<string> { "Accesory States" }));
            pluginData.data.Add(AccStateSync.TriggerProperty.SerializeKey, MessagePackSerializer.Serialize(triggers));
            pluginData.data.Add(AccStateSync.TriggerGroup.SerializeKey, MessagePackSerializer.Serialize(groups));
            ExtendedSave.SetExtendedDataById(coordinate, "madevil.kk.ass", pluginData);
        }

        // TODO: Confirm ASS support
        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            var extendedData = GetCoordinateExtendedData(coordinate);
            if (extendedData != null)
            {
                if (extendedData.version < 2) Migrator.StandardCoordMigrator(coordinate, extendedData);
            }
            else if ((extendedData = ExtendedSave.GetExtendedDataById(coordinate, "madevil.kk.ass")) != null)
            {
                if (extendedData.version > 7)
                {
                    Settings.Logger.LogWarning(
                        "New version of AccessoryStateSync found, accessory states needs update for compatibility");
                }
                else
                {
                    var triggerPropertyList = new List<AccStateSync.TriggerProperty>();
                    var triggerGroupList = new List<AccStateSync.TriggerGroup>();

                    if (extendedData.version < 6)
                    {
                        AccStateSync.Migration.ConvertCharaPluginData(extendedData, ref triggerPropertyList,
                            ref triggerGroupList);
                    }
                    else
                    {
                        if (extendedData.data.TryGetValue("TriggerPropertyList", out var loadedTriggerProperty) &&
                            loadedTriggerProperty != null)
                        {
                            var tempTriggerProperty =
                                MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>(
                                    (byte[])loadedTriggerProperty);
                            if (tempTriggerProperty?.Count > 0)
                                triggerPropertyList.AddRange(tempTriggerProperty);

                            if (extendedData.data.TryGetValue("TriggerGroupList", out var loadedTriggerGroup) &&
                                loadedTriggerGroup != null)
                            {
                                var tempTriggerGroup =
                                    MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>(
                                        (byte[])loadedTriggerGroup);
                                if (tempTriggerGroup?.Count > 0)
                                {
                                    foreach (var group in tempTriggerGroup)
                                        if (group.Guid.IsNullOrEmpty())
                                            group.Guid = Guid.NewGuid().ToString("D").ToUpper();

                                    triggerGroupList.AddRange(tempTriggerGroup);
                                }
                            }
                        }
                    }

                    triggerPropertyList = triggerPropertyList ?? new List<AccStateSync.TriggerProperty>();
                    triggerGroupList = triggerGroupList ?? new List<AccStateSync.TriggerGroup>();

                    AccStateSync.ConvertAssCoordinate(coordinate, triggerPropertyList, triggerGroupList);
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
                    _showMain = show;
                    break;
                case 1:
                    _showSub = show;
                    break;
                default:
                    Settings.Logger.LogWarning($"Unknown Accessory Category [{category}] set to {show}");
                    break;
            }

            RefreshSlots();
        }

        internal void ChangeBindingSub(int hideSetting, NameData nameData)
        {
            var coordinateAccessory = ChaFileControl.coordinate[(int)CurrentCoordinate.Value].accessory.parts;
            var nowCoordinateAccessory = ChaControl.nowCoordinate.accessory.parts;
            foreach (var slot in from slot in SlotBindingData from bindingData in slot.Value.bindingDatas where bindingData.NameData == nameData select slot)
                coordinateAccessory[slot.Key].hideCategory = nowCoordinateAccessory[slot.Key].hideCategory = hideSetting;
        }

        /// <summary>
        ///     Iterate over whole list to deal with potentially cascading effects of one state affecting another which affects
        ///     another
        /// </summary>
        public void RefreshSlots(HashSet<int> slots)
        {
            var slotLength = PartsArray.Length;
            foreach (var slot in slots)
            {
                if (slot >= slotLength)
                    return;

                if (!SlotBindingData.TryGetValue(slot, out var slotData)) continue;

                RefreshSlots(slot, slotData);
            }
        }

        private void RefreshSlots(int slot, SlotData slotData)
        {
            var partsInfo = PartsArray[slot];
            if (partsInfo.type == 120)
                return;

            var show = ShouldShow(slotData);
            if (slotData.parented && ParentedNameDictionary.TryGetValue(partsInfo.parentKey, out var parentingState))
                show &= parentingState.Show;

            var isSub = partsInfo.hideCategory == 1;
            show &= (!isSub && _showMain) || (isSub && _showSub); //respect the hide/show main and sub buttons
            ChaControl.SetAccessoryState(slot, show);
        }

        /// <summary>
        ///     Iterrate over whole list to deal with potentially cascading effects of one state affecting another which affects
        ///     another
        /// </summary>
        public void RefreshSlots()
        {
            var slotLength = PartsArray.Length;
            foreach (var slotData in SlotBindingData)
            {
                if (slotData.Key >= slotLength)
                    return;
                RefreshSlots(slotData.Key, slotData.Value);
            }
        }

        /// <summary>
        ///     Check bindings of slotdata, respect
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

                var stateInfo = item.GetStateInfo(item.NameData.currentState, shoeType);
                if (stateInfo == null || item.NameData.binding < 0 || stateInfo.Priority < currentPriority)
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

        internal void SetClothesState()
        {
            var clothstates = ChaControl.fileStatus.clothesState;
            var refreshSet = new HashSet<int>();
            for (var i = 0; i < clothstates.Length; i++)
            {
                var nameData = NameDataList.FirstOrDefault(x => x.binding == i);
                if (nameData == null)
                    continue;
                if (nameData.currentState != clothstates[i])
                {
                    refreshSet.UnionWith(nameData.AssociatedSlots);
                    nameData.currentState = clothstates[i];
                }
            }

            RefreshSlots(refreshSet);
        }
    }
}