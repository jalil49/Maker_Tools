#if Studio
using Accessory_States.OnGUI;
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
    public class Studio : BaseGUI
    {
        #region Windows
        private readonly WindowGUI _characterSelect;
        private readonly WindowGUI _accessorySelectWindow;
        #endregion

        protected override int SelectedDropDown
        {
            get
            {
                if(GetControl == null)
                    return -1;
                return GetControl.SelectedDropDown;
            }
            set
            {
                if(GetControl == null)
                    return;
                GetControl.SelectedDropDown = value;
            }
        }

        protected override int SelectedSlot
        {
            get
            {
                if(GetControl == null)
                    return -1;
                return GetControl.SelectedSlot;
            }
            set
            {
                if(GetControl == null)
                    return;
                GetControl.SelectedSlot = value;
            }
        }

        protected override SlotData SelectedSlotData
        {
            get
            {
                CharaEvent control = GetController;
                if(control == null || SelectedSlot < 0)
                    return null;

                if(!control.SlotBindingData.TryGetValue(SelectedSlot, out SlotData slotData))
                {
                    slotData = GetController.SlotBindingData[SelectedSlot] = new SlotData();
                }

                return slotData;
            }
        }

        protected override CharaEventControl GetControl { get => _selectedCharaEventControl; set => _selectedCharaEventControl = value; }

        private readonly Dictionary<CharaEvent, CharaEventControl> CharaControls = new Dictionary<CharaEvent, CharaEventControl>();
        private int selectedChara = 0;

        private CharaEvent[] CharaEvents;

        private CharaEventControl _selectedCharaEventControl;

        #region Settings

        #endregion

        public Studio() : base("Studio")
        {
            #region Windows
            string section = "Studio";
            BepInEx.Configuration.ConfigFile config = Settings.Instance.Config;

            _characterSelect = new WindowGUI(config, section, "Character Select Window", new Rect(Screen.width * 0.57f, Screen.height * 0.1f, Screen.width * 0.14f, Screen.height * 0.2f), 1f, CharaSelectWindowDraw, new GUIContent("Character Select", "Select from selected Characters"), new ScrollGUI(CharaSelectScroll));

            _accessorySelectWindow = new WindowGUI(config, section, "Accessory Select Window", new Rect(Screen.width * 0.57f, Screen.height * 0.1f, Screen.width * 0.14f, Screen.height * 0.2f), 1f, AccessorySelectWindowDraw, new GUIContent("Accessory Select", "Select Accessories"), new ScrollGUI(AccessorySelectScroll));
            #endregion
        }

        #region KKAPI Events
        private void UpdateCharaEvents()
        {
            CharaEvent[] charaevents = StudioAPI.GetSelectedControllers<CharaEvent>().ToArray();
            if(Equals(CharaEvents, charaevents))
            {
                return;
            }

            CharaEvents = charaevents;
            GetControl = null;
            if(GetController != null)
            {
                selectedChara = Array.FindIndex(charaevents, x => x == GetController);
            }

            GetControl = GetCharaEventControl();
        }

        private CharaEventControl GetCharaEventControl()
        {
            if(selectedChara < 0 || selectedChara >= CharaEvents.Length)
                selectedChara = 0;

            if(CharaEvents.Length == 0)
            {
                return null;
            }

            if(!CharaControls.TryGetValue(GetController, out CharaEventControl charaEventControl))
            {
                CharaControls[GetController] = charaEventControl = new CharaEventControl(GetController);
            }

            return charaEventControl;
        }
        #endregion

        internal void ClearCoordinate(CharaEvent charaEvent)
        {
            CharaControls.Remove(charaEvent);
        }

        private void DrawCharaSelectMenu()
        {
            if(CharaEvents.Length < 2)
            {
                _characterSelect.ToggleShow(false);
                return;
            }

            GL.BeginHorizontal();
            {
                string dropDownName = "No Chara Selected";
                if(GetController != null)
                {
                    dropDownName = GetController.ChaControl.fileParam.fullname;
                }

                if(Button("<", "Previous Character") && CharaEvents.Length > 0)
                {
                    if(--selectedChara < 0)
                        selectedChara = CharaEvents.Length - 1;
                    GetControl = GetCharaEventControl();
                }

                if(Button(dropDownName, "Click to open Chara Select window"))
                    _characterSelect.ToggleShow();

                if(Button(">", "Next Character") && CharaEvents.Length > 0)
                {
                    if(++selectedChara >= CharaEvents.Length)
                        selectedChara = 0;
                    GetControl = GetCharaEventControl();
                }
            }

            GL.EndHorizontal();
        }

        private WindowReturn CharaSelectWindowDraw()
        {
            if(GetController == null || CharaEvents.Length < 2)
            {
                _characterSelect.ToggleShow(false);
            }

            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if(Button("X", "Close this window", false))
                    _characterSelect.ToggleShow();
            }

            GL.EndHorizontal();
            DrawCharaSelectMenu();
            return new WindowReturn();
        }

        private void CharaSelectScroll()
        {
            for(int i = 0; i < CharaEvents.Length; i++)
            {
                CharaEvent charaEvent = CharaEvents[i];
                bool selected = selectedChara == i;
                if(selected)
                    GL.BeginHorizontal(GUI.skin.box);
                else
                    GL.BeginHorizontal();
                {
                    GL.FlexibleSpace();
                    if(!selected && Button("Select", "Select this Character", false))
                    {
                        selectedChara = i;
                        GetControl = GetCharaEventControl();
                    }

                    Label(charaEvent.ChaFileControl.parameter.fullname, "", false);
                }

                GL.EndHorizontal();
            }
        }

        private void DrawAccessorySelectMenu()
        {
            if(GetController == null)
            {
                return;
            }

            GL.BeginHorizontal();
            {
                string dropDownName = "No Accessory Selected";
                if(GetController != null)
                {
                    ListInfoBase listInfoBase = GetController.ChaControl.infoAccessory[GetControl.SelectedSlot];
                    ChaFileAccessory.PartsInfo[] parts = GetController.PartsArray;
                    if(parts[GetControl.SelectedSlot].type != 120)
                        dropDownName = listInfoBase != null ? listInfoBase.Name : "Unknown";

                }

                if(Button("<", "Previous Accessory"))
                {
                    ChaFileAccessory.PartsInfo[] parts = GetController.PartsArray;
                    int slot = -1;

                    for(int i = GetControl.SelectedSlot - 1; i >= 0; i--)
                    {
                        if(parts[i].type != 120)
                        {
                            slot = i;
                            break;
                        }
                    }

                    if(slot == -1) //wrap around
                    {
                        for(int i = parts.Length - 1; i >= GetControl.SelectedSlot; i--)
                        {
                            if(parts[i].type != 120)
                            {
                                slot = i;
                                break;
                            }
                        }
                    }

                    GetControl.SelectedSlot = Math.Max(slot, 0);
                }

                if(Button(dropDownName, "Click to open Accessory Select window"))
                    _accessorySelectWindow.ToggleShow();

                if(Button(">", "Next Accessory"))
                {
                    ChaFileAccessory.PartsInfo[] parts = GetController.PartsArray;
                    int slot = -1;
                    for(int i = GetControl.SelectedSlot + 1; i < parts.Length; i++)
                    {
                        if(parts[i].type != 120)
                        {
                            slot = i;
                            break;
                        }
                    }

                    if(slot == -1) //wrap around
                    {
                        for(int i = 0; i <= GetControl.SelectedSlot; i++)
                        {
                            if(parts[i].type != 120)
                            {
                                slot = i;
                                break;
                            }
                        }
                    }

                    GetControl.SelectedSlot = Math.Max(slot, 0);
                }
            }

            GL.EndHorizontal();
        }

        private WindowReturn AccessorySelectWindowDraw()
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if(Button("X", "Close this window", false))
                    _accessorySelectWindow.ToggleShow();
            }

            GL.EndHorizontal();
            DrawAccessorySelectMenu();

            return new WindowReturn();
        }

        private void AccessorySelectScroll()
        {
            ChaFileAccessory.PartsInfo[] parts = GetController.PartsArray;
            ListInfoBase[] listInfoBases = GetController.ChaControl.infoAccessory;
            for(int i = 0; i < parts.Length; i++)
            {
                if(parts[i].type == 120)
                {
                    continue;
                }

                ListInfoBase listInfoBase = listInfoBases[i];
                bool selected = GetControl.SelectedSlot == i;
                if(selected)
                    GL.BeginHorizontal(GUI.skin.box);
                else
                    GL.BeginHorizontal();
                {
                    string name = listInfoBase != null ? listInfoBase.Name : "Unknown";
                    Label($"SLOT {i + 1}: ", "", false);
                    Label(name);

                    if(!selected && Button("Select", "Select this Accessory", false))
                    {
                        GetControl.SelectedSlot = i;
                    }
                }

                GL.EndHorizontal();
            }
        }

        protected override WindowReturn SlotWindowDraw()
        {
            SlotData slotData = SelectedSlotData;
            if(slotData == null)
            {
                Label("No Character Selected");
                return new WindowReturn();
            }

            _slotWindow.SetWindowName($"Slot {SelectedSlot + 1}");

            BindingData bData = GetSelectedBindingData(slotData);
            GL.BeginHorizontal();
            {
                Label(GetController.ChaControl.fileParam.fullname, "", false);

                GL.FlexibleSpace();
                if(Button("Preview", "Open preview window to modify states", false))
                {
                    _previewWindow.ToggleShow();
                    //SidebarToggle.SetValue(_previewWindow.Show);
                }

                if(Button("Settings", "Open Settings", false))
                    _settingWindow.ToggleShow();

                GL.Space(10);

                if(Button("X", "Close this window", false))
                    _slotWindow.ToggleShow();
            }

            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                ShoeTypeGUI.Draw();
                if(Toggle(slotData.Parented, "Enable Hide by Parent", "Enable Toggle to hide by parent") ^ slotData.Parented)
                {
                    slotData.Parented = !slotData.Parented;
                    GetController.UpdateParentedDict();
                    GetController.SaveSlotData(SelectedSlot);
                }
            }

            GL.EndHorizontal();
#if !KK
            if(ShoeTypeGUI.Value != 2)
            {
#if KKS
                Label("KK Functionality, KKS only uses Outdoor ATM by default");
#else
                Label("KK Functionality, may attempt to reimplement the feature later");
#endif

                GL.BeginHorizontal();
                {
                    if(ShoeTypeGUI.Value == 0)
                    {
#if KKS
                        Label("Non-functioning: May be implemented in the future");
#else
                        Label("Unknown functionality");
#endif
                    }

                    if(ShoeTypeGUI.Value == 1)
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
                if(Button("Group Data", "Create and Modify Custom Groups"))
                    _groupGUI.ToggleShow();

                if(Button("Use Presets Bindings", "Create and Use Predefine Presets to apply common settings."))
                    _presetWindow.ToggleShow();
            }

            GL.EndHorizontal();

            DrawCharaSelectMenu();

            DrawAccessorySelectMenu();

            GL.BeginHorizontal();
            {
                string dropDownName = "None";
                if(bData != null)
                {
                    dropDownName = bData.NameData.Name;
                }

                if(Button("<", "Previous binding for this accessory") && slotData.bindingDatas.Count > 0)
                    SelectedDropDown = SelectedDropDown == 0 ? slotData.bindingDatas.Count - 1 : SelectedDropDown - 1;
                if(Button(dropDownName, "Click to open window to apply binding groups"))
                    _addBinding.ToggleShow();
                if(Button(">", "Next binding for this accessory") && slotData.bindingDatas.Count > 0)
                    SelectedDropDown = SelectedDropDown == slotData.bindingDatas.Count - 1 ? 0 : SelectedDropDown + 1;
            }

            GL.EndHorizontal();

            if(bData == null)
                return new WindowReturn();
            ;

            if(!NameControls.TryGetValue(bData.NameData, out NameDataControl controls))
            {
                NameControls[bData.NameData] = controls = new NameDataControl(bData.NameData, GetControl);
            }

            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                controls.DrawStatePreview();
            }

            GL.EndHorizontal();

            _slotWindow.Draw();
            return new WindowReturn();
        }

        protected override WindowReturn PreviewWindowDraw()
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();

                GL.Space(10);
                if(Button("X", "Close this window", false))
                {
                    _previewWindow.ToggleShow();
                }
            }

            GL.EndHorizontal();

            DrawCharaSelectMenu();

            return new WindowReturn();
        }

        internal void ToggleSlotWindow()
        {
            _slotWindow.ToggleShow();
        }

        internal void TogglePreviewWindow()
        {
            _previewWindow.ToggleShow();
        }

        public override void OnGUI()
        {
            if(Event.current.type == (EventType)8)
                UpdateCharaEvents();

            _previewWindow.Draw();

            _slotWindow.Draw();
            _groupGUI.Draw();
            _presetWindow.Draw();
            _addBinding.Draw();
            _settingWindow.Draw();
            _accessorySelectWindow.Draw();
            _characterSelect.Draw();
        }
    }
}
#endif