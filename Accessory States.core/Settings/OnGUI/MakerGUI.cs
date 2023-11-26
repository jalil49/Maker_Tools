using Accessory_States.OnGUI;
using Extensions.GUI_Classes;
using Extensions.GUI_Classes.Config;
using KKAPI;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;
using System;
using System.Collections.Generic;
using UniRx;

namespace Accessory_States
{
    //Maker related 
    public class MakerGUI : StatesBaseGUI
    {
        private static bool _makerEnabled;
        private CharaEventControl _getControl;

        private SidebarToggle _sidebarToggle;

        private MakerGUI(RegisterSubCategoriesEvent e) : base("Maker Windows")
        {
            Instance = this;
            CreateMakerGUI(e);
        }

        public static MakerGUI Instance { get; private set; }

        private static CharaEvent CharaEvent => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        protected override SlotData SelectedSlotData
        {
            get
            {
                if(SelectedSlot < 0)
                {
                    return null;
                }

                if(!GetController.SlotBindingData.TryGetValue(SelectedSlot, out var slotData))
                {
                    slotData = GetController.SlotBindingData[SelectedSlot] = new SlotData();
                }

                return slotData;
            }
        }

        protected override int SelectedSlot
        {
            get => AccessoriesApi.SelectedMakerAccSlot;
            set { }
        }

        protected override int SelectedDropDown { get; set; }

        protected override CharaEventControl GetControl
        {
            get
            {
                if(CharaEventControls.TryGetValue(CharaEvent, out var control))
                {
                    return control;
                }

                return CharaEventControls[CharaEvent] = new CharaEventControl(CharaEvent);
            }
            set => _getControl = value;
        }

        private void ClearCoordinate()
        {
            SelectedDropDown = 0;
            NameControls.Clear();
            StateControls.Clear();
        }

        private static void UpdateClothNots()
        {
            var controller = CharaEvent;
            var clothNotData = controller.ClothNotData = new bool[3];
            clothNotData[0] = controller.ChaControl.notBot || GetClothingNot(0, ChaListDefine.KeyType.Coordinate) == 2;
            clothNotData[1] = controller.ChaControl.notBra || GetClothingNot(0, ChaListDefine.KeyType.Coordinate) == 1;
            clothNotData[2] = controller.ChaControl.notShorts ||
                              GetClothingNot(2, ChaListDefine.KeyType.Coordinate) == 2;
            controller.SaveCoordinateData();
        }

        public static bool ClothingUnlocker(int kind, ChaListDefine.KeyType value)
        {
            var listInfoBase = GetListInfoBase(kind);
            if(listInfoBase == null)
            {
                return false;
            }

            var intValue = GetClothingNot(listInfoBase, value);
            return intValue != listInfoBase.GetInfoInt(value);
        }

        public static int GetClothingNot(int kind, ChaListDefine.KeyType key)
        {
            return GetClothingNot(GetListInfoBase(kind), key);
        }

        public static int GetClothingNot(ListInfoBase listInfo, ChaListDefine.KeyType key)
        {
            if(listInfo == null)
            {
                return 0;
            }

            if(!(listInfo.dictInfo.TryGetValue((int)key, out var stringValue) &&
                  int.TryParse(stringValue, out var intValue)))
            {
                return 0;
            }

            return intValue;
        }

        public static ListInfoBase GetListInfoBase(int kind)
        {
            var lists = CharaEvent.ChaControl.infoClothes;
            if(kind >= lists.Length || kind < 0)
            {
                return null;
            }

            return lists[kind];
        }

        public override void OnGUI()
        {
            _previewWindow.Draw();

            if(!_slotWindow.Show || !AccessoriesApi.AccessoryCanvasVisible)
            {
                return;
            }

            var parts = CharaEvent.PartsArray;
            if(SelectedSlot < 0 || SelectedSlot >= parts.Length || parts[SelectedSlot].type == 120)
            {
                return;
            }

            _slotWindow.Draw();
            _groupGUI.Draw();
            _presetWindow.Draw();
            _addBinding.Draw();
            _settingWindow.Draw();
        }

        public override void TogglePreviewWindow()
        {
            base.TogglePreviewWindow();
            _sidebarToggle.SetValue(_previewWindow.Show);
        }

        #region KKAPI Events

        private void CreateMakerGUI(RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;
            var category = new MakerCategory(null, null);

            var guiButton = new MakerButton("States GUI", category, owner);
            MakerAPI.AddAccessoryWindowControl(guiButton, true);
            guiButton.OnClick.AddListener(ToggleSlotWindow);

            _sidebarToggle = e.AddSidebarControl(new SidebarToggle("Show States Preview Menu", false, owner));
            _sidebarToggle.ValueChanged.Subscribe(x => _previewWindow.ToggleShow(x));

            var groupingID = "Maker_Tools_" + Settings.NamingID.Value;
            guiButton.GroupingID = groupingID;
        }

        public static void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            _makerEnabled = Settings.Enable.Value;
            if(!_makerEnabled)
            {
                return;
            }

            Instance = new MakerGUI(e);
        }

        public static void Maker_started()
        {
            _makerEnabled = Settings.Enable.Value;
            if(!_makerEnabled)
            {
                return;
            }

            Settings.Instance.enabled = true;
            MakerAPI.MakerExiting += Maker_Ended;
            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;
            AccessoriesApi.SelectedMakerAccSlotChanged += AccessoriesApi_SelectedMakerAccSlotChanged;
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;
        }

        private static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            Instance.ClearCoordinate();
        }

        private static void AccessoriesApi_SelectedMakerAccSlotChanged(object sender, AccessorySlotEventArgs e)
        {
            Instance.StateControls.Clear();
        }

        private static void Maker_Ended(object sender, EventArgs e)
        {
            MakerAPI.MakerExiting -= Maker_Ended;
            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;
            AccessoriesApi.SelectedMakerAccSlotChanged -= AccessoriesApi_SelectedMakerAccSlotChanged;
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;

            _makerEnabled = false;
            Instance = null;
            Settings.Instance.enabled = false;
            WindowGUI.windowGUIs.Clear();
            WindowConfig.Clear();
        }

        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            var controller = CharaEvent;
            if(controller.SlotBindingData.TryGetValue(e.DestinationSlotIndex, out var slotData))
            {
                foreach(var item in slotData.bindingDatas)
                {
                    item.NameData.AssociatedSlots.Remove(e.DestinationSlotIndex);
                }
            }

            controller.SlotBindingData.Remove(e.DestinationSlotIndex);
            controller.LoadSlotData(e.DestinationSlotIndex);
            controller.UpdateParentedDict();
        }

        private static void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            var controller = CharaEvent;
            if(e.CopyDestination == controller.CurrentCoordinate.Value)
            {
                foreach(var slot in e.CopiedSlotIndexes)
                {
                    if(controller.SlotBindingData.TryGetValue(slot, out var slotData))
                    {
                        foreach(var bindingData in slotData.bindingDatas)
                        {
                            bindingData.NameData.AssociatedSlots.Remove(slot);
                        }
                    }

                    controller.SlotBindingData.Remove(slot);
                    controller.LoadSlotData(slot);
                }
            }
        }

        #endregion

        #region Custom Maker Events

        internal static void SlotAccTypeChange(int slotNo, int type)
        {
            if(KoikatuAPI.GetCurrentGameMode() != GameMode.Maker || type != 120)
            {
                return;
            }

            var controller = CharaEvent;
            controller.SlotBindingData.Remove(slotNo);
            controller.SaveSlotData(slotNo);
        }

        internal static void ClothingTypeChange()
        {
            UpdateClothNots();
        }

        internal static void MovIt(List<QueueItem> _)
        {
            CharaEvent.UpdatePluginData();
        }

        #endregion
    }
}