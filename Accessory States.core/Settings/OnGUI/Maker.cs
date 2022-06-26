using KKAPI;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Accessory_States
{
    //Maker related 
    public partial class Settings
    {
        private static readonly string[] shoetypetext = new string[] { "Inside", "Outside", "Both" };
        private static readonly string[] TabText = new string[] { "Assign", "Settings" };

        static MakerButton gui_button;

        static bool MakerEnabled = false;

        static CharaEvent ControllerGet => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        public static void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            MakerEnabled = Enable.Value;
            if (!MakerEnabled)
            {
                return;
            }

            var owner = Settings.Instance;
            var category = new MakerCategory(null, null);

            gui_button = new MakerButton("States GUI", category, owner);
            MakerAPI.AddAccessoryWindowControl(gui_button, true);
            gui_button.OnClick.AddListener(delegate () { _makerGUI.Show = !_makerGUI.Show; });


            var interfacebutton = e.AddSidebarControl(new SidebarToggle("ACC States", true, owner));
            interfacebutton.ValueChanged.Subscribe(x => _statesGUI.Show = !_statesGUI.Show);

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            gui_button.GroupingID = GroupingID;
        }

        public static void Maker_started()
        {
            MakerEnabled = Enable.Value;
            if (!MakerEnabled)
            {
                return;
            }
            MakerAPI.MakerExiting += (s, e) => Maker_Ended();

            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;

            _makerGUI = new MakerGUI() { Rect = new Rect(Screen.width * 0.33f, Screen.height * 0.09f, Screen.width * 0.225f, Screen.height * 0.273f) };
            _statesGUI = new StatesGUI() { Rect = new Rect(Screen.width * 0.33f, Screen.height * 0.09f, Screen.width * 0.2f, Screen.height * 0.3f) };
        }

        public static void Maker_Ended()
        {
            MakerAPI.MakerExiting -= (s, e) => Maker_Ended();

            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;

            MakerEnabled = false;
            _makerGUI = null;
            _statesGUI = null;
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
        internal static void Slot_ACC_Change(int slotNo, int type)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                return;
            }
            var controller = ControllerGet;
            if (type == 120)
            {
                controller.SlotBindingData.Remove(slotNo);
                controller.SaveSlotData(slotNo);
            }
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

        internal static void MovIt(List<QueueItem> _) => ControllerGet.UpdatePluginData();
    }
}
