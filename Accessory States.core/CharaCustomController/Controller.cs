using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Accessory_States
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        // TODO: Confirm ASS support
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            var insidermaker = currentGameMode == GameMode.Maker;

            var chafile = (currentGameMode == GameMode.Maker) ? MakerAPI.LastLoadedChaFile : ChaFileControl;

            var extendedData = GetExtendedData();
            if (extendedData != null)
            {
                if (extendedData.version < 2)
                {
                    Migration.Migrator.StandardCharaMigrator(ChaControl, extendedData);
                }
            }
            else if ((extendedData = ExtendedSave.GetExtendedDataById(chafile, "madevil.kk.ass")) != null)
            {
                var TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                var TriggerGroupList = new List<AccStateSync.TriggerGroup>();

                if (extendedData.version > 6)
                    Settings.Logger.LogWarning($"New version of AccessoryStateSync found, accessory states needs update for compatibility");
                else if (extendedData.version < 6)
                {
                    AccStateSync.Migration.ConvertCharaPluginData(extendedData, ref TriggerPropertyList, ref TriggerGroupList);
                }
                else
                {
                    if (extendedData.data.TryGetValue("TriggerPropertyList", out var _loadedTriggerProperty) && _loadedTriggerProperty != null)
                    {
                        var _tempTriggerProperty = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>((byte[])_loadedTriggerProperty);
                        if (_tempTriggerProperty?.Count > 0)
                            TriggerPropertyList.AddRange(_tempTriggerProperty);

                        if (extendedData.data.TryGetValue("TriggerGroupList", out var _loadedTriggerGroup) && _loadedTriggerGroup != null)
                        {
                            var _tempTriggerGroup = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>((byte[])_loadedTriggerGroup);
                            if (_tempTriggerGroup?.Count > 0)
                            {
                                foreach (var _group in _tempTriggerGroup)
                                {
                                    if (_group.GUID.IsNullOrEmpty())
                                        _group.GUID = System.Guid.NewGuid().ToString("D").ToUpper();
                                }
                                TriggerGroupList.AddRange(_tempTriggerGroup);
                            }
                        }
                    }
                }

                if (TriggerPropertyList == null)
                    TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                if (TriggerGroupList == null)
                    TriggerGroupList = new List<AccStateSync.TriggerGroup>();

                FullAssCardLoad(chafile, TriggerPropertyList, TriggerGroupList);
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
            FullAssCardSave(ref triggers, ref groups);
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

        // TODO: Confirm ASS support
        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            var extendedData = GetCoordinateExtendedData(coordinate);
            if (extendedData != null)
            {
                if (extendedData.version < 2)
                {
                    Migration.Migrator.StandardCoordMigrator(coordinate, extendedData);
                }
            }
            else if ((extendedData = ExtendedSave.GetExtendedDataById(coordinate, "madevil.kk.ass")) != null)
            {
                var TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                var TriggerGroupList = new List<AccStateSync.TriggerGroup>();

                if (extendedData.version > 6)
                    Settings.Logger.LogWarning($"New version of AccessoryStateSync found, accessory states needs update for compatibility");
                else if (extendedData.version < 6)
                {
                    AccStateSync.Migration.ConvertCharaPluginData(extendedData, ref TriggerPropertyList, ref TriggerGroupList);
                }
                else
                {
                    if (extendedData.data.TryGetValue("TriggerPropertyList", out var _loadedTriggerProperty) && _loadedTriggerProperty != null)
                    {
                        var _tempTriggerProperty = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>((byte[])_loadedTriggerProperty);
                        if (_tempTriggerProperty?.Count > 0)
                            TriggerPropertyList.AddRange(_tempTriggerProperty);

                        if (extendedData.data.TryGetValue("TriggerGroupList", out var _loadedTriggerGroup) && _loadedTriggerGroup != null)
                        {
                            var _tempTriggerGroup = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>((byte[])_loadedTriggerGroup);
                            if (_tempTriggerGroup?.Count > 0)
                            {
                                foreach (var _group in _tempTriggerGroup)
                                {
                                    if (_group.GUID.IsNullOrEmpty())
                                        _group.GUID = System.Guid.NewGuid().ToString("D").ToUpper();
                                }
                                TriggerGroupList.AddRange(_tempTriggerGroup);
                            }
                        }
                    }
                }

                if (TriggerPropertyList == null)
                    TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                if (TriggerGroupList == null)
                    TriggerGroupList = new List<AccStateSync.TriggerGroup>();

                ConvertAssCoordinate(coordinate, TriggerPropertyList, TriggerGroupList);
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

        internal void ChangeBindingSub(int hidesetting, NameData name)
        {
            var coordinateaccessory = ChaFileControl.coordinate[(int)CurrentCoordinate.Value].accessory.parts;
            var nowcoodaccessory = ChaControl.nowCoordinate.accessory.parts;
            foreach (var slot in SlotBindingData)
            {
                foreach (var bindingData in slot.Value.bindingDatas)
                {
                    if (bindingData.NameData != name)
                        continue;
                    coordinateaccessory[slot.Key].hideCategory = nowcoodaccessory[slot.Key].hideCategory = hidesetting;
                }
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
                var partsInfo = PartsArray[slotData.Key];
                if (partsInfo.type == 120)
                    continue;
                var show = ShouldShow(slotData.Value);
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
                if (stateInfo == null || item.NameData.Binding < 0 || stateInfo.Priority < currentPriority)
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
            for (var i = 0; i < clothstates.Length; i++)
            {
                var nameData = NameDataList.FirstOrDefault(x => x.Binding == i);
                if (nameData == null)
                    continue;
                nameData.CurrentState = clothstates[i];
            }
            RefreshSlots();
        }
    }
}
