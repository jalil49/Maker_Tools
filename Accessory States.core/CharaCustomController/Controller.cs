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
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            var insidermaker = currentGameMode == GameMode.Maker;

            GUI_int_state_copy_Dict.Clear();

            chafile = (currentGameMode == GameMode.Maker) ? MakerAPI.LastLoadedChaFile : ChaFileControl;

            var Extended_Data = GetExtendedData();
            if (Extended_Data != null)
            {
                if (Extended_Data.version < 2)
                {
                    Migrator.StandardCharaMigrator(ChaControl, Extended_Data);
                }
            }

            var _pluginData = ExtendedSave.GetExtendedDataById(chafile, "madevil.kk.ass");
            if (Extended_Data == null && _pluginData != null)
            {
                var TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                var TriggerGroupList = new List<AccStateSync.TriggerGroup>();

                if (_pluginData.version > 6)
                    Settings.Logger.LogWarning($"New version of AccessoryStateSync found, accessory states needs update for compatibility");
                else if (_pluginData.version < 6)
                {
                    AccStateSync.Migration.ConvertCharaPluginData(_pluginData, ref TriggerPropertyList, ref TriggerGroupList);
                }
                else
                {
                    if (_pluginData.data.TryGetValue("TriggerPropertyList", out var _loadedTriggerProperty) && _loadedTriggerProperty != null)
                    {
                        var _tempTriggerProperty = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>((byte[])_loadedTriggerProperty);
                        if (_tempTriggerProperty?.Count > 0)
                            TriggerPropertyList.AddRange(_tempTriggerProperty);

                        if (_pluginData.data.TryGetValue("TriggerGroupList", out var _loadedTriggerGroup) && _loadedTriggerGroup != null)
                        {
                            var _tempTriggerGroup = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>((byte[])_loadedTriggerGroup);
                            if (_tempTriggerGroup?.Count > 0)
                            {
                                foreach (var _group in _tempTriggerGroup)
                                {
                                    if (_group.GUID.IsNullOrEmpty())
                                        _group.GUID = Guid.NewGuid().ToString("D").ToUpper();
                                }
                                TriggerGroupList.AddRange(_tempTriggerGroup);
                            }
                        }
                    }
                }

                if (TriggerPropertyList == null) TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                if (TriggerGroupList == null) TriggerGroupList = new List<AccStateSync.TriggerGroup>();

                AccStateSyncConvert(TriggerPropertyList, TriggerGroupList);
            }

            CurrentCoordinate.Subscribe(x =>
            {
                GUI_int_state_copy_Dict.Clear();
                UpdatePluginData();
                StartCoroutine(ChangeOutfitCoroutine());
            });
        }

        private IEnumerator<int> ChangeOutfitCoroutine()
        {
            yield return 0;
            Refresh();
            if (ForceClothDataUpdate)
            {
                StartCoroutine(ForceClothNotUpdate());
            }

            if (MakerAPI.InsideMaker)
                UpdateTogglesGUI();
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            var States_Data = new PluginData() { version = Constants.SaveVersion };
            SetExtendedData(States_Data);

            if (!Settings.ASS_SAVE.Value || ASSExists)
            {
                return;
            }

            var _pluginData = new PluginData() { version = 6 };

            AccStateSyncConvert(out var TriggerPropertyList, out var TriggerGroup);

            _pluginData.data.Add("TriggerPropertyList", MessagePackSerializer.Serialize(TriggerPropertyList));
            _pluginData.data.Add("TriggerGroupList", MessagePackSerializer.Serialize(TriggerGroup));
            _pluginData.data.Add("ExternalManipulation", MessagePackSerializer.Serialize("Accessory_States"));

            ExtendedSave.SetExtendedDataById(ChaFileControl, "madevil.kk.ass", (TriggerPropertyList.Count == 0) ? null : _pluginData);
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {

            if (!Settings.ASS_SAVE.Value || ASSExists)
            {
                return;
            }

            //var SavedData = new PluginData() { version = 6 };

            //NowCoordinateData.Accstatesyncconvert(-1, out var _tempTriggerProperty, out var _tempTriggerGroup);

            //SavedData.data.Add("TriggerPropertyList", MessagePackSerializer.Serialize(_tempTriggerProperty));
            //SavedData.data.Add("TriggerGroupList", MessagePackSerializer.Serialize(_tempTriggerGroup));
            //ExtendedSave.SetExtendedDataById(coordinate, "madevil.kk.ass", (nulldata) ? null : SavedData);
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            var Extended_Data = GetCoordinateExtendedData(coordinate);
            if (Extended_Data != null)
            {
                if (Extended_Data.version < 2)
                {
                    Migrator.StandardCoordMigrator(coordinate, Extended_Data);
                }
            }

            var _pluginData = ExtendedSave.GetExtendedDataById(coordinate, "madevil.kk.ass");
            if (_pluginData != null && Extended_Data == null)
            {

                var TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                var TriggerGroupList = new List<AccStateSync.TriggerGroup>();
                if (_pluginData.version > 6)
                    Settings.Logger.LogWarning($"New version of AccessoryStateSync found, accessory states needs update for compatibility");
                else if (_pluginData.version < 6)
                {
                    AccStateSync.Migration.ConvertOutfitPluginData((int)CurrentCoordinate.Value, _pluginData, ref TriggerPropertyList, ref TriggerGroupList);
                }
                else
                {
                    if (_pluginData.data.TryGetValue("TriggerPropertyList", out var _loadedTriggerProperty) && _loadedTriggerProperty != null)
                    {
                        TriggerPropertyList = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>((byte[])_loadedTriggerProperty);

                        if (_pluginData.data.TryGetValue("TriggerGroupList", out var _loadedTriggerGroup) && _loadedTriggerGroup != null)
                        {
                            TriggerGroupList = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>((byte[])_loadedTriggerGroup);
                        }
                    }
                }

                if (TriggerPropertyList == null) TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                if (TriggerGroupList == null) TriggerGroupList = new List<AccStateSync.TriggerGroup>();

                AccStateSyncConvertCoordProcess(coordinate, TriggerPropertyList, TriggerGroupList);
            }

            UpdatePluginData();

            //call event
            if (ForceClothDataUpdate && !MakerAPI.InsideMaker)
            {
                StartCoroutine(ForceClothNotUpdate());
            }

            Refresh();
        }

        private void SetClothesState_switch(int clothesKind, byte state)
        {
            switch (clothesKind)
            {
                case 0://top
                    SetClothesState_switch_Case(ClothNotData[0], ChaControl.notBot != ClothNotData[0], clothesKind, state, 1);//1 is bot
                    SetClothesState_switch_Case(ClothNotData[1], ChaControl.notBra != ClothNotData[1], clothesKind, state, 2);//2 is bra; line added for clothingunlock
                    break;
                case 1://bot
                    SetClothesState_switch_Case(ClothNotData[0], ChaControl.notBot != ClothNotData[0], clothesKind, state, 0);//0 is top
                    break;
                case 2://bra
                    SetClothesState_switch_Case(ClothNotData[2], ChaControl.notShorts != ClothNotData[2], clothesKind, state, 3);//3 is underwear
                    SetClothesState_switch_Case(ClothNotData[1], ChaControl.notBra != ClothNotData[1], clothesKind, state, 0);//line added for clothingunlock
                    break;
                case 3://underwear
                    SetClothesState_switch_Case(ClothNotData[2], ChaControl.notShorts != ClothNotData[2], clothesKind, state, 2);
                    break;
                case 7://innershoes
                    SetClothesState_switch_Case_2(state, 8);
                    break;
                case 8://outershoes
                    SetClothesState_switch_Case_2(state, 7);
                    break;
                default:
                    break;
            }
        }

        private void SetClothesState_switch_Case(bool condition, bool clothingunlocked, int clothesKind, byte currentstate, int relatedcloth)
        {
            if (condition)
            {
                var clothesState = ChaControl.fileStatus.clothesState;
                byte setrelatedclothset;
                var currentvisible = clothesState[clothesKind] != 3;
                var relatedvisible = clothesState[relatedcloth] != 3;
                if (!currentvisible && relatedvisible)
                {
                    setrelatedclothset = 3;
                }
                else if (!relatedvisible && currentvisible)
                {
                    setrelatedclothset = currentstate;
                }
                else
                {
                    return;
                }
                if (clothingunlocked && !(StopMakerLoop && MakerAPI.InsideMaker))
                {
                    ChaControl.SetClothesState(relatedcloth, setrelatedclothset);
                }

                ChangedOutfit(relatedcloth, setrelatedclothset);
            }
        }

        private void SetClothesState_switch_Case_2(byte state, int Clotheskind)
        {
            if (state == 0)
            {
                if (ChaControl.IsClothesStateKind(Clotheskind))
                {
                    ChangedOutfit(Clotheskind, state);
                }
            }
            else
            {
                ChangedOutfit(Clotheskind, state);
            }
        }

        public void Refresh()
        {
            if (DisableRefresh)
            {
                return;
            }
            lastknownshoetype = ChaControl.fileStatus.shoesType;
            foreach (var item in ParentedNameDictionary)
            {
                if (!GUI_Parent_Dict.TryGetValue(item.Key, out var show))
                {
                    show = true;
                }
                ParentToggle(item.Key, show);
            }

            for (var i = 0; i < 8; i++)
            {
                ChangedOutfit(i, ChaControl.fileStatus.clothesState[i]);
            }

            var gui = Constants.ClothingLength;
            foreach (var item in Names)
            {
                if (GUI_Custom_Dict.TryGetValue(gui, out var state))
                {
                    CustomGroups(gui, state[0]);
                    continue;
                }
                CustomGroups(gui, item.DefaultState);
                gui++;
            }
        }

        internal void SetClothesState(int clothesKind, byte state)
        {
            ChangedOutfit(clothesKind, state);
            SetClothesState_switch(clothesKind, state);
            var currentshoe = ChaControl.fileStatus.shoesType;
            if (clothesKind > 6 && lastknownshoetype != currentshoe)
            {
                lastknownshoetype = currentshoe;
                var clothesState = ChaControl.fileStatus.clothesState;
                for (var i = 0; i < 7; i++)
                {
                    SetClothesState(i, clothesState[i]);
                }
            }
        }

        private static bool ShowState(int state, List<int[]> list)
        {
            return list.Any(x => state >= x[0] && state <= x[1]);
        }

        internal int MaxState(int binding)
        {
            if (binding < 9)
            {
                return 3;
            }
            var max = 1;
            foreach (var item in SlotInfo.Values.Where(x => x.Binding == binding))
            {
                item.States.ForEach(x => max = Math.Max(x[1], max));
            }
            return max;
        }

        private static int MaxState(List<int[]> list)
        {
            var max = 0;
            list.ForEach(x => max = Math.Max(x[1], max));
            return max;
        }

        private void ChangedOutfit(int clothesKind, byte state)
        {
            var shoetype = ChaControl.fileStatus.shoesType;
            var Accessory_list = SlotInfo.Where(x => x.Value.Binding == clothesKind);
            foreach (var item in Accessory_list)
            {
                if (item.Key >= PartsArray.Length)
                    return;

                var show = false;
                var issub = PartsArray[item.Key].hideCategory == 1;
                if ((!issub || issub && ShowSub) && (item.Value.ShoeType == 2 || item.Value.ShoeType == shoetype))
                {
                    show = ShowState(state, item.Value.States);
                }
                ChaControl.SetAccessoryState(item.Key, show);
            }
        }

        public void CustomGroups(int kind, int state)
        {
            var shoetype = ChaControl.fileStatus.shoesType;
            var Accessory_list = SlotInfo.Where(x => x.Value.Binding == kind);
            foreach (var item in Accessory_list)
            {
                if (item.Key >= PartsArray.Length)
                    return;

                var show = false;

                var issub = PartsArray[item.Key].hideCategory == 1;
                if ((!issub && ShowMain || issub && ShowSub) && (item.Value.ShoeType == 2 || item.Value.ShoeType == shoetype))
                {
                    show = ShowState(state, item.Value.States);
                }
                ChaControl.SetAccessoryState(item.Key, show);
            }
        }

        public void ParentToggle(string parent, bool toggleshow)
        {
            var shoetype = ChaControl.fileStatus.shoesType;
            var ParentedList = ParentedNameDictionary[parent];
            foreach (var slot in ParentedList)
            {
                if (slot.Key >= PartsArray.Length)
                    return;

                var show = false;

                var issub = PartsArray[slot.Key].hideCategory == 1;
                if ((!issub || issub && ShowSub) && (slot.Value.ShoeType == 2 || slot.Value.ShoeType == shoetype))
                {
                    show = toggleshow;
                }

                ChaControl.SetAccessoryState(slot.Key, show);
            }
        }

        private IEnumerator ForceClothNotUpdate()
        {
            yield return null;
            UpdateClothingNots();
            Refresh();
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

            Refresh();
        }

        private void ChangeBindingSub(int hidesetting)
        {
            var coordinateaccessory = ChaFileControl.coordinate[(int)CurrentCoordinate.Value].accessory.parts;
            var nowcoodaccessory = ChaControl.nowCoordinate.accessory.parts;
            var slotslist = SlotInfo.Where(x => x.Value.Binding == Selectedkind).Select(x => x.Key);
            foreach (var slot in slotslist)
            {
                coordinateaccessory[slot].hideCategory = nowcoodaccessory[slot].hideCategory = hidesetting;
            }
        }
    }
}
