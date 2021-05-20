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

        static MakerTextbox ThemeText;

        static MakerRadioButtons RadioToggle;

        static MakerButton ApplyTheme;

        static Rect _Custom_GroupsRect;

        static private Vector2 _accessorySlotsScrollPos = Vector2.zero;

        static private readonly Dictionary<int, int[]> GUI_Custom_Dict = new Dictionary<int, int[]>();

        static private readonly Dictionary<string, bool> GUI_Parent_Dict = new Dictionary<string, bool>();

        static private bool ShowInterface = false;

        static public Dictionary<int, int> Gui_states = new Dictionary<int, int>();

        const int Defined_Bindings = 8;
        const int Binding_offset = 6;
        static bool MakerEnabled = false;

        public static void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            MakerEnabled = Settings.Enable.Value;
            if (!MakerEnabled)
            {
                return;
            }

            var owner = Settings.Instance;
            var category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 3, "Accessory Settings");
            e.AddSubCategory(category);

            string[] ACC_app_Dropdown = new string[] { "None", "Top", "Bottom", "Bra", "Panty", "Gloves", "Pantyhose", "Socks", "Shoes" };

            ACC_Appearance_dropdown = MakerAPI.AddEditableAccessoryWindowControl<MakerDropdown, int>(new MakerDropdown("Clothing\nBind", ACC_app_Dropdown, category, 0, owner));
            ACC_Appearance_dropdown.ValueChanged += ACC_Appearance_dropdown_ValueChanged;

            var ThemeTextBox = new MakerTextbox(category, "Name: ", "", owner);
            ThemeText = MakerAPI.AddAccessoryWindowControl<MakerTextbox>(ThemeTextBox);

            var radio = new MakerRadioButtons(category, owner, "Modify", new string[] { "Add", "Remove", "Rename" });
            RadioToggle = MakerAPI.AddAccessoryWindowControl<MakerRadioButtons>(radio);
            RadioToggle.ValueChanged.Subscribe(x => RadioChanged(x));

            var AddRemoveModifyButton = new MakerButton("Modify State Group", category, owner);
            ApplyTheme = MakerAPI.AddAccessoryWindowControl<MakerButton>(AddRemoveModifyButton);
            ApplyTheme.OnClick.AddListener(delegate ()
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

                //List<TMP_Dropdown.OptionData> options = Custom_Dropdown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;
                var controlobjects = ACC_Appearance_dropdown.Control.ControlObjects.ToArray();
                List<TMP_Dropdown.OptionData> options = controlobjects[ACC_Appearance_dropdown.CurrentlySelectedIndex].GetComponentInChildren<TMP_Dropdown>().options;
                int kind = 0;
                int index = -1;
                switch (radio.Value)
                {
                    case 0:
                        if (options.Any(x => x.text == ThemeText.Value))
                        {
                            return;
                        }
                        //options.Add(new TMP_Dropdown.OptionData(ThemeText.Value));
                        options.Add(new TMP_Dropdown.OptionData(ThemeText.Value));
                        kind = Defined_Bindings + 1 + Controller.ThisCharactersData.Now_ACC_Name_Dictionary.Count();
                        Controller.ThisCharactersData.Now_ACC_Name_Dictionary[kind] = ThemeText.Value;
                        break;
                    case 1:
                        for (int i = Defined_Bindings + 1; i < options.Count; i++)
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
                        if (index == -1)
                        {
                            return;
                        }
                        options.RemoveAt(index);
                        kind = index + Binding_offset;
                        var removal = Controller.ThisCharactersData.Now_ACC_Binding_Dictionary.Where(x => x.Value == kind).ToArray();
                        for (int i = 0; i < removal.Length; i++)
                        {
                            Controller.ThisCharactersData.Now_ACC_Binding_Dictionary.Remove(removal[i].Key);
                            ACC_Appearance_dropdown.SetValue(removal[i].Key, 0, false);
                        }
                        removal = Controller.ThisCharactersData.Now_ACC_Binding_Dictionary.Where(x => x.Value > kind).ToArray();
                        for (int i = 0; i < removal.Length; i++)
                        {
                            int Change = Controller.ThisCharactersData.Now_ACC_Binding_Dictionary[removal[i].Key] -= 1;
                            ACC_Appearance_dropdown.SetValue(removal[i].Key, Change, false);
                        }
                        Controller.ThisCharactersData.Now_ACC_Name_Dictionary.Remove(kind);
                        break;
                    case 2:
                        if (ACC_Appearance_dropdown.GetSelectedValue() < Defined_Bindings + 1)
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
                        var text = Controller.ThisCharactersData.Now_ACC_Name_Dictionary.First(x => x.Value == options[ACC_Appearance_dropdown.GetSelectedValue()].text);
                        Controller.ThisCharactersData.Now_ACC_Name_Dictionary[text.Key] = ThemeText.Value;
                        options[ACC_Appearance_dropdown.GetSelectedValue()].text = ThemeText.Value;
                        break;
                    default:
                        break;
                }
                foreach (var item in ACC_Appearance_dropdown.Control.ControlObjects)
                {
                    item.GetComponentInChildren<TMP_Dropdown>().options = options;
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

            SetupInterface();

            var interfacebutton = e.AddSidebarControl(new SidebarToggle("ACC States", true, owner));
            interfacebutton.ValueChanged.Subscribe(x => ShowInterface = x);

            //var testtoggle1 = new MakerToggle(category, "Unify", owner);
            //MakerAPI.AddAccessoryWindowControl(testtoggle1).ValueChanged.Subscribe(x => { Settings.Logger.LogWarning($"Normal {testtoggle1.ControlObjects.Count()}"); });
            //testtoggle2 = new MakerToggle(category, "unique", owner);
            //MakerAPI.AddEditableAccessoryWindowControl<MakerToggle, bool>(testtoggle2).ValueChanged += CharaEvent_ValueChanged;

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            ACC_Appearance_dropdown.Control.GroupingID = GroupingID;
            ACC_Appearance_state.Control.GroupingID = GroupingID;
            ACC_Appearance_state2.Control.GroupingID = GroupingID;
            ACC_Is_Parented.Control.GroupingID = GroupingID;
            ApplyTheme.GroupingID = GroupingID;
            ThemeText.GroupingID = GroupingID;
            RadioToggle.GroupingID = GroupingID;
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
            Hooks.SetClothesState += Maker_SetClothes;

            AccessoriesApi.MakerAccSlotAdded += AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;
            AccessoriesApi.AccessoryKindChanged += AccessoriesApi_AccessoryKindChanged;

            MakerAPI.MakerFinishedLoading += (s, e) => VisibiltyToggle();
            AccessoriesApi.SelectedMakerAccSlotChanged += (s, e) => VisibiltyToggle();
            Hooks.Slot_ACC_Change += Hooks_Slot_ACC_Change;
            Hooks.MovIt += Hooks_MovIt;
        }

        private static void Hooks_Slot_ACC_Change(object sender, Slot_ACC_Change_ARG e)
        {
            if (e.Type == 120)
            {
                var ThisCharactersData = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().ThisCharactersData;
                ThisCharactersData.Now_ACC_Binding_Dictionary.Remove(e.SlotNo);
                ThisCharactersData.Now_ACC_State_array.Remove(e.SlotNo);
                ThisCharactersData.Now_Parented_Dictionary.Remove(e.SlotNo);
                ACC_Appearance_dropdown.SetValue(e.SlotNo, 0, false);
                ACC_Appearance_state.SetValue(e.SlotNo, 0, false);
                ACC_Appearance_state2.SetValue(e.SlotNo, 3, false);
                ACC_Is_Parented.SetValue(e.SlotNo, false, false);
            }
            VisibiltyToggle();
        }

        public static void Maker_Ended()
        {
            MakerAPI.MakerExiting -= (s, e) => Maker_Ended();

            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;
            Hooks.SetClothesState -= Maker_SetClothes;

            AccessoriesApi.MakerAccSlotAdded -= AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;
            AccessoriesApi.AccessoryKindChanged -= AccessoriesApi_AccessoryKindChanged;

            AccessoriesApi.SelectedMakerAccSlotChanged -= (s, e) => VisibiltyToggle();
            MakerAPI.MakerFinishedLoading -= (s, e) => VisibiltyToggle();
            Hooks.Slot_ACC_Change -= Hooks_Slot_ACC_Change;
            Hooks.MovIt -= Hooks_MovIt;

            ShowInterface = false;
        }

        private static void Maker_SetClothes(object sender, ClothChangeEventArgs e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            Controller.ChangedOutfit(e.clothesKind, e.state);
            Controller.SetClothesState_switch(e.clothesKind, e.state);
        }

        private static void AccessoriesApi_AccessoryKindChanged(object sender, AccessorySlotEventArgs e)
        {
            VisibiltyToggle();
        }

        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            ACC_Appearance_dropdown.SetValue(e.DestinationSlotIndex, ACC_Appearance_dropdown.GetValue(e.SourceSlotIndex));
            ACC_Appearance_state.SetValue(e.DestinationSlotIndex, ACC_Appearance_state.GetValue(e.SourceSlotIndex));
            ACC_Appearance_state2.SetValue(e.DestinationSlotIndex, ACC_Appearance_state2.GetValue(e.SourceSlotIndex));
            ACC_Is_Parented.SetValue(e.DestinationSlotIndex, ACC_Is_Parented.GetValue(e.SourceSlotIndex));
            VisibiltyToggle();
        }

        private static void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            var ThisCharactersData = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().ThisCharactersData;

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

        private static void ACC_Is_Parented_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<bool> e)
        {
            List<TMP_Dropdown.OptionData> options = ACC_Appearance_dropdown.Control.ControlObjects.ElementAt(e.SlotIndex).GetComponentInChildren<TMP_Dropdown>().options;
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

            if (e.NewValue)
            {
                var Changed_Option = new TMP_Dropdown.OptionData(Controller.Accessorys_Parts[e.SlotIndex].parentKey);

                Controller.ThisCharactersData.Now_Parented_Dictionary[e.SlotIndex] = true;
                if (!options.Any(x => x.text == Changed_Option.text))
                {
                    options.Add(Changed_Option);
                }
            }
            else
            {
                var Dict = Controller.ThisCharactersData.Now_Parented_Name_Dictionary;

                Dict.TryGetValue(e.SlotIndex, out var name);
                if (Dict.Where(x => x.Value == name).Count() == 1)
                {
                    options.RemoveAll(x => x.text == name);
                }
                Controller.ThisCharactersData.Now_Parented_Dictionary.Remove(e.SlotIndex);
            }
            Controller.ThisCharactersData.Update_Parented_Name();
        }

        private static void ACC_Appearance_state_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<float> e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

            if (!Controller.ThisCharactersData.Now_ACC_State_array.TryGetValue(e.SlotIndex, out int[] data))
            {
                Controller.ThisCharactersData.Now_ACC_State_array.Add(e.SlotIndex, new int[] { Mathf.RoundToInt(e.NewValue), 3 });
            }
            else
            {
                data[0] = Mathf.RoundToInt(e.NewValue);
            }
        }

        private static void ACC_Appearance_state2_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<float> e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

            if (!Controller.ThisCharactersData.Now_ACC_State_array.TryGetValue(e.SlotIndex, out int[] data))
            {
                Controller.ThisCharactersData.Now_ACC_State_array.Add(e.SlotIndex, new int[] { 0, Mathf.RoundToInt(e.NewValue) });
            }
            else
            {
                data[1] = Mathf.RoundToInt(e.NewValue);
            }
            var slider = ACC_Appearance_state.Control.ControlObjects.ToArray()[e.SlotIndex].GetComponentInChildren<Slider>();
            var slider2 = ACC_Appearance_state2.Control.ControlObjects.ToArray()[e.SlotIndex].GetComponentInChildren<Slider>();
            if (ACC_Appearance_dropdown.GetValue(e.SlotIndex) > 8)
            {
                Controller.Update_Custom_GUI();
                if (Mathf.RoundToInt(slider2.value) == Mathf.RoundToInt(slider2.maxValue))
                {
                    slider.maxValue += 1;
                    slider2.maxValue += 1;
                }
            }
        }

        private static void ACC_Appearance_dropdown_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<int> e)
        {
            //{ "None", "Top", "Bottom", "Bra", "Panty", "Gloves", "Pantyhose", "Socks", "Shoes" };
            //Settings.Logger.LogWarning($"Dropdown slot {e.SlotIndex}, value {e.NewValue}");
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

            if (e.NewValue < Defined_Bindings + 1)
            {
                Controller.ThisCharactersData.Now_ACC_Binding_Dictionary[e.SlotIndex] = e.NewValue;
            }
            else
            {
                Controller.ThisCharactersData.Now_ACC_Binding_Dictionary[e.SlotIndex] = e.NewValue + Binding_offset;
            }

            if (e.NewValue == 0)
            {
                Controller.ThisCharactersData.Now_ACC_Binding_Dictionary.Remove(e.SlotIndex);
                Controller.ThisCharactersData.Now_ACC_State_array.Remove(e.SlotIndex);
            }
            else if (e.NewValue < 5)
            {
                ACC_Appearance_state2.Control.ControlObjects.ToArray()[e.SlotIndex].GetComponentInChildren<Slider>().maxValue = 3;
                ACC_Appearance_state2.SetValue(e.SlotIndex, 3);
            }
            else if (e.NewValue < Defined_Bindings + 1)
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
            //var Custom_Options = Custom_Dropdown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;
            //var Parented_Options = Parented_Dropdown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;
            var ACC_Appearance_dropdown_Controls = ACC_Appearance_dropdown.Control.ControlObjects.ToList();
            List<TMP_Dropdown.OptionData> Appearance_Options = new List<TMP_Dropdown.OptionData>(ACC_Appearance_dropdown_Controls[0].GetComponentInChildren<TMP_Dropdown>().options);

            //if (Custom_Options.Count > 1)
            //{
            //    Custom_Options.RemoveRange(1, Custom_Options.Count - 1);
            //}

            //if (Parented_Options.Count > 1)
            //{
            //    Parented_Options.RemoveRange(1, Parented_Options.Count - 1);
            //}

            if (Appearance_Options.Count > Defined_Bindings + 1)
            {
                Appearance_Options.RemoveRange(Defined_Bindings + 1, Appearance_Options.Count - (Defined_Bindings + 1));
            }

            foreach (var item in ThisCharactersData.Now_ACC_Name_Dictionary)
            {
                var temp = new TMP_Dropdown.OptionData(item.Value);
                //Custom_Options.Add(temp);
                Appearance_Options.Add(temp);
            }

            //foreach (var item in ThisCharactersData.Now_Parented_Name_Dictionary.Values.Distinct())
            //{
            //    Parented_Options.Add(new TMP_Dropdown.OptionData(item));
            //}

            foreach (var item in ACC_Appearance_dropdown.Control.ControlObjects)
            {
                item.GetComponentInChildren<TMP_Dropdown>().ClearOptions();
                item.GetComponentInChildren<TMP_Dropdown>().AddOptions(Appearance_Options);
            }

            foreach (var item in ThisCharactersData.Now_ACC_Binding_Dictionary)
            {
                if (item.Value < Defined_Bindings + 1)
                {
                    ACC_Appearance_dropdown.SetValue(item.Key, item.Value, false);
                }
                else
                {
                    ACC_Appearance_dropdown.SetValue(item.Key, item.Value - Binding_offset, false);
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

            while (ACC_Is_Parented.Control.ControlObjects.Count() < ACCData)
            {
                yield return null;
            }
            ThisCharactersData.Update_Now_Coordinate();

            Refresh();

            Update_Drop_boxes();

            var ACCAPPS1 = ACC_Appearance_state.Control.ControlObjects;
            var ACCAPPS2 = ACC_Appearance_state2.Control.ControlObjects;
            for (int i = 0; i < ACCData; i++)
            {
                if (!ThisCharactersData.Now_ACC_State_array.TryGetValue(i, out var states))
                {
                    states = new int[] { 0, 3 };
                }
                ACCAPPS2.ElementAt(i).GetComponentInChildren<Slider>().maxValue = Math.Max(3, states[1]);
                ACCAPPS1.ElementAt(i).GetComponentInChildren<Slider>().maxValue = Math.Max(3, states[1]);
                ACC_Appearance_state2.SetValue(i, states[1], false);
                ACC_Appearance_state.SetValue(i, states[0], false);

                ThisCharactersData.Now_Parented_Dictionary.TryGetValue(i, out bool IsParented);
                ACC_Is_Parented.SetValue(i, IsParented, false);

                ThisCharactersData.Now_ACC_Binding_Dictionary.TryGetValue(i, out var binding);
                if (binding > 14)
                {
                    binding -= Binding_offset;
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
                    var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

                    var Max = 1;
                    Controller.ThisCharactersData.Now_ACC_Binding_Dictionary.TryGetValue(Slot, out int Kind);
                    foreach (var item in Controller.ThisCharactersData.Now_ACC_Binding_Dictionary.Where(x => x.Value == Kind))
                    {
                        if (Controller.ThisCharactersData.Now_ACC_State_array.TryGetValue(item.Key, out var states))
                        {
                            Max = Math.Max(Max, states[1]);
                        }
                    }
                    slider.maxValue = ++Max;
                    slider2.maxValue = Max;
                }
            }
        }

        private static void SetupInterface()
        {
            float Height = 50f + Screen.height / 2;
            float Margin = 5f;
            float Width = 130f;

            var distanceFromRightEdge = Screen.width / 10f;
            var x = Screen.width - distanceFromRightEdge - Width - Margin;
            var windowRect = new Rect(x, Margin, Width, Height);

            _Custom_GroupsRect = windowRect;
            _Custom_GroupsRect.x += 7;
            _Custom_GroupsRect.width -= 7;
            _Custom_GroupsRect.y += Height + Margin;
            _Custom_GroupsRect.height = 300f;
        }

        private void Update_Custom_GUI()
        {
            foreach (var Custom in ThisCharactersData.Now_ACC_Name_Dictionary)
            {
                if (!GUI_Custom_Dict.TryGetValue(Custom.Key, out var States))
                {
                    States = new int[] { 0, 2 };
                }
                int max = 0;
                var list = ThisCharactersData.Now_ACC_Binding_Dictionary.Where(x => x.Value == Custom.Key);
                var state_dic = ThisCharactersData.Now_ACC_State_array;
                foreach (var item in list)
                {
                    max = Math.Max(max, state_dic[item.Key][1]);
                }
                States[1] = max + 2;
                GUI_Custom_Dict[Custom.Key] = States;
            }
        }

        private void OnGUI()
        {
            if (!ShowInterface || !MakerAPI.IsInterfaceVisible())
                return;
            GUILayout.BeginArea(_Custom_GroupsRect);
            {
                GUILayout.BeginScrollView(_accessorySlotsScrollPos);
                {
                    GUILayout.BeginVertical();
                    {
                        //Custom Groups
                        foreach (var item in ThisCharactersData.Now_ACC_Name_Dictionary)
                        {
                            DrawCustomButton(item.Key, item.Value);
                        }
                        //Parent
                        foreach (var item in ThisCharactersData.Now_Parented_Name_Dictionary.Values.Distinct())
                        {
                            DrawParentButton(item);
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        private void DrawCustomButton(int Kind, string Name)
        {
            if (!GUI_Custom_Dict.TryGetValue(Kind, out int[] State))
            {
                State = new int[] { 0, 2 };
            }

            if (GUILayout.Button($"{Name}: {State[0]}"))
            {
                State[0] = (State[0] + 1) % State[1];
                Custom_Groups(Kind, State[0]);
            }
            GUILayout.Space(-5);

            GUI_Custom_Dict[Kind] = State;

        }

        private static void DrawParentButton(string Parent)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

            if (!GUI_Parent_Dict.TryGetValue(Parent, out bool isOn))
            {
                isOn = true;
            }
            if (GUILayout.Button($"{Parent}: {(isOn ? "On" : "Off")}"))
            {
                isOn = !isOn;
                Controller.Parent_toggle(Parent, isOn);
            }
            GUILayout.Space(-5);
            GUI_Parent_Dict[Parent] = isOn;
        }

        private static void RadioChanged(int toggle)
        {
            foreach (var item in RadioToggle.ControlObjects)
            {
                var toggles = item.GetComponentsInChildren<Toggle>();
                for (int i = 0; i < 3; i++)
                {
                    toggles[i].isOn = toggle == i;
                }
            }
        }

        private static void Hooks_MovIt(object sender, MovUrAcc_Event e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            var ThisCharactersData = Controller.ThisCharactersData;
            foreach (var item in e.Queue)
            {
                Settings.Logger.LogWarning($"Moving Acc from slot {item.srcSlot} to {item.dstSlot}");
                if (ThisCharactersData.Now_ACC_Binding_Dictionary.TryGetValue(item.srcSlot, out var binding))
                {
                    ThisCharactersData.Now_ACC_Binding_Dictionary[item.dstSlot] = binding;
                    if (binding > Defined_Bindings)
                    {
                        binding -= Binding_offset;
                    }
                    ACC_Appearance_dropdown.SetValue(item.dstSlot, binding, false);
                    ACC_Appearance_dropdown.SetValue(item.srcSlot, 0, false);
                    ThisCharactersData.Now_ACC_Binding_Dictionary.Remove(item.srcSlot);
                }
                if (ThisCharactersData.Now_ACC_State_array.TryGetValue(item.srcSlot, out var states))
                {
                    ThisCharactersData.Now_ACC_State_array[item.dstSlot] = states;
                    ThisCharactersData.Now_ACC_State_array.Remove(item.srcSlot);
                    ACC_Appearance_state.SetValue(item.srcSlot, 0, false);
                    ACC_Appearance_state2.SetValue(item.srcSlot, 3, false);
                    ACC_Appearance_state.SetValue(item.dstSlot, states[0], false);
                    ACC_Appearance_state2.SetValue(item.dstSlot, states[1], false);
                }
                if (ThisCharactersData.Now_Parented_Dictionary.TryGetValue(item.srcSlot, out var Isparented))
                {
                    ACC_Is_Parented.SetValue(item.dstSlot, Isparented, false);
                    ACC_Is_Parented.SetValue(item.srcSlot, false, false);
                    ThisCharactersData.Now_Parented_Dictionary[item.dstSlot] = Isparented;
                    ThisCharactersData.Now_Parented_Dictionary.Remove(item.srcSlot);
                }
            }
        }
    }
}
