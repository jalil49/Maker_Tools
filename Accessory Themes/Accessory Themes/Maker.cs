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

namespace Accessory_Themes
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        static MakerColor ACC_GUIslider1;
        static MakerColor ACC_GUIslider2;
        static MakerColor ACC_GUIslider3;
        static MakerColor ACC_GUIslider4;
        //MakerColor PersonalSkewSlider;
        static MakerColor makerColorSimilar;
        // MakerColor Personal_GUIslider1;
        // MakerColor Personal_GUIslider2;
        // MakerColor Personal_GUIslider3;
        // MakerColor Personal_GUIslider4;
        static MakerColor RelativeSkewColor;

        static MakerDropdown ThemesDropDown_ACC;
        static MakerDropdown ThemesDropDown_Setting;
        static MakerDropdown SimilarDropdown;

        static MakerToggle IsThemeRelativeBool;

        static MakerTextbox CopyTextbox;
        static MakerTextbox ThemeText;
        static MakerTextbox Tolerance;

        static MakerButton ApplyTheme;

        static MakerRadioButtons radio;

        static MakerToggle Clearthemes;

        static MakerDropdown SimpleParentDropdown;
        static MakerDropdown ThemeNamesDropdown;
        static MakerDropdown ParentDropdown;

        static MakerSlider ValuesSlider;
        static MakerSlider SaturationSlider;

        //MakerCoordinateLoadToggle PersonalSkew_Toggle;

        static bool MakerEnabled = false;

        static AccessoryControlWrapper<MakerDropdown, int> Themes;

        public static void MakerAPI_MakerStartedLoading(object sender, RegisterCustomControlsEvent e)
        {
            MakerEnabled = Settings.Enable.Value;
            if (!MakerEnabled)
            {
                return;
            }
            MakerAPI.MakerExiting += MakerAPI_MakerExiting;

            AccessoriesApi.MakerAccSlotAdded += AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;
            AccessoriesApi.SelectedMakerAccSlotChanged += (s, e2) => VisibiltyToggle();
            MakerAPI.MakerFinishedLoading += (s, e2) => VisibiltyToggle();
            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;
            Hooks.Slot_ACC_Change += Hooks_Slot_ACC_Change; ;
        }

        public static void MakerAPI_MakerExiting(object sender, EventArgs e)
        {
            MakerAPI.MakerExiting -= MakerAPI_MakerExiting;
            AccessoriesApi.MakerAccSlotAdded -= AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;
            AccessoriesApi.SelectedMakerAccSlotChanged -= (s, e2) => VisibiltyToggle();
            MakerAPI.MakerFinishedLoading -= (s, e2) => VisibiltyToggle();

            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;
            Hooks.Slot_ACC_Change -= Hooks_Slot_ACC_Change;
            Hooks.MovIt += Hooks_MovIt;
        }

        public static void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            MakerEnabled = Settings.Enable.Value;
            if (!MakerEnabled)
            {
                return;
            }

            var owner = Settings.Instance;
            #region Personal Settings
            MakerCategory category = new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings");

            e.AddSubCategory(category);

            #region PersonalTheme
            //string[] outfits = new string[] { "School Uniform", "AfterSchool", "Gym", "Swimsuit", "Club", "Casual", "Nightwear" };
            //var PersonalTheme = new MakerDropdown("Personal Theme: ", outfits, category, 0, owner);

            //var Personal_Theme = MakerAPI.AddControl<MakerDropdown>(PersonalTheme);
            //Personal_Theme.ValueChanged.Subscribe(b => PersonalSkewSlider.SetValue(PersonalColorSkew[b]));
            //PersonalSkewSlider = new MakerColor("Personal Color Skew", true, category, Color.white, owner);
            //e.AddControl(PersonalSkewSlider).ValueChanged.Subscribe(b => PersonalColorSkew[Personal_Theme.Value] = b);

            ////var Pslider1 = new MakerColor("color 1", true, category, Color.white, owner);
            ////var Pslider2 = new MakerColor("color 2", true, category, Color.white, owner);
            ////var Pslider3 = new MakerColor("color 3", true, category, Color.white, owner);
            ////var Pslider4 = new MakerColor("color 4", true, category, Color.white, owner);

            ////Personal_GUIslider1 = MakerAPI.AddControl(Pslider1);
            ////Personal_GUIslider1.BindToFunctionController<Required_ACC_Controller, Color>(
            ////    (controller) => colors[CoordinateNum][ThemeNamesDropdown.Value][0],
            ////    (controller, value) => colors[CoordinateNum][ThemeNamesDropdown.Value][0] = value
            ////    );

            ////Personal_GUIslider2 = MakerAPI.AddControl(Pslider2);
            ////Personal_GUIslider2.BindToFunctionController<Required_ACC_Controller, Color>(
            ////    (controller) => colors[CoordinateNum][ThemeNamesDropdown.Value][1],
            ////    (controller, value) => colors[CoordinateNum][ThemeNamesDropdown.Value][1] = value
            ////    );

            ////Personal_GUIslider3 = MakerAPI.AddControl(Pslider3);
            ////Personal_GUIslider3.BindToFunctionController<Required_ACC_Controller, Color>(
            ////    (controller) => colors[CoordinateNum][ThemeNamesDropdown.Value][2],
            ////    (controller, value) => colors[CoordinateNum][ThemeNamesDropdown.Value][2] = value
            ////    );
            ////Personal_GUIslider4 = MakerAPI.AddControl(Pslider4);
            ////Personal_GUIslider4.BindToFunctionController<Required_ACC_Controller, Color>(
            ////    (controller) => colors[CoordinateNum][ThemeNamesDropdown.Value][3],
            ////    (controller, value) => colors[CoordinateNum][ThemeNamesDropdown.Value][3] = value
            ////    );

            ////var PersonalCopyTextbox = new MakerTextbox(category, "Accessory to Copy", "1", owner);
            ////PersonalCopyTextbox.BindToFunctionController<Required_ACC_Controller, string>(
            ////    (controller) => CopyACCTextBox,
            ////    (controller, value) => CopyACCTextBox = value);
            ////e.AddControl(PersonalCopyTextbox);


            ////var CopyPersonalbutton = new MakerButton("Copy Color from Accessory", category, owner);
            ////CopyPersonalbutton.OnClick.AddListener(delegate ()
            ////{
            ////    Copy_ACC_Color(true);
            ////});
            ////e.AddControl(CopyPersonalbutton);
            //PersonalSkew_Toggle = e.AddCoordinateLoadToggle(new MakerCoordinateLoadToggle("Apply Personal Skew", false));

            #endregion

            #endregion

            #region Accessory Window Settings
            category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 2, "Accessory Settings");
            //e.AddSubCategory(category);

            ThemesDropDown_ACC = new MakerDropdown("Theme: ", MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().ThemeNames[MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().CoordinateNum].ToArray(), category, 0, owner);
            Themes = MakerAPI.AddEditableAccessoryWindowControl<MakerDropdown, int>(ThemesDropDown_ACC);
            Themes.ValueChanged += Themes_ValueChanged;


            var ThemeTextBox = new MakerTextbox(category, "Name: ", "", owner);
            ThemeText = MakerAPI.AddAccessoryWindowControl<MakerTextbox>(ThemeTextBox);

            radio = MakerAPI.AddAccessoryWindowControl<MakerRadioButtons>(new MakerRadioButtons(category, owner, "Modify", new string[] { "Add", "Remove", "Rename" }));
            radio.ValueChanged.Subscribe(x => RadioChanged(x));

            var AddRemoveThemeButton = new MakerButton("Modify Theme", category, owner);
            ApplyTheme = MakerAPI.AddAccessoryWindowControl<MakerButton>(AddRemoveThemeButton);
            ApplyTheme.OnClick.AddListener(delegate ()
            {
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().AddThemeValueToList(AccessoriesApi.SelectedMakerAccSlot);
            });
            #endregion

            #region Accessory Setting Tab
            category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 2, "Accessory Settings");
            e.AddSubCategory(category);

            e.AddControl(new MakerText("Theme Settings", category, owner));

            Tolerance = new MakerTextbox(category, "Tolerance: ", "0.05", owner);
            e.AddControl(Tolerance);

            var AutoMakeTheme = new MakerButton("Generate Themes automatically", category, owner);
            AutoMakeTheme.OnClick.AddListener(delegate ()
            {
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().AutoTheme();
            });
            e.AddControl(AutoMakeTheme);

            Clearthemes = new MakerToggle(category, "Clear Existing Themes", false, owner);
            e.AddControl(Clearthemes);
            ThemesDropDown_Setting = new MakerDropdown("Theme: ", MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().ThemeNames[MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().CoordinateNum].ToArray(), category, 0, owner);

            ThemeNamesDropdown = MakerAPI.AddControl<MakerDropdown>(ThemesDropDown_Setting);
            ThemeNamesDropdown.ValueChanged.Subscribe(x => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().Theme_Changed());

            IsThemeRelativeBool = new MakerToggle(category, "Fixed Color Theme", owner);
            e.AddControl(IsThemeRelativeBool).ValueChanged.Subscribe(b => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().RelativeThemeBool[MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().CoordinateNum][ThemeNamesDropdown.Value] = b);

            #region Sliders
            var slider1 = new MakerColor("Color 1", true, category, Color.white, owner);
            var slider2 = new MakerColor("Color 2", true, category, Color.white, owner);
            var slider3 = new MakerColor("Color 3", true, category, Color.white, owner);
            var slider4 = new MakerColor("Color 4", true, category, Color.white, owner);

            ACC_GUIslider1 = MakerAPI.AddControl(slider1);
            ACC_GUIslider1.ValueChanged.Subscribe(x =>
            {
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().UpdateSliderColor(0, x);
            });

            ACC_GUIslider2 = MakerAPI.AddControl(slider2);
            ACC_GUIslider2.ValueChanged.Subscribe(x =>
            {
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().UpdateSliderColor(1, x);
            });

            ACC_GUIslider3 = MakerAPI.AddControl(slider3);
            ACC_GUIslider3.ValueChanged.Subscribe(x =>
            {
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().UpdateSliderColor(2, x);
            });

            ACC_GUIslider4 = MakerAPI.AddControl(slider4);
            ACC_GUIslider4.ValueChanged.Subscribe(x =>
            {
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().UpdateSliderColor(3, x);
            });
            #endregion

            #region copy color
            CopyTextbox = new MakerTextbox(category, "Accessory to Copy", "1", owner);
            e.AddControl(CopyTextbox);

            var Copybutton = new MakerButton("Copy Color from Accessory", category, owner);
            Copybutton.OnClick.AddListener(delegate ()
            {
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().Copy_ACC_Color();
            });
            e.AddControl(Copybutton);
            #endregion

            #region Parent color stuff
            SimpleParentDropdown = new MakerDropdown("Parent List", Constants.InclusionList.ToArray(), category, 0, owner);
            e.AddControl(SimpleParentDropdown);

            var SimpleParentCopyColorButton = new MakerButton("Copy Theme to parent accessories", category, owner);
            SimpleParentCopyColorButton.OnClick.AddListener(delegate ()
            {
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().ColorSetByParent(true);
            });
            e.AddControl(SimpleParentCopyColorButton);
            Parentlist[0] = "None";//pet peave
            ParentDropdown = new MakerDropdown("Advanced Parent List", Parentlist, category, 0, owner);
            e.AddControl(ParentDropdown);
            var ParentCopyColorButton = new MakerButton("Copy Theme to advanced parent", category, owner);
            ParentCopyColorButton.OnClick.AddListener(delegate ()
            {
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().ColorSetByParent();
            });
            e.AddControl(ParentCopyColorButton);
            #endregion

            var SimSlider = new MakerColor("Selected", true, category, new Color(), owner);
            makerColorSimilar = e.AddControl(SimSlider);
            makerColorSimilar.ValueChanged.Subscribe(b => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().RelativeAssignColors(b));
            SimilarDropdown = new MakerDropdown("Relative Colors", new string[] { "None" }, category, 0, owner);
            SimilarDropdown.ValueChanged.Subscribe(b => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().AssignRelativeColorBox(b));
            e.AddControl(SimilarDropdown);


            var SimilarColorButton = new MakerButton("Get Relative Colors", category, owner);
            SimilarColorButton.OnClick.AddListener(delegate ()
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
                var CoordinateNum = Controller.CoordinateNum;
                if (Controller.ThemeNames[CoordinateNum].Count == 1)
                {
                    return;
                }
                //AddClothColors();
                Controller.FindRelativeColors();
                Controller.Update_RelativeColor_Dropbox();
                if (Controller.Relative_ACC_Dictionary[CoordinateNum].Count > 0)
                {
                    makerColorSimilar.SetValue(Controller.colors[CoordinateNum][Controller.Relative_ACC_Dictionary[CoordinateNum][0][0][0]][Controller.Relative_ACC_Dictionary[CoordinateNum][0][0][1]], false);
                }
                else
                {
                    makerColorSimilar.SetValue(new Color(), false);
                }
            });
            e.AddControl(SimilarColorButton);

            RelativeSkewColor = new MakerColor("Skew Relative Base", false, category, Color.red, owner);
            RelativeSkewColor = e.AddControl(RelativeSkewColor);

            SaturationSlider = e.AddControl(new MakerSlider(category, "Saturation", -1, 1, 0, owner));

            ValuesSlider = e.AddControl(new MakerSlider(category, "Value", -1, 1, 0, owner));

            var SkewButton = new MakerButton("Apply Skew", category, owner);
            SkewButton.OnClick.AddListener(delegate ()
            {
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().RelativeSkew();
            });
            e.AddControl(SkewButton);
            var UndoSkewButton = new MakerButton("Undo Skew", category, owner);
            UndoSkewButton.OnClick.AddListener(delegate ()
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
                if (Controller.UndoACCSkew[Controller.CoordinateNum].Count == 0)
                {
                    return;
                }
                Controller.RelativeSkew(true);
            });
            e.AddControl(UndoSkewButton);

            #endregion

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            Themes.Control.GroupingID = GroupingID;
            ApplyTheme.GroupingID = GroupingID;
            ThemeText.GroupingID = GroupingID;
            radio.GroupingID = GroupingID;
        }

        private static void Hooks_Slot_ACC_Change(object sender, Slot_ACC_Change_ARG e)
        {
            if (e.SlotNo == 120)
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
                Controller.ACC_Theme_Dictionary[Controller.CoordinateNum].Remove(e.SlotNo);
            }
            VisibiltyToggle();
        }

        private static void Themes_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<int> e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            if (e.NewValue != 0)
            {
                Controller.HairAcc = Controller.ACI_Ref.HairAcc[Controller.CoordinateNum];
                Controller.ACC_Theme_Dictionary[Controller.CoordinateNum][e.SlotIndex] = e.NewValue;
                Controller.ChangeACCColor(e.SlotIndex, e.NewValue);
            }
            else
            {
                Controller.ACC_Theme_Dictionary[Controller.CoordinateNum].Remove(e.SlotIndex);
            }
        }

        private static void VisibiltyToggle()
        {
            if (!MakerAPI.InsideMaker)
                return;

            var accessory = MakerAPI.GetCharacterControl().GetAccessoryObject(AccessoriesApi.SelectedMakerAccSlot);
            if (accessory == null)
            {
                Themes.Control.Visible.OnNext(false);
                ApplyTheme.Visible.OnNext(false);
                ThemeText.Visible.OnNext(false);
                radio.Visible.OnNext(false);
            }
            else
            {
                Themes.Control.Visible.OnNext(true);
                ApplyTheme.Visible.OnNext(true);
                ThemeText.Visible.OnNext(true);
                radio.Visible.OnNext(true);
            }
        }

        private static void AccessoriesApi_MakerAccSlotAdded(object sender, AccessorySlotEventArgs e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            Controller.StartCoroutine(Controller.WaitForSlots());
        }

        private static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            Controller.StartCoroutine(Controller.WaitForSlots());
        }

        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            if (Controller.ACC_Theme_Dictionary[Controller.CoordinateNum].TryGetValue(e.SourceSlotIndex, out int ACC_Dict))
            {
                Controller.ACC_Theme_Dictionary[Controller.CoordinateNum].Add(e.DestinationSlotIndex, ACC_Dict);
                Themes.SetValue(e.DestinationSlotIndex, ACC_Dict);
            }
            VisibiltyToggle();
        }

        private static void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

            var CopiedSlots = e.CopiedSlotIndexes.ToArray();
            var Source = (int)e.CopySource;
            var Dest = (int)e.CopyDestination;
            //Settings.Logger.LogWarning($"Source {Source} Dest {Dest}");
            for (int i = 0; i < CopiedSlots.Length; i++)
            {
                if (!Controller.ACC_Theme_Dictionary[Source].TryGetValue(CopiedSlots[i], out int value))
                {
                    Controller.ACC_Theme_Dictionary[Dest].Remove(CopiedSlots[i]);
                    continue;
                }

                if (!Controller.ThemeNames[Dest].Contains(Controller.ThemeNames[Source][value]))
                {
                    //Settings.Logger.LogWarning($"new theme; count {ThemeNames[Dest].Count}");
                    //foreach (var item in ACC_Theme_Dictionary[Dest])
                    //{
                    //    Settings.Logger.LogWarning(item.Key);
                    //}
                    Controller.ThemeNames[Dest].Add(Controller.ThemeNames[Source][value]);
                    Controller.colors[Dest].Add(Controller.colors[Source][value]);
                    Controller.ACC_Theme_Dictionary[Dest].Add(CopiedSlots[i], Controller.ThemeNames[Dest].Count);
                }
                else
                {
                    //Settings.Logger.LogWarning($"existing theme");
                    int index = Controller.ThemeNames[Dest].IndexOf(Controller.ThemeNames[Source][value]);
                    Controller.ACC_Theme_Dictionary[Dest][CopiedSlots[i]] = index;
                }
            }
            VisibiltyToggle();
        }

        private void Update_ACC_Dropbox()
        {
            List<TMP_Dropdown.OptionData> list = ThemeNames[CoordinateNum].Select(x => new TMP_Dropdown.OptionData(x)).ToList();
            var acc_slots = ThemesDropDown_ACC.ControlObjects;

            for (int slot = 0, n = acc_slots.Count(); slot < n; slot++)
            {
                acc_slots.ElementAt(slot).GetComponentInChildren<TMP_Dropdown>().options = list;
            }

            ThemesDropDown_Setting.ControlObject.GetComponentInChildren<TMP_Dropdown>().options = list;
        }

        private void Update_RelativeColor_Dropbox()
        {
            List<string> color_names = new List<string>();
            var dict = Relative_ACC_Dictionary[CoordinateNum];

            var names = ThemeNames[CoordinateNum];
            for (int i = 0; i < dict.Count; i++)
            {
                color_names.Add($"{names[dict[i][0][0]]}_{dict[i][0][1] + 1}");
            }

            if (color_names.Count == 0)
            {
                color_names.Add("None");
            }
            List<TMP_Dropdown.OptionData> lists = color_names.Select(x => new TMP_Dropdown.OptionData(x)).ToList();
            var old = SimilarDropdown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;
            var acc_slots = SimilarDropdown.ControlObjects.ToList();
            for (int slot = 0; slot < acc_slots.Count; slot++)
            {
                acc_slots[slot].GetComponentInChildren<TMP_Dropdown>().options = lists;
            }
        }

        private IEnumerator WaitForSlots()
        {
            yield return null;
            GetALLACC();
            while (!MakerAPI.InsideAndLoaded || Themes.Control.ControlObjects.Count() < ACCData.Count)
            {
                yield return 0;
            }

            var set = ACC_Theme_Dictionary[CoordinateNum];
            Update_ACC_Dropbox();
            Update_RelativeColor_Dropbox();
            for (int SlotIndex = 0, ACC_Count = ACCData.Count; SlotIndex < ACC_Count; SlotIndex++)
            {
                if (set.ContainsKey(SlotIndex))
                {
                    Themes.SetValue(SlotIndex, set[SlotIndex], false);
                }
                else
                {
                    Themes.SetValue(SlotIndex, 0, false);
                }
            }
            IsThemeRelativeBool.SetValue(RelativeThemeBool[CoordinateNum][ThemeNamesDropdown.Value], false);

            VisibiltyToggle();
        }

        private static void RadioChanged(int toggle)
        {
            foreach (var item in radio.ControlObjects)
            {
                var toggles = item.GetComponentsInChildren<Toggle>();
                for (int i = 0; i < 3; i++)
                {
                    toggles[i].isOn = toggle == i;
                }
            }
        }

        private void AutoTheme()
        {
            GetALLACC();
            var themed = ACC_Theme_Dictionary[CoordinateNum];
            if (Clearthemes.Value)
            {
                ACC_Theme_Dictionary[CoordinateNum].Clear();
                ThemeNames[CoordinateNum].Clear();
                ThemeNames[CoordinateNum].Add("None");
                colors[CoordinateNum].Clear();
                colors[CoordinateNum].Add(new Color[] { new Color(), new Color(), new Color(), new Color() });
            }
            radio.SetValue(0);
            for (int Slot = 0; Slot < ACCData.Count + 20; Slot++)
            {
                ChaFileAccessory.PartsInfo SlotInfo;
                if (Slot < 20)
                {
                    SlotInfo = ChaControl.nowCoordinate.accessory.parts[Slot];
                }
                else
                {
                    SlotInfo = ACCData[Slot - 20];
                }
                if (themed.ContainsKey(Slot) || SlotInfo.type < 121)
                {
                    continue;
                }
                ThemeText.SetValue($"Gen_Slot{(Slot + 1):000}", false);
                AddThemeValueToList(Slot, true);
            }
        }

        private void AddThemeValueToList(int slot, bool Generated = false)
        {
            string Text = ThemeText.Value;
            ChaFileAccessory.PartsInfo SlotInfo;
            if (slot < 20)
            {
                SlotInfo = ChaControl.nowCoordinate.accessory.parts[slot];
            }
            else
            {
                SlotInfo = ACCData[slot - 20];
            }
            if (Text.Length == 0 || radio.Value == 0 && ThemeNames[CoordinateNum].Contains(Text) || Text == "None" || SlotInfo.type == 120)
            {
                return;
            }
            if (radio.Value == 0)
            {
                ThemeNames[CoordinateNum].Add(Text);
                RelativeThemeBool[CoordinateNum].Add(false);
                Color[] current = SlotInfo.color;
                Update_ACC_Dropbox();

                colors[CoordinateNum].Add(current);
                ACC_Theme_Dictionary[CoordinateNum][slot] = ThemeNames[CoordinateNum].Count - 1;
                Themes.SetValue(slot, ThemeNames[CoordinateNum].Count - 1, false);
                var Themed = ACC_Theme_Dictionary[CoordinateNum];
                if (!Generated)
                {
                    slot = 0;
                }
                ChaFileAccessory.PartsInfo SlotInfo2;
                for (; slot < ACCData.Count + 20; slot++)
                {
                    if (slot < 20)
                    {
                        SlotInfo2 = ChaControl.nowCoordinate.accessory.parts[slot];
                    }
                    else
                    {
                        SlotInfo2 = ACCData[slot - 20];
                    }

                    if (Themed.ContainsKey(slot) || SlotInfo2.type == 120)
                    {
                        continue;
                    }
                    if (ColorComparison(current, SlotInfo2.color))
                    {
                        Settings.Logger.LogWarning("Setting " + slot);
                        Themed[slot] = ThemeNames[CoordinateNum].Count - 1;
                        Themes.SetValue(slot, ThemeNames[CoordinateNum].Count - 1, false);
                    }
                }
                Update_ACC_Dropbox();
                return;
            }
            else if (radio.Value == 1)
            {
                int index = ThemeNames[CoordinateNum].IndexOf(Text);
                if (index < 1)
                    return;
                ThemeNames[CoordinateNum].RemoveAt(index);
                colors[CoordinateNum].RemoveAt(index);
                RelativeThemeBool[CoordinateNum].RemoveAt(index);
                var removeindex = ACC_Theme_Dictionary[CoordinateNum].Where(x => x.Value == index).ToArray();
                for (int i = 0; i < removeindex.Length; i++)
                {
                    ACC_Theme_Dictionary[CoordinateNum].Remove(removeindex[i].Key);
                    Themes.SetValue(removeindex[i].Key, 0, false);
                }
                removeindex = ACC_Theme_Dictionary[CoordinateNum].Where(x => x.Value > index).ToArray();
                for (int i = 0; i < removeindex.Length; i++)
                {
                    var next = ACC_Theme_Dictionary[CoordinateNum][removeindex[i].Key] -= 1;
                    Themes.SetValue(removeindex[i].Key, next, false);
                }
            }
            else
            {
                if (ACC_Theme_Dictionary[CoordinateNum].TryGetValue(slot, out int index))
                {
                    if (index < 1)
                    {
                        return;
                    }
                    ThemeNames[CoordinateNum][index] = Text;
                }
            }
            Update_ACC_Dropbox();
        }

        private static void Hooks_MovIt(object sender, MovUrAcc_Event e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            var ACC_Theme_Dictionary = Controller.ACC_Theme_Dictionary[Controller.CoordinateNum];
            foreach (var item in e.Queue)
            {
                if (ACC_Theme_Dictionary.TryGetValue(item.srcSlot, out int themenum))
                {
                    ACC_Theme_Dictionary[item.dstSlot] = themenum;
                    ACC_Theme_Dictionary.Remove(item.dstSlot);
                    Themes.SetValue(item.dstSlot, themenum, false);
                    Themes.SetValue(item.srcSlot, 0, false);
                }
            }
        }
    }
}
