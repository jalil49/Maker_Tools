using Extensions.GUI_Classes;
using GUIHelper;
using KKAPI;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;
using GL = UnityEngine.GUILayout;


namespace Accessory_States
{
    //Maker related 
    public class Maker
    {
        #region Windows
        private readonly WindowGUI _makerGUI;//main SelectedSlot window
        private readonly WindowGUI _groupGUI;//General NameData window, add, change, remove Namedata and other modifications
        private readonly WindowGUI _presetWindow;
        private readonly WindowGUI _AddBinding;
        private readonly WindowGUI _previewWindow;
        private readonly WindowGUI _settingWindow;
        #endregion

        #region Scrolls for windows
        private readonly ScrollGUI<BindingData> _slotWindowScroll;
        private readonly ScrollGUI<SlotData> _bindingScroll;
        private readonly ScrollGUI<List<NameData>> _groupScroll;
        private readonly ScrollGUI<List<NameData>> _previewScroll;
        private readonly ScrollGUI<bool> _settingScroll;
        #endregion

        #region Settings
        private readonly ToolbarGUI _AssPreference;
        #endregion

        private readonly GUIContent[] shoeTypeGUIContext;
        private static int ShoeTypeView = 2;
        int SelectedDropDown;

        private static bool MakerEnabled = false;
        private readonly Dictionary<NameData, NameDataControls> NameControls = new Dictionary<NameData, NameDataControls>();
        private readonly Dictionary<StateInfo, StateInfoControls> StateControls = new Dictionary<StateInfo, StateInfoControls>();

        static CharaEvent GetController => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        static int SelectedSlot => AccessoriesApi.SelectedMakerAccSlot;
        private SlotData SelectedSlotData
        {
            get
            {
                if (!GetController.SlotBindingData.TryGetValue(SelectedSlot, out var slotData))
                {
                    slotData = GetController.SlotBindingData[SelectedSlot] = new SlotData();
                }

                return slotData;
            }
        }

        #region Settings

        #endregion

        Maker(RegisterSubCategoriesEvent e)
        {
            ShoeTypeView = 2;
            SelectedDropDown = 0;

            #region Windows
            var windowcount = 1;
            _makerGUI = new WindowGUI()
            {
                content = new GUIContent("Slot 0", "Modify data attached to this Accessory"),
                WindowID = windowcount++,
                Rect = new Rect(Screen.width * 0.33f, Screen.height * 0.09f, 0f, Screen.height * 0.2f),
                WindowFunction = SlotWindowDraw
            };

            _groupGUI = new WindowGUI()
            {
                content = new GUIContent("Group Data", "Modify existing Grouping data"),
                WindowID = windowcount++,
                Rect = new Rect(Screen.width * 0.58f, Screen.height * 0.09f, Screen.width * 0.15f, Screen.height * 0.25f),
                WindowRef = _makerGUI,
                RectAdjustVec = new Vector2(10f, 10f),
                WindowFunction = GroupWindowDraw
            };

            _presetWindow = new WindowGUI()
            {
                content = new GUIContent("Presets", "Apply, Create or Modify Presets"),
                WindowID = windowcount++,
                Rect = new Rect(Screen.width * 0.33f, Screen.height * 0.09f, Screen.width * 0.15f, Screen.height * 0.25f),
                WindowRef = _makerGUI,
                RectAdjustVec = new Vector2(0f, 10f),
                WindowFunction = PresetWindowDraw
            };

            _settingWindow = new WindowGUI()
            {
                content = new GUIContent("Settings", "Modify Plugin Settings"),
                WindowID = windowcount++,
                Rect = new Rect(Screen.width * 0.33f, Screen.height * 0.09f, Screen.width * 0.15f, Screen.height * 0.25f),
                WindowRef = _makerGUI,
                RectAdjustVec = new Vector2(0f, 10f),
                WindowFunction = SettingWindowDraw
            };

            _AddBinding = new WindowGUI()
            {
                content = new GUIContent("Group Select", "Select a group to Show"),
                WindowID = windowcount++,
                Rect = new Rect(Screen.width * 0.33f, Screen.height * 0.09f, Screen.width * 0.075f, Screen.height * 0.25f),
                WindowRef = _makerGUI,
                RectAdjustVec = new Vector2(10f, 0f),
                WindowFunction = SelectBindingWindowDraw
            };

            _previewWindow = new WindowGUI()
            {
                content = new GUIContent("State Preview", "Adjust State Values"),
                WindowID = windowcount++,
                Rect = new Rect(Screen.width * 0.33f, Screen.height * 0.09f, Screen.width * 0.1f, Screen.height * 0.2f),
                WindowRef = _makerGUI,
                RectAdjustVec = new Vector2(10f + _AddBinding.Rect.width + _AddBinding.RectAdjustVec.x, 0f),
                WindowFunction = PreviewWindowDraw
            };
            #endregion

            #region Scrolls
            var scrollLayoutElements = new GUILayoutOption[] { };
            _slotWindowScroll = new ScrollGUI<BindingData>()
            {
                action = SlotWindowScroll,
                layoutOptions = scrollLayoutElements
            };

            _bindingScroll = new ScrollGUI<SlotData>()
            {
                action = BindingWindowScroll,
                layoutOptions = scrollLayoutElements
            };

            _groupScroll = new ScrollGUI<List<NameData>>()
            {
                action = GroupWindowScroll,
                layoutOptions = scrollLayoutElements
            };

            _previewScroll = new ScrollGUI<List<NameData>>()
            {
                action = PreviewScroll,
                layoutOptions = scrollLayoutElements
            };

            _settingScroll = new ScrollGUI<bool>()
            {
                action = SettingScroll,
                layoutOptions = scrollLayoutElements
            };

            #endregion


            shoeTypeGUIContext = new GUIContent[3];
#if KK
            shoeTypeGUIContext[0] = new GUIContent("Indoor", "Story/FreeH: Applies when inside buildings");
            shoeTypeGUIContext[1] = new GUIContent("Outdoor", "Story/FreeH: Applies when outside buildings");
#else
            shoeTypeGUIContext[0] = new GUIContent("Indoor", "Story/FreeH: Applies when inside buildings");
            shoeTypeGUIContext[1] = new GUIContent("Outdoor", "Story/FreeH: Applies when outside buildings");
#endif
            shoeTypeGUIContext[2] = new GUIContent("Both", "Applies regardless of Location");

            var assContent = new GUIContent[]
            {
                new GUIContent("Indoor","Save Accessory State Sync format with Indoor Values if not both"),
                new GUIContent("Outdoor","Save Accessory State Sync format with Outdoor Values if not both")
            };

            _AssPreference = new ToolbarGUI(assContent);

            CreateMakerGUI(e);
        }

        #region KKAPI Events

        private void CreateMakerGUI(RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;
            var category = new MakerCategory(null, null);

            var gui_button = new MakerButton("States GUI", category, owner);
            MakerAPI.AddAccessoryWindowControl(gui_button, true);
            gui_button.OnClick.AddListener(_makerGUI.ToggleShow);

            //TODO: Remember what this is
            var interfacebutton = e.AddSidebarControl(new SidebarToggle("Show  States Preview Menu", false, owner));
            interfacebutton.ValueChanged.Subscribe(x => _presetWindow.ToggleShow());

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            gui_button.GroupingID = GroupingID;
        }

        public static void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            MakerEnabled = Settings.Enable.Value;
            if (!MakerEnabled)
            {
                return;
            }
            Settings._maker = new Maker(e);
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
            AccessoriesApi.SelectedMakerAccSlotChanged += AccessoriesApi_SelectedMakerAccSlotChanged;
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;

        }

        private static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            Settings.UpdateGUI(GetController);
        }

        private static void AccessoriesApi_SelectedMakerAccSlotChanged(object sender, AccessorySlotEventArgs e)
        {
            Settings._maker.StateControls.Clear();
        }

        public static void Maker_Ended()
        {
            MakerAPI.MakerExiting -= (s, e) => Maker_Ended();
            AccessoriesApi.SelectedMakerAccSlotChanged -= AccessoriesApi_SelectedMakerAccSlotChanged;
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;

            MakerEnabled = false;
            Settings._maker = null;
        }

        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            var controller = GetController;
            controller.SlotBindingData.Remove(e.DestinationSlotIndex);
            controller.LoadSlotData(e.DestinationSlotIndex);
            controller.UpdateParentedDict();
        }

        private static void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            var controller = GetController;
            if (e.CopyDestination == controller.CurrentCoordinate.Value)
            {
                foreach (var item in e.CopiedSlotIndexes)
                {
                    controller.SlotBindingData.Remove(item);
                    controller.LoadSlotData(item);
                }
            }
        }

        #endregion

        #region Custom Maker Events
        internal static void SlotAccTypeChange(int slotNo, int type)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker || type != 120)
            {
                return;
            }
            var controller = GetController;
            controller.SlotBindingData.Remove(slotNo);
            controller.SaveSlotData(slotNo);
        }

        internal static void ClothingTypeChange(int kind, int index)
        {
            if (index != 0 || !MakerEnabled)
                return;

            UpdateClothNots();
            var ClothNotData = GetController.ClothNotData;

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
                default:
                    break;
            }
            Settings._maker.RemoveAllByBinding(kind);
        }

        internal static void MovIt(List<QueueItem> _) => GetController.UpdatePluginData();
        #endregion

        internal void ClearCoordinate()
        {
            SelectedDropDown = 0;
            NameControls.Clear();
            StateControls.Clear();
        }

        private static void UpdateClothNots()
        {
            var controller = GetController;
            var ClothNotData = controller.ClothNotData = new bool[3] { false, false, false };
            ClothNotData[0] = controller.ChaControl.notBot || GetClothingNot(0, ChaListDefine.KeyType.Coordinate) == 2;
            ClothNotData[1] = controller.ChaControl.notBra || GetClothingNot(0, ChaListDefine.KeyType.Coordinate) == 1;
            ClothNotData[2] = controller.ChaControl.notShorts || GetClothingNot(2, ChaListDefine.KeyType.Coordinate) == 2;
        }

        public static bool ClothingUnlocker(int kind, ChaListDefine.KeyType value)
        {
            var listInfoBase = GetListInfoBase(kind);
            if (listInfoBase == null)
                return false;
            var intValue = GetClothingNot(listInfoBase, value);
            if (intValue == listInfoBase.GetInfoInt(value))
                return false;

            return true;
        }

        public static int GetClothingNot(int kind, ChaListDefine.KeyType key)
        {
            return GetClothingNot(GetListInfoBase(kind), key);
        }
        public static int GetClothingNot(ListInfoBase listInfo, ChaListDefine.KeyType key)
        {
            if (listInfo == null)
                return 0;
            if (!(listInfo.dictInfo.TryGetValue((int)key, out var stringValue) && int.TryParse(stringValue, out var intValue)))
                return 0;

            return intValue;
        }
        public static ListInfoBase GetListInfoBase(int kind)
        {
            var lists = GetController.ChaControl.infoClothes;
            if (kind >= lists.Length || kind < 0)
                return null;
            return lists[kind];
        }
        private void RemoveAllByBinding(int kind)
        {
            if (NameControls.TryGetValue(GetController.Names.FirstOrDefault(x => x.Binding == kind), out var controls))
            {
                controls.Delete();
            }
        }

        private void SlotWindowDraw(int id)
        {
            _makerGUI.content.text = "Slot " + SelectedSlot;
            var slotData = SelectedSlotData;
            BindingData bData = null;

            if (SelectedDropDown >= slotData.bindingDatas.Count || SelectedDropDown < 0)
                SelectedDropDown = 0;
            if (slotData.bindingDatas.Count > 0)
            {
                bData = slotData.bindingDatas[SelectedDropDown];
            }

            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if (Button("Preview", expandwidth: false))
                    _previewWindow.ToggleShow();

                if (Button("Settings", "Open Settings", false))
                    _settingWindow.ToggleShow();

                GL.Space(10);

                if (Button("X", "Close this window", false))
                    _makerGUI.ToggleShow();
            }
            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                ShoeTypeView = GL.Toolbar(ShoeTypeView, shoeTypeGUIContext, ButtonStyle);
                if (Toggle(slotData.Parented, "Enable Hide by Parent") ^ slotData.Parented)
                {
                    slotData.Parented = !slotData.Parented;
                    GetController.SaveSlotData(SelectedSlot);
                }

            }
            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                if (Button("Add Bindings"))
                    _groupGUI.ToggleShow();

                if (Button("Use Presets Bindings"))
                    _presetWindow.ToggleShow();
            }
            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                var dropDownName = "None";
                if (bData != null)
                {
                    dropDownName = bData.NameData.Name;
                }
                if (Button("<") && slotData.bindingDatas.Count > 0)
                    SelectedDropDown = SelectedDropDown == 0 ? slotData.bindingDatas.Count - 1 : SelectedDropDown - 1;
                if (Button(dropDownName))
                    _AddBinding.ToggleShow();
                if (Button(">") && slotData.bindingDatas.Count > 0)
                    SelectedDropDown = SelectedDropDown == slotData.bindingDatas.Count - 1 ? 0 : SelectedDropDown + 1;
            }
            GL.EndHorizontal();

            if (bData == null)
                return;

            if (!NameControls.TryGetValue(bData.NameData, out var controls))
            {
                NameControls[bData.NameData] = controls = new NameDataControls(bData.NameData, GetController);
            }

            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                controls.DrawStatePreview();
            }
            GL.EndHorizontal();

            _slotWindowScroll.Draw(bData);

            GL.FlexibleSpace();
        }

        private void SlotWindowScroll(BindingData bData)
        {
            for (var i = 0; i < bData.States.Count; i++)
            {
                var item = bData.States[i];
                if (!(ShoeTypeView == 2 || item.ShoeType == ShoeTypeView || item.ShoeType == 2))
                    continue;

                if (!StateControls.TryGetValue(item, out var controls))
                {
                    StateControls[item] = controls = new StateInfoControls(bData, item, GetController);
                }

                if (bData.NameData.CurrentState == i)
                    GL.BeginHorizontal(GUI.skin.box);
                else
                    GL.BeginHorizontal();
                controls.Draw();
                GL.EndHorizontal();
            }
        }

        private void GroupWindowDraw(int id)
        {
            var names = GetController.Names;

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
                    GetController.Names.Add(new NameData() { Binding = max, Name = "Group " + max });
                }
            }
            GL.EndHorizontal();

            _groupScroll.Draw(names);
        }

        private void GroupWindowScroll(List<NameData> names)
        {
            for (var i = 0; i < names.Count; i++)
            {
                if (names[i].Binding < Constants.ClothingLength)
                    continue;
                if (!NameControls.TryGetValue(names[i], out var controls))
                {
                    NameControls[names[i]] = controls = new NameDataControls(names[i], GetController);
                }
                GL.BeginVertical(GUI.skin.box);

                GL.BeginHorizontal(GUI.skin.box);
                {
                    controls.DrawGroupRename();

                    if (Button("Delete", "Delete all data related to this group", false))
                    {
                        controls.Delete();
                        names.RemoveAt(i);
                        GL.EndHorizontal();
                        GL.EndVertical();
                        break;
                    };

                }

                GL.EndHorizontal();
                controls.DrawStateRename();
                GL.EndVertical();
            }
        }

        private void PresetWindowDraw(int id)
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if (Button("X", "Close this window", false))
                    _presetWindow.ToggleShow();
            }
            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                if (Button("Add Presets"))
                {

                }
            }
            GL.EndHorizontal();

            //Draw Presets
            //apply presets to selected slot
            //TODO: Remember to make presets
        }

        private void SelectBindingWindowDraw(int id)
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if (Button("X", "Close this window", false))
                    _AddBinding.ToggleShow();
            }
            GL.EndHorizontal();

            _bindingScroll.Draw(SelectedSlotData);
        }

        private void BindingWindowScroll(SlotData SlotData)
        {
            foreach (var item in GetController.Names)
            {
                if (item.Binding < 0)
                    continue;
                var nameDataIndex = SlotData.bindingDatas.FindIndex(x => x.NameData == item);
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
                            SlotData.bindingDatas.RemoveAt(nameDataIndex);
                            GetController.SaveSlotData(SelectedSlot);
                            if (SelectedDropDown >= SlotData.bindingDatas.Count || SelectedDropDown < 0)
                                SelectedDropDown = 0;
                        }
                    }
                    else
                    {
                        GL.FlexibleSpace();
                        if (Button("Add", expandwidth: false))
                        {
                            SlotData.bindingDatas.Add(new BindingData() { NameData = item, States = item.GetDefaultStates(SelectedSlot) });
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

                if (Button("Settings", "", false))
                    _settingWindow.ToggleShow();
                GL.Space(10);
                if (Button("X", "Close this window", false))
                    _previewWindow.ToggleShow();
            }
            GL.EndHorizontal();

            _previewScroll.Draw(GetController.Names);
        }

        private void PreviewScroll(List<NameData> nameDatas)
        {
            var bindingDatas = SelectedSlotData.bindingDatas;
            NameData selectedName = null;
            if (bindingDatas.Count > 0)
            {
                selectedName = bindingDatas[SelectedDropDown].NameData;
            }

            foreach (var nameData in nameDatas)
            {
                if (!NameControls.TryGetValue(nameData, out var controls))
                {
                    NameControls[nameData] = controls = new NameDataControls(nameData, GetController);
                }
                if (nameData.Binding < 0)
                    continue;
                if (nameData == selectedName)
                    GL.BeginHorizontal(GUI.skin.box);
                else
                    GL.BeginHorizontal();
                controls.DrawStatePreview();
                GL.EndHorizontal();
            }
        }

        private void SettingWindowDraw(int id)
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();

                if (Button("G-", "Decrease GUI size", false))
                    DecrementFontSize();

                if (Button("G+", "Increase GUI size", false))
                    IncrementFontSize();

                GL.Space(10);

                if (Button("X", "Close this window", false))
                    _settingWindow.ToggleShow();
            }
            GL.EndHorizontal();

            _settingScroll.Draw(true);
        }

        private void SettingScroll(bool _val)
        {
            GL.BeginVertical(GUI.skin.box);
            {
                Label("Accessory State Sync:");

                Settings.ASS_SAVE.Value = Toggle(Settings.ASS_SAVE.Value, "Save Accessory State Sync Format");
                GL.BeginHorizontal();
                {
                    Label("On ASS Save: ShoeType ");
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
                    Label("Clear Loaded Presets", false);
                    GL.FlexibleSpace();
                    if (Button("Clear", "Empty Loaded Preset list", false))
                    {

                    }
                }
                GL.EndHorizontal();

                GL.BeginHorizontal();
                {
                    Label("Load Default Presets", false);
                    GL.FlexibleSpace();
                    if (Button("Load Defaults", "Recreate Hard Coded Presets", false))
                    {

                    }
                }
                GL.EndHorizontal();

                GL.BeginHorizontal();
                {
                    Label("Load Presets From disk", false);
                    GL.FlexibleSpace();
                    if (Button("Disk Load", "Reload Presets from Disk", false))
                    {

                    }
                }
                GL.EndHorizontal();

                GL.BeginHorizontal();
                {
                    Label("Delete Saved Presets", false);
                    GL.FlexibleSpace();
                    if (Button("Disk Delete", "Delete Preset Cache from disk", false))
                    {

                    }
                }
                GL.EndHorizontal();
            }
            GL.EndVertical();
        }

        internal void OnGUI()
        {
            if (!_makerGUI.Show)
                return;

            var parts = GetController.PartsArray;
            if (SelectedSlot >= parts.Length || SelectedSlot < 0 || parts[SelectedSlot].type == 120)
                return;

            _makerGUI.Draw();
            _groupGUI.Draw();
            _presetWindow.Draw();
            _AddBinding.Draw();
            _settingWindow.Draw();
            _previewWindow.Draw();
        }

        private class NameDataControls
        {
            public NameData NameData;
            public CharaEvent CharaEvent;
            private string newName;
            private readonly Dictionary<int, TextFieldGUI> StatesRename = new Dictionary<int, TextFieldGUI>();
            private readonly IntTextFieldGUI _currentState;
            public NameDataControls(NameData name, CharaEvent chara)
            {
                NameData = name;
                newName = name.Name;
                CharaEvent = chara;
                _currentState = new IntTextFieldGUI(NameData.CurrentState.ToString(), GL.ExpandWidth(false))
                {
                    action = (val) => { NameData.CurrentState = val; }
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
                        states.Add(new StateInfo() { State = i, Binding = NameData.Binding, Slot = SelectedSlot });
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
                newName = TextField(newName, false);
                GL.FlexibleSpace();
                if (!newName.Equals(NameData.Name) && Button("Update Name", "Updates Name for all accessories as well as saving current data", false))
                {
                    NameData.Name = newName;
                    Save();
                }

                if (Button("Set Main", "Change all accessory parts in group to Main Hide Category", false))
                {
                    CharaEvent.ChangeBindingSub(0);
                }

                if (Button("Set Sub", "Change all accessory parts in group to Sub Hide Category", false))
                {
                    CharaEvent.ChangeBindingSub(1);
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
            }

            public void DrawStateRename()
            {
                for (var j = 0; j < NameData.StateLength; j++)
                {
                    var newStateName = TryGetTextField(j);
                    GL.BeginHorizontal();
                    {
                        Label(j + ": ", false);
                        newStateName.ConfirmDraw();
                        if (j == NameData.StateLength - 1 && Button("Remove", expandwidth: false))
                        {
                            NameData.StateNames.Remove(j);
                            StatesRename.Remove(j);
                            Save();
                        }
                        GL.FlexibleSpace();
                    }
                    GL.EndHorizontal();
                }
            }

            public void DrawStatePreview()
            {
                GL.BeginHorizontal();
                {

                    GL.FlexibleSpace();
                    Label(NameData.Name + ": ", false);
                    if (Button("<", expandwidth: false))
                    {
                        NameData.DecrementCurrentState();
                        if (NameData.Binding < Constants.ClothingLength)
                        {
                            CharaEvent.ChaControl.SetClothesStateNext(NameData.Binding);
                            NameData.CurrentState = CharaEvent.ChaControl.fileStatus.clothesState[NameData.Binding];
                        }
                    }
                    _currentState.Draw(NameData.CurrentState);
                    if (Button(">", expandwidth: false))
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
                    StatesRename[state] = newStateName = new TextFieldGUI(NameData.GetStateName(state), GL.ExpandWidth(true), GL.MinWidth(30))
                    {
                        action = (string input) =>
                        {
                            NameData.StateNames[state] = input;
                            Save();
                        }
                    };
                }
                return newStateName;
            }
        }

        private class StateInfoControls
        {
            public StateInfo StateInfo;
            public CharaEvent CharaEvent;
            public BindingData bData;
            private readonly IntTextFieldGUI PriorityField;
            private readonly string nameAppend;
            public StateInfoControls(BindingData bindingData, StateInfo name, CharaEvent chara)
            {
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
                            CharaEvent.SaveSlotData(SelectedSlot);
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
                    Label(StateInfo.State + ": " + bData.NameData.GetStateName(StateInfo.State) + nameAppend);
                    if (ShoeTypeView != 2)
                    {
                        if (StateInfo.ShoeType == 2 && Button("Shoe Split", "Make this slot distinguish between being indoors and outdoors", false))
                        {
                            bData.States.Remove(StateInfo);
                            bData.States.Add(new StateInfo() { Binding = StateInfo.Binding, Slot = SelectedSlot, Priority = StateInfo.Priority, Show = StateInfo.Show, State = StateInfo.State, ShoeType = 0 });
                            bData.States.Add(new StateInfo() { Binding = StateInfo.Binding, Slot = SelectedSlot, Priority = StateInfo.Priority, Show = StateInfo.Show, State = StateInfo.State, ShoeType = 1 });
                            bData.Sort();
                            CharaEvent.SaveSlotData(SelectedSlot);
                            CharaEvent.RefreshSlots();
                        }

                        if (StateInfo.ShoeType != 2 && Button("Shoe Merge", "Remove association between indoor and outdoors", false))
                        {
                            bData.States.RemoveAll(x => x.State == StateInfo.State);
                            bData.States.Add(new StateInfo() { Binding = StateInfo.Binding, Slot = SelectedSlot, Priority = StateInfo.Priority, Show = StateInfo.Show, State = StateInfo.State, ShoeType = 2 });
                            bData.Sort();
                            CharaEvent.SaveSlotData(SelectedSlot);
                            CharaEvent.RefreshSlots();
                        }
                    }

                    if (Button(StateInfo.Show ? "Show" : "Hide", expandwidth: false))
                    {
                        StateInfo.Show = !StateInfo.Show;
                        CharaEvent.SaveSlotData(SelectedSlot);
                        CharaEvent.RefreshSlots();
                    }

                    if (Button("↑", "Increase the priority of this state when comparing", false))
                    {
                        StateInfo.Priority++;
                        CharaEvent.SaveSlotData(SelectedSlot);
                        CharaEvent.RefreshSlots();
                    }

                    PriorityField.Draw(StateInfo.Priority);

                    if (Button("↓", "Decrease the priority of this state when comparing", false) && StateInfo.Priority != 0)
                    {
                        StateInfo.Priority = Math.Max(0, StateInfo.Priority - 1);
                        CharaEvent.SaveSlotData(SelectedSlot);
                        CharaEvent.RefreshSlots();
                    }
                }
                GL.EndHorizontal();
            }
        }
    }
}
