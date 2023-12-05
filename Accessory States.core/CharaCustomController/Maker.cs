using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Accessory_States
{
    partial class CharaEvent : CharaCustomFunctionController
    {
        private static AccessoryControlWrapper<MakerDropdown, int> _accAppearanceDropdown;
        private static AccessoryControlWrapper<MakerSlider, float> _accAppearanceState;
        private static AccessoryControlWrapper<MakerSlider, float> _accAppearanceState2;

        private static readonly int MaxDefinedKey = Constants.ConstantOutfitNames.Keys.Max();

        private static MakerButton _guiButton;

        private static bool _makerEnabled;

        private static CharaEvent ControllerGet => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        public static void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            _makerEnabled = Settings.Enable.Value;
            if (!_makerEnabled) return;

            var owner = Settings.Instance;
            var category = new MakerCategory(null, null);
            //var category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 3, "Accessory Settings");

            var accAppDropdown = Constants.ConstantOutfitNames.Values.ToArray();
            //string[] ACC_app_Dropdown = new string[] { "None", "Top", "Bottom", "Bra", "Panty", "Gloves", "Pantyhose", "Socks", "Shoes" };

            _accAppearanceDropdown =
                MakerAPI.AddEditableAccessoryWindowControl<MakerDropdown, int>(
                    new MakerDropdown("Clothing\nBind", accAppDropdown, category, 0, owner), true);
            _accAppearanceDropdown.ValueChanged += (s, es) =>
                ControllerGet.ACC_Appearance_dropdown_ValueChanged(es.SlotIndex, es.NewValue - 1);

            var startSlider = new MakerSlider(category, "Start", 0, 3, 0, owner)
            {
                WholeNumbers = true
            };
            _accAppearanceState = MakerAPI.AddEditableAccessoryWindowControl<MakerSlider, float>(startSlider, true);
            _accAppearanceState.ValueChanged += (s, es) =>
                ControllerGet.ACC_Appearance_state_ValueChanged(es.SlotIndex, Mathf.RoundToInt(es.NewValue), 0);

            var stopSlider = new MakerSlider(category, "Stop", 0, 3, 3, owner)
            {
                WholeNumbers = true
            };
            _accAppearanceState2 = MakerAPI.AddEditableAccessoryWindowControl<MakerSlider, float>(stopSlider, true);
            _accAppearanceState2.ValueChanged += (s, es) =>
                ControllerGet.ACC_Appearance_state2_ValueChanged(es.SlotIndex, Mathf.RoundToInt(es.NewValue), 0);

            _guiButton = new MakerButton("States GUI", category, owner);
            MakerAPI.AddAccessoryWindowControl(_guiButton, true);
            _guiButton.OnClick.AddListener(delegate { GUI_Toggle(); });

            SetupInterface();

            var interfacebutton = e.AddSidebarControl(new SidebarToggle("ACC States", true, owner));
            interfacebutton.ValueChanged.Subscribe(x => _showToggleInterface = x);

            var groupingID = "Maker_Tools_" + Settings.NamingID.Value;
            _accAppearanceDropdown.Control.GroupingID = groupingID;
            _accAppearanceState.Control.GroupingID = groupingID;
            _accAppearanceState2.Control.GroupingID = groupingID;
            _guiButton.GroupingID = groupingID;
        }

        internal void RemoveOutfitEvent()
        {
            var max = _coordinate.Keys.Max();
            if (max <= 6)
                return;
            RemoveOutfit(max);
        }

        internal void AddOutfitEvent()
        {
            CreateOutfit(_coordinate.Count);
        }

        public static void Maker_started()
        {
            _makerEnabled = Settings.Enable.Value;
            if (!_makerEnabled) return;
            MakerAPI.MakerExiting += (s, e) => Maker_Ended();

            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;

            AccessoriesApi.MakerAccSlotAdded += AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;
        }

        public static void Maker_Ended()
        {
            MakerAPI.MakerExiting -= (s, e) => Maker_Ended();

            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;

            AccessoriesApi.MakerAccSlotAdded -= AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;

            _showCustomGui = false;
            _makerEnabled = false;
            _showToggleInterface = false;
        }

        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            ControllerGet.AccessoriesTransferred(e);
        }

        private static void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            ControllerGet.AccessoriesCopied(e);
        }

        private void AccessoriesTransferred(AccessoryTransferEventArgs e)
        {
            _accAppearanceDropdown.SetValue(e.DestinationSlotIndex, _accAppearanceDropdown.GetValue(e.SourceSlotIndex),
                false);
            _accAppearanceState.SetValue(e.DestinationSlotIndex, _accAppearanceState.GetValue(e.SourceSlotIndex),
                false);
            _accAppearanceState2.SetValue(e.DestinationSlotIndex, _accAppearanceState2.GetValue(e.SourceSlotIndex),
                false);

            var slotInfo = SlotInfo;

            if (slotInfo.ContainsKey(e.SourceSlotIndex))
                slotInfo[e.DestinationSlotIndex] = new SlotData(slotInfo[e.SourceSlotIndex]);
            else
                slotInfo.Remove(e.DestinationSlotIndex);
            Update_Parented_Name();
        }

        private void AccessoriesCopied(AccessoryCopyEventArgs e)
        {
            var source = _coordinate[(int)e.CopySource];
            var dest = _coordinate[(int)e.CopySource];
            var convertednames = new Dictionary<int, int>();
            foreach (var slot in e.CopiedSlotIndexes)
            {
                if (source.SlotInfo.TryGetValue(slot, out var slotInfo))
                {
                    dest.SlotInfo[slot] = new SlotData(slotInfo);
                }
                else
                {
                    dest.SlotInfo.Remove(slot);
                    continue;
                }

                var binding = slotInfo.Binding;
                if (binding > 8 && source.Names.TryGetValue(binding, out var custom))
                {
                    if (dest.Names.ContainsKey(binding))
                    {
                        if (!convertednames.TryGetValue(binding, out var newvalue))
                        {
                            newvalue = 9;
                            while (dest.Names.ContainsKey(newvalue)) newvalue++;
                            convertednames[binding] = newvalue;
                        }

                        binding = newvalue;
                    }

                    dest.Names[binding] = custom;
                }
            }

            Update_Parented_Name();
            Refresh();
        }

        private static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            var controller = ControllerGet;
            controller.StartCoroutine(controller.WaitForSlots());
        }

        private static void AccessoriesApi_MakerAccSlotAdded(object sender, AccessorySlotEventArgs e)
        {
            var controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

            controller.StartCoroutine(Wait());

            IEnumerator Wait()
            {
                yield return null;
            }
        }

        private void ACC_Is_Parented_ValueChanged(int slot, bool isparented)
        {
            if (!SlotInfo.TryGetValue(slot, out var slotdata)) slotdata = SlotInfo[slot] = new SlotData();
            slotdata.parented = isparented;

            Update_Parented_Name();

            if (!GUIParentDict.TryGetValue(Parts[slot].parentKey, out var show) || !isparented) show = true;
            ChaControl.SetAccessoryState(slot, show);
        }

        private void ACC_Appearance_state_ValueChanged(int slot, int newvalue, int index)
        {
            if (!SlotInfo.TryGetValue(slot, out var data))
                data = SlotInfo[slot] = new SlotData { States = new List<int[]> { new[] { newvalue, 3 } } };

            data.States[index][0] = newvalue;

            if (data.Binding < 0) return;

            UpdateAccessoryshow(data, slot);
        }

        private void ACC_Appearance_state2_ValueChanged(int slot, int newvalue, int index)
        {
            if (!SlotInfo.TryGetValue(slot, out var data))
                data = SlotInfo[slot] = new SlotData { States = new List<int[]> { new[] { 0, newvalue } } };

            data.States[index][1] = newvalue;

            var slider = _accAppearanceState.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>();
            var slider2 = _accAppearanceState2.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>();
            if (_autoscale && _accAppearanceDropdown.GetValue(slot) >= Constants.ConstantOutfitNames.Count)
            {
                Update_Toggles_GUI();
                if (Mathf.RoundToInt(slider2.value) == Mathf.RoundToInt(slider2.maxValue))
                {
                    slider.maxValue += 1;
                    slider2.maxValue += 1;
                }
            }

            if (data.Binding < 0) return;
            UpdateAccessoryshow(data, slot);
        }

        private void UpdateAccessoryshow(SlotData data, int slot)
        {
            int state;
            var binding = data.Binding;
            if (binding < 9)
                state = ChaControl.fileStatus.clothesState[binding];
            else
                state = GUICustomDict[slot][0];

            var show = ShowState(state, data.States);
            if (data.ShoeType != 2 && ChaControl.fileStatus.shoesType != data.ShoeType) show = false;
            ChaControl.SetAccessoryState(slot, show);
        }

        private void ACC_Appearance_dropdown_ValueChanged(int slot, int newvalue)
        {
            var delete = newvalue < 0;
            var kind = 0;
            if (!delete)
            {
                if (newvalue <= MaxDefinedKey)
                    kind = SlotInfo[slot].Binding = newvalue;
                else
                    kind = SlotInfo[slot].Binding = Names.ElementAt(newvalue - MaxDefinedKey - 1).Key;
            }

            if (delete)
            {
                //do nothing
            }
            else if (newvalue < 9)
            {
                _accAppearanceState.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue =
                    3;
                _accAppearanceState2.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue =
                    3;
                _accAppearanceState.SetValue(slot, 0);
                _accAppearanceState2.SetValue(slot, 3);
            }
            else
            {
                _accAppearanceState.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue =
                    _minilimit;
                _accAppearanceState2.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue =
                    _minilimit;
                _accAppearanceState.SetValue(slot, 0);
                _accAppearanceState2.SetValue(slot, _minilimit);
            }

            if (!AssExists) return;

            if (delete)
                DeleteGroup(kind);
            else
                ChangeTriggerProperty(kind);
        }

        private void Update_Drop_boxes()
        {
            var accAppearanceDropdownControls = _accAppearanceDropdown.Control.ControlObjects.ToList();
            var appearanceOptions =
                new List<TMP_Dropdown.OptionData>(accAppearanceDropdownControls[0]
                    .GetComponentInChildren<TMP_Dropdown>().options);
            var optioncount = Constants.ConstantOutfitNames.Count;
            var namedict = Names;
            var n = namedict.Count;

            if (appearanceOptions.Count > optioncount)
                appearanceOptions.RemoveRange(optioncount, appearanceOptions.Count - optioncount);

            foreach (var item in namedict)
            {
                var temp = new TMP_Dropdown.OptionData(item.Value.Name);
                appearanceOptions.Add(temp);
            }

            foreach (var item in _accAppearanceDropdown.Control.ControlObjects)
            {
                item.GetComponentInChildren<TMP_Dropdown>().ClearOptions();
                item.GetComponentInChildren<TMP_Dropdown>().AddOptions(appearanceOptions);
            }

            foreach (var item in SlotInfo)
                if (item.Value.Binding < Constants.ConstantOutfitNames.Count)
                {
                    _accAppearanceDropdown.SetValue(item.Key, item.Value.Binding + 1, false);
                }
                else
                {
                    var index = 0;
                    for (; index < n; index++)
                        if (namedict.ElementAt(index).Key == item.Value.Binding)
                            break;
                    _accAppearanceDropdown.SetValue(item.Key, index + Constants.ConstantOutfitNames.Count, false);
                }
        }

        private IEnumerator WaitForSlots()
        {
            if (!MakerAPI.InsideMaker) yield break;

            var accData = Parts.Length;
            do
            {
                yield return null;
            } while (!MakerAPI.InsideAndLoaded || _accAppearanceState.Control.ControlObjects.Count() < accData);

            UpdateNowCoordinate();
            Refresh();

            Update_Drop_boxes();
            var slider1 = _accAppearanceState.Control.ControlObjects;
            var slider2 = _accAppearanceState2.Control.ControlObjects;
            var slotInfo = SlotInfo;
            for (var i = 0; i < accData; i++)
            {
                if (!slotInfo.TryGetValue(i, out var slotdata))
                {
                    slotdata = new SlotData();
                    slotInfo[i] = slotdata;
                }

                var binding = slotdata.Binding;
                var max = MaxState(binding);
                var zerostate = slotdata.States[0];
                slider1.ElementAt(i).GetComponentInChildren<Slider>().maxValue = Math.Max(3, max);

                _accAppearanceState.SetValue(i, zerostate[0], false);

                slider2.ElementAt(i).GetComponentInChildren<Slider>().maxValue = Math.Max(3, max);

                _accAppearanceState2.SetValue(i, zerostate[1], false);

                if (binding == -1)
                    binding = 0;
                else if (binding < 9)
                    binding += 1;
                else
                    binding = 10 + IndexOfName(binding);
                _accAppearanceDropdown.SetValue(i, binding, false);
            }

            UpdateClothingNots();
            Update_Toggles_GUI();
        }

        internal void MovIt(List<QueueItem> queue)
        {
            var slotInfo = SlotInfo;
            foreach (var item in queue)
            {
                Settings.Logger.LogDebug($"Moving Acc from slot {item.SrcSlot} to {item.DstSlot}");

                var dropdown = 0;
                var state1 = 0;
                var state2 = 3;
                if (slotInfo.TryGetValue(item.SrcSlot, out var slotdata))
                {
                    var binding = slotdata.Binding;
                    var states = slotdata.States;

                    dropdown = binding > 8 ? 10 + IndexOfName(binding) : binding + 1;
                    state1 = states[0][0];
                    state2 = states[0][1];
                    slotInfo[item.DstSlot] = new SlotData(slotdata);
                }
                else
                {
                    slotInfo.Remove(item.DstSlot);
                }

                slotInfo.Remove(item.SrcSlot);

                _accAppearanceDropdown.SetValue(item.DstSlot, dropdown, false);
                _accAppearanceState.SetValue(item.DstSlot, state1, false);
                _accAppearanceState2.SetValue(item.DstSlot, state2, false);


                _accAppearanceDropdown.SetValue(item.SrcSlot, 0, false);
                _accAppearanceState.SetValue(item.SrcSlot, 0, false);
                _accAppearanceState2.SetValue(item.SrcSlot, 3, false);
            }
        }

        internal void Slot_ACC_Change(int slotNo, int type)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker) return;
            if (type == 120)
            {
                SlotInfo.Remove(slotNo);
                _accAppearanceDropdown.SetValue(slotNo, 0, false);
                _accAppearanceState.SetValue(slotNo, 0, false);
                _accAppearanceState2.SetValue(slotNo, 3, false);
            }
        }

        private int IndexOfName(int binding)
        {
            var namedict = Names;
            for (int i = 0, n = namedict.Count; i < n; i++)
                if (namedict.ElementAt(i).Key == binding)
                    return i;
            return 0;
        }

        internal void UpdateClothingNots()
        {
            var clothingnot = ClothNotData;
            for (int i = 0, n = clothingnot.Length; i < n; i++) clothingnot[i] = false;

            var clothinfo = ChaControl.infoClothes;

            UpdateTopClothingNots(clothinfo[0], ref clothingnot);
            UpdateBraClothingNots(clothinfo[2], ref clothingnot);

            ForceClothDataUpdate = false;
        }

        private void UpdateTopClothingNots(ListInfoBase infoBase, ref bool[] clothingnot)
        {
            if (infoBase == null) return;
            Hooks.ClothingNotPatch.IsShortsCheck = false;
            var listInfoResult = Hooks.ClothingNotPatch.ListInfoResult;

            var key = ChaListDefine.KeyType.Coordinate;
            infoBase.GetInfo(key);
            var notbot = listInfoResult[key] == 2; //only in ChangeClothesTopAsync

            key = ChaListDefine.KeyType.NotBra;
            infoBase.GetInfo(key);
            var notbra = listInfoResult[key] == 1; //only in ChangeClothesTopAsync

            clothingnot[0] = clothingnot[0] || notbot;
            clothingnot[1] = clothingnot[1] || notbra;
        }

        private void UpdateBraClothingNots(ListInfoBase infoBase, ref bool[] clothingnot)
        {
            if (infoBase == null) return;
            Hooks.ClothingNotPatch.IsShortsCheck = true;

            var listInfoResult = Hooks.ClothingNotPatch.ListInfoResult;
            var key = ChaListDefine.KeyType.Coordinate;

            infoBase.GetInfo(key); //kk uses coordinate to hide shorts

            var notShorts = listInfoResult[key] == 2; //only in ChangeClothesBraAsync

            clothingnot[2] = clothingnot[2] || notShorts;
        }

        internal void ClothingTypeChange(int kind, int index)
        {
            if (index != 0)
                return;
            switch (kind)
            {
                case 1:
                    if (ClothNotData[0])
                        return;
                    break;
                case 2:
                    if (ClothNotData[1])
                        return;
                    break;
                case 3:
                    if (ClothNotData[2])
                        return;
                    break;
                case 7:
                    if (ChaControl.nowCoordinate.clothes.parts[8].id == 0) RemoveClothingBinding(9);
                    break;
                case 8:
                    if (ChaControl.nowCoordinate.clothes.parts[7].id == 0) RemoveClothingBinding(9);
                    break;
            }

            RemoveClothingBinding(kind);
        }

        private void RemoveClothingBinding(int kind)
        {
            var slotInfo = SlotInfo;
            var removelist = slotInfo.Where(x => x.Value.Binding == kind).ToList();
            for (int i = 0, n = removelist.Count; i < n; i++) slotInfo.Remove(removelist[i].Key);
        }
    }
}