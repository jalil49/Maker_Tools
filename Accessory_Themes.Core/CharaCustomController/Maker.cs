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

namespace Accessory_Themes
{
    public partial class CharaEvent : CharaCustomFunctionController
    {

        static readonly MakerColor[] ACC_GUIsliders = new MakerColor[4];
        static MakerColor makerColorSimilar;

        static MakerButton Guibutton;

        //MakerColor PersonalSkewSlider;
        //static MakerColor[] Personal_GUIsliders = new MakerColor[4];

        static MakerColor RelativeSkewColor;

        static MakerDropdown ThemesDropDown_ACC;
        static MakerDropdown ThemesDropDown_Setting;
        static MakerDropdown RelativeDropdown;

        static MakerToggle IsThemeRelativeBool;

        static MakerTextbox CopyTextbox;
        static string Tolerance;

        static readonly string[] bulkrange = new string[] { "1", "2" };

        static bool Clearthemes = false;

        static MakerDropdown SimpleParentDropdown;
        static MakerDropdown ParentDropdown;

        static MakerSlider ValuesSlider;
        static MakerSlider SaturationSlider;

        //MakerCoordinateLoadToggle PersonalSkew_Toggle;

        static bool MakerEnabled = false;

        static AccessoryControlWrapper<MakerDropdown, int> ThemesDropdownwrapper;

        private ChaFileAccessory.PartsInfo[] Parts => ChaControl.nowCoordinate.accessory.parts;
        static CharaEvent ControllerGet => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        public static void MakerAPI_MakerStartedLoading(object sender, RegisterCustomControlsEvent e)
        {
            MakerEnabled = Settings.Enable.Value;
            if (!MakerEnabled)
            {
                return;
            }
            MakerAPI.MakerExiting += MakerAPI_MakerExiting;

            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;
            MakerAPI.MakerFinishedLoading += MakerAPI_MakerFinishedLoading;
            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;
        }

        private static void MakerAPI_MakerFinishedLoading(object sender, EventArgs e)
        {
            AccessoriesApi.MakerAccSlotAdded += AccessoriesApi_MakerAccSlotAdded;
        }

        internal void RemoveOutfitEvent()
        {
            Removeoutfit(data.MaxKey);
        }

        internal void AddOutfitEvent()
        {
            for (var i = data.MaxKey; i < ChaFileControl.coordinate.Length; i++)
                Createoutfit(i);
        }

        public static void MakerAPI_MakerExiting(object sender, EventArgs e)
        {
            MakerAPI.MakerExiting -= MakerAPI_MakerExiting;
            AccessoriesApi.MakerAccSlotAdded += AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;
            MakerAPI.MakerFinishedLoading -= MakerAPI_MakerFinishedLoading;

            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;
        }

        public static void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent registerevent)
        {
            MakerEnabled = Settings.Enable.Value;
            if (!MakerEnabled)
            {
                return;
            }

            var owner = Settings.Instance;
            #region Personal Settings
            //MakerCategory category = new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings");

            //e.AddSubCategory(category);

            #region PersonalTheme
            //string[] outfits = new string[] { "School Uniform", "AfterSchool", "Gym", "Swimsuit", "Club", "Casual", "Nightwear" };
            //var PersonalTheme = new MakerDropdown("Personal Theme: ", outfits, category, 0, owner);

            //var Personal_Theme = MakerAPI.AddControl<MakerDropdown>(PersonalTheme);
            //Personal_Theme.ValueChanged.Subscribe(b => PersonalSkewSlider.SetValue(PersonalColorSkew[b]));
            //PersonalSkewSlider = new MakerColor("Personal Color Skew", true, category, Color.white, owner);
            //e.AddControl(PersonalSkewSlider).ValueChanged.Subscribe(b => PersonalColorSkew[Personal_Theme.Value] = b);

            #region Sliders

            //for (int i = 0; i < 4; i++)
            //{
            //    var slider = new MakerColor($"Color {i + 1}", true, category, Color.white, owner);
            //    Personal_GUIsliders[i] = MakerAPI.AddControl(slider);
            //    Personal_GUIsliders[i].ValueChanged.Subscribe(x =>
            //    {

            //    });
            //}

            #endregion

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
            //PersonalSkew_Toggle = e.AddCoordinateLoadToggle(new MakerCoordinateLoadToggle("Apply Personal Skew", false));

            #endregion

            #endregion
            #region Accessory Window Settings

            var category = new MakerCategory(null, null);
            //e.AddSubCategory(category);

            var emptyarray = new string[] { "None" };

            ThemesDropDown_ACC = new MakerDropdown("Theme: ", emptyarray, category, 0, owner);
            ThemesDropdownwrapper = MakerAPI.AddEditableAccessoryWindowControl<MakerDropdown, int>(ThemesDropDown_ACC, true);
            ThemesDropdownwrapper.ValueChanged += (s, e) => ControllerGet.Themes_ValueChanged(e.SlotIndex, e.NewValue - 1);

            Guibutton = new MakerButton("Accessory Themes GUI", category, owner);
            MakerAPI.AddAccessoryWindowControl(Guibutton, true);
            Guibutton.OnClick.AddListener(delegate () { ShowCustomGui = !ShowCustomGui; });

            #endregion

            #region Accessory Setting Tab
            category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 2, "Accessory Settings");
            registerevent.AddSubCategory(category);

            registerevent.AddControl(new MakerText("Theme Settings", category, owner));

            var Tolerancetext = new MakerTextbox(category, "Tolerance: ", "0.05", owner);
            registerevent.AddControl(Tolerancetext).ValueChanged.Subscribe(x => Tolerance = x);

            var AutoMakeTheme = new MakerButton("Generate Themes automatically", category, owner);
            AutoMakeTheme.OnClick.AddListener(delegate ()
            {
                ControllerGet.AutoTheme();
            });
            registerevent.AddControl(AutoMakeTheme);

            var clearexist = new MakerToggle(category, "Clear Existing Themes", false, owner);
            registerevent.AddControl(clearexist).ValueChanged.Subscribe(x => Clearthemes = x);
            var tempThemesDropDown_Setting = new MakerDropdown("Theme: ", emptyarray, category, 0, owner);

            ThemesDropDown_Setting = MakerAPI.AddControl(tempThemesDropDown_Setting);
            ThemesDropDown_Setting.ValueChanged.Subscribe(x => ControllerGet.Theme_Changed());

            IsThemeRelativeBool = new MakerToggle(category, "Fixed Color Theme", owner);
            registerevent.AddControl(IsThemeRelativeBool).ValueChanged.Subscribe(b =>
            {
                var themenum = CharaEvent.ThemesDropDown_Setting.Value - 1;

                if (themenum < 0)
                {
                    return;
                }
                ControllerGet.Themes[themenum].Isrelative = b;
            });

            #region Sliders

            for (var i = 0; i < 4; i++)
            {
                CreateColorSliders(i, category);
            }

            #endregion

            #region copy color
            CopyTextbox = new MakerTextbox(category, "Accessory to Copy", "1", owner);
            registerevent.AddControl(CopyTextbox);

            var Copybutton = new MakerButton("Copy Color from Accessory", category, owner);
            Copybutton.OnClick.AddListener(delegate ()
            {
                ControllerGet.Copy_ACC_Color();
            });
            registerevent.AddControl(Copybutton);
            #endregion

            #region Parent color stuff
            SimpleParentDropdown = new MakerDropdown("Parent List", Constants.InclusionList.ToArray(), category, 0, owner);
            registerevent.AddControl(SimpleParentDropdown);

            var SimpleParentCopyColorButton = new MakerButton("Copy Theme to parent accessories", category, owner);
            SimpleParentCopyColorButton.OnClick.AddListener(delegate ()
            {
                ControllerGet.ColorSetByParent(true);
            });
            registerevent.AddControl(SimpleParentCopyColorButton);
            var Parentlist = Enum.GetNames(typeof(ChaAccessoryDefine.AccessoryParentKey));
            Parentlist[0] = "None";//pet peave
            ParentDropdown = new MakerDropdown("Advanced Parent List", Parentlist, category, 0, owner);
            registerevent.AddControl(ParentDropdown);
            var ParentCopyColorButton = new MakerButton("Copy Theme to advanced parent", category, owner);
            ParentCopyColorButton.OnClick.AddListener(delegate ()
            {
                ControllerGet.ColorSetByParent();
            });
            registerevent.AddControl(ParentCopyColorButton);
            #endregion

            var SimSlider = new MakerColor("Selected", true, category, new Color(), owner);
            makerColorSimilar = registerevent.AddControl(SimSlider);
            makerColorSimilar.ValueChanged.Subscribe(b => ControllerGet.RelativeAssignColors(b));
            RelativeDropdown = new MakerDropdown("Relative Colors", emptyarray, category, 0, owner);
            RelativeDropdown.ValueChanged.Subscribe(b => ControllerGet.AssignRelativeColorBox(b));
            registerevent.AddControl(RelativeDropdown);


            var SimilarColorButton = new MakerButton("Get Relative Colors", category, owner);
            SimilarColorButton.OnClick.AddListener(delegate ()
            {
                var Controller = ControllerGet;
                if (Controller.Themes.Count == 1)
                {
                    return;
                }
                //AddClothColors();
                Controller.FindRelativeColors();
                Controller.Update_RelativeColor_Dropbox();
                if (Controller.Relative_ACC_Dictionary.Count > 0)
                {
                    makerColorSimilar.SetValue(Controller.Themes[Controller.Relative_ACC_Dictionary[0][0][0]].Colors[Controller.Relative_ACC_Dictionary[0][0][1]], false);
                }
                else
                {
                    makerColorSimilar.SetValue(new Color(), false);
                }
            });
            registerevent.AddControl(SimilarColorButton);

            RelativeSkewColor = new MakerColor("Skew Relative Base", false, category, Color.red, owner);
            RelativeSkewColor = registerevent.AddControl(RelativeSkewColor);

            SaturationSlider = registerevent.AddControl(new MakerSlider(category, "Saturation", -1, 1, 0, owner));

            ValuesSlider = registerevent.AddControl(new MakerSlider(category, "Value", -1, 1, 0, owner));

            var SkewButton = new MakerButton("Apply Skew", category, owner);
            SkewButton.OnClick.AddListener(delegate ()
            {
                ControllerGet.RelativeSkew();
            });
            registerevent.AddControl(SkewButton);
            var UndoSkewButton = new MakerButton("Undo Skew", category, owner);
            UndoSkewButton.OnClick.AddListener(delegate ()
            {
                var Controller = ControllerGet;
                if (Controller.UndoACCSkew.Count == 0)
                {
                    return;
                }
                Controller.RelativeSkew(true);
            });
            registerevent.AddControl(UndoSkewButton);

            #endregion

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            ThemesDropdownwrapper.Control.GroupingID = GroupingID;
            Guibutton.GroupingID = GroupingID;
        }

        private static void CreateColorSliders(int i, MakerCategory category)
        {
            var slider = new MakerColor($"Color {i + 1}", true, category, Color.white, Settings.Instance);
            ACC_GUIsliders[i] = MakerAPI.AddControl(slider);
            ACC_GUIsliders[i].ValueChanged.Subscribe(x =>
            {
                ControllerGet.UpdateSliderColor(i, x);
            });
        }

        private void BulkProcess(int themenum)
        {
            if (themenum < 0)
                return;

            if (int.TryParse(bulkrange[0], out var split1) && int.TryParse(bulkrange[1], out var split2))
            {
                var small = Math.Max(Math.Min(split1, split2) - 1, 0);
                var large = Math.Min(Math.Max(split1, split2), Parts.Length);
                var themeslots = Themes[themenum].ThemedSlots;
                var themed = Theme_Dict;
                for (int slot = small, n = large; slot < n; slot++)
                {
                    if (themed.ContainsKey(slot))
                    {
                        continue;
                    }
                    if (!themeslots.Contains(slot))
                        themeslots.Add(slot);
                    ChangeACCColor(slot, themenum);
                    ThemesDropdownwrapper.SetValue(slot, themenum, false);
                }
            }

            PopulateThemeDict();
        }

        internal void Slot_ACC_Change(int slot, int type)
        {
            if (!MakerEnabled || !MakerAPI.InsideAndLoaded)
            {
                return;
            }
            if (type == 120)
            {
                if (Theme_Dict.TryGetValue(slot, out var themenum) && themenum > -1)
                {
                    Themes[themenum].ThemedSlots.Remove(slot);
                    PopulateThemeDict();
                }
            }
        }

        private void Themes_ValueChanged(int slot, int value)
        {
            var delete = value < 0;

            if (Theme_Dict.TryGetValue(slot, out var bind))
            {
                Themes[bind].ThemedSlots.Remove(slot);
            }

            if (!delete)
            {
                HairAcc = ACI_Ref.HairAcc;
                Theme_Dict[slot] = value;
                Themes[value].ThemedSlots.Add(slot);
                ChangeACCColor(slot, value);
                return;
            }

            Theme_Dict.Remove(slot);
        }

        private static void AccessoriesApi_MakerAccSlotAdded(object sender, AccessorySlotEventArgs e)
        {
            var Controller = ControllerGet;
            Controller.StartCoroutine(Controller.WaitForSlots());
        }

        private static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            var Controller = ControllerGet;
            Controller.StartCoroutine(Controller.WaitForSlots());
        }

        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            ControllerGet.AccessoryTransferred(e);
        }
        private void AccessoryTransferred(AccessoryTransferEventArgs e)
        {
            if (Theme_Dict.TryGetValue(e.SourceSlotIndex, out var ACC_Dict))
            {
                Theme_Dict[e.DestinationSlotIndex] = ACC_Dict;
                ThemesDropdownwrapper.SetValue(e.DestinationSlotIndex, ACC_Dict, false);
            }
            else
            {
                Theme_Dict.Remove(e.DestinationSlotIndex);
                ThemesDropdownwrapper.SetValue(e.DestinationSlotIndex, 0, false);
            }
        }

        private static void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            ControllerGet.AccessoriesCopied(e);
        }

        private void AccessoriesCopied(AccessoryCopyEventArgs e)
        {
            var Source = Coordinate[(int)e.CopySource];
            var Dest = Coordinate[(int)e.CopyDestination];

            for (int i = 0, n = e.CopiedSlotIndexes.Count(); i < n; i++)
            {
                var slot = e.CopiedSlotIndexes.ElementAt(i);

                if (!Source.Theme_Dict.TryGetValue(slot, out var themenum))
                {
                    if (Dest.Theme_Dict.TryGetValue(slot, out var destthemenum))
                    {
                        Dest.Themes[destthemenum].ThemedSlots.Remove(slot);
                    }
                    Dest.Theme_Dict.Remove(slot);
                    continue;
                }

                var sourcetheme = Source.Themes.ElementAt(themenum);
                var destinationtheme = Dest.Themes.FirstOrDefault(x => x.Colors == sourcetheme.Colors && x.Isrelative == sourcetheme.Isrelative && x.ThemeName == sourcetheme.ThemeName);
                if (destinationtheme == null)
                {
                    destinationtheme = new ThemeData(sourcetheme, true);
                    Dest.Themes.Add(destinationtheme);
                }

                if (!destinationtheme.ThemedSlots.Contains(slot))
                    destinationtheme.ThemedSlots.Add(slot);

                Dest.Theme_Dict[slot] = Dest.Themes.IndexOf(sourcetheme);
            }
            PopulateThemeDict((int)e.CopyDestination);
        }

        private void Update_ACC_Dropbox()
        {
            var list = Themes.Select(x => new TMP_Dropdown.OptionData(x.ThemeName));
            var acc_slots = ThemesDropDown_ACC.ControlObjects;
            var options = ThemesDropDown_ACC.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;
            if (options.Count > 1)
            {
                options.RemoveRange(1, options.Count - 1);
            }
            options.AddRange(list);

            for (int slot = 0, n = acc_slots.Count(); slot < n; slot++)
            {
                acc_slots.ElementAt(slot).GetComponentInChildren<TMP_Dropdown>().options = options;
            }
            ThemesDropDown_Setting.ControlObject.GetComponentInChildren<TMP_Dropdown>().options = options;
        }


        private void Update_RelativeColor_Dropbox()
        {
            var options = RelativeDropdown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;

            if (options.Count > 1)
            {
                options.RemoveRange(1, options.Count - 1);
            }

            var dict = Relative_ACC_Dictionary;

            var names = Themes;

            for (var i = 0; i < dict.Count; i++)
            {
                var array = dict[i][0];
                options.Add(new TMP_Dropdown.OptionData($"{names[array[0]].ThemeName}_{array[1] + 1}"));
            }

            //var acc_slots = SimilarDropdown.ControlObject;
            //    acc_slots.GetComponentInChildren<TMP_Dropdown>().options = options;
        }

        private IEnumerator WaitForSlots()
        {
            if (!MakerAPI.InsideMaker || !MakerEnabled)
            {
                yield break;
            }


            do
            {
                yield return null;
            } while (!MakerAPI.InsideAndLoaded);
            makerColorSimilar.SetValue(new Color(), false);

            var set = Theme_Dict;
            Update_ACC_Dropbox();
            Update_RelativeColor_Dropbox();
            for (int SlotIndex = 0, ACC_Count = Parts.Length; SlotIndex < ACC_Count; SlotIndex++)
            {
                if (set.ContainsKey(SlotIndex))
                {
                    ThemesDropdownwrapper.SetValue(SlotIndex, set[SlotIndex], false);
                }
                else
                {
                    ThemesDropdownwrapper.SetValue(SlotIndex, 0, false);
                }
            }
            ThemesDropDown_Setting.SetValue(0, false);
            IsThemeRelativeBool.SetValue(false, false);
        }

        private void AutoTheme()
        {
            if (Clearthemes)
            {
                Themes.Clear();
                Theme_Dict.Clear();
            }


            for (int slot = 0, n = Parts.Length; slot < n; slot++)
            {
                AddThemeValueToList(slot);
            }
        }

        private void AddThemeValueToList(int slot, bool generated = true)
        {

            var themed = Theme_Dict;
            var slotinfo = AccessoriesApi.GetPartsInfo(slot);

            if (themed.ContainsKey(slot) || slotinfo.type < 121)
            {
                return;
            }

            var current = slotinfo.color;

            var theme = new ThemeData($"Gen_Slot {(slot + 1):000}", current);
            theme.ThemedSlots.Add(slot);
            Themes.Add(theme);

            Update_ACC_Dropbox();

            var themecount = Themes.Count;
            themed[slot] = themecount;
            ThemesDropdownwrapper.SetValue(slot, themecount, false);

            if (!generated)
            {
                slot = 0;
            }

            for (var n = Parts.Length; slot < n; slot++)
            {
                var SlotInfo2 = AccessoriesApi.GetPartsInfo(slot);

                if (themed.ContainsKey(slot) || SlotInfo2.type == 120)
                {
                    continue;
                }

                if (ColorComparison(current, SlotInfo2.color))
                {
                    theme.ThemedSlots.Add(slot);
                    themed[slot] = themecount;
                    ThemesDropdownwrapper.SetValue(slot, themecount, false);
                }
            }
        }

        internal void MovIt(List<QueueItem> queue)
        {
            var themes = Themes;
            var themedict = Theme_Dict;
            foreach (var item in queue)
            {
                if (themedict.TryGetValue(item.SrcSlot, out var themenum))
                {
                    themes[themenum].ThemedSlots.Remove(item.SrcSlot);
                    themes[themenum].ThemedSlots.Add(item.DstSlot);
                    ThemesDropdownwrapper.SetValue(item.DstSlot, themenum + 1, false);
                    ThemesDropdownwrapper.SetValue(item.SrcSlot, 0, false);
                }
            }
            PopulateThemeDict();
        }

        private void PopulateThemeDict(int coordnum)
        {
            var coord = Coordinate[coordnum];
            coord.Theme_Dict = PopulateThemeDict(coord);
        }

        private void PopulateThemeDict()
        {
            NowCoordinate.Theme_Dict = PopulateThemeDict(NowCoordinate);
        }

        private Dictionary<int, int> PopulateThemeDict(CoordinateData coordinateData)
        {
            var dict = coordinateData.Theme_Dict;
            var themes = coordinateData.Themes;
            dict.Clear();
            for (int i = 0, n = themes.Count; i < n; i++)
            {
                var theme = themes[i];
                foreach (var slot in theme.ThemedSlots)
                {
                    dict[slot] = i;
                }
            }
            return dict;
        }
    }
}
