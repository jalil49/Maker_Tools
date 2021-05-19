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

namespace Accessory_States
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        public Data ThisCharactersData;
        ChaFile chafile;
        public static event EventHandler<CoordinateLoadedEventARG> Coordloaded;
        public List<bool> Accessory_Show_Bools = new List<bool>();
        internal List<ChaFileAccessory.PartsInfo> Accessorys_Parts = new List<ChaFileAccessory.PartsInfo>();

        protected override void OnDestroy()
        {
            Constants.CharacterInfo.Remove(ThisCharactersData);
            base.OnDestroy();
        }

        private void Settings_ChangedCoord(object sender, ChangeCoordinateTypeARG e)
        {
            if (ChaControl.name != null && ChaControl.fileStatus != null && e.Character.name != null && e.Character.name == ChaControl.name)
            {
                Refresh();
            }
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            {
                StartCoroutine(WaitForSlots());
            }
        }

        private void Settings_SetClothesState(object sender, ClothChangeEventArgs e)
        {
            if (e.Character.name == ChaControl.name)
            {
                //Settings.Logger.LogWarning("coortype" + ChaControl.fileStatus.coordinateType);
                ChangedOutfit(e.clothesKind, e.state);
                SetClothesState_switch(e.clothesKind, e.state);
            }
        }

        private void ChangedOutfit(int clothesKind, byte state)
        {
            clothesKind = Math.Min(clothesKind, 7);

            var Accessory_list = ThisCharactersData.Now_ACC_Binding_Dictionary.Where(x => x.Value - 1 == clothesKind);
            foreach (var item in Accessory_list)
            {

                if (!ThisCharactersData.Now_ACC_State_array.TryGetValue(item.Key, out var data) || Accessorys_Parts.Count() <= item.Key)
                {
                    continue;
                }
                //if (item.Key < 20)
                //{
                ChaControl.SetAccessoryState(item.Key, state >= data[0] && state <= data[1]);
                //}
                //else
                //{
                //    Accessory_Show_Bools[item.Key - 20] = state >= data[0] && state <= data[1];
                //}
            }
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (currentGameMode == GameMode.Maker && !Settings.Enable.Value)
            {
                return;
            }
            ThisCharactersData = Constants.CharacterInfo.Find(x => ChaControl.fileParam.personality == x.Personality && x.FullName == ChaControl.fileParam.fullname && x.BirthDay == ChaControl.fileParam.strBirthDay);
            if (ThisCharactersData == null)
            {
                ThisCharactersData = new Data(ChaControl.fileParam.personality, ChaControl.fileParam.strBirthDay, ChaControl.fileParam.fullname, this);
                Constants.CharacterInfo.Add(ThisCharactersData);
            }
            if (!ThisCharactersData.processed || currentGameMode == GameMode.Maker || GameAPI.InsideHScene)
            {
                ThisCharactersData.Controller = this;
                chafile = ChaFileControl;
                if (currentGameMode == GameMode.Maker)
                {
                    chafile = MakerAPI.LastLoadedChaFile;
                }

                ThisCharactersData.Clear();

                ThisCharactersData.Clear_Now_Coordinate();

                Update_More_Accessories();

                var Extended_Data = GetExtendedData();
                if (Extended_Data != null)
                {
                    if (Extended_Data.data.TryGetValue("ACC_Binding_Dictionary", out var ByteData) && ByteData != null)
                    {
                        ThisCharactersData.ACC_Binding_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                    }
                    if (Extended_Data.data.TryGetValue("ACC_State_array", out ByteData) && ByteData != null)
                    {
                        ThisCharactersData.ACC_State_array = MessagePackSerializer.Deserialize<Dictionary<int, int[]>[]>((byte[])ByteData);
                    }
                    if (Extended_Data.data.TryGetValue("ACC_Name_Dictionary", out ByteData) && ByteData != null)
                    {
                        ThisCharactersData.ACC_Name_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, string>[]>((byte[])ByteData);
                    }
                    if (Extended_Data.data.TryGetValue("ACC_Parented_Dictionary", out ByteData) && ByteData != null)
                    {
                        ThisCharactersData.ACC_Parented_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, bool>[]>((byte[])ByteData);
                    }
                }

                Extended_Data = ExtendedSave.GetExtendedDataById(chafile, "madevil.kk.ass");
                if (Extended_Data != null)
                {
                    if (Extended_Data.data.TryGetValue("CharaTriggerInfo", out var loadedCharaTriggerInfo) && loadedCharaTriggerInfo != null)
                    {
                        int outfitsize = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length;
                        Dictionary<int, int>[] Converted_Dictionary = new Dictionary<int, int>[outfitsize];
                        Dictionary<int, int[]>[] Converted_array = new Dictionary<int, int[]>[outfitsize];
                        for (int i = 0; i < outfitsize; i++)
                        {
                            Converted_Dictionary[i] = new Dictionary<int, int>();
                            Converted_array[i] = new Dictionary<int, int[]>();
                        }
                        Dictionary<int, AccStateSync.OutfitTriggerInfo> CharaTriggerInfo = new Dictionary<int, AccStateSync.OutfitTriggerInfo>();
                        Dictionary<int, Dictionary<string, AccStateSync.VirtualGroupInfo>> CharaVirtualGroupInfo = new Dictionary<int, Dictionary<string, AccStateSync.VirtualGroupInfo>>();
                        if (Extended_Data.version < 2)
                        {
                            List<AccStateSync.OutfitTriggerInfoV1> OldCharaTriggerInfo = MessagePackSerializer.Deserialize<List<AccStateSync.OutfitTriggerInfoV1>>((byte[])loadedCharaTriggerInfo);
                            for (int i = 0; i < 7; i++)
                                CharaTriggerInfo[i] = AccStateSync.UpgradeOutfitTriggerInfoV1(OldCharaTriggerInfo[i]);
                        }
                        else
                            CharaTriggerInfo = MessagePackSerializer.Deserialize<Dictionary<int, AccStateSync.OutfitTriggerInfo>>((byte[])loadedCharaTriggerInfo);

                        if (Extended_Data.version < 5)
                        {
                            if (Extended_Data.data.TryGetValue("CharaVirtualGroupNames", out var loadedCharaVirtualGroupNames) && loadedCharaVirtualGroupNames != null)
                            {
                                if (Extended_Data.version < 2)
                                {
                                    var OldCharaVirtualGroupNames = MessagePackSerializer.Deserialize<List<Dictionary<string, string>>>((byte[])loadedCharaVirtualGroupNames);
                                    for (int i = 0; i < 7; i++)
                                    {
                                        Dictionary<string, string> VirtualGroupNames = AccStateSync.UpgradeVirtualGroupNamesV1(OldCharaVirtualGroupNames[i]);
                                        CharaVirtualGroupInfo[i] = AccStateSync.UpgradeVirtualGroupNamesV2(VirtualGroupNames);
                                    }
                                }
                                else
                                {
                                    Dictionary<int, Dictionary<string, string>> CharaVirtualGroupNames = MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<string, string>>>((byte[])loadedCharaVirtualGroupNames);
                                    for (int i = 0; i < 7; i++)
                                        CharaVirtualGroupInfo[i] = AccStateSync.UpgradeVirtualGroupNamesV2(CharaVirtualGroupNames[i]);
                                }
                            }
                        }
                        else
                        {
                            if (Extended_Data.data.TryGetValue("CharaVirtualGroupInfo", out var loadedCharaVirtualGroupInfo) && loadedCharaVirtualGroupInfo != null)
                                CharaVirtualGroupInfo = MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<string, AccStateSync.VirtualGroupInfo>>>((byte[])loadedCharaVirtualGroupInfo);
                        }
                        //foreach (var chara in CharaVirtualGroupInfo)
                        //{
                        //    foreach (var VGI in chara.Value)
                        //    {
                        //        Settings.Logger.LogWarning($"{(ChaFileDefine.ClothesKind)chara.Key}, VGI Key: {VGI.Key}, VGI group: {VGI.Value.Group}, VGI Kind: {VGI.Value.Kind}, VGI Label: {VGI.Value.Label}, VGI Secondary: {VGI.Value.Secondary}");
                        //    }
                        //}
                        for (int outfitnum = 0; outfitnum < 7; outfitnum++)
                        {
                            if (CharaVirtualGroupInfo.TryGetValue(outfitnum, out var dict) && ThisCharactersData.ACC_Name_Dictionary[outfitnum].Count == 0)
                            {
                                int offset = -1;

                                foreach (var item in dict)
                                {
                                    if (item.Value.Kind > 9)
                                    {
                                        ThisCharactersData.ACC_Name_Dictionary[outfitnum][item.Value.Kind + 5] = item.Key;
                                    }
                                    else if (item.Value.Kind == 9)
                                    {
                                        ThisCharactersData.ACC_Name_Dictionary[outfitnum][offset] = item.Key;
                                    }
                                    offset--;
                                }
                            }
                            //foreach (var item in ThisCharactersData.ACC_Name_Dictionary[outfitnum])
                            //{
                            //    Settings.Logger.LogWarning($"{(ChaFileDefine.ClothesKind)outfitnum} added {item.Value} with kind {item.Key}");
                            //}
                        }

                        //foreach (var OutfitTrigger in CharaTriggerInfo)
                        //{
                        //    Settings.Logger.LogWarning($"\nChara Trigger: {OutfitTrigger.Key}\t index: {OutfitTrigger.Value.Index}");
                        //    foreach (var ACC_trigger in OutfitTrigger.Value.Parts)
                        //    {
                        //        Settings.Logger.LogWarning($"\n\tACC Trigger: {ACC_trigger.Key}\t index: {ACC_trigger.Value} group: {ACC_trigger.Value.Group} slot: {ACC_trigger.Value.Slot} kind: {ACC_trigger.Value.Kind}");
                        //        for (int i = 0; i < ACC_trigger.Value.State.Count; i++)
                        //        {
                        //            Settings.Logger.LogWarning($"\n\t\t\t\t\t{ACC_trigger.Value.State[i]}");
                        //        }
                        //    }
                        //}

                        for (int outfitnum = 0; outfitnum < outfitsize; outfitnum++)
                        {

                            int offset = 0;
                            if (ThisCharactersData.ACC_Binding_Dictionary[outfitnum].Any(x => x.Value > 0))
                            {
                                continue;
                            }

                            var parts = CharaTriggerInfo[outfitnum].Parts;
                            for (int slot = 0; slot < parts.Count; slot++)
                            {
                                var Clotheskind = parts.ElementAt(slot).Value.Kind + 1;
                                if (Clotheskind == 9)
                                {
                                    Clotheskind = 8;
                                }
                                else if (Clotheskind == 10)
                                {
                                    Clotheskind += 5 + offset;
                                    var temp = ThisCharactersData.ACC_Name_Dictionary[outfitnum].First(x => x.Value == parts.ElementAt(slot).Value.Group);
                                    if (temp.Key < 0)
                                    {
                                        ThisCharactersData.ACC_Name_Dictionary[outfitnum][Clotheskind] = temp.Value;
                                        ThisCharactersData.ACC_Name_Dictionary[outfitnum].Remove(temp.Key);
                                        offset++;
                                    }
                                    else
                                    {
                                        Clotheskind = temp.Key;
                                    }
                                }
                                else if (Clotheskind > 10)
                                {
                                    Clotheskind += 4 + offset;
                                    offset++;
                                }
                                Converted_Dictionary[outfitnum][parts.ElementAt(slot).Value.Slot] = Clotheskind;
                                int indexstart = Math.Max(parts.ElementAt(slot).Value.State.IndexOf(true), 0);
                                int indexstop = Math.Max(parts.ElementAt(slot).Value.State.FindLastIndex(x => x), 0);
                                Converted_array[outfitnum][parts.ElementAt(slot).Value.Slot] = new int[] { indexstart, indexstop };
                            }
                            ThisCharactersData.ACC_State_array[outfitnum] = Converted_array[outfitnum];
                            ThisCharactersData.ACC_Binding_Dictionary[outfitnum] = Converted_Dictionary[outfitnum];
                        }
                    }
                }

                ThisCharactersData.Update_Now_Coordinate();
                CurrentCoordinate.Subscribe(x =>
                {
                    Refresh(); StartCoroutine(WaitForSlots());
                });
            }
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            if (!Settings.Enable.Value)
            {
                return;
            }

            PluginData MyData = new PluginData();
            MyData.data.Add("ACC_Binding_Dictionary", MessagePackSerializer.Serialize(ThisCharactersData.ACC_Binding_Dictionary));
            MyData.data.Add("ACC_State_array", MessagePackSerializer.Serialize(ThisCharactersData.ACC_State_array));
            MyData.data.Add("ACC_Name_Dictionary", MessagePackSerializer.Serialize(ThisCharactersData.ACC_Name_Dictionary));
            MyData.data.Add("ACC_Parented_Dictionary", MessagePackSerializer.Serialize(ThisCharactersData.ACC_Parented_Dictionary));
            SetExtendedData(MyData);
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            if (!Settings.Enable.Value)
            {
                return;
            }

            PluginData MyData = new PluginData();
            MyData.data.Add("ACC_Binding_Dictionary", MessagePackSerializer.Serialize(ThisCharactersData.Now_ACC_Binding_Dictionary));
            MyData.data.Add("ACC_State_array", MessagePackSerializer.Serialize(ThisCharactersData.Now_ACC_State_array));
            MyData.data.Add("ACC_Name_Dictionary", MessagePackSerializer.Serialize(ThisCharactersData.Now_ACC_Name_Dictionary));
            MyData.data.Add("ACC_Parented_Dictionary", MessagePackSerializer.Serialize(ThisCharactersData.Now_Parented_Dictionary));
            SetExtendedData(MyData);
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker && !Settings.Enable.Value)
            {
                return;
            }

            var MyData = GetCoordinateExtendedData(coordinate);
            ThisCharactersData.Clear_Now_Coordinate();
            if (MyData != null)
            {
                if (MyData.data.TryGetValue("ACC_Binding_Dictionary", out var ByteData) && ByteData != null)
                {
                    ThisCharactersData.Now_ACC_Binding_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("ACC_State_array", out ByteData) && ByteData != null)
                {
                    ThisCharactersData.Now_ACC_State_array = MessagePackSerializer.Deserialize<Dictionary<int, int[]>>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("ACC_Name_Dictionary", out ByteData) && ByteData != null)
                {
                    ThisCharactersData.Now_ACC_Name_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, string>>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("ACC_Parented_Dictionary", out ByteData) && ByteData != null)
                {
                    ThisCharactersData.Now_Parented_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, bool>>((byte[])ByteData);
                }
            }
            PluginData ExtendedData = ExtendedSave.GetExtendedDataById(coordinate, "madevil.kk.ass");
            if (ExtendedData != null && ExtendedData.data.TryGetValue("OutfitTriggerInfo", out var loadedOutfitTriggerInfo) && loadedOutfitTriggerInfo != null && MyData == null)
            {
                int CoordinateNum = (int)CurrentCoordinate.Value;
                Dictionary<int, int> Converted_Dictionary = new Dictionary<int, int>();
                Dictionary<int, int[]> Converted_array = new Dictionary<int, int[]>();
                Dictionary<int, string> Converted_Parent_Dictionary = new Dictionary<int, string>();

                Dictionary<int, AccStateSync.OutfitTriggerInfo> CharaTriggerInfo = new Dictionary<int, AccStateSync.OutfitTriggerInfo>();
                Dictionary<int, Dictionary<string, AccStateSync.VirtualGroupInfo>> CharaVirtualGroupInfo = new Dictionary<int, Dictionary<string, AccStateSync.VirtualGroupInfo>>();
                {
                    //copied code
                    if (ExtendedData.version < 2)
                    {
                        AccStateSync.OutfitTriggerInfoV1 OldCharaTriggerInfo = MessagePackSerializer.Deserialize<AccStateSync.OutfitTriggerInfoV1>((byte[])loadedOutfitTriggerInfo);
                        CharaTriggerInfo[CoordinateNum] = AccStateSync.UpgradeOutfitTriggerInfoV1(OldCharaTriggerInfo);
                    }
                    else
                        CharaTriggerInfo[CoordinateNum] = MessagePackSerializer.Deserialize<AccStateSync.OutfitTriggerInfo>((byte[])loadedOutfitTriggerInfo);

                    if (ExtendedData.version < 5)
                    {
                        if (ExtendedData.data.TryGetValue("OutfitVirtualGroupNames", out var loadedOutfitVirtualGroupNames) && loadedOutfitVirtualGroupNames != null)
                        {
                            Dictionary<string, string> OutfitVirtualGroupNames = MessagePackSerializer.Deserialize<Dictionary<string, string>>((byte[])loadedOutfitVirtualGroupNames);
                            CharaVirtualGroupInfo[CoordinateNum] = AccStateSync.UpgradeVirtualGroupNamesV2(OutfitVirtualGroupNames);
                        }
                    }
                    else
                    {
                        if (ExtendedData.data.TryGetValue("OutfitVirtualGroupInfo", out var loadedOutfitVirtualGroupInfo) && loadedOutfitVirtualGroupInfo != null)
                            CharaVirtualGroupInfo[CoordinateNum] = MessagePackSerializer.Deserialize<Dictionary<string, AccStateSync.VirtualGroupInfo>>((byte[])loadedOutfitVirtualGroupInfo);
                    }
                }
                var accessoryparents = new List<string>();
                int accessorystart = 10;
                if (CharaVirtualGroupInfo.TryGetValue(CoordinateNum, out var dict))
                {
                    foreach (var item in dict)
                    {
                        if (item.Value.Kind != 9)
                        {
                            ThisCharactersData.Now_ACC_Name_Dictionary[item.Value.Kind] = item.Key;
                        }
                        else
                        {
                            accessoryparents.Add(item.Key);
                        }
                    }

                    accessorystart = ThisCharactersData.Now_ACC_Name_Dictionary.Count + 10;
                    int offset = 0;
                    foreach (var item in accessoryparents)
                    {
                        ThisCharactersData.Now_ACC_Name_Dictionary[accessorystart + (offset++)] = item;
                    }
                    //foreach (var item in ThisCharactersData.Now_ACC_Name_Dictionary)
                    //{
                    //    Settings.Logger.LogWarning($"{(ChaFileDefine.ClothesKind)CoordinateNum} added {item.Value} with kind {item.Key}");
                    //}
                }

                var parts = CharaTriggerInfo[CoordinateNum].Parts;
                for (int slot = 0; slot < parts.Count; slot++)
                {
                    var Clotheskind = parts.ElementAt(slot).Value.Kind + 1;
                    if (Clotheskind == 9)
                    {
                        Clotheskind = 8;
                    }
                    else if (Clotheskind == 10)
                    {
                        var result = parts.ElementAt(slot).Value.Group;
                        Clotheskind = accessoryparents.IndexOf(result) + accessorystart + 1;
                    }
                    Converted_Dictionary[parts.ElementAt(slot).Value.Slot] = Clotheskind;
                    int indexstart = Math.Max(parts.ElementAt(slot).Value.State.IndexOf(true), 0);
                    int indexstop = Math.Max(parts.ElementAt(slot).Value.State.FindLastIndex(x => x), 0);
                    Converted_array[parts.ElementAt(slot).Value.Slot] = new int[] { indexstart, indexstop };
                }
                ThisCharactersData.Now_ACC_State_array = Converted_array;
                ThisCharactersData.Now_ACC_Binding_Dictionary = Converted_Dictionary;
                //ThisCharactersData.Now_Parented_Dictionary = Converted_Parent_Dictionary;
                var chara = CharaVirtualGroupInfo[CoordinateNum];
                //foreach (var VGI in CharaTriggerInfo[CoordinateNum].Parts)
                //{
                //    Settings.Logger.LogWarning($"CTI Key: {VGI.Key}, CTI group: {VGI.Value.Group}, CTI Kind: {VGI.Value.Kind} Slot: {VGI.Value.Slot}");
                //    foreach (var item in VGI.Value.State)
                //    {
                //        Settings.Logger.LogWarning($"CTI state: {item}");
                //    }
                //}
                //foreach (var VGI in chara)
                //{
                //    Settings.Logger.LogWarning($"{(ChaFileDefine.ClothesKind)CoordinateNum}, VGI Key: {VGI.Key}, VGI group: {VGI.Value.Group}, VGI Kind: {VGI.Value.Kind}, VGI Label: {VGI.Value.Label}, VGI Secondary: {VGI.Value.Secondary}");
                //}


                //call event
                var args = new CoordinateLoadedEventARG(ChaControl, coordinate);
                if (Coordloaded == null || Coordloaded.GetInvocationList().Length == 0)
                {
                    return;
                }
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
                int outfitnum = (int)CurrentCoordinate.Value;
                ThisCharactersData.ACC_Binding_Dictionary[outfitnum] = ThisCharactersData.Now_ACC_Binding_Dictionary;
                ThisCharactersData.ACC_State_array[outfitnum] = ThisCharactersData.Now_ACC_State_array;
                ThisCharactersData.ACC_Name_Dictionary[outfitnum] = ThisCharactersData.Now_ACC_Name_Dictionary;
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

                if (ChaControl.fileStatus.clothesState[clothesKind] == value2)
                {
                    ChangedOutfit(value1, value2);
                }
                else if (ChaControl.fileStatus.clothesState[value1] == value2)
                {
                    ChangedOutfit(value1, state);
                }
                else if (ChaControl.fileStatus.clothesState[clothesKind] == 0)
                {
                    ChangedOutfit(value1, 0);
                }
                else if (ChaControl.fileStatus.clothesState[value1] == 0)
                {
                    ChangedOutfit(value1, 0);
                }
            }
        }

        public void Refresh()
        {
            Update_More_Accessories();
            for (int i = 0; i < 9; i++)
            {
                ChangedOutfit(i, ChaControl.fileStatus.clothesState[i]);
            }
            foreach (var item in ThisCharactersData.Now_ACC_Name_Dictionary.Keys)
            {
                Custom_Groups(item, 0);
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

        public void Reset()
        {
            for (int i = 0, n = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length; i < n; i++)
            {
                ThisCharactersData.ACC_Binding_Dictionary[i].Clear();
                ThisCharactersData.ACC_State_array[i].Clear();
            }
        }

        private void Update_More_Accessories()
        {
            WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();
            if (_accessoriesByChar.TryGetValue(ChaFileControl, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
            }
            Accessory_Show_Bools = data.showAccessories;

            Accessorys_Parts = ChaControl.nowCoordinate.accessory.parts.ToList();
            Accessorys_Parts.AddRange(data.nowAccessories);
        }

        public void Custom_Groups(int kind, int state)
        {
            var Accessory_list = ThisCharactersData.Now_ACC_Binding_Dictionary.Where(x => x.Value == kind);
            foreach (var item in Accessory_list)
            {
                if (!ThisCharactersData.Now_ACC_State_array.TryGetValue(item.Key, out var data))
                {
                    continue;
                }
                ChaControl.SetAccessoryState(item.Key, state >= data[0] && state <= data[1]);
                //if (item.Key < 20)
                //{
                //    ChaControl.SetAccessoryState(item.Key, !ChaControl.fileStatus.showAccessory[item.Key]);
                //}
                //else
                //{
                //    Accessory_Show_Bools[item.Key - 20] = !Accessory_Show_Bools[item.Key - 20];
                //}
            }
        }

        //public void Parent_toggle(string parent)
        //{
        //    var ParentedList = ThisCharactersData.Now_Parented_Name_Dictionary.Where(x => x.Value == parent);
        //    foreach (var item in ParentedList)
        //    {
        //        if (item.Key < 20)
        //        {
        //            ChaControl.SetAccessoryState(item.Key, !ChaControl.fileStatus.showAccessory[item.Key]);
        //        }
        //        else
        //        {
        //            Accessory_Show_Bools[item.Key - 20] = !Accessory_Show_Bools[item.Key - 20];
        //        }
        //    }
        //}

        public void Parent_toggle(string parent, bool show)
        {
            var ParentedList = ThisCharactersData.Now_Parented_Name_Dictionary.Where(x => x.Value == parent);
            foreach (var item in ParentedList)
            {
                if (item.Key < 20)
                {
                    ChaControl.SetAccessoryState(item.Key, show);
                }
                else
                {
                    Accessory_Show_Bools[item.Key - 20] = show;
                }
            }
        }

        //protected override void Update()
        //{
        //    if (Input.anyKeyDown)
        //    {
        //        if (Input.GetKeyDown(KeyCode.N))
        //        {
        //            foreach (var item in ThisCharactersData.ACC_Name_Dictionary[0])
        //            {
        //                Settings.Logger.LogWarning($"kind {item.Key} is called {item.Value}");
        //            }
        //            foreach (var item in ThisCharactersData.ACC_Binding_Dictionary[0])
        //            {
        //                Settings.Logger.LogWarning($"slot {item.Key} is part of kind {item.Value}");
        //            }
        //        }
        //    }
        //    base.Update();
        //}

        public void Register()
        {
            Hooks.SetClothesState += Settings_SetClothesState;
            Hooks.ChangedCoord += Settings_ChangedCoord;
        }

        public void DeRegister()
        {
            Hooks.SetClothesState -= Settings_SetClothesState;
            Hooks.ChangedCoord -= Settings_ChangedCoord;
        }
    }
}
