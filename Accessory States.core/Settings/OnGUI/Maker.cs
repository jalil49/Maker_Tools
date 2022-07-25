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
    public class Maker : BaseGUI
    {
        protected static bool MakerEnabled = false;

        private static CharaEvent CharaEvent => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        protected override SlotData SelectedSlotData
        {
            get
            {
                if(SelectedSlot < 0)
                    return null;
                if(!GetController.SlotBindingData.TryGetValue(SelectedSlot, out SlotData slotData))
                {
                    slotData = GetController.SlotBindingData[SelectedSlot] = new SlotData();
                }

                return slotData;
            }
        }

        protected SidebarToggle SidebarToggle;

        protected override int SelectedSlot { get => AccessoriesApi.SelectedMakerAccSlot; set { } }

        protected override int SelectedDropDown { get => _selectedDropDown; set => _selectedDropDown = value; }
        protected override CharaEventControl GetControl { get => _CharaEventControl; set => _CharaEventControl = value; }

        protected CharaEventControl _CharaEventControl;

        private int _selectedDropDown;
        Maker(RegisterSubCategoriesEvent e) : base("Maker Windows")
        {
            CreateMakerGUI(e);
            GetControl = new CharaEventControl(CharaEvent);
        }

        #region KKAPI Events

        private void CreateMakerGUI(RegisterSubCategoriesEvent e)
        {
            Settings owner = Settings.Instance;
            MakerCategory category = new MakerCategory(null, null);

            MakerButton gui_button = new MakerButton("States GUI", category, owner);
            MakerAPI.AddAccessoryWindowControl(gui_button, true);
            gui_button.OnClick.AddListener(_slotWindow.ToggleShow);

            SidebarToggle = e.AddSidebarControl(new SidebarToggle("Show States Preview Menu", false, owner));
            SidebarToggle.ValueChanged.Subscribe(x => _previewWindow.ToggleShow(x));

            string GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            gui_button.GroupingID = GroupingID;
        }

        public static void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            MakerEnabled = Settings.Enable.Value;
            if(!MakerEnabled)
            {
                return;
            }

            Settings._maker = new Maker(e);
        }

        public static void Maker_started()
        {
            MakerEnabled = Settings.Enable.Value;
            if(!MakerEnabled)
            {
                return;
            }

            Settings.Instance.enabled = true;
            MakerAPI.MakerExiting += (s, e) => Maker_Ended();
            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;
            AccessoriesApi.SelectedMakerAccSlotChanged += AccessoriesApi_SelectedMakerAccSlotChanged;
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;
        }

        private static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            Settings.UpdateGUI(CharaEvent);
        }

        private static void AccessoriesApi_SelectedMakerAccSlotChanged(object sender, AccessorySlotEventArgs e)
        {
            Settings._maker.StateControls.Clear();
        }

        public static void Maker_Ended()
        {
            MakerAPI.MakerExiting -= (s, e) => Maker_Ended();
            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;
            AccessoriesApi.SelectedMakerAccSlotChanged -= AccessoriesApi_SelectedMakerAccSlotChanged;
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;

            MakerEnabled = false;
            Settings._maker = null;
            Settings.Instance.enabled = false;
            WindowGUI.windowGUIs.Clear();
            WindowConfig.Clear();
        }

        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            CharaEvent controller = CharaEvent;
            if(controller.SlotBindingData.TryGetValue(e.DestinationSlotIndex, out SlotData slotData))
            {
                foreach(BindingData item in slotData.bindingDatas)
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
            CharaEvent controller = CharaEvent;
            if(e.CopyDestination == controller.CurrentCoordinate.Value)
            {
                foreach(int slot in e.CopiedSlotIndexes)
                {
                    if(controller.SlotBindingData.TryGetValue(slot, out SlotData slotData))
                    {
                        foreach(BindingData bindingData in slotData.bindingDatas)
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

            CharaEvent controller = CharaEvent;
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

        internal void ClearCoordinate()
        {
            SelectedDropDown = 0;
            NameControls.Clear();
            StateControls.Clear();
        }

        private static void UpdateClothNots()
        {
            CharaEvent controller = CharaEvent;
            bool[] ClothNotData = controller.ClothNotData = new bool[3] { false, false, false };
            ClothNotData[0] = controller.ChaControl.notBot || GetClothingNot(0, ChaListDefine.KeyType.Coordinate) == 2;
            ClothNotData[1] = controller.ChaControl.notBra || GetClothingNot(0, ChaListDefine.KeyType.Coordinate) == 1;
            ClothNotData[2] = controller.ChaControl.notShorts || GetClothingNot(2, ChaListDefine.KeyType.Coordinate) == 2;
            controller.SaveCoordinateData();
        }

        public static bool ClothingUnlocker(int kind, ChaListDefine.KeyType value)
        {
            ListInfoBase listInfoBase = GetListInfoBase(kind);
            if(listInfoBase == null)
                return false;
            int intValue = GetClothingNot(listInfoBase, value);
            if(intValue == listInfoBase.GetInfoInt(value))
                return false;

            return true;
        }

        public static int GetClothingNot(int kind, ChaListDefine.KeyType key)
        {
            return GetClothingNot(GetListInfoBase(kind), key);
        }

        public static int GetClothingNot(ListInfoBase listInfo, ChaListDefine.KeyType key)
        {
            if(listInfo == null)
                return 0;
            if(!(listInfo.dictInfo.TryGetValue((int)key, out string stringValue) && int.TryParse(stringValue, out int intValue)))
                return 0;

            return intValue;
        }

        public static ListInfoBase GetListInfoBase(int kind)
        {
            ListInfoBase[] lists = CharaEvent.ChaControl.infoClothes;
            if(kind >= lists.Length || kind < 0)
                return null;
            return lists[kind];
        }

        public override void OnGUI()
        {
            _previewWindow.Draw();

            if(!_slotWindow.Show || !AccessoriesApi.AccessoryCanvasVisible)
                return;

            ChaFileAccessory.PartsInfo[] parts = CharaEvent.PartsArray;
            if(SelectedSlot < 0 || SelectedSlot >= parts.Length || parts[SelectedSlot].type == 120)
                return;

            _slotWindow.Draw();
            _groupGUI.Draw();
            _presetWindow.Draw();
            _addBinding.Draw();
            _settingWindow.Draw();
        }

        public override void TogglePreviewWindow()
        {
            base.TogglePreviewWindow();
            SidebarToggle.SetValue(_previewWindow.Show);
        }
    }
}
