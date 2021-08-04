using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using MessagePack;
using MoreAccessoriesKOI;
using System;
using System.Collections.Generic;
using System.Linq;
using ToolBox;
using UniRx;
using UnityEngine;

namespace Accessory_States
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            bool insidermaker = currentGameMode == GameMode.Maker;

            GUI_int_state_copy_Dict.Clear();

            ThisCharactersData = Constants.CharacterInfo.Find(x => ChaControl.fileParam.personality == x.Personality && x.FullName == ChaControl.fileParam.fullname && x.BirthDay == ChaControl.fileParam.strBirthDay);
            if (ThisCharactersData == null)
            {
                ThisCharactersData = new Data(ChaControl.fileParam.personality, ChaControl.fileParam.strBirthDay, ChaControl.fileParam.fullname, this);
                Constants.CharacterInfo.Add(ThisCharactersData);
            }
            if (!ThisCharactersData.processed || currentGameMode == GameMode.Maker
#if !KKS
                || GameAPI.InsideHScene
#endif
                )
            {
                ThisCharactersData.processed = true;
                for (int i = 0; i < ChaFileControl.coordinate.Length; i++)
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

                ThisCharactersData.Controller = this;
                chafile = (currentGameMode == GameMode.Maker) ? MakerAPI.LastLoadedChaFile : ChaFileControl;

                ThisCharactersData.Clear();

                ThisCharactersData.Clear_Now_Coordinate();

                Update_More_Accessories();

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
                        Migrator.MigrateV0(Extended_Data, ref ThisCharactersData);
                    }
                    else
                    {
                        Settings.Logger.LogWarning("New plugin version found on card please update");
                    }
                }

                var _pluginData = ExtendedSave.GetExtendedDataById(chafile, "madevil.kk.ass");
                if (Extended_Data == null && _pluginData != null)
                {
                    List<AccStateSync.TriggerProperty> TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                    List<AccStateSync.TriggerGroup> TriggerGroupList = new List<AccStateSync.TriggerGroup>();

                    if (_pluginData.version > 6)
                        Settings.Logger.LogWarning($"New version of AccessoryStateSync found, accessory states needs update for compatibility");
                    else if (_pluginData.version < 6)
                    {
                        AccStateSync.Migration.ConvertCharaPluginData(_pluginData, ref TriggerPropertyList, ref TriggerGroupList);
                    }
                    else
                    {
                        if (_pluginData.data.TryGetValue("TriggerPropertyList", out object _loadedTriggerProperty) && _loadedTriggerProperty != null)
                        {
                            List<AccStateSync.TriggerProperty> _tempTriggerProperty = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>((byte[])_loadedTriggerProperty);
                            if (_tempTriggerProperty?.Count > 0)
                                TriggerPropertyList.AddRange(_tempTriggerProperty);

                            if (_pluginData.data.TryGetValue("TriggerGroupList", out object _loadedTriggerGroup) && _loadedTriggerGroup != null)
                            {
                                List<AccStateSync.TriggerGroup> _tempTriggerGroup = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>((byte[])_loadedTriggerGroup);
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

                if (currentGameMode == GameMode.Maker)
                    Update_Custom_GUI();

                ThisCharactersData.Update_Now_Coordinate();
                CurrentCoordinate.Subscribe(x =>
                {
                    ThisCharactersData.Update_Now_Coordinate();
                    Refresh();
                    StartCoroutine(WaitForSlots());
                    GUI_int_state_copy_Dict.Clear();
                    if (currentGameMode == GameMode.Maker)
                        Update_Custom_GUI();
                });
            }
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            foreach (var item in Coordinate)
            {
                item.Value.CleanUp();
            }

            PluginData States_Data = new PluginData() { version = 1 };
            States_Data.data.Add("CoordinateData", MessagePackSerializer.Serialize(Coordinate));
            SetExtendedData(States_Data);

            if (!Settings.ASS_SAVE.Value || ASS_Exists)
            {
                return;
            }

            PluginData _pluginData = new PluginData() { version = 6 };

            List<AccStateSync.TriggerProperty> TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
            List<AccStateSync.TriggerGroup> TriggerGroup = new List<AccStateSync.TriggerGroup>();

            foreach (var item in Coordinate)
            {
                item.Value.Accstatesyncconvert(item.Key, out var _tempTriggerProperty, out var _tempTriggerGroup);
                TriggerPropertyList.AddRange(_tempTriggerProperty);
                TriggerGroup.AddRange(_tempTriggerGroup);
            }

            _pluginData.data.Add("TriggerPropertyList", MessagePackSerializer.Serialize(TriggerPropertyList));
            _pluginData.data.Add("TriggerGroupList", MessagePackSerializer.Serialize(TriggerGroup));

            ExtendedSave.SetExtendedDataById(ChaFileControl, "madevil.kk.ass", _pluginData);
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            NowCoordinate.CleanUp();

            PluginData MyData = new PluginData() { version = 1 };
            MyData.data.Add("CoordinateData", MessagePackSerializer.Serialize(NowCoordinate));
            SetCoordinateExtendedData(coordinate, MyData);

            if (!Settings.ASS_SAVE.Value || ASS_Exists)
            {
                return;
            }

            PluginData SavedData = new PluginData() { version = 6 };

            NowCoordinate.Accstatesyncconvert(-1, out var _tempTriggerProperty, out var _tempTriggerGroup);

            SavedData.data.Add("TriggerPropertyList", MessagePackSerializer.Serialize(_tempTriggerProperty));
            SavedData.data.Add("TriggerGroupList", MessagePackSerializer.Serialize(_tempTriggerGroup));
            ExtendedSave.SetExtendedDataById(coordinate, "madevil.kk.ass", SavedData);
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            var Extended_Data = GetCoordinateExtendedData(coordinate);

            ThisCharactersData.Clear_Now_Coordinate();

            if (Extended_Data != null)
            {
                if (Extended_Data.version == 1)
                {
                    if (Extended_Data.data.TryGetValue("CoordinateData", out var ByteData) && ByteData != null)
                    {
                        ThisCharactersData.NowCoordinate = MessagePackSerializer.Deserialize<CoordinateData>((byte[])ByteData);
                    }
                }
                else if (Extended_Data.version == 0)
                {
                    ThisCharactersData.NowCoordinate = Migrator.CoordinateMigrateV0(Extended_Data);
                }
            }
            PluginData _pluginData = ExtendedSave.GetExtendedDataById(coordinate, "madevil.kk.ass");
            if (_pluginData != null && Extended_Data == null)
            {
                int CoordinateNum = (int)CurrentCoordinate.Value;
                Dictionary<int, int> Converted_Dictionary = new Dictionary<int, int>();
                Dictionary<int, List<int[]>> Converted_array = new Dictionary<int, List<int[]>>();
                Dictionary<int, string> Converted_Parent_Dictionary = new Dictionary<int, string>();

                List<AccStateSync.TriggerProperty> TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                List<AccStateSync.TriggerGroup> TriggerGroupList = new List<AccStateSync.TriggerGroup>();

                if (_pluginData.version > 6)
                    Settings.Logger.LogWarning($"New version of AccessoryStateSync found, accessory states needs update for compatibility");
                else if (_pluginData.version < 6)
                {
                    AccStateSync.Migration.ConvertOutfitPluginData(CoordinateNum, _pluginData, ref TriggerPropertyList, ref TriggerGroupList);
                }
                else
                {
                    if (_pluginData.data.TryGetValue("TriggerPropertyList", out object _loadedTriggerProperty) && _loadedTriggerProperty != null)
                    {
                        List<AccStateSync.TriggerProperty> _tempTriggerProperty = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>((byte[])_loadedTriggerProperty);
                        if (_tempTriggerProperty?.Count > 0)
                        {
                            _tempTriggerProperty.ForEach(x => x.Coordinate = CoordinateNum);
                            TriggerPropertyList.AddRange(_tempTriggerProperty);
                        }

                        if (_pluginData.data.TryGetValue("TriggerGroupList", out object _loadedTriggerGroup) && _loadedTriggerGroup != null)
                        {
                            List<AccStateSync.TriggerGroup> _tempTriggerGroup = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>((byte[])_loadedTriggerGroup);
                            if (_tempTriggerGroup?.Count > 0)
                            {
                                foreach (AccStateSync.TriggerGroup _group in _tempTriggerGroup)
                                {
                                    _group.Coordinate = CoordinateNum;

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

                NowCoordinate.Accstatesyncconvert(TriggerPropertyList, TriggerGroupList);
            }

            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            {
                Coordinate[(int)CurrentCoordinate.Value] = ThisCharactersData.NowCoordinate;
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

            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            {
                Coordinate[(int)CurrentCoordinate.Value] = ThisCharactersData.NowCoordinate;
            }

            Refresh();
        }

        private void SetClothesState_switch(int clothesKind, byte state)
        {
            switch (clothesKind)
            {
                case 0:
                    SetClothesState_switch_Case(ChaControl.notBot, clothesKind, state, 1, 3);
                    break;
                case 1:
                    SetClothesState_switch_Case(ChaControl.notBot, clothesKind, state, 0, 3);
                    break;
                case 2:
                    SetClothesState_switch_Case(ChaControl.notShorts, clothesKind, state, 3, 3);
                    break;
                case 3:
                    SetClothesState_switch_Case(ChaControl.notShorts, clothesKind, state, 2, 3);
                    break;
                case 7:
                    SetClothesState_switch_Case_2(state, 8);
                    break;
                case 8:
                    SetClothesState_switch_Case_2(state, 7);
                    break;
            }
        }

        private void SetClothesState_switch_Case(bool condition, int clothesKind, byte state, int value1, byte value2)
        {
            if (condition)
            {
                byte variable = 0;
                if (ChaControl.fileStatus.clothesState[clothesKind] == value2)
                {
                    variable = value2;
                }
                else if (ChaControl.fileStatus.clothesState[value1] == value2)
                {
                    variable = state;
                }
                else if (!(ChaControl.fileStatus.clothesState[value1] == 0 || ChaControl.fileStatus.clothesState[clothesKind] == 0))
                {
                    return;
                }
                ChangedOutfit(value1, variable);
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
            Update_More_Accessories();
            for (int i = 0; i < 8; i++)
            {
                ChangedOutfit(i, ChaControl.fileStatus.clothesState[i]);
            }

            foreach (var item in Names.Keys)
            {
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

        internal void Update_More_Accessories()
        {
            WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();
            if (_accessoriesByChar.TryGetValue(ChaFileControl, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
            }
            Accessorys_Parts.Clear();
            Accessorys_Parts.AddRange(ChaControl.nowCoordinate.accessory.parts);
            Accessorys_Parts.AddRange(data.nowAccessories);
        }

        public void Custom_Groups(int kind, int state)
        {
            var Accessory_list = Slotinfo.Where(x => x.Value.Binding == kind);
            foreach (var item in Accessory_list)
            {
                ChaControl.SetAccessoryState(item.Key, ShowState(state, item.Value.States, false));
            }
        }

        public void Parent_toggle(string parent, bool show)
        {
            var ParentedList = ThisCharactersData.Now_Parented_Name_Dictionary[parent];
            foreach (var slot in ParentedList)
            {
                ChaControl.SetAccessoryState(slot, show);
            }
        }

        protected override void Update()
        {
            if (Input.anyKeyDown)
            {
                if (Input.GetKeyDown(KeyCode.N))
                {
                    //foreach (var item in ThisCharactersData.ACC_Name_Dictionary[0])
                    //{
                    //    Settings.Logger.LogWarning($"kind {item.Key} is called {item.Value}");
                    //}
                    foreach (var item in Slotinfo)
                    {
                        Settings.Logger.LogWarning($"slot {item.Key} is part of kind {item.Value.Binding}");
                        Settings.Logger.LogWarning($"has the following states:");
                        if (item.Value.States.Count == 0)
                        {
                            Settings.Logger.LogWarning($"\tError no states assigned");
                        }
                        else
                        {
                            foreach (var item2 in item.Value.States)
                            {
                                Settings.Logger.LogWarning($"\t{item2[0]} to {item2[1]}");
                            }
                        }
                    }
                }
            }
            base.Update();
        }

        internal void ChangedCoord()
        {
            Refresh();
        }

        internal void SetClothesState(int clothesKind, byte state)
        {
            if (MakerAPI.InsideMaker && !MakerAPI.InsideAndLoaded)
            {
                return;
            }
            ChangedOutfit(clothesKind, state);
            SetClothesState_switch(clothesKind, state);
        }

        private static bool ShowState(int state, List<int[]> list, bool singlestate)
        {
            if (singlestate)
            {
                for (int i = 0, n = list.Count; i < n; i++)
                {
                    var single = list[i];
                    for (int j = 0, nn = single.Length; j < nn; j++)
                    {
                        if (single[j] != 0)
                        {
                            single[j] = 3;
                        }
                    }
                }
            }
            return list.Any(x => state >= x[0] && state <= x[1]);
        }

        private static int MaxState(List<int[]> list)
        {
            int max = 0;
            list.ForEach(x => max = Math.Max(x[1], max));
            return max;
        }

        private int MaxState(int slot, int binding)
        {
            if (binding <= Max_Defined_Key)
            {
                return 3;
            }
            int max = 0;
            var bindinglist = Coordinate[slot].Slotinfo.Values.Where(x => x.Binding == binding);
            foreach (var item in bindinglist)
            {
                item.States.ForEach(x => max = Math.Max(x[1], max));
            }
            return max;
        }

        private void ChangedOutfit(int clothesKind, byte state)
        {
            bool single = clothesKind > 3;
            byte shoetype = ChaControl.fileStatus.shoesType;
            var Accessory_list = Slotinfo.Where(x => x.Value.Binding == clothesKind && (x.Value.Shoetype == 2 || x.Value.Shoetype == shoetype));
            foreach (var item in Accessory_list)
            {
                ChaControl.SetAccessoryState(item.Key, ShowState(state, item.Value.States, single));
            }
        }

        private void Clearoutfit(int key)
        {
            ThisCharactersData.Clearoutfit(key);
        }

        private void Createoutfit(int key)
        {
            ThisCharactersData.Createoutfit(key);
        }

        private void Moveoutfit(int dest, int src)
        {
            ThisCharactersData.Moveoutfit(dest, src);
        }

        private void Removeoutfit(int key)
        {
            ThisCharactersData.Removeoutfit(key);
        }

    }
}
