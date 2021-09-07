using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
#if !KKS
using KKAPI.MainGame;
#endif
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

            for (var i = 0; i < ChaFileControl.coordinate.Length; i++)
            {
                if (Coordinate.ContainsKey(i))
                    Clearoutfit(i);
                else
                    Createoutfit(i);
            }
            for (int i = ChaFileControl.coordinate.Length, n = Coordinate.Keys.Max() + 1; i < n; i++)
            {
                Removeoutfit(i);
            }

            chafile = (currentGameMode == GameMode.Maker) ? MakerAPI.LastLoadedChaFile : ChaFileControl;

            Clear();

            Clear_Now_Coordinate();

            var Extended_Data = GetExtendedData();
            if (Extended_Data != null)
            {
                if (Extended_Data.version == 1)
                {
                    if (Extended_Data.data.TryGetValue("CoordinateData", out var ByteData) && ByteData != null)
                    {
                        Coordinate = MessagePackSerializer.Deserialize<Dictionary<int, CoordinateData>>((byte[])ByteData);
                    }
                }
                else if (Extended_Data.version == 0)
                {
                    Migrator.MigrateV0(Extended_Data, ref Coordinate);
                }
                else
                {
                    Settings.Logger.LogWarning("New plugin version found on card please update");
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

                foreach (var item in Coordinate)
                {
                    item.Value.Accstatesyncconvert(TriggerPropertyList.Where(x => x.Coordinate == item.Key).ToList(), TriggerGroupList.Where(x => x.Coordinate == item.Key).ToList());
                }
            }

            Update_Now_Coordinate();
            CurrentCoordinate.Subscribe(x =>
            {
                ShowCustomGui = false;
                if (!Coordinate.ContainsKey((int)x))
                    Createoutfit((int)x);

                StartCoroutine(ChangeOutfitCoroutine());
            });

        }

        private IEnumerator<int> ChangeOutfitCoroutine()
        {
            GUI_int_state_copy_Dict.Clear();
            yield return 0;
            Update_Now_Coordinate();
            yield return 0;
            Refresh();
            StartCoroutine(WaitForSlots());
            if (ForceClothDataUpdate)
            {
                StartCoroutine(ForceClothNotUpdate());
            }

            if (MakerAPI.InsideMaker)
                Update_Toggles_GUI();
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            foreach (var item in Coordinate)
            {
                item.Value.CleanUp();
            }
            var nulldata = Coordinate.All(x => x.Value.Slotinfo.Count == 0);
            var States_Data = new PluginData() { version = 1 };
            States_Data.data.Add("CoordinateData", MessagePackSerializer.Serialize(Coordinate));
            SetExtendedData((nulldata) ? null : States_Data);

            if (!Settings.ASS_SAVE.Value || ASS_Exists)
            {
                return;
            }

            var _pluginData = new PluginData() { version = 6 };

            var TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
            var TriggerGroup = new List<AccStateSync.TriggerGroup>();

            foreach (var item in Coordinate)
            {
                item.Value.Accstatesyncconvert(item.Key, out var _tempTriggerProperty, out var _tempTriggerGroup);
                TriggerPropertyList.AddRange(_tempTriggerProperty);
                TriggerGroup.AddRange(_tempTriggerGroup);
            }

            _pluginData.data.Add("TriggerPropertyList", MessagePackSerializer.Serialize(TriggerPropertyList));
            _pluginData.data.Add("TriggerGroupList", MessagePackSerializer.Serialize(TriggerGroup));

            ExtendedSave.SetExtendedDataById(ChaFileControl, "madevil.kk.ass", (nulldata) ? null : _pluginData);
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            NowCoordinate.CleanUp();
            var nulldata = Slotinfo.Count == 0;
            var MyData = new PluginData() { version = 1 };
            MyData.data.Add("CoordinateData", MessagePackSerializer.Serialize(NowCoordinate));
            SetCoordinateExtendedData(coordinate, (nulldata) ? null : MyData);

            if (!Settings.ASS_SAVE.Value || ASS_Exists)
            {
                return;
            }

            var SavedData = new PluginData() { version = 6 };

            NowCoordinate.Accstatesyncconvert(-1, out var _tempTriggerProperty, out var _tempTriggerGroup);

            SavedData.data.Add("TriggerPropertyList", MessagePackSerializer.Serialize(_tempTriggerProperty));
            SavedData.data.Add("TriggerGroupList", MessagePackSerializer.Serialize(_tempTriggerGroup));
            ExtendedSave.SetExtendedDataById(coordinate, "madevil.kk.ass", (nulldata) ? null : SavedData);
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            Clear_Now_Coordinate();
            var Extended_Data = GetCoordinateExtendedData(coordinate);
            if (Extended_Data != null)
            {
                if (Extended_Data.version == 1)
                {
                    if (Extended_Data.data.TryGetValue("CoordinateData", out var ByteData) && ByteData != null)
                    {
                        NowCoordinate = MessagePackSerializer.Deserialize<CoordinateData>((byte[])ByteData);
                    }
                }
                else if (Extended_Data.version == 0)
                {
                    NowCoordinate = Migrator.CoordinateMigrateV0(Extended_Data);
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

                NowCoordinate.Accstatesyncconvert(TriggerPropertyList, TriggerGroupList);
            }

            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            {
                Coordinate[(int)CurrentCoordinate.Value] = NowCoordinate;
            }

            //call event
            var args = new CoordinateLoadedEventARG(ChaControl/*, coordinate*/);
            if (!(Coordloaded == null || Coordloaded.GetInvocationList().Length == 0))
            {
                try
                {
                    Coordloaded?.Invoke(null, args);
                }
                catch (Exception ex)
                {
                    Settings.Logger.LogError($"Subscriber crash in {nameof(Hooks)}.{nameof(Coordloaded)} - {ex}");
                }
            }

            if (MakerAPI.InsideMaker)
            {
                Coordinate[(int)CurrentCoordinate.Value] = NowCoordinate;
            }

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

        private static bool BothShoeCheck(List<AccStateSync.TriggerProperty> triggerPropertyList, AccStateSync.TriggerProperty selectedproperty, int checkvalue)
        {
            selectedproperty.RefKind = checkvalue;
            return triggerPropertyList.Any(x => x == selectedproperty);
        }

        public void Refresh()
        {
            lastknownshoetype = ChaControl.fileStatus.shoesType;
            foreach (var item in Now_Parented_Name_Dictionary)
            {
                if (!GUI_Parent_Dict.TryGetValue(item.Key, out var show))
                {
                    show = true;
                }
                Parent_toggle(item.Key, show);
            }

            for (var i = 0; i < 8; i++)
            {
                ChangedOutfit(i, ChaControl.fileStatus.clothesState[i]);
            }

            foreach (var item in Names.Keys)
            {
                if (GUI_Custom_Dict.TryGetValue(item, out var state))
                {
                    Custom_Groups(item, state[0]);
                    continue;
                }
                Custom_Groups(item, 0);
            }
        }

        public void Reset()
        {
            for (int i = 0, n = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length; i < n; i++)
            {
                Coordinate.Clear();
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

        private int MaxState(int outfitnum, int binding)
        {
            if (binding < 9)
            {
                return 3;
            }
            var max = 1;
            var bindinglist = Coordinate[outfitnum].Slotinfo.Values.Where(x => x.Binding == binding);
            foreach (var item in bindinglist)
            {
                item.States.ForEach(x => max = Math.Max(x[1], max));
            }
            return max;
        }

        private int MaxState(int binding)
        {
            if (binding < 9)
            {
                return 3;
            }
            var max = 1;
            var bindinglist = Slotinfo.Values.Where(x => x.Binding == binding);
            foreach (var item in bindinglist)
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
            var Accessory_list = Slotinfo.Where(x => x.Value.Binding == clothesKind);
            foreach (var item in Accessory_list)
            {
                if (item.Key >= Parts.Length)
                    return;

                var show = false;
                var issub = Parts[item.Key].hideCategory == 1;
                if ((!issub || issub && ShowSub) && (item.Value.Shoetype == 2 || item.Value.Shoetype == shoetype))
                {
                    show = ShowState(state, item.Value.States);
                }
                ChaControl.SetAccessoryState(item.Key, show);
            }
        }

        public void Custom_Groups(int kind, int state)
        {
            var shoetype = ChaControl.fileStatus.shoesType;
            var Accessory_list = Slotinfo.Where(x => x.Value.Binding == kind);
            foreach (var item in Accessory_list)
            {
                if (item.Key >= Parts.Length)
                    return;

                var show = false;

                var issub = Parts[item.Key].hideCategory == 1;
                if ((!issub || issub && ShowSub) && (item.Value.Shoetype == 2 || item.Value.Shoetype == shoetype))
                {
                    show = ShowState(state, item.Value.States);
                }
                ChaControl.SetAccessoryState(item.Key, show);
            }
        }

        public void Parent_toggle(string parent, bool toggleshow)
        {
            var shoetype = ChaControl.fileStatus.shoesType;
            var ParentedList = Now_Parented_Name_Dictionary[parent];
            foreach (var slot in ParentedList)
            {
                if (slot.Key >= Parts.Length)
                    return;

                var show = false;

                var issub = Parts[slot.Key].hideCategory == 1;
                if ((!issub || issub && ShowSub) && (slot.Value.Shoetype == 2 || slot.Value.Shoetype == shoetype))
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

        internal void SubChanged(bool show)
        {
            ShowSub = show;
            Refresh();
        }

        private void ChangeBindingSub(int hidesetting)
        {
            var coordinateaccessory = ChaFileControl.coordinate[(int)CurrentCoordinate.Value].accessory.parts;
            var nowcoodaccessory = ChaControl.nowCoordinate.accessory.parts;
            var slotslist = Slotinfo.Where(x => x.Value.Binding == Selectedkind).Select(x => x.Key);
            foreach (var slot in slotslist)
            {
                coordinateaccessory[slot].hideCategory = nowcoodaccessory[slot].hideCategory = hidesetting;
            }
        }
    }
}
