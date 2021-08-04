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
        static AccessoryControlWrapper<MakerToggle, bool> ACC_Is_Parented;

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

            string[] ACC_app_Dropdown = Constants.ConstantOutfitNames.Values.ToArray();
            //string[] ACC_app_Dropdown = new string[] { "None", "Top", "Bottom", "Bra", "Panty", "Gloves", "Pantyhose", "Socks", "Shoes" };

            ACC_Appearance_dropdown = MakerAPI.AddEditableAccessoryWindowControl<MakerDropdown, int>(new MakerDropdown("Clothing\nBind", ACC_app_Dropdown, category, 0, owner));
            ACC_Appearance_dropdown.ValueChanged += (s, es) => ControllerGet.ACC_Appearance_dropdown_ValueChanged(es.SlotIndex, es.NewValue - 1);

            var Start_Slider = new MakerSlider(category, "Start", 0, 3, 0, owner)
            {
                WholeNumbers = true
            };
            ACC_Appearance_state = MakerAPI.AddEditableAccessoryWindowControl<MakerSlider, float>(Start_Slider);
            ACC_Appearance_state.ValueChanged += (s, es) => ControllerGet.ACC_Appearance_state_ValueChanged(es.SlotIndex, Mathf.RoundToInt(es.NewValue), 0);

            var Stop_Slider = new MakerSlider(category, "Stop", 0, 3, 3, owner)
            {
                WholeNumbers = true
            };
            ACC_Appearance_state2 = MakerAPI.AddEditableAccessoryWindowControl<MakerSlider, float>(Stop_Slider);
            ACC_Appearance_state2.ValueChanged += (s, es) => ControllerGet.ACC_Appearance_state2_ValueChanged(es.SlotIndex, Mathf.RoundToInt(es.NewValue), 0);

            var Parented_Toggle = new MakerToggle(category, "Bind To Parent", false, owner);
            ACC_Is_Parented = MakerAPI.AddEditableAccessoryWindowControl<MakerToggle, bool>(Parented_Toggle);
            ACC_Is_Parented.ValueChanged += (s, es) => ControllerGet.ACC_Is_Parented_ValueChanged(es.SlotIndex, es.NewValue);

            gui_button = new MakerButton("States GUI", category, owner);
            MakerAPI.AddAccessoryWindowControl(gui_button);
            gui_button.OnClick.AddListener(delegate () { GUI_Toggle(); });

            SetupInterface();

            var interfacebutton = e.AddSidebarControl(new SidebarToggle("ACC States", true, owner));
            interfacebutton.ValueChanged.Subscribe(x => ShowToggleInterface = x);

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            ACC_Appearance_dropdown.Control.GroupingID = GroupingID;
            ACC_Appearance_state.Control.GroupingID = GroupingID;
            ACC_Appearance_state2.Control.GroupingID = GroupingID;
            ACC_Is_Parented.Control.GroupingID = GroupingID;
            gui_button.GroupingID = GroupingID;
        }

        internal void RemoveOutfitEvent()
        {
            int max = Coordinate.Keys.Max();
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
            AccessoriesApi.AccessoryKindChanged += AccessoriesApi_AccessoryKindChanged;

            MakerAPI.MakerFinishedLoading += (s, e) => VisibiltyToggle();
            AccessoriesApi.SelectedMakerAccSlotChanged += (s, e) => VisibiltyToggle();
        }

        public static void Maker_Ended()
        {
            MakerAPI.MakerExiting -= (s, e) => Maker_Ended();

            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;

            AccessoriesApi.MakerAccSlotAdded -= AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;
            AccessoriesApi.AccessoryKindChanged -= AccessoriesApi_AccessoryKindChanged;

            AccessoriesApi.SelectedMakerAccSlotChanged -= (s, e) => VisibiltyToggle();
            MakerAPI.MakerFinishedLoading -= (s, e) => VisibiltyToggle();

            ShowCustomGui = false;
            MakerEnabled = false;
            ShowToggleInterface = false;
        }

        private static void AccessoriesApi_AccessoryKindChanged(object sender, AccessorySlotEventArgs e)
        {
            VisibiltyToggle();
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
            ACC_Is_Parented.SetValue(e.DestinationSlotIndex, ACC_Is_Parented.GetValue(e.SourceSlotIndex), false);

            var thisdata = ThisCharactersData;
            var slotinfo = Slotinfo;
            var parented = Parented;

            if (slotinfo.ContainsKey(e.SourceSlotIndex))
            {
                slotinfo[e.DestinationSlotIndex] = new Slotdata(slotinfo[e.SourceSlotIndex]);
            }
            else
            {
                slotinfo.Remove(e.DestinationSlotIndex);
            }

            if (parented.ContainsKey(e.SourceSlotIndex))
            {
                parented[e.DestinationSlotIndex] = parented[e.SourceSlotIndex];
            }
            else
            {
                parented.Remove(e.DestinationSlotIndex);
            }
            thisdata.Update_Parented_Name();
            VisibiltyToggle();
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
                }
                var binding = slotinfo.Binding;
                if (binding > 8 && source.Names.TryGetValue(binding, out var custom))
                {
                    if (dest.Names.ContainsKey(binding))
                    {
                        if (!convertednames.TryGetValue(binding, out int newvalue))
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

                if (source.Parented.TryGetValue(slot, out var value2))
                {
                    dest.Parented[slot] = value2;
                }
                else
                {
                    dest.Parented.Remove(slot);
                }
            }
            VisibiltyToggle();
        }

        private static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            Controller.StartCoroutine(Controller.WaitForSlots());
            VisibiltyToggle();
        }

        private static void AccessoriesApi_MakerAccSlotAdded(object sender, AccessorySlotEventArgs e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

            Controller.StartCoroutine(Wait());
            IEnumerator Wait()
            {
                yield return null;
                Controller.Update_More_Accessories();
            }
        }

        private void ACC_Is_Parented_ValueChanged(int Slot, bool isparented)
        {
            Parented[Slot] = isparented;
            ThisCharactersData.Update_Parented_Name();
        }

        private void ACC_Appearance_state_ValueChanged(int slot, int newvalue, int index)
        {
            var slotinfo = Slotinfo;

            if (!slotinfo.TryGetValue(slot, out var data))
            {
                slotinfo.Add(slot, new Slotdata() { States = new List<int[]> { new int[] { newvalue, 3 } } });
                return;
            }

            data.States[index][0] = newvalue;
        }

        private void ACC_Appearance_state2_ValueChanged(int slot, int newvalue, int index)
        {
            var slotinfo = Slotinfo;

            if (!slotinfo.TryGetValue(slot, out var data))
            {
                slotinfo.Add(slot, new Slotdata() { States = new List<int[]> { new int[] { 0, newvalue } } });
                return;
            }

            data.States[index][1] = newvalue;

            var slider = ACC_Appearance_state.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>();
            var slider2 = ACC_Appearance_state2.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>();
            if (ACC_Appearance_dropdown.GetValue(slot) >= Constants.ConstantOutfitNames.Count)
            {
                Update_Custom_GUI();
                if (Mathf.RoundToInt(slider2.value) == Mathf.RoundToInt(slider2.maxValue))
                {
                    slider.maxValue += 1;
                    slider2.maxValue += 1;
                }
            }
        }

        private void ACC_Appearance_dropdown_ValueChanged(int slot, int newvalue)
        {
            bool delete = newvalue < 0;
            int kind = 0;
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
                Slotinfo.Remove(slot);
            }
            else if (newvalue < 5)
            {
                ACC_Appearance_state.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue = 3;
                ACC_Appearance_state2.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue = 3;
                ACC_Appearance_state.SetValue(slot, 0);
                ACC_Appearance_state2.SetValue(slot, 3);
            }
            else if (newvalue <= Max_Defined_Key)
            {
                ACC_Appearance_state.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue = 1;
                ACC_Appearance_state2.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue = 1;
                ACC_Appearance_state.SetValue(slot, 0);
                ACC_Appearance_state2.SetValue(slot, 1);
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
            List<TMP_Dropdown.OptionData> Appearance_Options = new List<TMP_Dropdown.OptionData>(ACC_Appearance_dropdown_Controls[0].GetComponentInChildren<TMP_Dropdown>().options);
            int optioncount = Constants.ConstantOutfitNames.Count;
            var namedict = Names;
            int n = namedict.Count;

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
                    int index = 0;
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
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                yield break;
            }
            yield return null;

            Update_More_Accessories();

            int ACCData = Accessorys_Parts.Count();
            do
            {
                yield return null;
            } while (!MakerAPI.InsideAndLoaded && ACC_Is_Parented.Control.ControlObjects.Count() < ACCData);
            ThisCharactersData.Update_Now_Coordinate();

            Refresh();

            Update_Drop_boxes();

            var slider1 = ACC_Appearance_state.Control.ControlObjects;
            var slider2 = ACC_Appearance_state2.Control.ControlObjects;
            var slotinfo = Slotinfo;
            for (int i = 0; i < ACCData; i++)
            {
                if (!slotinfo.TryGetValue(i, out var slotdata))
                {
                    slotdata = new Slotdata();
                }
                var max = MaxState(slotdata.States);
                slider2.ElementAt(i).GetComponentInChildren<Slider>().maxValue = Math.Max(3, max);
                slider1.ElementAt(i).GetComponentInChildren<Slider>().maxValue = Math.Max(3, max);
                ACC_Appearance_state2.SetValue(i, slotdata.States[0][1], false);
                ACC_Appearance_state.SetValue(i, slotdata.States[0][0], false);

                Parented.TryGetValue(i, out bool IsParented);
                ACC_Is_Parented.SetValue(i, IsParented, false);

                var binding = slotdata.Binding;
                if (binding < 10)
                {
                    binding += IndexOfName(binding);
                }
                ACC_Appearance_dropdown.SetValue(i, binding, false);
            }
            Update_Custom_GUI();
            VisibiltyToggle();
        }

        private static void VisibiltyToggle()
        {
            if (!MakerAPI.InsideMaker)
                return;
            var Slot = AccessoriesApi.SelectedMakerAccSlot;
#pragma warning disable CS0612 // Type or member is obsolete
            var accessory = AccessoriesApi.GetPartsInfo(Slot).type == 120;
#pragma warning restore CS0612 // Type or member is obsolete
            if (accessory)
            {
                ACC_Appearance_dropdown.Control.Visible.OnNext(false);
                ACC_Appearance_state.Control.Visible.OnNext(false);
                ACC_Appearance_state2.Control.Visible.OnNext(false);
                ACC_Is_Parented.Control.Visible.OnNext(false);
                gui_button.Visible.OnNext(false);
            }
            else
            {
                ACC_Appearance_dropdown.Control.Visible.OnNext(true);
                ACC_Appearance_state.Control.Visible.OnNext(true);
                ACC_Appearance_state2.Control.Visible.OnNext(true);
                ACC_Is_Parented.Control.Visible.OnNext(true);
                gui_button.Visible.OnNext(true);
                if (ACC_Appearance_dropdown.GetValue(Slot) > 8)
                {
                    var slotinfo = ControllerGet.Slotinfo;

                    var Max = 1;
                    if (slotinfo.TryGetValue(Slot, out var slotdata))
                    {
                        var datalist = slotinfo.Values.Where(x => x.Binding == slotdata.Binding);
                        foreach (var item in datalist)
                        {
                            Max = Math.Max(Max, item.States[0][1]);
                        }
                    }

                    var slider = ACC_Appearance_state.Control.ControlObjects.ElementAt(Slot).GetComponentInChildren<Slider>();
                    var slider2 = ACC_Appearance_state2.Control.ControlObjects.ElementAt(Slot).GetComponentInChildren<Slider>();
                    slider.maxValue = ++Max;
                    slider2.maxValue = Max;
                }
            }
        }

        internal void MovIt(List<QueueItem> queue)
        {
            var slotinfo = Slotinfo;
            foreach (var item in queue)
            {
                Settings.Logger.LogDebug($"Moving Acc from slot {item.SrcSlot} to {item.DstSlot}");

                if (slotinfo.TryGetValue(item.SrcSlot, out var slotdata))
                {
                    var binding = slotdata.Binding;
                    var states = slotdata.States;
                    int bindingresult = (binding > Max_Defined_Key) ? Max_Defined_Key + 1 + IndexOfName(binding) : binding;

                    slotinfo[item.DstSlot] = new Slotdata(slotdata);

                    ACC_Appearance_dropdown.SetValue(item.DstSlot, bindingresult, false);
                    ACC_Appearance_dropdown.SetValue(item.SrcSlot, 0, false);

                    ACC_Appearance_state.SetValue(item.SrcSlot, 0, false);
                    ACC_Appearance_state2.SetValue(item.SrcSlot, 3, false);
                    ACC_Appearance_state.SetValue(item.DstSlot, states[0][0], false);
                    ACC_Appearance_state2.SetValue(item.DstSlot, states[0][1], false);

                    slotinfo.Remove(item.SrcSlot);
                }
                else
                {
                    slotinfo.Remove(item.DstSlot);
                }

                if (Parented.TryGetValue(item.SrcSlot, out var Isparented))
                {
                    ACC_Is_Parented.SetValue(item.DstSlot, Isparented, false);
                    ACC_Is_Parented.SetValue(item.SrcSlot, false, false);
                    Parented[item.DstSlot] = Isparented;
                    Parented.Remove(item.SrcSlot);
                }
                else
                {
                    Parented.Remove(item.DstSlot);
                    ACC_Is_Parented.SetValue(item.DstSlot, false, false);
                }
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
                Parented.Remove(slotNo);
                ACC_Appearance_dropdown.SetValue(slotNo, 0, false);
                ACC_Appearance_state.SetValue(slotNo, 0, false);
                ACC_Appearance_state2.SetValue(slotNo, 3, false);
                ACC_Is_Parented.SetValue(slotNo, false, false);
            }
            VisibiltyToggle();
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

        internal void ClothingTypeChange(int kind, int index)
        {
            if (index > 0)
                return;
            switch (kind)
            {
                case 1:
                    if (ChaControl.notBot)
                        return;
                    break;
                case 2:
                    if (ChaControl.notBra)
                        return;
                    break;
                case 3:
                    if (ChaControl.notShorts)
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
