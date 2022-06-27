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

namespace Accessory_States
{
    //Maker related 
    public class Maker
    {

        int SelectedDropDown;
        private WindowGUI _makerGUI;//main SelectedSlot window
        private WindowGUI _statesGUI;//General NameData window, add, change, remove Namedata and other modifications
        private WindowGUI _presetWindow;
        private WindowGUI _AddBinding;
        private readonly string[] shoetypetext = new string[] { "Inside", "Outside", "Both" };
        private readonly string[] TabText = new string[] { "Assign", "Settings" };
        private int ShoeTypeView = 2;
        static bool MakerEnabled = false;

        static CharaEvent ControllerGet => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        static int SelectedSlot => AccessoriesApi.SelectedMakerAccSlot;
        private SlotData SelectedSlotData
        {
            get
            {
                if (!ControllerGet.SlotBindingData.TryGetValue(SelectedSlot, out var slotData))
                {
                    slotData = ControllerGet.SlotBindingData[SelectedSlot] = new SlotData();
                }

                return slotData;
            }
        }

        Maker(RegisterSubCategoriesEvent e)
        {
            var windowcount = 1;
            _makerGUI = new WindowGUI()
            {
                content = new GUIContent("Slot 0", "Modify data attached to this Accessory"),
                WindowID = windowcount++,
                Rect = new Rect(Screen.width * 0.33f, Screen.height * 0.09f, Screen.width * 0.225f, Screen.height * 0.273f),
                WindowFunction = SlotWindowDraw
            };

            _statesGUI = new WindowGUI()
            {
                content = new GUIContent("Group Data", "Modify existing Grouping data"),
                WindowID = windowcount++,
                Rect = new Rect(Screen.width * 0.33f, Screen.height * 0.09f, Screen.width * 0.225f, Screen.height * 0.273f),
                WindowFunction = GroupWindowDraw
            };
            _presetWindow = new WindowGUI()
            {
                content = new GUIContent("Presets", "Apply, Create or Modify Presets"),
                WindowID = windowcount++,
                Rect = new Rect(Screen.width * 0.33f, Screen.height * 0.09f, Screen.width * 0.225f, Screen.height * 0.273f),
                WindowFunction = PresetWindowDraw
            };
            _AddBinding = new WindowGUI()
            {
                content = new GUIContent("Group Select", "Select a group t" +
                "" +
                "o"),
                WindowID = windowcount++,
                Rect = new Rect(Screen.width * 0.33f, Screen.height * 0.09f, Screen.width * 0.225f, Screen.height * 0.273f),
                WindowFunction = SelectBindingWindowDraw
            };
            SelectedDropDown = 0;

            CreateMakerGUI(e);
        }

        private void CreateMakerGUI(RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;
            var category = new MakerCategory(null, null);

            var gui_button = new MakerButton("States GUI", category, owner);
            MakerAPI.AddAccessoryWindowControl(gui_button, true);
            gui_button.OnClick.AddListener(delegate () { _makerGUI.Show = !_makerGUI.Show; });

            var interfacebutton = e.AddSidebarControl(new SidebarToggle("ACC States", true, owner));
            interfacebutton.ValueChanged.Subscribe(x => _statesGUI.Show = !_statesGUI.Show);

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            gui_button.GroupingID = GroupingID;
        }

        #region KKAPI Events
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

            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;

        }

        public static void Maker_Ended()
        {
            MakerAPI.MakerExiting -= (s, e) => Maker_Ended();

            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;

            MakerEnabled = false;
            Settings._maker = null;
        }

        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            var controller = ControllerGet;
            controller.SlotBindingData.Remove(e.DestinationSlotIndex);
            controller.LoadSlotData(e.DestinationSlotIndex);
            controller.UpdateParentedDict();
        }

        private static void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            var controller = ControllerGet;
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
            var controller = ControllerGet;
            controller.SlotBindingData.Remove(slotNo);
            controller.SaveSlotData(slotNo);
        }

        internal static void ClothingTypeChange(int kind, int index)
        {
            if (index != 0)
                return;

            UpdateClothNots();
            var ClothNotData = ControllerGet.ClothNotData;

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
            RemoveAllByBinding(kind);
        }

        internal static void MovIt(List<QueueItem> _) => ControllerGet.UpdatePluginData();
        #endregion

        private static void UpdateClothNots()
        {
            var controller = ControllerGet;
            var ClothNotData = controller.ClothNotData = new bool[3] { false, false, false };
            ClothNotData[0] = controller.ChaControl.notBot || GetClothingNot(0, ChaListDefine.KeyType.Coordinate) == 2;
            ClothNotData[1] = controller.ChaControl.notBra || GetClothingNot(0, ChaListDefine.KeyType.Coordinate) == 1;
            ClothNotData[2] = controller.ChaControl.notShorts || GetClothingNot(2, ChaListDefine.KeyType.Coordinate) == 2;
        }

        public static bool ClothingUnlocker(int kind, ChaListDefine.KeyType value)
        {
            var listInfoBase = GetListInfoBase(kind);
            if (listInfoBase == null) return false;
            var intValue = GetClothingNot(listInfoBase, value);
            if (intValue == listInfoBase.GetInfoInt(value)) return false;

            return true;
        }

        public static int GetClothingNot(int kind, ChaListDefine.KeyType key)
        {
            return GetClothingNot(GetListInfoBase(kind), key);
        }
        public static int GetClothingNot(ListInfoBase listInfo, ChaListDefine.KeyType key)
        {
            if (listInfo == null) return 0;
            if (!(listInfo.dictInfo.TryGetValue((int)key, out var stringValue) && int.TryParse(stringValue, out var intValue))) return 0;

            return intValue;
        }
        public static ListInfoBase GetListInfoBase(int kind)
        {
            var lists = ControllerGet.ChaControl.infoClothes;
            if (kind >= lists.Length || kind < 0) return null;
            return lists[kind];
        }
        private static void RemoveAllByBinding(int kind)
        {
            RemoveNameData(ControllerGet.Names.FirstOrDefault(x => x.Binding == kind));
        }

        private static void RemoveNameData(NameData nameData)
        {
            if (nameData == null) return;
            var controller = ControllerGet;

            foreach (var item in controller.SlotBindingData)
            {
                var result = item.Value.bindingDatas.RemoveAll(x => x.NameData == nameData);
                if (result > 0)
                {
                    controller.SaveSlotData(item.Key);
                }
            }
        }

        private void SlotWindowDraw(int id)
        {
            _makerGUI.content.text = "Slot " + SelectedSlot;
            var slotData = SelectedSlotData;
            BindingData bData = null;

            if (SelectedDropDown >= slotData.bindingDatas.Count || SelectedDropDown < 0) SelectedDropDown = 0;
            if (slotData.bindingDatas.Count > 0)
            {
                bData = slotData.bindingDatas[SelectedDropDown];
            }

            GUILayout.BeginHorizontal();
            {
                ShoeTypeView = GUILayout.Toolbar(ShoeTypeView, shoetypetext);
                slotData.Parented = Toggle(slotData.Parented, "Enable Hide by Parent");
                GUILayout.FlexibleSpace();
                if (Button("X", false)) _makerGUI.ToggleShow();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (Button("Add Bindings")) _statesGUI.ToggleShow();

                if (Button("Use Presets Bindings")) _presetWindow.ToggleShow();
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            {
                var dropDownName = "None";
                if (bData != null)
                {
                    dropDownName = bData.NameData.Name;
                }
                if (Button("<") && slotData.bindingDatas.Count > 0) SelectedDropDown = SelectedDropDown == 0 ? slotData.bindingDatas.Count - 1 : SelectedDropDown - 1;
                if (Button(dropDownName)) _AddBinding.ToggleShow();
                if (Button(">") && slotData.bindingDatas.Count > 0) SelectedDropDown = SelectedDropDown == slotData.bindingDatas.Count - 1 ? 0 : SelectedDropDown + 1;
            }
            GUILayout.EndHorizontal();

            if (bData == null) return;
            if (bData.States.Count != bData.NameData.StateLength)
            {
                for (var i = 0; i < bData.NameData.StateLength; i++)
                {
                    if (bData.States.Any(x => x.State == i)) continue;
                    bData.States.Add(new StateInfo() { State = i, Binding = bData.GetBinding(), Slot = SelectedSlot });
                }
                bData.States.Sort((x, y) => x.State.CompareTo(y.State));
            }
            for (var i = 0; i < bData.States.Count; i++)
            {
                var item = bData.States[i];
                if (!(ShoeTypeView == 2 || item.ShoeType == ShoeTypeView || item.ShoeType == 2)) continue;
                GUILayout.BeginHorizontal();
                {
                    var nameAppend = item.ShoeType == 2 ? "" : item.ShoeType == 0 ? " (Indoor)" : " (Outdoor)";

                    Label(item.State + ": " + bData.NameData.GetStateName(item.State) + nameAppend);
                    if (ShoeTypeView != 2)
                    {
                        if (item.ShoeType == 2 && Button("Shoe Split"))
                        {
                            bData.States.RemoveAt(i);
                            bData.States.Add(new StateInfo() { Binding = item.Binding, Slot = SelectedSlot, Priority = item.Priority, Show = item.Show, State = item.State, ShoeType = 0 });
                            bData.States.Add(new StateInfo() { Binding = item.Binding, Slot = SelectedSlot, Priority = item.Priority, Show = item.Show, State = item.State, ShoeType = 1 });
                            bData.Sort();
                            GUILayout.EndHorizontal();
                            break;
                        }
                        if (item.ShoeType != 2 && Button("Shoe Merge"))
                        {
                            bData.States.RemoveAll(x => x.State == item.State);
                            bData.States.Add(new StateInfo() { Binding = item.Binding, Slot = SelectedSlot, Priority = item.Priority, Show = item.Show, State = item.State, ShoeType = 2 });
                            bData.Sort();
                            GUILayout.EndHorizontal();
                            break;
                        }
                    }
                    if (Button(item.Show ? "Show" : "Hide")) item.Show = !item.Show;
                    if (Button("↑", false)) item.Priority++;
                    if (int.TryParse(TextField(item.Priority.ToString(), false), out var newPriority) && newPriority != item.Priority) item.Priority = Math.Max(0, newPriority);
                    if (Button("↓", false)) item.Priority = Math.Max(0, item.Priority - 1);
                }
                GUILayout.EndHorizontal();
            }
        }

        private void GroupWindowDraw(int id)
        {
            var names = ControllerGet.Names;

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (Button("X", false)) _statesGUI.ToggleShow();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (Button("Add New Group"))
                {
                    var max = names.Max(x => x.Binding) + 1;
                    ControllerGet.Names.Add(new NameData() { Binding = max, Name = "Group " + max });
                }
            }
            GUILayout.EndHorizontal();

            for (var i = 0; i < names.Count; i++)
            {
                if (names[i].Binding < Constants.ClothingLength) continue;
                GUILayout.BeginVertical(new GUISkin().box);
                GUILayout.BeginHorizontal(new GUISkin().box);
                {
                    names[i].Name = TextField(names[i].Name);

                    if (Button("Delete"))
                    {
                        RemoveNameData(names[i]);
                        names.RemoveAt(i);
                    };
                }
                GUILayout.EndHorizontal();
                for (var j = 0; j < names[i].StateLength; j++)
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (!names[i].StateNames.TryGetValue(j, out var state))
                        {
                            state = "State " + j;
                        }
                        names[i].StateNames[j] = TextField(state);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
        }

        private void PresetWindowDraw(int id)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (Button("X", false)) _presetWindow.ToggleShow();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (Button("Add Presets"))
                {

                }
            }
            GUILayout.EndHorizontal();

            //Draw Presets
            //apply presets to selected slot
            //TODO: Remember to make presets
        }

        private void SelectBindingWindowDraw(int id)
        {
            if (SelectedSlotData.bindingDatas.Count == 0) _AddBinding.ToggleShow();

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (Button("X", false)) _AddBinding.ToggleShow();
            }
            GUILayout.EndHorizontal();

            var SlotData = SelectedSlotData;
            foreach (var item in ControllerGet.Names)
            {
                if (item.Binding < 0) continue;

                GUILayout.BeginHorizontal();
                {
                    Label(item.Name);
                    var nameDataIndex = SlotData.bindingDatas.FindIndex(x => x.NameData == item);
                    if (nameDataIndex > -1)
                    {
                        if (Button("Select"))
                        {
                            SelectedDropDown = nameDataIndex;
                        }
                        if (Button("Remove"))
                        {
                            SlotData.bindingDatas.RemoveAt(nameDataIndex);
                            if (SelectedDropDown >= SlotData.bindingDatas.Count || SelectedDropDown < 0) SelectedDropDown = 0;
                        }
                    }
                    else
                    {
                        GUILayout.FlexibleSpace();
                        if (Button("Add"))
                        {
                            SlotData.bindingDatas.Add(new BindingData() { NameData = item, States = item.GetDefaultStates(SelectedSlot) });
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        internal void OnGUI()
        {
            if (!_makerGUI.Show) return;

            var SelectedSlot = AccessoriesApi.SelectedMakerAccSlot;
            var parts = ControllerGet.PartsArray;
            if (SelectedSlot >= parts.Length) return;

            _makerGUI.Draw();
            if (_statesGUI.Show) _statesGUI.Draw();
            if (_presetWindow.Show) _presetWindow.Draw();
            if (_AddBinding.Show) _AddBinding.Draw();
        }
    }
}
