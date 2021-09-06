using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Accessory_States
{
    partial class CharaEvent : CharaCustomFunctionController
    {
        static AccessoryControlWrapper<MakerDropdown, int> ACC_Appearance_dropdown;
        static AccessoryControlWrapper<MakerSlider, float> ACC_Appearance_state;
        static AccessoryControlWrapper<MakerSlider, float> ACC_Appearance_state2;

        private static readonly int Max_Defined_Key = Constants.ConstantOutfitNames.Keys.Max();

        static MakerButton gui_button;

        static bool MakerEnabled = false;

        static CharaEvent ControllerGet => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        public static void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            MakerEnabled = Settings.Enable.Value;
            if (!MakerEnabled)
            {
                return;
            }

            var owner = Settings.Instance;
            var category = new MakerCategory(null, null);
            //var category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 3, "Accessory Settings");

            var ACC_app_Dropdown = Constants.ConstantOutfitNames.Values.ToArray();
            //string[] ACC_app_Dropdown = new string[] { "None", "Top", "Bottom", "Bra", "Panty", "Gloves", "Pantyhose", "Socks", "Shoes" };

            ACC_Appearance_dropdown = MakerAPI.AddEditableAccessoryWindowControl<MakerDropdown, int>(new MakerDropdown("Clothing\nBind", ACC_app_Dropdown, category, 0, owner), true);
            ACC_Appearance_dropdown.ValueChanged += (s, es) => ControllerGet.ACC_Appearance_dropdown_ValueChanged(es.SlotIndex, es.NewValue - 1);

            var Start_Slider = new MakerSlider(category, "Start", 0, 3, 0, owner)
            {
                WholeNumbers = true
            };
            ACC_Appearance_state = MakerAPI.AddEditableAccessoryWindowControl<MakerSlider, float>(Start_Slider, true);
            ACC_Appearance_state.ValueChanged += (s, es) => ControllerGet.ACC_Appearance_state_ValueChanged(es.SlotIndex, Mathf.RoundToInt(es.NewValue), 0);

            var Stop_Slider = new MakerSlider(category, "Stop", 0, 3, 3, owner)
            {
                WholeNumbers = true
            };
            ACC_Appearance_state2 = MakerAPI.AddEditableAccessoryWindowControl<MakerSlider, float>(Stop_Slider, true);
            ACC_Appearance_state2.ValueChanged += (s, es) => ControllerGet.ACC_Appearance_state2_ValueChanged(es.SlotIndex, Mathf.RoundToInt(es.NewValue), 0);

            gui_button = new MakerButton("States GUI", category, owner);
            MakerAPI.AddAccessoryWindowControl(gui_button, true);
            gui_button.OnClick.AddListener(delegate () { GUI_Toggle(); });

            SetupInterface();

            var interfacebutton = e.AddSidebarControl(new SidebarToggle("ACC States", true, owner));
            interfacebutton.ValueChanged.Subscribe(x => ShowToggleInterface = x);

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            ACC_Appearance_dropdown.Control.GroupingID = GroupingID;
            ACC_Appearance_state.Control.GroupingID = GroupingID;
            ACC_Appearance_state2.Control.GroupingID = GroupingID;
            gui_button.GroupingID = GroupingID;
        }

        internal void RemoveOutfitEvent()
        {
            var max = Coordinate.Keys.Max();
            if (max <= 6)
                return;
            Removeoutfit(max);
        }

        internal void AddOutfitEvent()
        {
            Createoutfit(Coordinate.Count);
        }

        public static void Maker_started()
        {
            MakerEnabled = Settings.Enable.Value;
            if (!MakerEnabled)
            {
                return;
            }
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

            ShowCustomGui = false;
            MakerEnabled = false;
            ShowToggleInterface = false;
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
            ACC_Appearance_dropdown.SetValue(e.DestinationSlotIndex, ACC_Appearance_dropdown.GetValue(e.SourceSlotIndex), false);
            ACC_Appearance_state.SetValue(e.DestinationSlotIndex, ACC_Appearance_state.GetValue(e.SourceSlotIndex), false);
            ACC_Appearance_state2.SetValue(e.DestinationSlotIndex, ACC_Appearance_state2.GetValue(e.SourceSlotIndex), false);

            var slotinfo = Slotinfo;

            if (slotinfo.ContainsKey(e.SourceSlotIndex))
            {
                slotinfo[e.DestinationSlotIndex] = new Slotdata(slotinfo[e.SourceSlotIndex]);
            }
            else
            {
                slotinfo.Remove(e.DestinationSlotIndex);
            }
            Update_Parented_Name();
        }

        private void AccessoriesCopied(AccessoryCopyEventArgs e)
        {
            var source = Coordinate[(int)e.CopySource];
            var dest = Coordinate[(int)e.CopySource];
            var convertednames = new Dictionary<int, int>();
            foreach (var slot in e.CopiedSlotIndexes)
            {

                if (source.Slotinfo.TryGetValue(slot, out var slotinfo))
                {
                    dest.Slotinfo[slot] = new Slotdata(slotinfo);
                }
                else
                {
                    dest.Slotinfo.Remove(slot);
                    continue;
                }
                var binding = slotinfo.Binding;
                if (binding > 8 && source.Names.TryGetValue(binding, out var custom))
                {
                    if (dest.Names.ContainsKey(binding))
                    {
                        if (!convertednames.TryGetValue(binding, out var newvalue))
                        {
                            newvalue = 9;
                            while (dest.Names.ContainsKey(newvalue))
                            {
                                newvalue++;
                            }
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
            var Controller = ControllerGet;
            Controller.StartCoroutine(Controller.WaitForSlots());
        }

        private static void AccessoriesApi_MakerAccSlotAdded(object sender, AccessorySlotEventArgs e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

            Controller.StartCoroutine(Wait());
            IEnumerator Wait()
            {
                yield return null;
            }
        }

        private void ACC_Is_Parented_ValueChanged(int slot, bool isparented)
        {
            if (!Slotinfo.TryGetValue(slot, out var slotdata))
            {
                slotdata = Slotinfo[slot] = new Slotdata();
            }
            slotdata.Parented = isparented;

            Update_Parented_Name();

            if (!GUI_Parent_Dict.TryGetValue(Parts[slot].parentKey, out var show) || !isparented)
            {
                show = true;
            }
            ChaControl.SetAccessoryState(slot, show);
        }

        private void ACC_Appearance_state_ValueChanged(int slot, int newvalue, int index)
        {
            if (!Slotinfo.TryGetValue(slot, out var data))
            {
                data = Slotinfo[slot] = new Slotdata() { States = new List<int[]> { new int[] { newvalue, 3 } } };
            }

            data.States[index][0] = newvalue;

            if (data.Binding < 0)
            {
                return;
            }

            UpdateAccessoryshow(data, slot);
        }

        private void ACC_Appearance_state2_ValueChanged(int slot, int newvalue, int index)
        {
            if (!Slotinfo.TryGetValue(slot, out var data))
            {
                data = Slotinfo[slot] = new Slotdata() { States = new List<int[]> { new int[] { 0, newvalue } } };
            }

            data.States[index][1] = newvalue;

            var slider = ACC_Appearance_state.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>();
            var slider2 = ACC_Appearance_state2.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>();
            if (autoscale && ACC_Appearance_dropdown.GetValue(slot) >= Constants.ConstantOutfitNames.Count)
            {
                Update_Toggles_GUI();
                if (Mathf.RoundToInt(slider2.value) == Mathf.RoundToInt(slider2.maxValue))
                {
                    slider.maxValue += 1;
                    slider2.maxValue += 1;
                }
            }
            if (data.Binding < 0)
            {
                return;
            }
            UpdateAccessoryshow(data, slot);
        }

        private void UpdateAccessoryshow(Slotdata data, int slot)
        {
            int state;
            var binding = data.Binding;
            if (binding < 9)
            {
                state = ChaControl.fileStatus.clothesState[binding];
            }
            else
            {
                state = GUI_Custom_Dict[slot][0];
            }

            var show = ShowState(state, data.States);
            if (data.Shoetype != 2 && ChaControl.fileStatus.shoesType != data.Shoetype)
            {
                show = false;
            }
            ChaControl.SetAccessoryState(slot, show);
        }

        private void ACC_Appearance_dropdown_ValueChanged(int slot, int newvalue)
        {
            var delete = newvalue < 0;
            var kind = 0;
            if (!delete)
            {
                if (newvalue <= Max_Defined_Key)
                {
                    kind = Slotinfo[slot].Binding = newvalue;
                }
                else
                {
                    kind = Slotinfo[slot].Binding = Names.ElementAt(newvalue - Max_Defined_Key - 1).Key;
                }
            }

            if (delete)
            {
                //do nothing
            }
            else if (newvalue < 9)
            {
                ACC_Appearance_state.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue = 3;
                ACC_Appearance_state2.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue = 3;
                ACC_Appearance_state.SetValue(slot, 0);
                ACC_Appearance_state2.SetValue(slot, 3);
            }
            else
            {
                ACC_Appearance_state.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue = minilimit;
                ACC_Appearance_state2.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue = minilimit;
                ACC_Appearance_state.SetValue(slot, 0);
                ACC_Appearance_state2.SetValue(slot, minilimit);
            }

            if (!ASS_Exists)
            {
                return;
            }

            if (delete)
            {
                DeleteGroup(kind);
            }
            else
            {
                ChangeTriggerProperty(kind);
            }
        }

        private void Update_Drop_boxes()
        {
            var ACC_Appearance_dropdown_Controls = ACC_Appearance_dropdown.Control.ControlObjects.ToList();
            var Appearance_Options = new List<TMP_Dropdown.OptionData>(ACC_Appearance_dropdown_Controls[0].GetComponentInChildren<TMP_Dropdown>().options);
            var optioncount = Constants.ConstantOutfitNames.Count;
            var namedict = Names;
            var n = namedict.Count;

            if (Appearance_Options.Count > optioncount)
            {
                Appearance_Options.RemoveRange(optioncount, Appearance_Options.Count - (optioncount));
            }

            foreach (var item in namedict)
            {
                var temp = new TMP_Dropdown.OptionData(item.Value.Name);
                Appearance_Options.Add(temp);
            }

            foreach (var item in ACC_Appearance_dropdown.Control.ControlObjects)
            {
                item.GetComponentInChildren<TMP_Dropdown>().ClearOptions();
                item.GetComponentInChildren<TMP_Dropdown>().AddOptions(Appearance_Options);
            }

            foreach (var item in Slotinfo)
            {
                if (item.Value.Binding < Constants.ConstantOutfitNames.Count)
                {
                    ACC_Appearance_dropdown.SetValue(item.Key, item.Value.Binding + 1, false);
                }
                else
                {
                    var index = 0;
                    for (; index < n; index++)
                    {
                        if (namedict.ElementAt(index).Key == item.Value.Binding)
                        {
                            break;
                        }
                    }
                    ACC_Appearance_dropdown.SetValue(item.Key, index + Constants.ConstantOutfitNames.Count, false);
                }
            }
        }

        private IEnumerator WaitForSlots()
        {
            if (!MakerAPI.InsideMaker)
            {
                yield break;
            }

            var ACCData = Parts.Length;
            do
            {
                yield return null;
            } while (!MakerAPI.InsideAndLoaded || ACC_Appearance_state.Control.ControlObjects.Count() < ACCData);
            Update_Now_Coordinate();
            Refresh();

            Update_Drop_boxes();
            var slider1 = ACC_Appearance_state.Control.ControlObjects;
            var slider2 = ACC_Appearance_state2.Control.ControlObjects;
            var slotinfo = Slotinfo;
            for (var i = 0; i < ACCData; i++)
            {
                if (!slotinfo.TryGetValue(i, out var slotdata))
                {
                    slotdata = new Slotdata();
                    slotinfo[i] = slotdata;
                }
                var binding = slotdata.Binding;
                var max = MaxState(binding);
                var zerostate = slotdata.States[0];
                slider1.ElementAt(i).GetComponentInChildren<Slider>().maxValue = Math.Max(3, max);

                ACC_Appearance_state.SetValue(i, zerostate[0], false);

                slider2.ElementAt(i).GetComponentInChildren<Slider>().maxValue = Math.Max(3, max);

                ACC_Appearance_state2.SetValue(i, zerostate[1], false);

                if (binding == -1)
                {
                    binding = 0;
                }
                else if (binding < 9)
                {
                    binding += 1;
                }
                else
                {
                    binding = 10 + IndexOfName(binding);
                }
                ACC_Appearance_dropdown.SetValue(i, binding, false);
            }
            UpdateClothingNots();
            Update_Toggles_GUI();
        }

        internal void MovIt(List<QueueItem> queue)
        {
            var slotinfo = Slotinfo;
            foreach (var item in queue)
            {
                Settings.Logger.LogDebug($"Moving Acc from slot {item.SrcSlot} to {item.DstSlot}");

                var dropdown = 0;
                var state1 = 0;
                var state2 = 3;
                if (slotinfo.TryGetValue(item.SrcSlot, out var slotdata))
                {
                    var binding = slotdata.Binding;
                    var states = slotdata.States;

                    dropdown = (binding > 8) ? 10 + IndexOfName(binding) : binding + 1;
                    state1 = states[0][0];
                    state2 = states[0][1];
                    slotinfo[item.DstSlot] = new Slotdata(slotdata);
                }
                else
                {
                    slotinfo.Remove(item.DstSlot);
                }

                slotinfo.Remove(item.SrcSlot);

                ACC_Appearance_dropdown.SetValue(item.DstSlot, dropdown, false);
                ACC_Appearance_state.SetValue(item.DstSlot, state1, false);
                ACC_Appearance_state2.SetValue(item.DstSlot, state2, false);


                ACC_Appearance_dropdown.SetValue(item.SrcSlot, 0, false);
                ACC_Appearance_state.SetValue(item.SrcSlot, 0, false);
                ACC_Appearance_state2.SetValue(item.SrcSlot, 3, false);
            }
        }

        internal void Slot_ACC_Change(int slotNo, int type)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                return;
            }
            if (type == 120)
            {
                Slotinfo.Remove(slotNo);
                ACC_Appearance_dropdown.SetValue(slotNo, 0, false);
                ACC_Appearance_state.SetValue(slotNo, 0, false);
                ACC_Appearance_state2.SetValue(slotNo, 3, false);
            }
        }

        private int IndexOfName(int binding)
        {
            var namedict = Names;
            for (int i = 0, n = namedict.Count; i < n; i++)
            {
                if (namedict.ElementAt(i).Key == binding)
                {
                    return i;
                }
            }
            return 0;
        }

        internal void UpdateClothingNots()
        {
            var clothingnot = ClothNotData;
            for (int i = 0, n = clothingnot.Length; i < n; i++)
            {
                clothingnot[i] = false;
            }

            var clothinfo = ChaControl.infoClothes;

            UpdateTopClothingNots(clothinfo[0], ref clothingnot);
            UpdateBraClothingNots(clothinfo[2], ref clothingnot);

            ForceClothDataUpdate = false;
        }

        private void UpdateTopClothingNots(ListInfoBase infoBase, ref bool[] clothingnot)
        {
            if (infoBase == null)
            {
                return;
            }
            Hooks.ClothingNotPatch.IsshortsCheck = false;
            var ListInfoResult = Hooks.ClothingNotPatch.ListInfoResult;

            var key = ChaListDefine.KeyType.Coordinate;
            infoBase.GetInfo(key);
            var notbot = ListInfoResult[key] == 2; //only in ChangeClothesTopAsync

            key = ChaListDefine.KeyType.NotBra;
            infoBase.GetInfo(key);
            var notbra = ListInfoResult[key] == 1; //only in ChangeClothesTopAsync

            clothingnot[0] = clothingnot[0] || notbot;
            clothingnot[1] = clothingnot[1] || notbra;
        }

        private void UpdateBraClothingNots(ListInfoBase infoBase, ref bool[] clothingnot)
        {
            if (infoBase == null)
            {
                return;
            }
            Hooks.ClothingNotPatch.IsshortsCheck = true;

            var ListInfoResult = Hooks.ClothingNotPatch.ListInfoResult;
            var key = ChaListDefine.KeyType.Coordinate;

            infoBase.GetInfo(key);//kk uses coordinate to hide shorts

            var notShorts = ListInfoResult[key] == 2; //only in ChangeClothesBraAsync

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
                    if (ChaControl.nowCoordinate.clothes.parts[8].id == 0)
                    {
                        RemoveClothingBinding(9);
                    }
                    break;
                case 8:
                    if (ChaControl.nowCoordinate.clothes.parts[7].id == 0)
                    {
                        RemoveClothingBinding(9);
                    }
                    break;
                default:
                    break;
            }
            RemoveClothingBinding(kind);
        }

        private void RemoveClothingBinding(int kind)
        {
            var slotinfo = Slotinfo;
            var removelist = slotinfo.Where(x => x.Value.Binding == kind).ToList();
            for (int i = 0, n = removelist.Count; i < n; i++)
            {
                slotinfo.Remove(removelist[i].Key);
            }
        }
    }
}
