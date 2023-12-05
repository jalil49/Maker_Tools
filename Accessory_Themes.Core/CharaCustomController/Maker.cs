using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using TMPro;
using UniRx;
using UnityEngine;

namespace Accessory_Themes
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private static readonly MakerColor[] AccGuIsliders = new MakerColor[4];
        private static MakerColor _makerColorSimilar;

        private static MakerButton _guibutton;

        //MakerColor PersonalSkewSlider;
        //static MakerColor[] Personal_GUIsliders = new MakerColor[4];

        private static MakerColor _relativeSkewColor;

        private static MakerDropdown _themesDropDownAcc;
        private static MakerDropdown _themesDropDownSetting;
        private static MakerDropdown _relativeDropdown;

        private static MakerToggle _isThemeRelativeBool;

        private static MakerTextbox _copyTextbox;
        private static string _tolerance;

        private static readonly string[] Bulkrange = { "1", "2" };

        private static bool _clearthemes;

        private static MakerDropdown _simpleParentDropdown;
        private static MakerDropdown _parentDropdown;

        private static MakerSlider _valuesSlider;
        private static MakerSlider _saturationSlider;

        //MakerCoordinateLoadToggle PersonalSkew_Toggle;

        private static bool _makerEnabled;

        private static AccessoryControlWrapper<MakerDropdown, int> _themesDropdownwrapper;

        private ChaFileAccessory.PartsInfo[] Parts => ChaControl.nowCoordinate.accessory.parts;
        private static CharaEvent ControllerGet => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        public static void MakerAPI_MakerStartedLoading(object sender, RegisterCustomControlsEvent e)
        {
            _makerEnabled = Settings.Enable.Value;
            if (!_makerEnabled) return;
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
            Removeoutfit(_data.MaxKey);
        }

        internal void AddOutfitEvent()
        {
            for (var i = _data.MaxKey; i < ChaFileControl.coordinate.Length; i++)
                Createoutfit(i);
        }

        private static void MakerAPI_MakerExiting(object sender, EventArgs e)
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
            _makerEnabled = Settings.Enable.Value;
            if (!_makerEnabled) return;

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

            var emptyarray = new[] { "None" };

            _themesDropDownAcc = new MakerDropdown("Theme: ", emptyarray, category, 0, owner);
            _themesDropdownwrapper =
                MakerAPI.AddEditableAccessoryWindowControl<MakerDropdown, int>(_themesDropDownAcc, true);
            _themesDropdownwrapper.ValueChanged +=
                (s, e) => ControllerGet.Themes_ValueChanged(e.SlotIndex, e.NewValue - 1);

            _guibutton = new MakerButton("Accessory Themes GUI", category, owner);
            MakerAPI.AddAccessoryWindowControl(_guibutton, true);
            _guibutton.OnClick.AddListener(delegate { _showCustomGui = !_showCustomGui; });

            #endregion

            #region Accessory Setting Tab

            category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 2,
                "Accessory Settings");
            registerevent.AddSubCategory(category);

            registerevent.AddControl(new MakerText("Theme Settings", category, owner));

            var tolerancetext = new MakerTextbox(category, "Tolerance: ", "0.05", owner);
            registerevent.AddControl(tolerancetext).ValueChanged.Subscribe(x => _tolerance = x);

            var autoMakeTheme = new MakerButton("Generate Themes automatically", category, owner);
            autoMakeTheme.OnClick.AddListener(delegate { ControllerGet.AutoTheme(); });
            registerevent.AddControl(autoMakeTheme);

            var clearexist = new MakerToggle(category, "Clear Existing Themes", false, owner);
            registerevent.AddControl(clearexist).ValueChanged.Subscribe(x => _clearthemes = x);
            var tempThemesDropDownSetting = new MakerDropdown("Theme: ", emptyarray, category, 0, owner);

            _themesDropDownSetting = MakerAPI.AddControl(tempThemesDropDownSetting);
            _themesDropDownSetting.ValueChanged.Subscribe(x => ControllerGet.Theme_Changed());

            _isThemeRelativeBool = new MakerToggle(category, "Fixed Color Theme", owner);
            registerevent.AddControl(_isThemeRelativeBool).ValueChanged.Subscribe(b =>
            {
                var themeNum = _themesDropDownSetting.Value - 1;

                if (themeNum < 0) return;
                ControllerGet.Themes[themeNum].IsRelative = b;
            });

            #region Sliders

            for (var i = 0; i < 4; i++) CreateColorSliders(i, category);

            #endregion

            #region copy color

            _copyTextbox = new MakerTextbox(category, "Accessory to Copy", "1", owner);
            registerevent.AddControl(_copyTextbox);

            var copybutton = new MakerButton("Copy Color from Accessory", category, owner);
            copybutton.OnClick.AddListener(delegate { ControllerGet.Copy_ACC_Color(); });
            registerevent.AddControl(copybutton);

            #endregion

            #region Parent color stuff

            _simpleParentDropdown =
                new MakerDropdown("Parent List", Constants.InclusionList.ToArray(), category, 0, owner);
            registerevent.AddControl(_simpleParentDropdown);

            var simpleParentCopyColorButton = new MakerButton("Copy Theme to parent accessories", category, owner);
            simpleParentCopyColorButton.OnClick.AddListener(delegate { ControllerGet.ColorSetByParent(true); });
            registerevent.AddControl(simpleParentCopyColorButton);
            var parentlist = Enum.GetNames(typeof(ChaAccessoryDefine.AccessoryParentKey));
            parentlist[0] = "None"; //pet peave
            _parentDropdown = new MakerDropdown("Advanced Parent List", parentlist, category, 0, owner);
            registerevent.AddControl(_parentDropdown);
            var parentCopyColorButton = new MakerButton("Copy Theme to advanced parent", category, owner);
            parentCopyColorButton.OnClick.AddListener(delegate { ControllerGet.ColorSetByParent(); });
            registerevent.AddControl(parentCopyColorButton);

            #endregion

            var simSlider = new MakerColor("Selected", true, category, new Color(), owner);
            _makerColorSimilar = registerevent.AddControl(simSlider);
            _makerColorSimilar.ValueChanged.Subscribe(b => ControllerGet.RelativeAssignColors(b));
            _relativeDropdown = new MakerDropdown("Relative Colors", emptyarray, category, 0, owner);
            _relativeDropdown.ValueChanged.Subscribe(b => ControllerGet.AssignRelativeColorBox(b));
            registerevent.AddControl(_relativeDropdown);


            var similarColorButton = new MakerButton("Get Relative Colors", category, owner);
            similarColorButton.OnClick.AddListener(delegate
            {
                var controller = ControllerGet;
                if (controller.Themes.Count == 1) return;
                //AddClothColors();
                controller.FindRelativeColors();
                controller.Update_RelativeColor_Dropbox();
                if (controller.RelativeAccDictionary.Count > 0)
                    _makerColorSimilar.SetValue(
                        controller.Themes[controller.RelativeAccDictionary[0][0][0]]
                            .Colors[controller.RelativeAccDictionary[0][0][1]], false);
                else
                    _makerColorSimilar.SetValue(new Color(), false);
            });
            registerevent.AddControl(similarColorButton);

            _relativeSkewColor = new MakerColor("Skew Relative Base", false, category, Color.red, owner);
            _relativeSkewColor = registerevent.AddControl(_relativeSkewColor);

            _saturationSlider = registerevent.AddControl(new MakerSlider(category, "Saturation", -1, 1, 0, owner));

            _valuesSlider = registerevent.AddControl(new MakerSlider(category, "Value", -1, 1, 0, owner));

            var skewButton = new MakerButton("Apply Skew", category, owner);
            skewButton.OnClick.AddListener(delegate { ControllerGet.RelativeSkew(); });
            registerevent.AddControl(skewButton);
            var undoSkewButton = new MakerButton("Undo Skew", category, owner);
            undoSkewButton.OnClick.AddListener(delegate
            {
                var controller = ControllerGet;
                if (controller.UndoAccSkew.Count == 0) return;
                controller.RelativeSkew(true);
            });
            registerevent.AddControl(undoSkewButton);

            #endregion

            var groupingID = "Maker_Tools_" + Settings.NamingID.Value;
            _themesDropdownwrapper.Control.GroupingID = groupingID;
            _guibutton.GroupingID = groupingID;
        }

        private static void CreateColorSliders(int i, MakerCategory category)
        {
            var slider = new MakerColor($"Color {i + 1}", true, category, Color.white, Settings.Instance);
            AccGuIsliders[i] = MakerAPI.AddControl(slider);
            AccGuIsliders[i].ValueChanged.Subscribe(x => { ControllerGet.UpdateSliderColor(i, x); });
        }

        private void BulkProcess(int themeNum)
        {
            if (themeNum < 0)
                return;

            if (int.TryParse(Bulkrange[0], out var split1) && int.TryParse(Bulkrange[1], out var split2))
            {
                var small = Math.Max(Math.Min(split1, split2) - 1, 0);
                var large = Math.Min(Math.Max(split1, split2), Parts.Length);
                var themedSlots = Themes[themeNum].ThemedSlots;
                var themed = ThemeDict;
                for (var slot = small; slot < large; slot++)
                {
                    if (themed.ContainsKey(slot)) continue;
                    if (!themedSlots.Contains(slot))
                        themedSlots.Add(slot);
                    ChangeAccColor(slot, themeNum);
                    _themesDropdownwrapper.SetValue(slot, themeNum, false);
                }
            }

            PopulateThemeDict();
        }

        internal void Slot_ACC_Change(int slot, int type)
        {
            if (!_makerEnabled || !MakerAPI.InsideAndLoaded) return;
            if (type == 120)
                if (ThemeDict.TryGetValue(slot, out var themeNum) && themeNum > -1)
                {
                    Themes[themeNum].ThemedSlots.Remove(slot);
                    PopulateThemeDict();
                }
        }

        private void Themes_ValueChanged(int slot, int value)
        {
            var delete = value < 0;

            if (ThemeDict.TryGetValue(slot, out var bind)) Themes[bind].ThemedSlots.Remove(slot);

            if (!delete)
            {
                _hairAcc = _aciRef.HairAcc;
                ThemeDict[slot] = value;
                Themes[value].ThemedSlots.Add(slot);
                ChangeAccColor(slot, value);
                return;
            }

            ThemeDict.Remove(slot);
        }

        private static void AccessoriesApi_MakerAccSlotAdded(object sender, AccessorySlotEventArgs e)
        {
            var controller = ControllerGet;
            controller.StartCoroutine(controller.WaitForSlots());
        }

        private static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            var controller = ControllerGet;
            controller.StartCoroutine(controller.WaitForSlots());
        }

        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            ControllerGet.AccessoryTransferred(e);
        }

        private void AccessoryTransferred(AccessoryTransferEventArgs e)
        {
            if (ThemeDict.TryGetValue(e.SourceSlotIndex, out var accDict))
            {
                ThemeDict[e.DestinationSlotIndex] = accDict;
                _themesDropdownwrapper.SetValue(e.DestinationSlotIndex, accDict, false);
            }
            else
            {
                ThemeDict.Remove(e.DestinationSlotIndex);
                _themesDropdownwrapper.SetValue(e.DestinationSlotIndex, 0, false);
            }
        }

        private static void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            ControllerGet.AccessoriesCopied(e);
        }

        private void AccessoriesCopied(AccessoryCopyEventArgs e)
        {
            var source = Coordinate[(int)e.CopySource];
            var dest = Coordinate[(int)e.CopyDestination];

            for (int i = 0, n = e.CopiedSlotIndexes.Count(); i < n; i++)
            {
                var slot = e.CopiedSlotIndexes.ElementAt(i);

                if (!source.ThemeDict.TryGetValue(slot, out var themeNum))
                {
                    if (dest.ThemeDict.TryGetValue(slot, out var destThemeNum))
                        dest.themes[destThemeNum].ThemedSlots.Remove(slot);
                    dest.ThemeDict.Remove(slot);
                    continue;
                }

                var sourceTheme = source.themes.ElementAt(themeNum);
                var destinationTheme = dest.themes.FirstOrDefault(x =>
                    x.Colors == sourceTheme.Colors && x.IsRelative == sourceTheme.IsRelative &&
                    x.ThemeName == sourceTheme.ThemeName);
                if (destinationTheme == null)
                {
                    destinationTheme = new ThemeData(sourceTheme, true);
                    dest.themes.Add(destinationTheme);
                }

                if (!destinationTheme.ThemedSlots.Contains(slot))
                    destinationTheme.ThemedSlots.Add(slot);

                dest.ThemeDict[slot] = dest.themes.IndexOf(sourceTheme);
            }

            PopulateThemeDict((int)e.CopyDestination);
        }

        private void Update_ACC_Dropbox()
        {
            var list = Themes.Select(x => new TMP_Dropdown.OptionData(x.ThemeName));
            var accSlots = _themesDropDownAcc.ControlObjects;
            var options = _themesDropDownAcc.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;
            if (options.Count > 1) options.RemoveRange(1, options.Count - 1);
            options.AddRange(list);

            for (int slot = 0, n = accSlots.Count(); slot < n; slot++)
                accSlots.ElementAt(slot).GetComponentInChildren<TMP_Dropdown>().options = options;
            _themesDropDownSetting.ControlObject.GetComponentInChildren<TMP_Dropdown>().options = options;
        }


        private void Update_RelativeColor_Dropbox()
        {
            var options = _relativeDropdown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;

            if (options.Count > 1) options.RemoveRange(1, options.Count - 1);

            var dict = RelativeAccDictionary;

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
            if (!MakerAPI.InsideMaker || !_makerEnabled) yield break;


            do
            {
                yield return null;
            } while (!MakerAPI.InsideAndLoaded);

            _makerColorSimilar.SetValue(new Color(), false);

            var set = ThemeDict;
            Update_ACC_Dropbox();
            Update_RelativeColor_Dropbox();
            for (int slotIndex = 0, accCount = Parts.Length; slotIndex < accCount; slotIndex++)
                if (set.TryGetValue(slotIndex, out var value))
                    _themesDropdownwrapper.SetValue(slotIndex, value, false);
                else
                    _themesDropdownwrapper.SetValue(slotIndex, 0, false);
            _themesDropDownSetting.SetValue(0, false);
            _isThemeRelativeBool.SetValue(false, false);
        }

        private void AutoTheme()
        {
            if (_clearthemes)
            {
                Themes.Clear();
                ThemeDict.Clear();
            }


            for (int slot = 0, n = Parts.Length; slot < n; slot++) AddThemeValueToList(slot);
        }

        private void AddThemeValueToList(int slot, bool generated = true)
        {
            var themed = ThemeDict;
            var slotInfo = AccessoriesApi.GetPartsInfo(slot);

            if (themed.ContainsKey(slot) || slotInfo.type < 121) return;

            var current = slotInfo.color;

            var theme = new ThemeData($"Gen_Slot {slot + 1:000}", current);
            theme.ThemedSlots.Add(slot);
            Themes.Add(theme);

            Update_ACC_Dropbox();

            var themeCount = Themes.Count;
            themed[slot] = themeCount;
            _themesDropdownwrapper.SetValue(slot, themeCount, false);

            if (!generated) slot = 0;

            for (var n = Parts.Length; slot < n; slot++)
            {
                var slotInfo2 = AccessoriesApi.GetPartsInfo(slot);

                if (themed.ContainsKey(slot) || slotInfo2.type == 120) continue;

                if (ColorComparison(current, slotInfo2.color))
                {
                    theme.ThemedSlots.Add(slot);
                    themed[slot] = themeCount;
                    _themesDropdownwrapper.SetValue(slot, themeCount, false);
                }
            }
        }

        internal void MovIt(List<QueueItem> queue)
        {
            var themes = Themes;
            var themeDict = ThemeDict;
            foreach (var item in queue)
                if (themeDict.TryGetValue(item.SrcSlot, out var themeNum))
                {
                    themes[themeNum].ThemedSlots.Remove(item.SrcSlot);
                    themes[themeNum].ThemedSlots.Add(item.DstSlot);
                    _themesDropdownwrapper.SetValue(item.DstSlot, themeNum + 1, false);
                    _themesDropdownwrapper.SetValue(item.SrcSlot, 0, false);
                }

            PopulateThemeDict();
        }

        private void PopulateThemeDict(int coordNum)
        {
            var coord = Coordinate[coordNum];
            coord.ThemeDict = PopulateThemeDict(coord);
        }

        private void PopulateThemeDict()
        {
            NowCoordinate.ThemeDict = PopulateThemeDict(NowCoordinate);
        }

        private Dictionary<int, int> PopulateThemeDict(CoordinateData coordinateData)
        {
            var dict = coordinateData.ThemeDict;
            var themes = coordinateData.themes;
            dict.Clear();
            for (int i = 0, n = themes.Count; i < n; i++)
            {
                var theme = themes[i];
                foreach (var slot in theme.ThemedSlots) dict[slot] = i;
            }

            return dict;
        }
    }
}