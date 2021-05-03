using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Accessory_Themes_and_Info
{
    public partial class Required_ACC_Controller : CharaCustomFunctionController
    {
        MakerColor ACC_GUIslider1;
        MakerColor ACC_GUIslider2;
        MakerColor ACC_GUIslider3;
        MakerColor ACC_GUIslider4;
        MakerColor PersonalSkewSlider;
        MakerColor makerColorSimilar;
        // MakerColor Personal_GUIslider1;
        // MakerColor Personal_GUIslider2;
        // MakerColor Personal_GUIslider3;
        // MakerColor Personal_GUIslider4;

        MakerDropdown ThemesDropDown_ACC;
        MakerDropdown ThemesDropDown_Setting;
        MakerDropdown SimilarDropdown;

        MakerToggle IsThemeRelativeBool;
        readonly MakerToggle[] CoordinateKeepToggles = new MakerToggle[9];
        MakerTextbox CopyTextbox;

        MakerToggle IsUnderwear;
        MakerTextbox ThemeText;
        MakerButton ApplyTheme;
        MakerRadioButtons radio;

        AccessoryControlWrapper<MakerToggle, bool> AccKeepToggles;
        AccessoryControlWrapper<MakerDropdown, int> Themes;
        AccessoryControlWrapper<MakerToggle, bool> HairKeepToggles;
        MakerToggle clearthemes;
        MakerTextbox ToleranceTextBox;

        MakerDropdown SimpleParentDropdown;
        MakerDropdown ThemeNamesDropdown;
        MakerDropdown ParentDropdown;

        MakerCoordinateLoadToggle PersonalSkew_Toggle;

        bool finishedloading = false;

        private void MakerAPI_MakerStartedLoading(object sender, RegisterCustomControlsEvent e)
        {
            AccessoriesApi.MakerAccSlotAdded += AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;
            AccessoriesApi.SelectedMakerAccSlotChanged += (s, e2) => VisibiltyToggle();

            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;
            Hooks.Slot_ACC_Change += Hooks_Slot_ACC_Change;
        }

        private void MakerAPI_MakerExiting(object sender, EventArgs e)
        {
            AccessoriesApi.MakerAccSlotAdded -= AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;
            AccessoriesApi.SelectedMakerAccSlotChanged -= (s, e2) => VisibiltyToggle();

            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;
            Hooks.Slot_ACC_Change -= Hooks_Slot_ACC_Change;
        }


        private void Hooks_Slot_ACC_Change(object sender, Slot_ACC_Change_ARG e)
        {
            VisibiltyToggle();
        }

        private void MakerAPI_MakerFinishedLoading(object sender, System.EventArgs e)
        {
            VisibiltyToggle();
        }

        private void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;
            #region Personal Settings
            MakerCategory category = new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings");

            e.AddSubCategory(category);

            e.AddControl(new MakerText("Toggle when all Hair accessories and accessories you want to keep on this character are ready.\nExample Mecha Chika who requires her arm and legs accessories", category, owner));
            e.AddControl(new MakerSeparator(category, owner));

            e.AddControl(new MakerToggle(category, "Cosplay Academy Ready", false, owner)).BindToFunctionController<Required_ACC_Controller, bool>(
                 (controller) => Character_Cosplay_Ready,
                 (controller, value) => Character_Cosplay_Ready = value);

            e.AddControl(new MakerText("Select data that shouldn't be overwritten by other mods.\nExample Mecha Chika who doesn't work with socks/pantyhose and requires her shoes/glove", category, owner));

            #region Keep Toggles

            var TopKeep = e.AddControl(new MakerToggle(category, "Keep Top", owner));

            TopKeep.BindToFunctionController<Required_ACC_Controller, bool>(
                (controller) => PersonalClothingBools[0],
                (controller, value) => PersonalClothingBools[0] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep Bottom", owner));

            TopKeep.BindToFunctionController<Required_ACC_Controller, bool>(
                (controller) => PersonalClothingBools[1],
                (controller, value) => PersonalClothingBools[1] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep Bra", owner));

            TopKeep.BindToFunctionController<Required_ACC_Controller, bool>(
                (controller) => PersonalClothingBools[2],
                (controller, value) => PersonalClothingBools[2] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep Underwear", owner));

            TopKeep.BindToFunctionController<Required_ACC_Controller, bool>(
                (controller) => PersonalClothingBools[3],
                (controller, value) => PersonalClothingBools[3] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep Gloves", owner));

            TopKeep.BindToFunctionController<Required_ACC_Controller, bool>(
                (controller) => PersonalClothingBools[4],
                (controller, value) => PersonalClothingBools[4] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep Pantyhose", owner));

            TopKeep.BindToFunctionController<Required_ACC_Controller, bool>(
                (controller) => PersonalClothingBools[5],
                (controller, value) => PersonalClothingBools[5] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep socks", owner));

            TopKeep.BindToFunctionController<Required_ACC_Controller, bool>(
                (controller) => PersonalClothingBools[6],
                (controller, value) => PersonalClothingBools[6] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep Indoor Shoes", owner));

            TopKeep.BindToFunctionController<Required_ACC_Controller, bool>(
                (controller) => PersonalClothingBools[7],
                (controller, value) => PersonalClothingBools[7] = value);

            TopKeep = e.AddControl(new MakerToggle(category, "Keep Outdoor Shoes", owner));
            TopKeep.BindToFunctionController<Required_ACC_Controller, bool>(
                (controller) => PersonalClothingBools[8],
                (controller, value) => PersonalClothingBools[8] = value);

            #endregion

            #region PersonalTheme
            string[] outfits = new string[] { "School Uniform", "AfterSchool", "Gym", "Swimsuit", "Club", "Casual", "Nightwear" };
            var PersonalTheme = new MakerDropdown("Personal Theme: ", outfits, category, 0, owner);

            var Personal_Theme = MakerAPI.AddControl<MakerDropdown>(PersonalTheme);
            Personal_Theme.ValueChanged.Subscribe(b => PersonalSkewSlider.SetValue(PersonalColorSkew[b]));
            PersonalSkewSlider = new MakerColor("Personal Color Skew", true, category, Color.white, owner);
            e.AddControl(PersonalSkewSlider).ValueChanged.Subscribe(b => PersonalColorSkew[Personal_Theme.Value] = b);

            //var Pslider1 = new MakerColor("color 1", true, category, Color.white, owner);
            //var Pslider2 = new MakerColor("color 2", true, category, Color.white, owner);
            //var Pslider3 = new MakerColor("color 3", true, category, Color.white, owner);
            //var Pslider4 = new MakerColor("color 4", true, category, Color.white, owner);

            //Personal_GUIslider1 = MakerAPI.AddControl(Pslider1);
            //Personal_GUIslider1.BindToFunctionController<Required_ACC_Controller, Color>(
            //    (controller) => colors[CoordinateNum][ThemeNamesDropdown.Value][0],
            //    (controller, value) => colors[CoordinateNum][ThemeNamesDropdown.Value][0] = value
            //    );

            //Personal_GUIslider2 = MakerAPI.AddControl(Pslider2);
            //Personal_GUIslider2.BindToFunctionController<Required_ACC_Controller, Color>(
            //    (controller) => colors[CoordinateNum][ThemeNamesDropdown.Value][1],
            //    (controller, value) => colors[CoordinateNum][ThemeNamesDropdown.Value][1] = value
            //    );

            //Personal_GUIslider3 = MakerAPI.AddControl(Pslider3);
            //Personal_GUIslider3.BindToFunctionController<Required_ACC_Controller, Color>(
            //    (controller) => colors[CoordinateNum][ThemeNamesDropdown.Value][2],
            //    (controller, value) => colors[CoordinateNum][ThemeNamesDropdown.Value][2] = value
            //    );
            //Personal_GUIslider4 = MakerAPI.AddControl(Pslider4);
            //Personal_GUIslider4.BindToFunctionController<Required_ACC_Controller, Color>(
            //    (controller) => colors[CoordinateNum][ThemeNamesDropdown.Value][3],
            //    (controller, value) => colors[CoordinateNum][ThemeNamesDropdown.Value][3] = value
            //    );

            //var PersonalCopyTextbox = new MakerTextbox(category, "Accessory to Copy", "1", owner);
            //PersonalCopyTextbox.BindToFunctionController<Required_ACC_Controller, string>(
            //    (controller) => CopyACCTextBox,
            //    (controller, value) => CopyACCTextBox = value);
            //e.AddControl(PersonalCopyTextbox);


            //var CopyPersonalbutton = new MakerButton("Copy Color from Accessory", category, owner);
            //CopyPersonalbutton.OnClick.AddListener(delegate ()
            //{
            //    Copy_ACC_Color(true);
            //});
            //e.AddControl(CopyPersonalbutton);
            #endregion

            #endregion

            #region Accessory Window Settings
            category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 2, "Accessory Settings");
            //e.AddSubCategory(category);
            #region Toggles
            var Keep = new MakerToggle(category, "Keep this Accessory", owner);
            AccKeepToggles = MakerAPI.AddEditableAccessoryWindowControl<MakerToggle, bool>(Keep);
            AccKeepToggles.ValueChanged += AccKeep_ValueChanged;

            var HairKeep = new MakerToggle(category, "Is this a hair piece?", owner);
            HairKeepToggles = MakerAPI.AddEditableAccessoryWindowControl<MakerToggle, bool>(HairKeep);
            HairKeepToggles.ValueChanged += AccHairKeep_ValueChanged;
            #endregion

            ThemesDropDown_ACC = new MakerDropdown("Theme: ", ThemeNames[CoordinateNum].ToArray(), category, 0, owner);
            Themes = MakerAPI.AddEditableAccessoryWindowControl<MakerDropdown, int>(ThemesDropDown_ACC);
            Themes.ValueChanged += Themes_ValueChanged;


            var ThemeTextBox = new MakerTextbox(category, "Name: ", "", owner);
            ThemeText = MakerAPI.AddAccessoryWindowControl<MakerTextbox>(ThemeTextBox);

            radio = MakerAPI.AddAccessoryWindowControl<MakerRadioButtons>(new MakerRadioButtons(category, owner, "Theme option", 0, new string[] { "Add Theme", "Remove Theme", "Rename" }));
            radio.ValueChanged.Subscribe(b => RadioChanged(b));

            var AddRemoveThemeButton = new MakerButton("add/remove Theme", category, owner);
            ApplyTheme = MakerAPI.AddAccessoryWindowControl<MakerButton>(AddRemoveThemeButton);
            ApplyTheme.OnClick.AddListener(delegate ()
            {
                AddThemeValueToList();
            });
            #endregion

            #region Accessory Setting Tab
            category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 2, "Accessory Settings");
            e.AddSubCategory(category);

            e.AddControl(new MakerText("Accessory Settings", category, owner));

            ToleranceTextBox = new MakerTextbox(category, "Tolerance: ", "0.05", owner);
            e.AddControl(ToleranceTextBox);

            var AutoMakeTheme = new MakerButton("Generate Themes automatically", category, owner);
            AutoMakeTheme.OnClick.AddListener(delegate ()
            {
                AutoTheme();
            });
            e.AddControl(AutoMakeTheme);

            clearthemes = new MakerToggle(category, "clear existing: ", false, owner);
            e.AddControl(clearthemes);

            ThemesDropDown_Setting = new MakerDropdown("Theme: ", ThemeNames[CoordinateNum].ToArray(), category, 0, owner);

            ThemeNamesDropdown = MakerAPI.AddControl<MakerDropdown>(ThemesDropDown_Setting);
            ThemeNamesDropdown.ValueChanged.Subscribe(x => Theme_Changed());

            IsThemeRelativeBool = new MakerToggle(category, "Fixed Color Theme", owner);
            e.AddControl(IsThemeRelativeBool).ValueChanged.Subscribe(b => RelativeThemeBool[CoordinateNum][ThemeNamesDropdown.Value] = b);

            #region Sliders
            var slider1 = new MakerColor("color 1", true, category, Color.white, owner);
            var slider2 = new MakerColor("color 2", true, category, Color.white, owner);
            var slider3 = new MakerColor("color 3", true, category, Color.white, owner);
            var slider4 = new MakerColor("color 4", true, category, Color.white, owner);

            ACC_GUIslider1 = MakerAPI.AddControl(slider1);
            ACC_GUIslider1.BindToFunctionController<Required_ACC_Controller, Color>(
                                (controller) => colors[CoordinateNum][ThemeNamesDropdown.Value][0],
                (controller, value) => UpdateSliderColor(0, value)
                );

            ACC_GUIslider2 = MakerAPI.AddControl(slider2);
            ACC_GUIslider2.BindToFunctionController<Required_ACC_Controller, Color>(
                                (controller) => colors[CoordinateNum][ThemeNamesDropdown.Value][1],
                (controller, value) => UpdateSliderColor(1, value)
                );

            ACC_GUIslider3 = MakerAPI.AddControl(slider3);
            ACC_GUIslider3.BindToFunctionController<Required_ACC_Controller, Color>(
                                (controller) => colors[CoordinateNum][ThemeNamesDropdown.Value][2],
                (controller, value) => UpdateSliderColor(2, value)
                );
            ACC_GUIslider4 = MakerAPI.AddControl(slider4);
            ACC_GUIslider4.BindToFunctionController<Required_ACC_Controller, Color>(
                                (controller) => colors[CoordinateNum][ThemeNamesDropdown.Value][3],
                (controller, value) => UpdateSliderColor(3, value)
                );
            #endregion

            #region copy color
            CopyTextbox = new MakerTextbox(category, "Accessory to Copy", "1", owner);
            e.AddControl(CopyTextbox);

            var Copybutton = new MakerButton("Copy Color from Accessory", category, owner);
            Copybutton.OnClick.AddListener(delegate ()
            {
                Copy_ACC_Color();
            });
            e.AddControl(Copybutton);
            #endregion

            #region Parent color stuff
            SimpleParentDropdown = new MakerDropdown("Parent List", Constants.InclusionList.ToArray(), category, 0, owner);
            e.AddControl(SimpleParentDropdown);

            var SimpleParentCopyColorButton = new MakerButton("Copy Theme to parent accessories", category, owner);
            SimpleParentCopyColorButton.OnClick.AddListener(delegate ()
            {
                ColorSetByParent(true);
            });
            e.AddControl(SimpleParentCopyColorButton);
            Parentlist[0] = "None";//pet peave
            ParentDropdown = new MakerDropdown("Advanced Parent List", Parentlist, category, 0, owner);
            e.AddControl(ParentDropdown);
            var ParentCopyColorButton = new MakerButton("Copy Theme to advanced parent", category, owner);
            ParentCopyColorButton.OnClick.AddListener(delegate ()
            {
                ColorSetByParent();
            });
            e.AddControl(ParentCopyColorButton);
            #endregion

            var SimSlider = new MakerColor("rel", true, category, new Color(), owner);
            makerColorSimilar = e.AddControl(SimSlider);
            makerColorSimilar.ValueChanged.Subscribe(b => RelativeAssignColors(b));
            SimilarDropdown = new MakerDropdown("Relative Colors", new string[] { "None" }, category, 0, owner);
            SimilarDropdown.ValueChanged.Subscribe(b => AssignRelativeColorBox(b));
            e.AddControl(SimilarDropdown);


            var SimilarColorButton = new MakerButton("get relative colors", category, owner);
            SimilarColorButton.OnClick.AddListener(delegate ()
            {
                if (ThemeNames[CoordinateNum].Count == 1)
                {
                    return;
                }
                disableupdate = true;
                //AddClothColors();
                FindRelativeColors();
                Update_RelativeColor_Dropbox();
                if (Relative_ACC_Dictionary[CoordinateNum].Count > 0)
                {
                    makerColorSimilar.Value = colors[CoordinateNum][Relative_ACC_Dictionary[CoordinateNum][0][0][0]][Relative_ACC_Dictionary[CoordinateNum][0][0][1]];
                }
                else
                {
                    makerColorSimilar.SetValue(new Color());
                }
                disableupdate = false;
            });
            e.AddControl(SimilarColorButton);

            var RelativeSkewColor = new MakerColor("Skew Relative Base", true, category, Color.white, owner);
            RelativeSkewColor = e.AddControl(RelativeSkewColor);
            RelativeSkewColor.ValueChanged.Subscribe(b => RelativeColorSkew = b);

            e.AddControl(new MakerToggle(category, "Subtract Hue", owner)).ValueChanged.Subscribe(b => SubtractHue = b);
            e.AddControl(new MakerToggle(category, "Subtract Saturation", owner)).ValueChanged.Subscribe(b => SubtractSaturation = b);
            e.AddControl(new MakerToggle(category, "Subtract Value", owner)).ValueChanged.Subscribe(b => SubtractValue = b);

            var SkewButton = new MakerButton("Apply Skew", category, owner);
            SkewButton.OnClick.AddListener(delegate ()
            {
                RelativeSkew();
            });
            e.AddControl(SkewButton);
            var UndoSkewButton = new MakerButton("Undo Skew", category, owner);
            UndoSkewButton.OnClick.AddListener(delegate ()
            {
                if (UndoACCSkew[CoordinateNum].Count == 0)
                {
                    return;
                }
                RelativeSkew(true);
            });
            e.AddControl(UndoSkewButton);

            #endregion

            #region Clothing Settings

            category = new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings");
            e.AddSubCategory(category);
            e.AddControl(new MakerText("Settings to be applied when saving coordinates\nThis isn't saved to Character Cards", category, owner));
            CoordinateKeepToggles[0] = new MakerToggle(category, $"Keep Top", owner);
            CoordinateKeepToggles[1] = new MakerToggle(category, $"Keep Bottom", owner);
            CoordinateKeepToggles[2] = new MakerToggle(category, $"Keep Bra", owner);
            CoordinateKeepToggles[3] = new MakerToggle(category, $"Keep Underwear", owner);
            CoordinateKeepToggles[4] = new MakerToggle(category, $"Keep Gloves", owner);
            CoordinateKeepToggles[5] = new MakerToggle(category, $"Keep Pantyhose", owner);
            CoordinateKeepToggles[6] = new MakerToggle(category, $"Keep Socks", owner);
            CoordinateKeepToggles[7] = new MakerToggle(category, $"Keep Indoor Shoes", owner);
            CoordinateKeepToggles[8] = new MakerToggle(category, $"Keep Outdoor Shoes", owner);
            e.AddControl(CoordinateKeepToggles[0]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][0] = x);
            e.AddControl(CoordinateKeepToggles[1]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][1] = x);
            e.AddControl(CoordinateKeepToggles[2]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][2] = x);
            e.AddControl(CoordinateKeepToggles[3]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][3] = x);
            e.AddControl(CoordinateKeepToggles[4]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][4] = x);
            e.AddControl(CoordinateKeepToggles[5]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][5] = x);
            e.AddControl(CoordinateKeepToggles[6]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][6] = x);
            e.AddControl(CoordinateKeepToggles[7]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][7] = x);
            e.AddControl(CoordinateKeepToggles[8]).ValueChanged.Subscribe(x => CoordinateSaveBools[CoordinateNum][8] = x);
            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Underwear settings", category, owner));

            IsUnderwear = new MakerToggle(category, "Is underwear coordinate", owner);
            e.AddControl(IsUnderwear).ValueChanged.Subscribe(x => underwearbool = x);

            #endregion

            PersonalSkew_Toggle = e.AddCoordinateLoadToggle(new MakerCoordinateLoadToggle("Apply Personal Skew", false));

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            AccKeepToggles.Control.GroupingID = GroupingID;
            Themes.Control.GroupingID = GroupingID;
            HairKeepToggles.Control.GroupingID = GroupingID;
            ApplyTheme.GroupingID = GroupingID;
            ThemeText.GroupingID = GroupingID;
            radio.GroupingID = GroupingID;
        }


        private void Themes_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<int> e)
        {
            if (disableupdate)
            {
                return;
            }
            Settings.Logger.LogWarning("color themes trigger");
            if (e.NewValue != 0)
            {
                ACC_Theme_Dictionary[CoordinateNum][e.SlotIndex] = e.NewValue;
                ChangeACCColor(e.SlotIndex, e.NewValue);
            }
            else
            {
                ACC_Theme_Dictionary[CoordinateNum].Remove(e.SlotIndex);
            }
        }

        private void AccHairKeep_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<bool> e)
        {
            if (HairAcc == null)
            {
                return;
            }
            if (e.NewValue)
            {
                HairAcc[CoordinateNum].Add(e.SlotIndex);
            }
            else
            {
                HairAcc[CoordinateNum].Remove(e.SlotIndex);
            }
        }

        private void AccKeep_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<bool> e)
        {
            if (AccKeep == null)
            {
                return;
            }
            if (e.NewValue)
            {
                AccKeep[CoordinateNum].Add(e.SlotIndex);
            }
            else
            {
                AccKeep[CoordinateNum].Remove(e.SlotIndex);
            }
        }

        private void RadioChanged(int value)
        {
            var ControlObjects = radio.ControlObjects.ToArray();
            foreach (var ControlObject in ControlObjects)
            {
                var Toggles = ControlObject.GetComponentsInChildren<Toggle>();
                for (int Index = 0, n2 = Toggles.Length; Index < n2; Index++)
                {
                    Toggles[Index].isOn = value == Index;
                }
            }
        }

        private void VisibiltyToggle()
        {
            if (!MakerAPI.InsideMaker)
                return;

            var accessory = MakerAPI.GetCharacterControl().GetAccessoryObject(AccessoriesApi.SelectedMakerAccSlot);
            if (accessory == null)
            {

                AccKeepToggles.Control.Visible.OnNext(false);
                Themes.Control.Visible.OnNext(false);
                HairKeepToggles.Control.Visible.OnNext(false);
                ApplyTheme.Visible.OnNext(false);
                ThemeText.Visible.OnNext(false);
                radio.Visible.OnNext(false);
            }
            else
            {
                HairKeepToggles.Control.Visible.OnNext(true);
                Themes.Control.Visible.OnNext(true);
                AccKeepToggles.Control.Visible.OnNext(true);
                ApplyTheme.Visible.OnNext(true);
                ThemeText.Visible.OnNext(true);
                radio.Visible.OnNext(true);
            }
        }

        private void AccessoriesApi_MakerAccSlotAdded(object sender, AccessorySlotEventArgs e)
        {
            GetALLACC();
            Update_ACC_Dropbox();
        }
    }
}
