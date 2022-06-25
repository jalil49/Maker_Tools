using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Accessory_States
{
    partial class CharaEvent : CharaCustomFunctionController
    {
        static MakerButton gui_button;

        static bool MakerEnabled = false;

        static CharaEvent ControllerGet => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        public static void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            MakerEnabled = Settings.Enable.Value;
            if (!MakerEnabled)
            {
                return;
            }

            var owner = Settings.Instance;
            var category = new MakerCategory(null, null);

            gui_button = new MakerButton("States GUI", category, owner);
            MakerAPI.AddAccessoryWindowControl(gui_button, true);
            gui_button.OnClick.AddListener(delegate () { });


            var interfacebutton = e.AddSidebarControl(new SidebarToggle("ACC States", true, owner));
            interfacebutton.ValueChanged.Subscribe(x => ShowToggleInterface = x);

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            gui_button.GroupingID = GroupingID;
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

            ShowCustomGui = false;
            MakerEnabled = false;
            ShowToggleInterface = false;
        }

        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            ControllerGet.AccessoriesTransferred(e);
        }

        private static void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            ControllerGet.AccessoriesCopied(e);
        }

        private void AccessoriesTransferred(AccessoryTransferEventArgs e)
        {
            SlotBindingData.Remove(e.DestinationSlotIndex);
            LoadSlotData(e.DestinationSlotIndex);
            UpdateParentedDict();
        }

        private void AccessoriesCopied(AccessoryCopyEventArgs e)
        {
            if (e.CopyDestination == CurrentCoordinate.Value)
            {
                foreach (var item in e.CopiedSlotIndexes)
                {
                    SlotBindingData.Remove(item);
                    LoadSlotData(item);
                }
            }
        }
        private void ACC_Is_Parented_ValueChanged(int slot, bool isparented)
        {
            if (!SlotBindingData.TryGetValue(slot, out var slotdata))
            {
                slotdata = SlotBindingData[slot] = new SlotData();
            }
            slotdata.Parented = isparented;

            UpdateParentedDict();

            if (!GUI_Parent_Dict.TryGetValue(PartsArray[slot].parentKey, out var show) || !isparented)
            {
                show = true;
            }
            ChaControl.SetAccessoryState(slot, show);
        }

        internal void Slot_ACC_Change(int slotNo, int type)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                return;
            }
            if (type == 120)
            {
                SlotBindingData.Remove(slotNo);
                SaveSlotData(slotNo);
            }
        }

        internal void ClothingTypeChange(int kind, int index)
        {
            if (index != 0)
                return;

            UpdateClothNots();

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
            RemoveClothingBinding(kind);
        }

        private void UpdateClothNots()
        {
            ClothNotData = new bool[3] { false, false, false };
            ClothNotData[0] = ChaControl.notBot || GetClothingNot(0, ChaListDefine.KeyType.Coordinate) == 2;
            ClothNotData[1] = ChaControl.notBra || GetClothingNot(0, ChaListDefine.KeyType.Coordinate) == 1;
            ClothNotData[2] = ChaControl.notShorts || GetClothingNot(2, ChaListDefine.KeyType.Coordinate) == 2;
        }

        public bool ClothingUnlocker(int kind, ChaListDefine.KeyType value)
        {
            var listInfoBase = GetListInfoBase(kind);
            if (listInfoBase == null) return false;
            var intValue = GetClothingNot(listInfoBase, value);
            if (intValue == listInfoBase.GetInfoInt(value)) return false;

            return true;
        }

        public int GetClothingNot(int kind, ChaListDefine.KeyType key)
        {
            return GetClothingNot(GetListInfoBase(kind), key);
        }
        public int GetClothingNot(ListInfoBase listInfo, ChaListDefine.KeyType key)
        {
            if (listInfo == null) return 0;
            if (!(listInfo.dictInfo.TryGetValue((int)key, out var stringValue) && int.TryParse(stringValue, out var intValue))) return 0;

            return intValue;
        }
        public ListInfoBase GetListInfoBase(int kind)
        {
            var lists = ChaControl.infoClothes;
            if (kind >= lists.Length || kind < 0) return null;
            return lists[kind];
        }
        private void RemoveClothingBinding(int kind)
        {
            var slotinfo = SlotInfo;
            var removelist = slotinfo.Where(x => x.Value.bindingDatas.Any(y => y.Binding == kind)).ToList();
            for (int i = 0, n = removelist.Count; i < n; i++)
            {
                var bindData = slotinfo[removelist[i].Key].bindingDatas;
                for (var j = bindData.Count - 1; j >= 0; j--)
                {
                    if (bindData[j].Binding == kind)
                        bindData.RemoveAt(j);
                }
                SaveSlotData(removelist[i].Key);
            }
        }
    }
}
