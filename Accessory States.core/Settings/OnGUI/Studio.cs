#if Studio
using Accessory_States.Classes.PresetStorage;
using Extensions.GUI_Classes;
using KKAPI.Studio;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

using static Extensions.OnGUIExtensions;
using GL = UnityEngine.GUILayout;



namespace Accessory_States
{
    public class Studio
    {
        #region Windows
        private readonly WindowGUI _slotWindow;//main SelectedSlot window
        private readonly WindowGUI _groupGUI;//General NameData window, add, change, remove Namedata and other modifications
        private readonly WindowGUI _presetWindow;
        private readonly WindowGUI _bindingSelect;
        private readonly WindowGUI _previewWindow;
        private readonly WindowGUI _settingWindow;
        private readonly WindowGUI _characterSelect;
        private readonly WindowGUI _accessorySelectWindow;
        #endregion

        #region Scrolls for windows
        private readonly ScrollGUI _slotWindowScroll;
        private readonly ScrollGUI _bindingScroll;

        private readonly ScrollGUI _groupScroll;
        private readonly ScrollGUI _previewScroll;
        private readonly ScrollGUI _settingScroll;
        private readonly ScrollGUI _singlePresetScroll;
        private readonly ScrollGUI _folderPresetScroll;
        private readonly ScrollGUI _charaSelectScroll;
        private readonly ScrollGUI _accessorySelectScroll;
        #endregion

        #region Settings
        private readonly ToolbarGUI _AssPreference;
        #endregion

        private readonly ToolbarGUI ShoeTypeGUI;
        private readonly ToolbarGUI PresetView;
        private readonly TextFieldGUI PresetFilter;


        #region Properties

        private int SelectedDropDown
        {
            get
            {
                if (SelectedCharaEventControls == null)
                    return -1;
                return SelectedCharaEventControls.SelectedDropDown;
            }
            set
            {
                if (SelectedCharaEventControls == null)
                    return;
                SelectedCharaEventControls.SelectedDropDown = value;
            }
        }

        private int SelectedSlot
        {
            get
            {
                if (SelectedCharaEventControls == null)
                    return -1;
                return SelectedCharaEventControls.SelectedSlot;
            }
            set
            {
                if (SelectedCharaEventControls == null)
                    return;
                SelectedCharaEventControls.SelectedSlot = value;
            }
        }

        private SlotData SelectedSlotData
        {
            get
            {
                var control = GetController;
                if (control == null || SelectedSlot < 0)
                    return null;

                if (!control.SlotBindingData.TryGetValue(SelectedSlot, out var slotData))
                {
                    slotData = GetController.SlotBindingData[SelectedSlot] = new SlotData();
                }

                return slotData;
            }
        }

        private Dictionary<NameData, NameDataControls> NameControls
        {
            get
            {
                if (SelectedCharaEventControls == null)
                    return null;
                return SelectedCharaEventControls.NameControls;
            }
        }

        private Dictionary<StateInfo, StateInfoControls> StateControls
        {
            get
            {
                if (SelectedCharaEventControls == null)
                    return null;
                return SelectedCharaEventControls.StateControls;
            }
        }

        private Dictionary<PresetData, PresetContols> SingleControls
        {
            get
            {
                if (SelectedCharaEventControls == null)
                    return null;
                return SelectedCharaEventControls.SingleControls;
            }
        }

        private Dictionary<PresetFolder, PresetFolderContols> PresetFolderControls
        {
            get
            {
                if (SelectedCharaEventControls == null)
                    return null;
                return SelectedCharaEventControls.PresetFolderControls;
            }
        }
        #endregion

        private readonly List<PresetData> SinglePresetDatas = new List<PresetData>();
        private readonly List<PresetFolder> presetFolders = new List<PresetFolder>();

        private readonly Dictionary<CharaEvent, CharaEventControl> CharaControls = new Dictionary<CharaEvent, CharaEventControl>();
        private int selectedChara = 0;

        private CharaEvent[] CharaEvents;

        private CharaEvent GetController;
        private CharaEventControl SelectedCharaEventControls;

        #region Settings

        #endregion

        public Studio()
        {
            #region Windows
            var windowcount = 1;
            _slotWindow = new WindowGUI(windowcount++, Settings.SlotWindowRectStudio, Settings.SlotWindowTransparencyStudio, SlotWindowDraw, new GUIContent("Slot 0", "Modify data attached to this Accessory"));

            _presetWindow = new WindowGUI(windowcount++, Settings.PresetWindowRectStudio, Settings.PresetWindowTransparencyStudio, PresetWindowDraw, new GUIContent("Presets", "Apply, Create or Modify Presets"));

            _groupGUI = new WindowGUI(windowcount++, Settings.GroupWindowRectStudio, Settings.GroupWindowTransparencyStudio, GroupWindowDraw, new GUIContent("Custom Group Data", "Modify Custom Groups"));

            _settingWindow = new WindowGUI(windowcount++, Settings.SettingWindowRectStudio, Settings.SettingWindowTransparencyStudio, SettingWindowDraw, new GUIContent("Settings", "Modify Plugin Settings"));

            _bindingSelect = new WindowGUI(windowcount++, Settings.AddBindingWindowRectStudio, Settings.AddBindingWindowTransparencyStudio, SelectBindingWindowDraw, new GUIContent("Group Select", "Select a group to Show"));

            _previewWindow = new WindowGUI(windowcount++, Settings.PreviewWindowRectStudio, Settings.PreviewWindowTransparencyStudio, PreviewWindowDraw, new GUIContent("State Preview", "Adjust State Values"));

            _characterSelect = new WindowGUI(windowcount++, Settings.CharaSelectWindowRectStudio, Settings.CharaSelectWindowTransparencyStudio, CharaSelectWindowDraw, new GUIContent("Character Select", "Select from selected Characters"));

            _accessorySelectWindow = new WindowGUI(windowcount++, Settings.AccessorySelectWindowRectStudio, Settings.AccessorySelectWindowTransparencyStudio, AccessorySelectWindowDraw, new GUIContent("Accessory Select", "Select Accessories"));
            #endregion

            #region Scrolls
            var scrollLayoutElements = new GUILayoutOption[] { };

            _slotWindowScroll = new ScrollGUI()
            {
                action = SlotWindowScroll,
                layoutOptions = scrollLayoutElements
            };


            _groupScroll = new ScrollGUI()
            {
                action = GroupWindowScroll,
                layoutOptions = scrollLayoutElements
            };


            _bindingScroll = new ScrollGUI()
            {
                action = BindingWindowScroll,
                layoutOptions = scrollLayoutElements
            };

            _previewScroll = new ScrollGUI()
            {
                action = PreviewScroll,
                layoutOptions = scrollLayoutElements
            };

            _settingScroll = new ScrollGUI()
            {
                action = SettingScroll,
                layoutOptions = scrollLayoutElements
            };

            _singlePresetScroll = new ScrollGUI()
            {
                action = PresetSingleScroll,
                layoutOptions = scrollLayoutElements
            };

            _folderPresetScroll = new ScrollGUI()
            {
                action = PresetFolderScroll,
                layoutOptions = scrollLayoutElements
            };

            _charaSelectScroll = new ScrollGUI()
            {
                action = CharaSelectScroll,
                layoutOptions = scrollLayoutElements
            };

            _accessorySelectScroll = new ScrollGUI()
            {
                action = AccessorySelectScroll,
                layoutOptions = scrollLayoutElements
            };
            #endregion


            var shoeTypeGUIContext = new GUIContent[3];
#if KK
            shoeTypeGUIContext[0] = new GUIContent("Indoor", "Story/FreeH: Applies when inside buildings");
            shoeTypeGUIContext[1] = new GUIContent("Outdoor", "Story/FreeH: Applies when outside buildings");
            shoeTypeGUIContext[2] = new GUIContent("Both", "Applies regardless of Location");
#else
            shoeTypeGUIContext[0] = new GUIContent("Indoor", "Non-functioning: can attempt in the future Story/FreeH: Applies when inside buildings");
            shoeTypeGUIContext[1] = new GUIContent("Outdoor", "Non-functioning: can attempt in the future Story/FreeH: Applies when outside buildings");
            shoeTypeGUIContext[2] = new GUIContent("Both", "Functioning: Applies regardless of Location");
#endif

            var assContent = new GUIContent[]
            {
                new GUIContent("Indoor","Save Accessory State Sync format with Indoor Values if not both"),
                new GUIContent("Outdoor","Save Accessory State Sync format with Outdoor Values if not both")
            };

#if KK
            _AssPreference = new ToolbarGUI(0, assContent);
#else
            _AssPreference = new ToolbarGUI(1, assContent);
#endif

            var PresetContent = new GUIContent[]
            {
                new GUIContent("Folder","Show Folder Presets"),
                new GUIContent("Single","Show invididual Presets")
            };

            PresetView = new ToolbarGUI(0, PresetContent);

            ShoeTypeGUI = new ToolbarGUI(2, shoeTypeGUIContext)
            {
                OnValueChange = (oldValue, newValue) =>
                {
                    var control = GetController.ChaControl;
                    if (newValue < 2)
                    {
#if KK
                        control.fileStatus.shoesType = (byte)newValue;
#endif
                        control.SetClothesState(7, control.fileStatus.clothesState[7]);
                    }
                }
            };

            PresetFilter = new TextFieldGUI(new GUIContent(""));
            CreateStudioGUI();
            DiskLoad();
        }

        #region KKAPI Events
        private void UpdateCharaEvents()
        {
            var charaevents = StudioAPI.GetSelectedControllers<CharaEvent>().ToArray();
            if (Equals(CharaEvents, charaevents))
            {
                return;
            }
            CharaEvents = charaevents;
            SelectedCharaEventControls = null;
            if (GetController != null)
            {
                selectedChara = Array.FindIndex(charaevents, x => x == GetController);
            }
            GetController = GetCharaEvent();
            SelectedCharaEventControls = GetCharaEventControl();
        }

        private CharaEvent GetCharaEvent()
        {
            if (selectedChara < 0 || selectedChara >= CharaEvents.Length)
                selectedChara = 0;

            if (CharaEvents.Length == 0)
            {
                return null;
            }

            return CharaEvents[selectedChara];
        }

        private CharaEventControl GetCharaEventControl()
        {
            if (GetController == null)
                return null;
            if (!CharaControls.TryGetValue(GetController, out var charaEventControl))
            {
                CharaControls[GetController] = charaEventControl = new CharaEventControl();
            }
            return charaEventControl;
        }

        private void CreateStudioGUI()
        {

        }
        #endregion

        private BindingData GetSelectedBindingData(SlotData slotData)
        {
            if (slotData == null)
                return null;
            if (SelectedDropDown >= slotData.bindingDatas.Count || SelectedDropDown < 0)
            {
                SelectedDropDown = 0;
            }

            if (slotData.bindingDatas.Count == 0)
                return null;

            return slotData.bindingDatas[SelectedDropDown];
        }

        internal void ClearCoordinate(CharaEvent charaEvent)
        {
            CharaControls.Remove(charaEvent);
        }

        private void PresetWindowDraw(int id)
        {
            GL.BeginHorizontal();
            {
                PresetView.Draw();

                GL.FlexibleSpace();
                if (Button("Settings", "Open Settings", false))
                    _settingWindow.ToggleShow();
                GL.Space(10);
                if (Button("X", "Close this window", false))
                    _presetWindow.ToggleShow();
            }
            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                Label("Search:", "Filter lists by Search", false);
                PresetFilter.ActiveDraw();
            }
            GL.EndHorizontal();

            switch (PresetView.Value)
            {
                case 0:
                    PresetFolderDraw();
                    break;
                case 1:
                    PresetSingleDraw();
                    break;
                default:
                    break;
            }
            //TODO: Remember to make presets
        }

        private void PresetFolderDraw()
        {
            GL.BeginHorizontal();
            {
                if (Button("New Preset Folder", "Create new folder group"))
                {
                    var presetFolder = new PresetFolder()
                    {
                        Name = $"Slot {SelectedSlot} Folder Preset"
                    };
                    presetFolders.Add(presetFolder);
                }
            }
            GL.EndHorizontal();

            _folderPresetScroll.Draw();
        }

        private void PresetSingleDraw()
        {
            GL.BeginHorizontal();
            {
                if (Button("New Preset"))
                {
                    var presetData = PresetData.ConvertSlotData(SelectedSlotData, SelectedSlot);
                    SinglePresetDatas.Add(presetData);
                }
            }
            GL.EndHorizontal();
            GL.BeginHorizontal();
            {
                _singlePresetScroll.Draw();
            }
            GL.EndHorizontal();
        }

        private void PresetSingleScroll()
        {
            var filter = PresetFilter.GUIContent.text.Trim();
            var validfilter = filter.Length > 0;
            for (var i = 0; i < SinglePresetDatas.Count; i++)
            {
                if (!SingleControls.TryGetValue(SinglePresetDatas[i], out var preset))
                {
                    preset = SingleControls[SinglePresetDatas[i]] = new PresetContols(SinglePresetDatas[i], SinglePresetDatas);
                }

                if (validfilter && preset.Filter(filter))
                    continue;

                GL.BeginVertical(GUI.skin.box);
                {
                    preset.Draw(GetController, SelectedSlot);
                }
                GL.EndVertical();
            }
        }

        private void PresetFolderScroll()
        {
            var filter = PresetFilter.GUIContent.text.Trim();
            var validfilter = filter.Length > 0;

            for (var i = 0; i < presetFolders.Count; i++)
            {
                var folder = presetFolders[i];
                if (!PresetFolderControls.TryGetValue(folder, out var folderPreset))
                {
                    folderPreset = PresetFolderControls[folder] = new PresetFolderContols(folder, presetFolders);
                }

                if (validfilter && folderPreset.Filter(filter))
                    continue;

                GL.BeginVertical(GUI.skin.box);
                {
                    folderPreset.Draw(SelectedSlotData, SelectedSlot);
                    if (folderPreset.ShowContents)
                    {
                        var presets = folder.PresetDatas;
                        for (var j = 0; j < presets.Count; j++)
                        {
                            var singlePreset = presets[j];
                            if (!SingleControls.TryGetValue(singlePreset, out var singlePresetControl))
                            {
                                singlePresetControl = new PresetContols(singlePreset, presets);
                                SingleControls[singlePreset] = singlePresetControl;
                            }
                            if (singlePreset.Filter(filter))
                                continue;
                            GL.BeginHorizontal(GUI.skin.box);
                            {
                                GL.Space(10);
                                GL.BeginVertical();
                                {
                                    singlePresetControl.Draw(GetController, SelectedSlot);
                                }
                                GL.EndVertical();
                            }
                            GL.EndHorizontal();
                        }
                    }
                    else
                    {
                        GL.Space(10);
                    }
                }
                GL.EndVertical();
            }
        }

        private void DrawCharaSelectMenu()
        {
            if (CharaEvents.Length < 2)
            {
                _characterSelect.ToggleShow(false);
                return;
            }
            GL.BeginHorizontal();
            {
                var dropDownName = "No Chara Selected";
                if (GetController != null)
                {
                    dropDownName = GetController.ChaControl.fileParam.fullname;
                }
                if (Button("<", "Previous Character") && CharaEvents.Length > 0)
                {
                    if (--selectedChara < 0)
                        selectedChara = CharaEvents.Length - 1;
                    GetController = GetCharaEvent();
                    SelectedCharaEventControls = GetCharaEventControl();
                }
                if (Button(dropDownName, "Click to open Chara Select window"))
                    _characterSelect.ToggleShow();

                if (Button(">", "Next Character") && CharaEvents.Length > 0)
                {
                    if (++selectedChara >= CharaEvents.Length)
                        selectedChara = 0;
                    GetController = GetCharaEvent();
                    SelectedCharaEventControls = GetCharaEventControl();
                }
            }
            GL.EndHorizontal();
        }

        private void CharaSelectWindowDraw(int id)
        {
            if (GetController == null || CharaEvents.Length < 2)
            {
                _characterSelect.ToggleShow(false);
            }
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if (Button("X", "Close this window", false))
                    _characterSelect.ToggleShow();
            }
            GL.EndHorizontal();
            DrawCharaSelectMenu();
            _charaSelectScroll.Draw();
        }

        private void CharaSelectScroll()
        {
            for (var i = 0; i < CharaEvents.Length; i++)
            {
                var charaEvent = CharaEvents[i];
                var selected = selectedChara == i;
                if (selected)
                    GL.BeginHorizontal(GUI.skin.box);
                else
                    GL.BeginHorizontal();
                {
                    GL.FlexibleSpace();
                    if (!selected && Button("Select", "Select this Character", false))
                    {
                        selectedChara = i;
                        GetController = charaEvent;
                        SelectedCharaEventControls = GetCharaEventControl();
                    }
                    Label(charaEvent.ChaFileControl.parameter.fullname, "", false);
                }
                GL.EndHorizontal();
            }
        }

        private void DrawAccessorySelectMenu()
        {
            if (GetController == null)
            {
                return;
            }

            GL.BeginHorizontal();
            {
                var dropDownName = "No Accessory Selected";
                if (GetController != null)
                {
                    var listInfoBase = GetController.ChaControl.infoAccessory[SelectedCharaEventControls.SelectedSlot];
                    var parts = GetController.PartsArray;
                    if (parts[SelectedCharaEventControls.SelectedSlot].type != 120)
                        dropDownName = listInfoBase != null ? listInfoBase.Name : "Unknown";

                }

                if (Button("<", "Previous Accessory"))
                {
                    var parts = GetController.PartsArray;
                    var slot = -1;
                    Settings.Logger.LogMessage("Previous clicked");
                    for (var i = SelectedCharaEventControls.SelectedSlot - 1; i >= 0; i--)
                    {
                        Settings.Logger.LogMessage("Previous " + i);
                        if (parts[i].type != 120)
                        {
                            slot = i;
                            Settings.Logger.LogMessage("Previous found " + i);
                            break;
                        }
                    }

                    if (slot == -1) //wrap around
                    {
                        Settings.Logger.LogMessage("Previous Wrap");
                        for (var i = parts.Length - 1; i >= SelectedCharaEventControls.SelectedSlot; i--)
                        {
                            Settings.Logger.LogMessage("Previous " + i);
                            if (parts[i].type != 120)
                            {
                                slot = i;
                                Settings.Logger.LogMessage("Previous found " + i);
                                break;
                            }
                        }
                    }
                    SelectedCharaEventControls.SelectedSlot = Math.Max(slot, 0);
                }

                if (Button(dropDownName, "Click to open Accessory Select window"))
                    _accessorySelectWindow.ToggleShow();

                if (Button(">", "Next Accessory"))
                {
                    var parts = GetController.PartsArray;
                    var slot = -1;
                    Settings.Logger.LogMessage("Next clicked");
                    for (var i = SelectedCharaEventControls.SelectedSlot + 1; i < parts.Length; i++)
                    {
                        Settings.Logger.LogMessage("next " + i);
                        if (parts[i].type != 120)
                        {
                            slot = i;
                            Settings.Logger.LogMessage("next found " + i);
                            break;
                        }
                    }

                    if (slot == -1) //wrap around
                    {
                        Settings.Logger.LogMessage("next wrap ");
                        for (var i = 0; i <= SelectedCharaEventControls.SelectedSlot; i++)
                        {
                            Settings.Logger.LogMessage("next " + i);

                            if (parts[i].type != 120)
                            {
                                slot = i;
                                Settings.Logger.LogMessage("next found" + i);
                                break;
                            }
                        }
                    }
                    SelectedCharaEventControls.SelectedSlot = Math.Max(slot, 0);
                }
            }
            GL.EndHorizontal();
        }

        private void AccessorySelectWindowDraw(int id)
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if (Button("X", "Close this window", false))
                    _accessorySelectWindow.ToggleShow();
            }
            GL.EndHorizontal();
            DrawAccessorySelectMenu();

            _accessorySelectScroll.Draw();
        }

        private void AccessorySelectScroll()
        {
            var parts = GetController.PartsArray;
            var listInfoBases = GetController.ChaControl.infoAccessory;
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].type == 120)
                {
                    continue;
                }
                var listInfoBase = listInfoBases[i];
                var selected = SelectedCharaEventControls.SelectedSlot == i;
                if (selected)
                    GL.BeginHorizontal(GUI.skin.box);
                else
                    GL.BeginHorizontal();
                {
                    var name = listInfoBase != null ? listInfoBase.Name : "Unknown";
                    Label($"SLOT {i + 1}: ", "", false);
                    Label(name);

                    if (!selected && Button("Select", "Select this Accessory", false))
                    {
                        SelectedCharaEventControls.SelectedSlot = i;
                    }
                }
                GL.EndHorizontal();
            }
        }

        private void SlotWindowDraw(int id)
        {
            var slotData = SelectedSlotData;
            if (slotData == null)
            {
                Label("No Character Selected");
                return;
            }


            _slotWindow.SetWindowName($"Slot {SelectedSlot + 1}");

            var bData = GetSelectedBindingData(slotData);
            GL.BeginHorizontal();
            {
                Label(GetController.ChaControl.fileParam.fullname, "", false);

                GL.FlexibleSpace();
                if (Button("Preview", "Open preview window to modify states", false))
                {
                    _previewWindow.ToggleShow();
                    //SidebarToggle.SetValue(_previewWindow.Show);
                }

                if (Button("Settings", "Open Settings", false))
                    _settingWindow.ToggleShow();

                GL.Space(10);

                if (Button("X", "Close this window", false))
                    _slotWindow.ToggleShow();
            }
            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                ShoeTypeGUI.Draw();
                if (Toggle(slotData.Parented, "Enable Hide by Parent", "Enable Toggle to hide by parent") ^ slotData.Parented)
                {
                    slotData.Parented = !slotData.Parented;
                    GetController.UpdateParentedDict();
                    GetController.SaveSlotData(SelectedSlot);
                }
            }
            GL.EndHorizontal();
#if !KK
            if (ShoeTypeGUI.Value != 2)
            {
#if KKS
                Label("KK Functionality, KKS only uses Outdoor ATM by default");
#else
                Label("KK Functionality, may attempt to reimplement the feature later");
#endif

                GL.BeginHorizontal();
                {
                    if (ShoeTypeGUI.Value == 0)
                    {
#if KKS
                        Label("Non-functioning: May be implemented in the future");
#else
                        Label("Unknown functionality");
#endif
                    }
                    if (ShoeTypeGUI.Value == 1)
                    {
#if KKS
                        Label("Default State: May implement Indoor in the future");
#else
                        Label("Unknown functionality");
#endif
                    }
                }
                GL.EndHorizontal();
            }
#endif

            GL.BeginHorizontal();
            {
                if (Button("Group Data", "Create and Modify Custom Groups"))
                    _groupGUI.ToggleShow();

                if (Button("Use Presets Bindings", "Create and Use Predefine Presets to apply common settings."))
                    _presetWindow.ToggleShow();
            }
            GL.EndHorizontal();

            DrawCharaSelectMenu();

            DrawAccessorySelectMenu();

            GL.BeginHorizontal();
            {
                var dropDownName = "None";
                if (bData != null)
                {
                    dropDownName = bData.NameData.Name;
                }
                if (Button("<", "Previous binding for this accessory") && slotData.bindingDatas.Count > 0)
                    SelectedDropDown = SelectedDropDown == 0 ? slotData.bindingDatas.Count - 1 : SelectedDropDown - 1;
                if (Button(dropDownName, "Click to open window to apply binding groups"))
                    _bindingSelect.ToggleShow();
                if (Button(">", "Next binding for this accessory") && slotData.bindingDatas.Count > 0)
                    SelectedDropDown = SelectedDropDown == slotData.bindingDatas.Count - 1 ? 0 : SelectedDropDown + 1;
            }
            GL.EndHorizontal();

            if (bData == null)
                return;

            if (!NameControls.TryGetValue(bData.NameData, out var controls))
            {
                NameControls[bData.NameData] = controls = new NameDataControls(bData.NameData, GetController, SelectedCharaEventControls);
            }

            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                controls.DrawStatePreview();
            }
            GL.EndHorizontal();

            _slotWindowScroll.Draw();

            GL.FlexibleSpace();
        }

        private void SlotWindowScroll()
        {
            var bData = GetSelectedBindingData(SelectedSlotData);
            if (bData == null)
                return;
            GL.BeginVertical(GUI.skin.box);
            for (var i = 0; i < bData.States.Count; i++)
            {
                var item = bData.States[i];
                if (!(ShoeTypeGUI.Value == 2 || item.ShoeType == ShoeTypeGUI.Value || item.ShoeType == 2))
                    continue;

                if (!StateControls.TryGetValue(item, out var controls))
                {
                    StateControls[item] = controls = new StateInfoControls(bData, item, GetController, SelectedCharaEventControls);
                }

                if (bData.NameData.CurrentState == i)
                    GL.BeginHorizontal(GUI.skin.box);
                else
                    GL.BeginHorizontal();
                controls.Draw();
                GL.EndHorizontal();
            }
            GL.EndVertical();
        }

        private void GroupWindowDraw(int id)
        {
            var control = GetController;
            if (control == null)
            {
                _groupGUI.ToggleShow(false);
                return;
            }
            var names = control.NameDataList;

            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if (Button("X", "Close this window", false))
                    _groupGUI.ToggleShow();
            }
            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                if (Button("Add New Group"))
                {
                    var max = names.Max(x => x.Binding) + 1;
                    names.Add(new NameData() { Binding = max, Name = "Group " + max });
                }
            }
            GL.EndHorizontal();

            _groupScroll.Draw();
        }

        private void GroupWindowScroll()
        {
            var control = GetController;
            if (control == null)
                return;
            var names = control.NameDataList;
            for (var i = 0; i < names.Count; i++)
            {
                if (names[i].Binding < Constants.ClothingLength)
                    continue;
                if (!NameControls.TryGetValue(names[i], out var controls))
                {
                    NameControls[names[i]] = controls = new NameDataControls(names[i], GetController, SelectedCharaEventControls);
                }
                GL.BeginVertical(GUI.skin.box);
                {
                    GL.BeginHorizontal(GUI.skin.box);
                    {
                        controls.DrawGroupRename();
                    }
                    GL.EndHorizontal();

                    controls.DrawGroupSetting();

                    controls.DrawStateRename();
                }
                GL.EndVertical();
            }
        }

        private void SelectBindingWindowDraw(int id)
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if (Button("X", "Close this window", false))
                    _bindingSelect.ToggleShow();
            }
            GL.EndHorizontal();

            _bindingScroll.Draw();
        }

        private void BindingWindowScroll()
        {
            var slotData = SelectedSlotData;
            if (slotData == null)
                return;
            foreach (var item in GetController.NameDataList)
            {
                if (item.Binding < 0)
                    continue;
                var nameDataIndex = slotData.bindingDatas.FindIndex(x => x.NameData == item);
                if (SelectedDropDown == nameDataIndex)
                    GL.BeginHorizontal(GUI.skin.box);
                else
                    GL.BeginHorizontal();
                {
                    Label(item.Name);
                    if (nameDataIndex > -1)
                    {
                        if (SelectedDropDown != nameDataIndex && Button("Select", expandwidth: false))
                        {
                            SelectedDropDown = nameDataIndex;
                        }

                        if (Button("Remove", "Remove data associated with this group from accessory", false))
                        {
                            slotData.bindingDatas.RemoveAt(nameDataIndex);
                            GetController.SaveSlotData(SelectedSlot);
                            if (SelectedDropDown >= slotData.bindingDatas.Count || SelectedDropDown < 0)
                                SelectedDropDown = 0;
                        }
                    }
                    else
                    {
                        GL.FlexibleSpace();
                        if (Button("Add", "Add this binding type to accessory", expandwidth: false))
                        {
                            slotData.bindingDatas.Add(new BindingData() { NameData = item, States = item.GetDefaultStates(SelectedSlot) });
                            GetController.SaveSlotData(SelectedSlot);
                        }
                    }
                }
                GL.EndHorizontal();
            }
        }

        private void PreviewWindowDraw(int id)
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();

                GL.Space(10);
                if (Button("X", "Close this window", false))
                {
                    _previewWindow.ToggleShow();
                }
            }
            GL.EndHorizontal();

            DrawCharaSelectMenu();

            _previewScroll.Draw();
        }

        private void PreviewScroll()
        {
            var control = GetController;
            if (control == null)
                return;
            var nameDatas = control.NameDataList;
            var bindingDatas = SelectedSlotData.bindingDatas;

            foreach (var nameData in nameDatas)
            {
                if (!NameControls.TryGetValue(nameData, out var controls))
                {
                    NameControls[nameData] = controls = new NameDataControls(nameData, GetController, SelectedCharaEventControls);
                }
                if (nameData.Binding < 0)
                    continue;
                if (bindingDatas.Any(x => x.NameData == nameData))
                    GL.BeginHorizontal(GUI.skin.box);
                else
                    GL.BeginHorizontal();
                controls.DrawStatePreview();
                GL.EndHorizontal();
            }
            var currentParent = GetController.PartsArray[SelectedSlot].parentKey;
            var dict = GetController.ParentedNameDictionary;
            for (var i = 0; i < dict.Count; i++)
            {
                var keyvalue = dict.ElementAt(i);
                if (currentParent == keyvalue.Key)
                    GL.BeginHorizontal(GUI.skin.box);
                else
                    GL.BeginHorizontal();
                {
                    Label(keyvalue.Key, "Display accessory if parented toggle is also enabled", false);
                    if (Button(keyvalue.Value.Show ? "Show" : "Hide", "If accessory should be shown or hidden with current state"))
                    {
                        keyvalue.Value.Toggle();
                        GetController.RefreshSlots(keyvalue.Value.AssociateSlots);
                    }
                }
                GL.EndHorizontal();
            }
        }

        private void SettingWindowDraw(int id)
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();

                if (Button("G-", "Decrease GUI size", false))
                {
                    Settings.StudioFontSize.Value = Math.Max(0, Settings.StudioFontSize.Value - 1);
                    Settings.Logger.LogMessage(Settings.StudioFontSize.Value);
                    SetFontSize(Settings.StudioFontSize.Value);
                }

                if (Button("G+", "Increase GUI size", false))
                {
                    Settings.StudioFontSize.Value += 1;
                    Settings.Logger.LogMessage(Settings.StudioFontSize.Value);
                    SetFontSize(Settings.StudioFontSize.Value);
                }

                GL.Space(10);

                if (Button("X", "Close this window", false))
                    _settingWindow.ToggleShow();
            }
            GL.EndHorizontal();

            _settingScroll.Draw();
        }

        private void SettingScroll()
        {
            GL.BeginVertical(GUI.skin.box);
            {
                Label("Settings:");
                GL.BeginHorizontal();
                {
                    Label("Preview Transparency", "", false);
                    _previewWindow.Transparency = HorizontalSlider(_previewWindow.Transparency, 0, 100, true);
                }
                GL.EndHorizontal();
            }
            GL.EndVertical();
            GL.Space(5);
            GL.BeginVertical(GUI.skin.box);
            {
                Label("Accessory State Sync:");

                Settings.ASS_SAVE.Value = Toggle(Settings.ASS_SAVE.Value, "Save Accessory State Sync Format", "Enable attempting to save using Madevil's format so it can maybe work with ASS");
                GL.BeginHorizontal();
                {
                    Label("On ASS Save: ShoeType ", "Chose which shoetype should be used when saving with Madevil's format");
                    _AssPreference.Draw();
                }
                GL.EndHorizontal();
            }
            GL.EndVertical();
            GL.Space(5);
            GL.BeginVertical(GUI.skin.box);
            {
                Label("Presets:");

                GL.BeginHorizontal();
                {
                    Label("Clear Loaded Presets", "", false);
                    GL.FlexibleSpace();
                    if (Button("Clear", "Empty Loaded Preset list", false))
                    {
                        SinglePresetDatas.Clear();
                        SingleControls.Clear();
                        presetFolders.Clear();
                        PresetFolderControls.Clear();
                    }
                }
                GL.EndHorizontal();

                GL.BeginHorizontal();
                {
                    Label("Load Default Presets", "", false);
                    GL.FlexibleSpace();
                    if (Button("Load Defaults", "Recreate Hard Coded Presets", false))
                    {
                        //TODO: Remember to make presets
                    }
                }
                GL.EndHorizontal();

                GL.BeginHorizontal();
                {
                    Label("Load Presets From disk", "", false);
                    GL.FlexibleSpace();
                    if (Button("Disk Load", "Reload Presets from Disk", false))
                    {
                        DiskLoad();
                    }
                }
                GL.EndHorizontal();

                GL.BeginHorizontal();
                {
                    Label("Delete Saved Presets", "", false);
                    GL.FlexibleSpace();
                    if (Button("Disk Delete", "Delete Preset Cache from disk", false))
                    {
                        Presets.DeleteCache();
                    }
                }
                GL.EndHorizontal();
            }
            GL.EndVertical();
        }

        private void DiskLoad()
        {
            Presets.LoadAllPresets(out var singles, out var folders);
            foreach (var preset in singles)
            {
                if (SinglePresetDatas.Any(x => x.FileName == preset.FileName))
                    continue;
                SinglePresetDatas.Add(preset);
            }

            foreach (var preset in folders)
            {
                if (presetFolders.Any(x => x.FileName == preset.FileName))
                    continue;
                presetFolders.Add(preset);
            }
        }

        internal void OnGui()
        {
            UpdateCharaEvents();

            _previewWindow.Draw();

            _slotWindow.Draw();
            _groupGUI.Draw();
            _presetWindow.Draw();
            _bindingSelect.Draw();
            _settingWindow.Draw();
            _accessorySelectWindow.Draw();
            _characterSelect.Draw();
        }

        internal void ToggleSlotWindow()
        {
            _slotWindow.ToggleShow();
        }

        internal void TogglePreviewWindow()
        {
            _previewWindow.ToggleShow();
        }

        internal class NameDataControls
        {
            public NameData NameData;
            public CharaEvent CharaEvent;
            private string newName;
            private readonly Dictionary<int, TextFieldGUI> StatesRename = new Dictionary<int, TextFieldGUI>();
            private readonly IntTextFieldGUI _currentState;
            private readonly IntTextFieldGUI _defaultState;
            private readonly CharaEventControl eventControl;
            internal NameDataControls(NameData name, CharaEvent chara, CharaEventControl control)
            {
                eventControl = control;
                NameData = name;
                newName = name.Name;
                CharaEvent = chara;
                _currentState = new IntTextFieldGUI(NameData.CurrentState.ToString(), GL.ExpandWidth(false))
                {
                    action = (val) => { NameData.CurrentState = val; }
                };
                _defaultState = new IntTextFieldGUI(name.DefaultState.ToString(), GL.ExpandWidth(false))
                {
                    action = (val) =>
                    {
                        if (val < 0)
                            val = 0;
                        if (val >= NameData.StateLength)
                            val = NameData.StateLength - 1;
                        NameData.DefaultState = val;
                    }
                };
            }

            public void Save()
            {
                foreach (var item in CharaEvent.SlotBindingData)
                {
                    if (!item.Value.TryGetBinding(NameData, out var binding))
                        continue;

                    var states = binding.States;
                    var sort = false;

                    for (var i = 0; i < NameData.StateLength; i++)
                    {
                        if (states.Any(x => x.State == i))
                            continue;
                        states.Add(new StateInfo() { State = i, Binding = NameData.Binding, Slot = eventControl.SelectedSlot });
                        sort = true;
                    }
                    states.RemoveAll(x => x.State >= NameData.StateLength);

                    if (sort)
                        binding.Sort();
                    CharaEvent.SaveSlotData(item.Key);
                }
            }

            public void Delete()
            {
                foreach (var item in CharaEvent.SlotBindingData)
                {
                    var result = item.Value.bindingDatas.RemoveAll(x => x.NameData == NameData);
                    if (result > 0)
                    {
                        CharaEvent.SaveSlotData(item.Key);
                    }
                }
            }

            public void DrawGroupRename()
            {
                newName = TextField(newName);
                if (!newName.Equals(NameData.Name) && Button("Update Name", "Updates Name for all accessories as well as saving current data", false))
                {
                    NameData.Name = newName;
                    Save();
                }

                if (Button("Add State", "Adds a new state to this group for related accessories", false))
                {
                    for (int j = 0, n = NameData.StateLength + 1; j < n; j++)
                    {
                        if (NameData.StateNames.ContainsKey(j))
                            continue;
                        NameData.StateNames[j] = "State " + j;
                    }
                    Save();
                };

                if (Button("Delete", "Delete all data related to this group", false))
                {
                    Delete();
                    CharaEvent.NameDataList.Remove(NameData);
                };
            }

            public void DrawGroupSetting()
            {
                GL.BeginHorizontal();
                {
                    GL.FlexibleSpace();
                    if (Button("Set Main", "Change all accessory parts in group to Main Hide Category", false))
                    {
                        CharaEvent.ChangeBindingSub(0, NameData);
                    }

                    if (Button("Set Sub", "Change all accessory parts in group to Sub Hide Category", false))
                    {
                        CharaEvent.ChangeBindingSub(1, NameData);
                    }
                }
                GL.EndHorizontal();

                GL.BeginHorizontal();
                {
                    GL.FlexibleSpace();
                    Label($"Default {NameData.DefaultState}: {NameData.GetStateName(NameData.DefaultState)}", "State to be used on load", false);
                    if (Button("<", "Decrease Default State", expandwidth: false))
                    {
                        NameData.DefaultState--;
                        if (NameData.DefaultState < 0)
                            NameData.DefaultState = NameData.StateLength - 1;
                    }
                    _defaultState.Draw(NameData.DefaultState);
                    if (Button(">", "Increase Default State", expandwidth: false))
                    {
                        NameData.DefaultState++;
                        if (NameData.DefaultState >= NameData.StateLength)
                            NameData.DefaultState = 0;
                    }
                }
                GL.EndHorizontal();
            }

            public void DrawStateRename()
            {
                for (var j = 0; j < NameData.StateLength; j++)
                {
                    var newStateName = TryGetTextField(j);
                    GL.BeginHorizontal();
                    {
                        Label(j + ": ", "", false);
                        newStateName.ConfirmDraw();

                        if (j == NameData.StateLength - 1 && Button("Remove", expandwidth: false))
                        {
                            NameData.StateNames.Remove(j);
                            StatesRename.Remove(j);
                            Save();
                        }
                    }
                    GL.EndHorizontal();
                }
            }

            public void DrawStatePreview()
            {
                GL.BeginHorizontal();
                {
                    GL.FlexibleSpace();
                    Label(NameData.Name + ": " + NameData.GetStateName(NameData.CurrentState), "", false);
                    if (Button("<", "Previous State", expandwidth: false))
                    {
                        NameData.DecrementCurrentState();
                        if (NameData.Binding < Constants.ClothingLength)
                        {
                            CharaEvent.ChaControl.SetClothesStatePrev(NameData.Binding);
                            NameData.CurrentState = CharaEvent.ChaControl.fileStatus.clothesState[NameData.Binding];
                        }
                    }
                    _currentState.Draw(NameData.CurrentState);
                    if (Button(">", "Next State", expandwidth: false))
                    {
                        NameData.IncrementCurrentState();
                        if (NameData.Binding < Constants.ClothingLength)
                        {
                            CharaEvent.ChaControl.SetClothesStateNext(NameData.Binding);
                            NameData.CurrentState = CharaEvent.ChaControl.fileStatus.clothesState[NameData.Binding];
                        }
                    }
                }
                GL.EndHorizontal();
            }

            private TextFieldGUI TryGetTextField(int state)
            {
                if (!StatesRename.TryGetValue(state, out var newStateName))
                {
                    StatesRename[state] = newStateName = new TextFieldGUI(new GUIContent(NameData.GetStateName(state)), GL.ExpandWidth(true), GL.MinWidth(30))
                    {
                        OnValueChange = (oldValue, newValue) =>
                         {
                             NameData.StateNames[state] = newValue;
                             Save();
                         }
                    };
                }
                return newStateName;
            }
        }

        internal class StateInfoControls
        {
            public StateInfo StateInfo;
            public CharaEvent CharaEvent;
            public BindingData bData;
            private readonly IntTextFieldGUI PriorityField;
            private readonly string nameAppend;
            private readonly CharaEventControl eventControl;

            public StateInfoControls(BindingData bindingData, StateInfo name, CharaEvent chara, CharaEventControl control)
            {
                eventControl = control;
                bData = bindingData;
                StateInfo = name;
                CharaEvent = chara;
                PriorityField = new IntTextFieldGUI(StateInfo.Priority.ToString(), GL.ExpandWidth(false), GL.MinWidth(10))
                {
                    action = (val) =>
                    {
                        if (val != StateInfo.Priority)
                        {
                            StateInfo.Priority = Math.Max(0, val);
                            CharaEvent.SaveSlotData(eventControl.SelectedSlot);
                            CharaEvent.RefreshSlots();
                        }
                    }
                };
                nameAppend = StateInfo.ShoeType == 2 ? "" : StateInfo.ShoeType == 0 ? " (Indoor)" : " (Outdoor)";
            }

            public void Draw()
            {
                GL.BeginHorizontal();
                {
                    GL.Space(20);
                    Label(StateInfo.State + ": " + bData.NameData.GetStateName(StateInfo.State) + nameAppend);
                    if (Settings._studio.ShoeTypeGUI.Value != 2)
                    {
                        if (StateInfo.ShoeType == 2 && Button("Shoe Split", "Make this slot distinguish between being indoors and outdoors", false))
                        {
                            bData.States.Remove(StateInfo);
                            bData.States.Add(new StateInfo() { Binding = bData.NameData.Binding, Slot = eventControl.SelectedSlot, Priority = StateInfo.Priority, Show = StateInfo.Show, State = StateInfo.State, ShoeType = 0 });
                            bData.States.Add(new StateInfo() { Binding = bData.NameData.Binding, Slot = eventControl.SelectedSlot, Priority = StateInfo.Priority, Show = StateInfo.Show, State = StateInfo.State, ShoeType = 1 });
                            bData.Sort();
                            CharaEvent.SaveSlotData(eventControl.SelectedSlot);
                            CharaEvent.RefreshSlots();
                        }

                        if (StateInfo.ShoeType != 2 && Button("Shoe Merge", "Remove association between indoor and outdoors", false))
                        {
                            bData.States.RemoveAll(x => x.State == StateInfo.State);
                            bData.States.Add(new StateInfo() { Binding = bData.NameData.Binding, Slot = eventControl.SelectedSlot, Priority = StateInfo.Priority, Show = StateInfo.Show, State = StateInfo.State, ShoeType = 2 });
                            bData.Sort();
                            CharaEvent.SaveSlotData(eventControl.SelectedSlot);
                            CharaEvent.RefreshSlots();
                        }
                    }

                    if (Button(StateInfo.Show ? "Show" : "Hide", expandwidth: false))
                    {
                        StateInfo.Show = !StateInfo.Show;
                        CharaEvent.SaveSlotData(eventControl.SelectedSlot);
                        CharaEvent.RefreshSlots();
                    }

                    if (Button("↑", "Increase the priority of this state when comparing", false))
                    {
                        StateInfo.Priority++;
                        CharaEvent.SaveSlotData(eventControl.SelectedSlot);
                        CharaEvent.RefreshSlots();
                    }

                    PriorityField.Draw(StateInfo.Priority);

                    if (Button("↓", "Decrease the priority of this state when comparing", false) && StateInfo.Priority != 0)
                    {
                        StateInfo.Priority = Math.Max(0, StateInfo.Priority - 1);
                        CharaEvent.SaveSlotData(eventControl.SelectedSlot);
                        CharaEvent.RefreshSlots();
                    }
                }
                GL.EndHorizontal();
            }
        }

        internal class PresetContols
        {
            public readonly PresetData PresetData;
            private readonly TextFieldGUI Name;
            private readonly TextFieldGUI FileName;
            private readonly TextAreaGUI Description;
            private readonly List<PresetData> Container;

            public PresetContols(PresetData presetData, List<PresetData> container)
            {
                PresetData = presetData;
                Container = container;

                Name = new TextFieldGUI(new GUIContent(presetData.Name), GL.ExpandWidth(true))
                {
                    OnValueChange = (oldValue, newValue) => { presetData.Name = newValue; }
                };

                FileName = new TextFieldGUI(new GUIContent(presetData.FileName))
                {
                    OnValueChange = (oldVal, newVal) =>
                    {
                        if (newVal.IsNullOrWhiteSpace())
                            newVal = PresetData.Name;
                        if (newVal.Length == 0)
                            newVal = PresetData.GetHashCode().ToString();

                        newVal = string.Concat(newVal.Split(System.IO.Path.GetInvalidFileNameChars())).Trim();

                        if (PresetData.SavedOnDisk)
                            Presets.Rename(oldVal, newVal);
                        PresetData.FileName = newVal;
                        FileName.ManuallySetNewText(PresetData.FileName);
                    }
                };

                Description = new TextAreaGUI(presetData.Description)
                {
                    action = (val) => { presetData.Description = val; }
                };
            }

            public bool Filter(string filter)
            {
                return PresetData.Filter(filter);
            }


            public void Draw(CharaEvent chara, int SelectedSlot)
            {
                GL.BeginHorizontal();
                {
                    Name.ActiveDraw();

                    var index = Container.IndexOf(PresetData);

                    if (Button("Apply", "Apply this preset's data to slot", false))
                    {

                        var slotData = chara.SlotBindingData[SelectedSlot] = PresetData.Data.DeepClone();
                        var names = chara.NameDataList;
                        foreach (var item in slotData.bindingDatas)
                        {
                            var reference = names.FirstOrDefault(x => item.NameData.Equals(x));
                            if (reference != null)
                            {
                                item.NameData = reference;
                                item.SetSlot(SelectedSlot);
                                continue;
                            }
                            item.NameData.Binding = names.Max(x => x.Binding) + 1;
                            item.SetBinding();
                            names.Add(item.NameData);
                        }
                        Save(chara, slotData.bindingDatas, SelectedSlot);
                        chara.RefreshSlots();
                    }

                    if (Button("Override", "Apply this slots data to preset", false))
                    {
                        PresetData.Data = chara.SlotBindingData[SelectedSlot].DeepClone();
                    }

                    if (Button("↑", $"Move Up: Index {index}", false) && index > 0)
                    {
                        {
                            Container.RemoveAt(index);
                            Container.Insert(index - 1, PresetData);
                        }
                    }

                    if (Button("↓", $"Move Down: Index {index}", false) && index < Container.Count - 1)
                    {
                        {
                            Container.RemoveAt(index);
                            Container.Insert(index + 1, PresetData);
                        }
                    }
                }
                GL.EndHorizontal();
                GL.Space(10);
                GL.BeginHorizontal();
                {
                    GL.FlexibleSpace();

                    if (FileName.GUIContent.text.Length > 0 && Button("Save", "Save Preset to disk", false))
                    {
                        PresetData.SaveFile();
                    }

                    if (Button("Remove", "Unload Reference", false))
                    {
                        Container.Remove(PresetData);
                    }

                    GL.Space(10);

                    if (PresetData.SavedOnDisk && Button("Delete", "Delete from disk", false))
                    {
                        Container.Remove(PresetData);
                        PresetData.Delete();
                    }

                }
                GL.EndHorizontal();

                GL.BeginHorizontal();
                {
                    GL.Space(10);
                    Label("File Name:", "", false);
                    FileName.ConfirmDraw();
                }
                GL.EndHorizontal();

                GL.BeginHorizontal();
                {
                    GL.Space(10);
                    Label("Description:", "", false);
                    Description.ActiveDraw();
                }
                GL.EndHorizontal();
            }

            private void Save(CharaEvent chara, List<BindingData> bindingDatas, int SelectedSlot)
            {
                foreach (var item in chara.SlotBindingData)
                {
                    var save = false;
                    foreach (var item2 in item.Value.bindingDatas)
                    {
                        if (!bindingDatas.Any(x => x == item2))
                            continue;
                        var NameData = item2.NameData;
                        var states = item2.States;
                        var sort = false;

                        for (var i = 0; i < NameData.StateLength; i++)
                        {
                            if (states.Any(x => x.State == i))
                                continue;
                            states.Add(new StateInfo() { State = i, Binding = NameData.Binding, Slot = SelectedSlot });
                            sort = true;
                            save = true;
                        }
                        states.RemoveAll(x => x.State >= NameData.StateLength);

                        if (sort)
                            item2.Sort();
                    }
                    if (save)
                        chara.SaveSlotData(item.Key);
                }
            }
        }

        internal class PresetFolderContols
        {
            public readonly PresetFolder PresetFolder;
            private readonly TextFieldGUI Name;
            private readonly TextFieldGUI FileName;
            private readonly TextAreaGUI Description;
            private readonly List<PresetFolder> Container;
            public bool ShowContents;
            public PresetFolderContols(PresetFolder presetFolder, List<PresetFolder> container)
            {
                PresetFolder = presetFolder;
                Container = container;

                Name = new TextFieldGUI(new GUIContent(presetFolder.Name), GL.ExpandWidth(true))
                {
                    OnValueChange = (oldValue, newValue) => { PresetFolder.Name = newValue; }
                };

                FileName = new TextFieldGUI(new GUIContent(presetFolder.FileName))
                {
                    OnValueChange = (oldVal, newVal) =>
                    {

                        if (newVal.IsNullOrWhiteSpace())
                            newVal = PresetFolder.Name;
                        if (newVal.Length == 0)
                            newVal = PresetFolder.GetHashCode().ToString();

                        newVal = string.Concat(newVal.Split(System.IO.Path.GetInvalidFileNameChars())).Trim();

                        if (PresetFolder.SavedOnDisk)
                            Presets.Rename(oldVal, newVal);
                        PresetFolder.FileName = newVal;
                        FileName.ManuallySetNewText(PresetFolder.FileName);
                    }
                };

                Description = new TextAreaGUI(presetFolder.Description)
                {
                    action = (val) => { PresetFolder.Description = val; }
                };
            }

            public bool Filter(string filter)
            {
                return PresetFolder.Filter(filter);
            }

            public void Draw(SlotData slotData, int slot)
            {
                GL.BeginHorizontal();
                {
                    if (Button(ShowContents ? "-" : "+", "Show or Hide Folder Contents", false))
                    {
                        ShowContents = !ShowContents;
                    }

                    Name.ActiveDraw();

                    var index = Container.IndexOf(PresetFolder);

                    if (Button("↑", $"Move Up: Index {index}", false) && index > 0)
                    {
                        Container.RemoveAt(index);
                        Container.Insert(index - 1, PresetFolder);
                    }

                    if (Button("↓", $"Move Down: Index {index}", false) && index < Container.Count - 1)
                    {
                        Container.RemoveAt(index);
                        Container.Insert(index + 1, PresetFolder);
                    }
                }
                GL.EndHorizontal();
                GL.Space(10);
                GL.BeginHorizontal();
                {
                    GL.FlexibleSpace();

                    if (Button("Add Preset", "", false))
                    {
                        PresetFolder.PresetDatas.Add(PresetData.ConvertSlotData(slotData, slot));
                    }

                    if (FileName.GUIContent.text.Length > 0 && Button("Save", "", false))
                    {
                        PresetFolder.SaveFile();
                    }

                    if (Button("Remove", "Unload from Memory", false))
                    {
                        Container.Remove(PresetFolder);
                    }
                    GL.Space(10);
                    if (PresetFolder.SavedOnDisk && Button("Delete", "Delete From disk", false))
                    {
                        Container.Remove(PresetFolder);
                        PresetFolder.Delete();
                    }
                }
                GL.EndHorizontal();

                GL.BeginHorizontal();
                {
                    GL.Space(10);
                    Label("File Name:", "", false);
                    FileName.ConfirmDraw();
                }
                GL.EndHorizontal();

                GL.BeginHorizontal();
                {
                    GL.Space(10);
                    Label("Description:", "", false);
                    Description.ActiveDraw();
                }
                GL.EndHorizontal();
            }
        }

        internal class CharaEventControl
        {
            public readonly Dictionary<NameData, NameDataControls> NameControls = new Dictionary<NameData, NameDataControls>();
            public readonly Dictionary<StateInfo, StateInfoControls> StateControls = new Dictionary<StateInfo, StateInfoControls>();
            public readonly Dictionary<PresetData, PresetContols> SingleControls = new Dictionary<PresetData, PresetContols>();
            public readonly Dictionary<PresetFolder, PresetFolderContols> PresetFolderControls = new Dictionary<PresetFolder, PresetFolderContols>();
            public int SelectedSlot = 0;
            public int SelectedDropDown = 0;
        }
    }
}
#endif