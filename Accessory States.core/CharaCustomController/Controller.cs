using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
using UniRx;

namespace Accessory_States
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        public void Reset()
        {
            for (int i = 0, n = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length; i < n; i++)
                _coordinate.Clear();
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            var insidermaker = currentGameMode == GameMode.Maker;

            GUIINTStateCopyDict.Clear();

            for (var i = 0; i < ChaFileControl.coordinate.Length; i++)
                if (_coordinate.ContainsKey(i))
                    ClearOutfit(i);
                else
                    CreateOutfit(i);
            for (int i = ChaFileControl.coordinate.Length, n = _coordinate.Keys.Max() + 1; i < n; i++) RemoveOutfit(i);

            _chaFile = currentGameMode == GameMode.Maker ? MakerAPI.LastLoadedChaFile : ChaFileControl;

            Clear();

            Clear_Now_Coordinate();

            var extendedData = GetExtendedData();
            if (extendedData != null)
            {
                if (extendedData.version == 1)
                {
                    if (extendedData.data.TryGetValue("CoordinateData", out var byteData) && byteData != null)
                        _coordinate =
                            MessagePackSerializer.Deserialize<Dictionary<int, CoordinateData>>((byte[])byteData);
                }
                else if (extendedData.version == 0)
                {
                    Migrator.MigrateV0(extendedData, ref _coordinate);
                }
                else
                {
                    Settings.Logger.LogWarning("New plugin version found on card please update");
                }
            }

            var pluginData = ExtendedSave.GetExtendedDataById(_chaFile, "madevil.kk.ass");
            if (extendedData == null && pluginData != null)
            {
                var triggerPropertyList = new List<AccStateSync.TriggerProperty>();
                var triggerGroupList = new List<AccStateSync.TriggerGroup>();

                if (pluginData.version > 6)
                {
                    Settings.Logger.LogWarning(
                        "New version of AccessoryStateSync found, accessory states needs update for compatibility");
                }
                else if (pluginData.version < 6)
                {
                    AccStateSync.Migration.ConvertCharaPluginData(pluginData, ref triggerPropertyList,
                        ref triggerGroupList);
                }
                else
                {
                    if (pluginData.data.TryGetValue("TriggerPropertyList", out var loadedTriggerProperty) &&
                        loadedTriggerProperty != null)
                    {
                        var tempTriggerProperty =
                            MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>(
                                (byte[])loadedTriggerProperty);
                        if (tempTriggerProperty?.Count > 0)
                            triggerPropertyList.AddRange(tempTriggerProperty);

                        if (pluginData.data.TryGetValue("TriggerGroupList", out var loadedTriggerGroup) &&
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

                if (triggerPropertyList == null) triggerPropertyList = new List<AccStateSync.TriggerProperty>();
                if (triggerGroupList == null) triggerGroupList = new List<AccStateSync.TriggerGroup>();

                foreach (var item in _coordinate)
                    item.Value.AccStateSyncConvert(triggerPropertyList.Where(x => x.Coordinate == item.Key).ToList(),
                        triggerGroupList.Where(x => x.Coordinate == item.Key).ToList());
            }

            UpdateNowCoordinate();
            CurrentCoordinate.Subscribe(x =>
            {
                _showCustomGui = false;
                if (!_coordinate.ContainsKey((int)x))
                    CreateOutfit((int)x);

                StartCoroutine(ChangeOutfitCoroutine());
            });
        }

        private IEnumerator<int> ChangeOutfitCoroutine()
        {
            GUIINTStateCopyDict.Clear();
            yield return 0;
            UpdateNowCoordinate();
            yield return 0;
            Refresh();
            StartCoroutine(WaitForSlots());
            if (ForceClothDataUpdate) StartCoroutine(ForceClothNotUpdate());

            if (MakerAPI.InsideMaker)
                Update_Toggles_GUI();
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            foreach (var item in _coordinate) item.Value.CleanUp();
            var nulldata = _coordinate.All(x => x.Value.SlotInfo.Count == 0);
            var statesData = new PluginData { version = 1 };
            statesData.data.Add("CoordinateData", MessagePackSerializer.Serialize(_coordinate));
            SetExtendedData(nulldata ? null : statesData);

            if (!Settings.AssSave.Value || AssExists) return;

            var pluginData = new PluginData { version = 6 };

            var triggerPropertyList = new List<AccStateSync.TriggerProperty>();
            var triggerGroup = new List<AccStateSync.TriggerGroup>();

            foreach (var item in _coordinate)
            {
                item.Value.AccStateSyncConvert(item.Key, out var tempTriggerProperty, out var tempTriggerGroup);
                triggerPropertyList.AddRange(tempTriggerProperty);
                triggerGroup.AddRange(tempTriggerGroup);
            }

            pluginData.data.Add("TriggerPropertyList", MessagePackSerializer.Serialize(triggerPropertyList));
            pluginData.data.Add("TriggerGroupList", MessagePackSerializer.Serialize(triggerGroup));

            ExtendedSave.SetExtendedDataById(ChaFileControl, "madevil.kk.ass", nulldata ? null : pluginData);
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            nowCoordinate.CleanUp();
            var nulldata = SlotInfo.Count == 0;
            var myData = new PluginData { version = 1 };
            myData.data.Add("CoordinateData", MessagePackSerializer.Serialize(nowCoordinate));
            SetCoordinateExtendedData(coordinate, nulldata ? null : myData);

            if (!Settings.AssSave.Value || AssExists) return;

            var savedData = new PluginData { version = 6 };

            nowCoordinate.AccStateSyncConvert(-1, out var tempTriggerProperty, out var tempTriggerGroup);

            savedData.data.Add("TriggerPropertyList", MessagePackSerializer.Serialize(tempTriggerProperty));
            savedData.data.Add("TriggerGroupList", MessagePackSerializer.Serialize(tempTriggerGroup));
            ExtendedSave.SetExtendedDataById(coordinate, "madevil.kk.ass", nulldata ? null : savedData);
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            Clear_Now_Coordinate();
            var extendedData = GetCoordinateExtendedData(coordinate);
            if (extendedData != null)
            {
                if (extendedData.version == 1)
                {
                    if (extendedData.data.TryGetValue("CoordinateData", out var byteData) && byteData != null)
                        nowCoordinate = MessagePackSerializer.Deserialize<CoordinateData>((byte[])byteData);
                }
                else if (extendedData.version == 0)
                {
                    nowCoordinate = Migrator.CoordinateMigrateV0(extendedData);
                }
            }

            var pluginData = ExtendedSave.GetExtendedDataById(coordinate, "madevil.kk.ass");
            if (pluginData != null && extendedData == null)
            {
                var triggerPropertyList = new List<AccStateSync.TriggerProperty>();
                var triggerGroupList = new List<AccStateSync.TriggerGroup>();
                if (pluginData.version > 6)
                {
                    Settings.Logger.LogWarning(
                        "New version of AccessoryStateSync found, accessory states needs update for compatibility");
                }
                else if (pluginData.version < 6)
                {
                    AccStateSync.Migration.ConvertOutfitPluginData((int)CurrentCoordinate.Value, pluginData,
                        ref triggerPropertyList, ref triggerGroupList);
                }
                else
                {
                    if (pluginData.data.TryGetValue("TriggerPropertyList", out var loadedTriggerProperty) &&
                        loadedTriggerProperty != null)
                    {
                        triggerPropertyList =
                            MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>(
                                (byte[])loadedTriggerProperty);

                        if (pluginData.data.TryGetValue("TriggerGroupList", out var loadedTriggerGroup) &&
                            loadedTriggerGroup != null)
                            triggerGroupList =
                                MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>(
                                    (byte[])loadedTriggerGroup);
                    }
                }

                if (triggerPropertyList == null) triggerPropertyList = new List<AccStateSync.TriggerProperty>();
                if (triggerGroupList == null) triggerGroupList = new List<AccStateSync.TriggerGroup>();

                nowCoordinate.AccStateSyncConvert(triggerPropertyList, triggerGroupList);
            }

            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
                _coordinate[(int)CurrentCoordinate.Value] = nowCoordinate;

            //call event
            var args = new CoordinateLoadedEventArg(ChaControl /*, coordinate*/);
            if (!(CoordinateLoaded == null || CoordinateLoaded.GetInvocationList().Length == 0))
                try
                {
                    CoordinateLoaded?.Invoke(null, args);
                }
                catch (Exception ex)
                {
                    Settings.Logger.LogError($"Subscriber crash in {nameof(Hooks)}.{nameof(CoordinateLoaded)} - {ex}");
                }

            if (MakerAPI.InsideMaker) _coordinate[(int)CurrentCoordinate.Value] = nowCoordinate;

            if (ForceClothDataUpdate && !MakerAPI.InsideMaker) StartCoroutine(ForceClothNotUpdate());

            Refresh();
        }

        private void SetClothesState_switch(int clothesKind, byte state)
        {
            switch (clothesKind)
            {
                case 0: //top
                    SetClothesState_switch_Case(ClothNotData[0], ChaControl.notBot != ClothNotData[0], clothesKind,
                        state, 1); //1 is bot
                    SetClothesState_switch_Case(ClothNotData[1], ChaControl.notBra != ClothNotData[1], clothesKind,
                        state, 2); //2 is bra; line added for clothingunlock
                    break;
                case 1: //bot
                    SetClothesState_switch_Case(ClothNotData[0], ChaControl.notBot != ClothNotData[0], clothesKind,
                        state, 0); //0 is top
                    break;
                case 2: //bra
                    SetClothesState_switch_Case(ClothNotData[2], ChaControl.notShorts != ClothNotData[2], clothesKind,
                        state, 3); //3 is underwear
                    SetClothesState_switch_Case(ClothNotData[1], ChaControl.notBra != ClothNotData[1], clothesKind,
                        state, 0); //line added for clothingunlock
                    break;
                case 3: //underwear
                    SetClothesState_switch_Case(ClothNotData[2], ChaControl.notShorts != ClothNotData[2], clothesKind,
                        state, 2);
                    break;
                case 7: //innershoes
                    SetClothesState_switch_Case_2(state, 8);
                    break;
                case 8: //outershoes
                    SetClothesState_switch_Case_2(state, 7);
                    break;
            }
        }

        private void SetClothesState_switch_Case(bool condition, bool clothingunlocked, int clothesKind,
            byte currentstate, int relatedcloth)
        {
            if (condition)
            {
                var clothesState = ChaControl.fileStatus.clothesState;
                byte setrelatedclothset;
                var currentvisible = clothesState[clothesKind] != 3;
                var relatedvisible = clothesState[relatedcloth] != 3;
                if (!currentvisible && relatedvisible)
                    setrelatedclothset = 3;
                else if (!relatedvisible && currentvisible)
                    setrelatedclothset = currentstate;
                else
                    return;
                if (clothingunlocked && !(StopMakerLoop && MakerAPI.InsideMaker))
                    ChaControl.SetClothesState(relatedcloth, setrelatedclothset);

                ChangedOutfit(relatedcloth, setrelatedclothset);
            }
        }

        private void SetClothesState_switch_Case_2(byte state, int clotheskind)
        {
            if (state == 0)
            {
                if (ChaControl.IsClothesStateKind(clotheskind)) ChangedOutfit(clotheskind, state);
            }
            else
            {
                ChangedOutfit(clotheskind, state);
            }
        }

        private static bool BothShoeCheck(List<AccStateSync.TriggerProperty> triggerPropertyList,
            AccStateSync.TriggerProperty selectedProperty, int checkValue)
        {
            selectedProperty.RefKind = checkValue;
            return triggerPropertyList.Any(x => x == selectedProperty);
        }

        public void Refresh()
        {
            _lastKnownShoeType = ChaControl.fileStatus.shoesType;
            foreach (var item in NowParentedNameDictionary)
            {
                if (!GUIParentDict.TryGetValue(item.Key, out var show)) show = true;
                Parent_toggle(item.Key, show);
            }

            for (var i = 0; i < 8; i++) ChangedOutfit(i, ChaControl.fileStatus.clothesState[i]);

            foreach (var item in Names.Keys)
            {
                if (GUICustomDict.TryGetValue(item, out var state))
                {
                    Custom_Groups(item, state[0]);
                    continue;
                }

                Custom_Groups(item, 0);
            }
        }

        internal void SetClothesState(int clothesKind, byte state)
        {
            ChangedOutfit(clothesKind, state);
            SetClothesState_switch(clothesKind, state);
            var currentshoe = ChaControl.fileStatus.shoesType;
            if (clothesKind > 6 && _lastKnownShoeType != currentshoe)
            {
                _lastKnownShoeType = currentshoe;
                var clothesState = ChaControl.fileStatus.clothesState;
                for (var i = 0; i < 7; i++) SetClothesState(i, clothesState[i]);
            }
        }

        private static bool ShowState(int state, List<int[]> list)
        {
            return list.Any(x => state >= x[0] && state <= x[1]);
        }

        private int MaxState(int outfitNum, int binding)
        {
            if (binding < 9) return 3;
            var max = 1;
            var bindinglist = _coordinate[outfitNum].SlotInfo.Values.Where(x => x.Binding == binding);
            foreach (var item in bindinglist) item.States.ForEach(x => max = Math.Max(x[1], max));
            return max;
        }

        internal int MaxState(int binding)
        {
            if (binding < 9) return 3;
            var max = 1;
            var bindinglist = SlotInfo.Values.Where(x => x.Binding == binding);
            foreach (var item in bindinglist) item.States.ForEach(x => max = Math.Max(x[1], max));
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
            var shoeType = ChaControl.fileStatus.shoesType;
            var accessoryList = SlotInfo.Where(x => x.Value.Binding == clothesKind);
            foreach (var item in accessoryList)
            {
                if (item.Key >= Parts.Length)
                    return;

                var show = false;
                var issub = Parts[item.Key].hideCategory == 1;
                if ((!issub || (issub && _showSub)) && (item.Value.ShoeType == 2 || item.Value.ShoeType == shoeType))
                    show = ShowState(state, item.Value.States);
                ChaControl.SetAccessoryState(item.Key, show);
            }
        }

        public void Custom_Groups(int kind, int state)
        {
            var shoeType = ChaControl.fileStatus.shoesType;
            var accessoryList = SlotInfo.Where(x => x.Value.Binding == kind);
            foreach (var item in accessoryList)
            {
                if (item.Key >= Parts.Length)
                    return;

                var show = false;

                var issub = Parts[item.Key].hideCategory == 1;
                if ((!issub || (issub && _showSub)) && (item.Value.ShoeType == 2 || item.Value.ShoeType == shoeType))
                    show = ShowState(state, item.Value.States);
                ChaControl.SetAccessoryState(item.Key, show);
            }
        }

        public void Parent_toggle(string parent, bool toggleshow)
        {
            var shoeType = ChaControl.fileStatus.shoesType;
            var parentedList = NowParentedNameDictionary[parent];
            foreach (var slot in parentedList)
            {
                if (slot.Key >= Parts.Length)
                    return;

                var show = false;

                var issub = Parts[slot.Key].hideCategory == 1;
                if ((!issub || (issub && _showSub)) && (slot.Value.ShoeType == 2 || slot.Value.ShoeType == shoeType))
                    show = toggleshow;

                ChaControl.SetAccessoryState(slot.Key, show);
            }
        }

        private IEnumerator ForceClothNotUpdate()
        {
            yield return null;
            UpdateClothingNots();
            Refresh();
        }

        internal void SubChanged(bool show)
        {
            _showSub = show;
            Refresh();
        }

        private void ChangeBindingSub(int hidesetting)
        {
            var coordinateaccessory = ChaFileControl.coordinate[(int)CurrentCoordinate.Value].accessory.parts;
            var nowcoodaccessory = ChaControl.nowCoordinate.accessory.parts;
            var slotslist = SlotInfo.Where(x => x.Value.Binding == _selectedkind).Select(x => x.Key);
            foreach (var slot in slotslist)
                coordinateaccessory[slot].hideCategory = nowcoodaccessory[slot].hideCategory = hidesetting;
        }
    }
}