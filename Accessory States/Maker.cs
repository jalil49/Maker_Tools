using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
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
        AccessoryControlWrapper<MakerDropdown, int> ACC_Appearance_dropdown;
        AccessoryControlWrapper<MakerSlider, float> ACC_Appearance_state;
        AccessoryControlWrapper<MakerSlider, float> ACC_Appearance_state2;
        AccessoryControlWrapper<MakerToggle, bool> ACC_Is_Parented;

        MakerTextbox ThemeText;
        MakerRadioButtons RadioToggle;
        MakerButton ApplyTheme;

        MakerDropdown Parented_Dropdown;
        MakerDropdown Custom_Dropdown;

        public Dictionary<int, int> Gui_states = new Dictionary<int, int>();

        private void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;
            var category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 3, "Accessory Settings");
            e.AddSubCategory(category);

            string[] ACC_app_Dropdown = new string[] { "None", "Top", "Bottom", "Bra", "Panty", "Gloves", "Pantyhose", "Socks", "Shoes" };

            ACC_Appearance_dropdown = MakerAPI.AddEditableAccessoryWindowControl<MakerDropdown, int>(new MakerDropdown("Clothing\nBind", ACC_app_Dropdown, category, 0, owner));
            ACC_Appearance_dropdown.ValueChanged += ACC_Appearance_dropdown_ValueChanged;

            var ThemeTextBox = new MakerTextbox(category, "Name: ", "", owner);
            ThemeText = MakerAPI.AddAccessoryWindowControl<MakerTextbox>(ThemeTextBox);

            var radio = new MakerRadioButtons(category, owner, "options", 0, new string[] { "Add Group", "Remove Group", "Rename" });
            RadioToggle = MakerAPI.AddAccessoryWindowControl<MakerRadioButtons>(radio);
            RadioToggle.ValueChanged.Subscribe(x => RadioChanged(x));

            var AddRemoveModifyButton = new MakerButton("Modify Group", category, owner);
            ApplyTheme = MakerAPI.AddAccessoryWindowControl<MakerButton>(AddRemoveModifyButton);
            ApplyTheme.OnClick.AddListener(delegate ()
            {
                List<TMP_Dropdown.OptionData> options = Custom_Dropdown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;
                var controlobjects = ACC_Appearance_dropdown.Control.ControlObjects.ToArray();
                List<TMP_Dropdown.OptionData> options2 = controlobjects[ACC_Appearance_dropdown.CurrentlySelectedIndex].GetComponentInChildren<TMP_Dropdown>().options;
                int kind = 0;
                int index = 0;
                int index2 = 0;
                switch (radio.Value)
                {
                    case 0:
                        if (options.Any(x => x.text == ThemeText.Value))
                        {
                            return;
                        }
                        options.Add(new TMP_Dropdown.OptionData(ThemeText.Value));
                        options2.Add(new TMP_Dropdown.OptionData(ThemeText.Value));
                        kind = 15 + ThisCharactersData.Now_ACC_Name_Dictionary.Count();
                        ThisCharactersData.Now_ACC_Name_Dictionary[kind] = ThemeText.Value;
                        break;
                    case 1:
                        if (ThemeText.Value == "None")
                        {
                            return;
                        }
                        for (int i = 0; i < options.Count; i++)
                        {
                            if (options[i].text == ThemeText.Value)
                            {
                                index = i;
                                break;
                            }
                            else if (i == options.Count - 1)
                            {
                                return;
                            }
                        }
                        for (int i = 0; i < options2.Count; i++)
                        {
                            if (options2[i].text == ThemeText.Value)
                            {
                                index2 = i;
                                break;
                            }
                            else if (i == options2.Count - 1)
                            {
                                return;
                            }
                        }
                        options.RemoveAt(index);
                        options2.RemoveAt(index2);
                        kind = index + 15;
                        var removal = ThisCharactersData.Now_ACC_Binding_Dictionary.Where(x => x.Value == kind);
                        foreach (var item in removal)
                        {
                            ThisCharactersData.Now_ACC_Binding_Dictionary.Remove(item.Key);
                            ACC_Appearance_dropdown.SetValue(item.Key, 0, false);
                        }
                        removal = ThisCharactersData.Now_ACC_Binding_Dictionary.Where(x => x.Value > kind);
                        foreach (var item in removal)
                        {
                            int Change = ThisCharactersData.Now_ACC_Binding_Dictionary[item.Key] -= 1;
                            ACC_Appearance_dropdown.SetValue(item.Key, Change, false);
                        }
                        ThisCharactersData.Now_ACC_Name_Dictionary.Remove(kind);
                        break;
                    case 2:
                        if (ACC_Appearance_dropdown.GetSelectedValue() < 9)
                        {
                            return;
                        }
                        foreach (var item in options)
                        {
                            if (item.text == ThemeText.Value)
                            {
                                return;
                            }
                        }
                        var result = options2[ACC_Appearance_dropdown.GetSelectedValue()].text;
                        for (int i = 0; i < options.Count; i++)
                        {
                            if (options[i].text == result)
                            {
                                options[i].text = ThemeText.Value;
                                break;
                            }
                        }
                        result = ThemeText.Value;
                        break;
                    default:
                        break;
                }
                foreach (var item in ACC_Appearance_dropdown.Control.ControlObjects)
                {
                    item.GetComponentInChildren<TMP_Dropdown>().options = options2;
                }
                if (radio.Value == 0)
                {
                    ACC_Appearance_dropdown.SetSelectedValue(kind);
                }
            });


            var Start_Slider = new MakerSlider(category, "Start", 0, 3, 0, owner)
            {
                WholeNumbers = true
            };
            ACC_Appearance_state = MakerAPI.AddEditableAccessoryWindowControl<MakerSlider, float>(Start_Slider);
            ACC_Appearance_state.ValueChanged += ACC_Appearance_state_ValueChanged;

            var Stop_Slider = new MakerSlider(category, "Stop", 0, 3, 3, owner)
            {
                WholeNumbers = true
            };
            ACC_Appearance_state2 = MakerAPI.AddEditableAccessoryWindowControl<MakerSlider, float>(Stop_Slider);
            ACC_Appearance_state2.ValueChanged += ACC_Appearance_state2_ValueChanged;

            var Parented_Toggle = new MakerToggle(category, "Bind To Parent", false, owner);
            ACC_Is_Parented = MakerAPI.AddEditableAccessoryWindowControl<MakerToggle, bool>(Parented_Toggle);
            ACC_Is_Parented.ValueChanged += ACC_Is_Parented_ValueChanged;

            var Parented_View_Toggle = new MakerToggle(category, "toggle parent view", true, owner);

            Parented_Dropdown = new MakerDropdown("Parented options", new string[] { "None" }, category, 0, owner);
            e.AddControl(Parented_Dropdown).ValueChanged.Subscribe(x => Parented_View_Toggle.SetValue(true, false));

            e.AddControl(Parented_View_Toggle).ValueChanged.Subscribe(x => Parent_toggle(Parented_Dropdown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options[Parented_Dropdown.Value].text, x));

            var Custom_View_Toggle = new MakerButton("toggle Custom view", category, owner);

            Custom_Dropdown = new MakerDropdown("Custom options", new string[] { "None" }, category, 0, owner);
            e.AddControl(Custom_Dropdown);

            e.AddControl(Custom_View_Toggle).OnClick.AddListener(delegate () { Custom_groups_GUI_Toggle(Custom_Dropdown.Value + 14); });

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            ACC_Appearance_dropdown.Control.GroupingID = GroupingID;
            ACC_Appearance_state.Control.GroupingID = GroupingID;
            ACC_Appearance_state2.Control.GroupingID = GroupingID;
            ACC_Is_Parented.Control.GroupingID = GroupingID;
            ApplyTheme.GroupingID = GroupingID;
            ThemeText.GroupingID = GroupingID;
            RadioToggle.GroupingID = GroupingID;
        }

        internal void Maker_started()
        {
            MakerAPI.RegisterCustomSubCategories += MakerAPI_RegisterCustomSubCategories;
            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;
            MakerAPI.MakerFinishedLoading += (s, e) => VisibiltyToggle();

            AccessoriesApi.MakerAccSlotAdded += AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;
            AccessoriesApi.AccessoryKindChanged += AccessoriesApi_AccessoryKindChanged;
            AccessoriesApi.SelectedMakerAccSlotChanged += (s, e) => VisibiltyToggle();

            Hooks.Slot_ACC_Change += (s, e) => VisibiltyToggle();
        }

        internal void Maker_Ended()
        {
            MakerAPI.RegisterCustomSubCategories -= MakerAPI_RegisterCustomSubCategories;
            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;
            MakerAPI.MakerFinishedLoading -= (s, e) => VisibiltyToggle();

            AccessoriesApi.MakerAccSlotAdded -= AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;
            AccessoriesApi.AccessoryKindChanged -= AccessoriesApi_AccessoryKindChanged;
            AccessoriesApi.SelectedMakerAccSlotChanged -= (s, e) => VisibiltyToggle();

            Hooks.Slot_ACC_Change -= (s, e) => VisibiltyToggle();
        }

        private void AccessoriesApi_AccessoryKindChanged(object sender, AccessorySlotEventArgs e)
        {
            ACC_Appearance_dropdown.SetValue(e.SlotIndex, 0);
            ACC_Appearance_state.SetValue(e.SlotIndex, 0);
            ACC_Appearance_state2.SetValue(e.SlotIndex, 3);
            ACC_Is_Parented.SetValue(e.SlotIndex, false);
            VisibiltyToggle();
        }

        private void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            ACC_Appearance_dropdown.SetValue(e.DestinationSlotIndex, ACC_Appearance_dropdown.GetValue(e.SourceSlotIndex));
            ACC_Appearance_state.SetValue(e.DestinationSlotIndex, ACC_Appearance_state.GetValue(e.SourceSlotIndex));
            ACC_Appearance_state2.SetValue(e.DestinationSlotIndex, ACC_Appearance_state2.GetValue(e.SourceSlotIndex));
            ACC_Is_Parented.SetValue(e.DestinationSlotIndex, ACC_Is_Parented.GetValue(e.SourceSlotIndex));
            VisibiltyToggle();
        }

        private void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            var binder = ThisCharactersData.ACC_Binding_Dictionary;
            var states = ThisCharactersData.ACC_State_array;
            var parents = ThisCharactersData.ACC_Parented_Dictionary;
            var names = ThisCharactersData.ACC_Name_Dictionary;
            var source = (int)e.CopySource;
            var dest = (int)e.CopyDestination;
            foreach (var slot in e.CopiedSlotIndexes)
            {
                if (binder[source].TryGetValue(slot, out var value))
                {
                    binder[dest][slot] = value;
                    states[dest][slot] = states[source][slot];
                }
                if (names[source].TryGetValue(value, out var custom))
                {
                    names[dest][slot] = custom;
                }
                if (parents[source].TryGetValue(slot, out var value2))
                {
                    parents[dest][slot] = value2;
                }
            }
            VisibiltyToggle();
        }

        private void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            StartCoroutine(WaitForSlots());
        }

        private void AccessoriesApi_MakerAccSlotAdded(object sender, AccessorySlotEventArgs e)
        {
            Update_More_Accessories();
        }

        private void Custom_groups_GUI_Toggle(int kind)
        {
            if (!Gui_states.TryGetValue(kind, out int state))
            {
                state = 0;
            }
            var binded = ThisCharactersData.Now_ACC_Binding_Dictionary.Where(x => x.Value == kind);

            int final = 0;
            foreach (var item in binded)
            {
                if (ThisCharactersData.Now_ACC_State_array[item.Key][1] > final)
                {
                    final = ThisCharactersData.Now_ACC_State_array[item.Key][1];
                    if (final == 3)
                    {
                        break;
                    }
                }
            }
            Gui_states[kind] = state = (state + 1) % (final + 2);

            Custom_Groups(kind, state);
        }

        private void ACC_Is_Parented_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<bool> e)
        {
            List<TMP_Dropdown.OptionData> options = Parented_Dropdown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;

            if (e.NewValue)
            {
                var Changed_Option = new TMP_Dropdown.OptionData(Accessorys_Parts[e.SlotIndex].parentKey);

                ThisCharactersData.Now_Parented_Dictionary[e.SlotIndex] = true;
                if (!options.Any(x => x.text == Changed_Option.text))
                {
                    options.Add(Changed_Option);
                }
            }
            else
            {
                var Dict = ThisCharactersData.Now_Parented_Name_Dictionary;

                Dict.TryGetValue(e.SlotIndex, out var name);
                if (Dict.Where(x => x.Value == name).Count() == 1)
                {
                    options.RemoveAll(x => x.text == name);
                }
                ThisCharactersData.Now_Parented_Dictionary.Remove(e.SlotIndex);
            }
            ThisCharactersData.Update_Parented_Name();
        }

        private void ACC_Appearance_state_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<float> e)
        {
            if (!ThisCharactersData.Now_ACC_State_array.TryGetValue(e.SlotIndex, out int[] data))
            {
                ThisCharactersData.Now_ACC_State_array.Add(e.SlotIndex, new int[] { Mathf.RoundToInt(e.NewValue), 3 });
            }
            else
            {
                data[0] = Mathf.RoundToInt(e.NewValue);
            }
            if (ACC_Appearance_state2.GetValue(e.SlotIndex) < ACC_Appearance_state.GetValue(e.SlotIndex))
            {
                ACC_Appearance_state.SetValue(e.SlotIndex, ACC_Appearance_state2.GetValue(e.SlotIndex));
            }
        }

        private void ACC_Appearance_state2_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<float> e)
        {

            if (!ThisCharactersData.Now_ACC_State_array.TryGetValue(e.SlotIndex, out int[] data))
            {
                ThisCharactersData.Now_ACC_State_array.Add(e.SlotIndex, new int[] { 0, Mathf.RoundToInt(e.NewValue) });
            }
            else
            {
                data[1] = Mathf.RoundToInt(e.NewValue);
            }
            var slider = ACC_Appearance_state.Control.ControlObjects.ToArray()[e.SlotIndex].GetComponentInChildren<Slider>();
            var slider2 = ACC_Appearance_state2.Control.ControlObjects.ToArray()[e.SlotIndex].GetComponentInChildren<Slider>();
            if (ACC_Appearance_dropdown.GetValue(e.SlotIndex) > 8 && Mathf.RoundToInt(slider2.value) == Mathf.RoundToInt(slider2.maxValue))
            {
                slider.maxValue += 1;
                slider2.maxValue += 1;
            }
        }

        private void ACC_Appearance_dropdown_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<int> e)
        {
            //{ "None", "Top", "Bottom", "Bra", "Panty", "Gloves", "Pantyhose", "Socks", "Shoes" };
            Settings.Logger.LogWarning($"Dropdown slot {e.SlotIndex}, value {e.NewValue}");

            if (e.NewValue < 9)
            {
                ThisCharactersData.Now_ACC_Binding_Dictionary[e.SlotIndex] = e.NewValue;
            }
            else
            {
                ThisCharactersData.Now_ACC_Binding_Dictionary[e.SlotIndex] = e.NewValue + 6;
            }

            if (e.NewValue == 0)
            {
                ThisCharactersData.Now_ACC_Binding_Dictionary.Remove(e.SlotIndex);
                ThisCharactersData.Now_ACC_State_array.Remove(e.SlotIndex);
            }
            else if (e.NewValue < 5)
            {
                ACC_Appearance_state2.Control.ControlObjects.ToArray()[e.SlotIndex].GetComponentInChildren<Slider>().maxValue = 3;
                ACC_Appearance_state2.SetValue(e.SlotIndex, 3);
            }
            else if (e.NewValue < 9)
            {
                ACC_Appearance_state2.Control.ControlObjects.ToArray()[e.SlotIndex].GetComponentInChildren<Slider>().maxValue = 1;
                ACC_Appearance_state2.SetValue(e.SlotIndex, 1);
            }
            else
            {
                ACC_Appearance_state.Control.ControlObjects.ToArray()[e.SlotIndex].GetComponentInChildren<Slider>().maxValue = 5;
                ACC_Appearance_state2.Control.ControlObjects.ToArray()[e.SlotIndex].GetComponentInChildren<Slider>().maxValue = 5;
                ACC_Appearance_state2.SetValue(e.SlotIndex, 0);
            }
        }

        private void Update_Drop_boxes()
        {
            var Custom_Options = Custom_Dropdown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;
            var Parented_Options = Parented_Dropdown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;
            var ACC_Appearance_dropdown_Controls = ACC_Appearance_dropdown.Control.ControlObjects.ToList();
            List<TMP_Dropdown.OptionData> Appearance_Options = new List<TMP_Dropdown.OptionData>(ACC_Appearance_dropdown_Controls[0].GetComponentInChildren<TMP_Dropdown>().options);

            if (Custom_Options.Count > 1)
            {
                Custom_Options.RemoveRange(1, Custom_Options.Count - 1);
            }

            if (Parented_Options.Count > 1)
            {
                Parented_Options.RemoveRange(1, Parented_Options.Count - 1);
            }

            if (Appearance_Options.Count > 9)
            {
                Appearance_Options.RemoveRange(9, Appearance_Options.Count - 9);
            }

            foreach (var item in ThisCharactersData.Now_ACC_Name_Dictionary)
            {
                var temp = new TMP_Dropdown.OptionData(item.Value);
                Custom_Options.Add(temp);
                Appearance_Options.Add(temp);
            }

            foreach (var item in ThisCharactersData.Now_Parented_Name_Dictionary.Values.Distinct())
            {
                Parented_Options.Add(new TMP_Dropdown.OptionData(item));
            }

            foreach (var item in ACC_Appearance_dropdown.Control.ControlObjects)
            {
                item.GetComponentInChildren<TMP_Dropdown>().ClearOptions();
                item.GetComponentInChildren<TMP_Dropdown>().AddOptions(Appearance_Options);
            }

            foreach (var item in ThisCharactersData.Now_ACC_Binding_Dictionary)
            {
                Settings.Logger.LogWarning($"slot {item.Key} value {item.Value} count {Appearance_Options.Count}");

                if (item.Value < 9)
                {
                    ACC_Appearance_dropdown.SetValue(item.Key, item.Value, false);
                    Settings.Logger.LogWarning($"slot {item.Key} value {item.Value}");
                }
                else
                {
                    ACC_Appearance_dropdown.SetValue(item.Key, item.Value - 6, false);
                    Settings.Logger.LogWarning($"slot {item.Key} value {item.Value - 6}");
                }
            }
        }

        private IEnumerator WaitForSlots()
        {
            Refresh();
            int ACCData = Accessorys_Parts.Count();

            while (ACC_Appearance_state.Control.ControlObjects.Count() < ACCData)
            {
                yield return 0;
            }
            foreach (var item in ThisCharactersData.Now_ACC_State_array)
            {
                ACC_Appearance_state2.Control.ControlObjects.ElementAt(item.Key).GetComponentInChildren<Slider>().maxValue = Math.Max(3, item.Value[1]);
                ACC_Appearance_state.Control.ControlObjects.ElementAt(item.Key).GetComponentInChildren<Slider>().maxValue = Math.Max(3, item.Value[1]);
                ACC_Appearance_state2.SetValue(item.Key, item.Value[1], false);
                ACC_Appearance_state.SetValue(item.Key, item.Value[0], false);
            }
            foreach (var item in ThisCharactersData.Now_Parented_Dictionary)
            {
                ACC_Is_Parented.SetValue(item.Key, item.Value, false);
            }
            Update_Drop_boxes();
            VisibiltyToggle();
        }

        private void VisibiltyToggle()
        {
            if (!MakerAPI.InsideMaker)
                return;
            var Slot = AccessoriesApi.SelectedMakerAccSlot;
            var accessory = MakerAPI.GetCharacterControl().GetAccessoryObject(Slot);
            if (accessory == null)
            {
                ACC_Appearance_dropdown.Control.Visible.OnNext(false);
                ACC_Appearance_state.Control.Visible.OnNext(false);
                ACC_Appearance_state2.Control.Visible.OnNext(false);
                ACC_Is_Parented.Control.Visible.OnNext(false);
                ApplyTheme.Visible.OnNext(false);
                ThemeText.Visible.OnNext(false);
                RadioToggle.Visible.OnNext(false);
            }
            else
            {
                ACC_Appearance_dropdown.Control.Visible.OnNext(true);
                ACC_Appearance_state.Control.Visible.OnNext(true);
                ACC_Appearance_state2.Control.Visible.OnNext(true);
                ACC_Is_Parented.Control.Visible.OnNext(true);
                ApplyTheme.Visible.OnNext(true);
                ThemeText.Visible.OnNext(true);
                RadioToggle.Visible.OnNext(true);
                var slider = ACC_Appearance_state.Control.ControlObjects.ToArray()[Slot].GetComponentInChildren<Slider>();
                var slider2 = ACC_Appearance_state2.Control.ControlObjects.ToArray()[Slot].GetComponentInChildren<Slider>();
                if (ACC_Appearance_dropdown.GetValue(Slot) > 8)
                {
                    var Max = 1;
                    ThisCharactersData.Now_ACC_Binding_Dictionary.TryGetValue(Slot, out int Kind);
                    foreach (var item in ThisCharactersData.Now_ACC_Binding_Dictionary.Where(x => x.Value == Kind))
                    {
                        if (ThisCharactersData.Now_ACC_State_array.TryGetValue(item.Key, out var states))
                        {
                            Max = Math.Max(Max, states[1]);
                        }
                    }
                    slider.maxValue = ++Max;
                    slider2.maxValue = Max;
                }
            }
        }

        private void RadioChanged(int value)
        {
            var ControlObjects = RadioToggle.ControlObjects.ToArray();
            foreach (var ControlObject in ControlObjects)
            {
                var Toggles = ControlObject.GetComponentsInChildren<Toggle>();
                for (int Index = 0, n2 = Toggles.Length; Index < n2; Index++)
                {
                    Toggles[Index].isOn = value == Index;
                }
            }
        }
    }
}
