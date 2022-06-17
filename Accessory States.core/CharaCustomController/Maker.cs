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

            var ACC_app_Dropdown = Constants.ConstantOutfitNames.Values.ToArray();

            gui_button = new MakerButton("States GUI", category, owner);
            MakerAPI.AddAccessoryWindowControl(gui_button, true);
            gui_button.OnClick.AddListener(delegate () { GUIToggle(); });

            SetupInterface();

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

            AccessoriesApi.MakerAccSlotAdded += AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;
        }

        public static void Maker_Ended()
        {
            MakerAPI.MakerExiting -= (s, e) => Maker_Ended();

            AccessoriesApi.MakerAccSlotAdded -= AccessoriesApi_MakerAccSlotAdded;
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
            LoadSlotData(e.DestinationSlotIndex);
            UpdateParentedDict();
        }

        private void AccessoriesCopied(AccessoryCopyEventArgs e)
        {
            if (e.CopyDestination == CurrentCoordinate.Value)
            {
                foreach (var item in e.CopiedSlotIndexes)
                {
                    LoadSlotData(item);
                }
                Refresh();
            }
        }

        private static void AccessoriesApi_MakerAccSlotAdded(object sender, AccessorySlotEventArgs e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

            Controller.StartCoroutine(Wait());
            IEnumerator Wait()
            {
                yield return null;
            }
        }

        private void ACC_Is_Parented_ValueChanged(int slot, bool isparented)
        {
            if (!SlotInfo.TryGetValue(slot, out var slotdata))
            {
                slotdata = SlotInfo[slot] = new SlotData();
            }
            slotdata.Parented = isparented;

            UpdateParentedDict();

            if (!GUI_Parent_Dict.TryGetValue(PartsArray[slot].parentKey, out var show) || !isparented)
            {
                show = true;
            }
            ChaControl.SetAccessoryState(slot, show);
        }

        private void UpdateAccessoryshow(SlotData data, int slot, int binding)
        {
            int state;
            if (binding < 9)
            {
                state = ChaControl.fileStatus.clothesState[binding];
            }
            else
            {
                state = GUI_Custom_Dict[binding][0];
            }

            var show = ShowState(state, binding, data);
            if (data.ShoeType != 2 && ChaControl.fileStatus.shoesType != data.ShoeType)
            {
                show = false;
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
                SlotInfo.Remove(slotNo);
                SaveSlotData(slotNo);
            }
        }

        //internal void UpdateClothingNots()
        //{
        //    var clothingnot = ClothNotData;
        //    for (int i = 0, n = clothingnot.Length; i < n; i++)
        //    {
        //        clothingnot[i] = false;
        //    }

        //    var clothinfo = ChaControl.infoClothes;

        //    UpdateTopClothingNots(clothinfo[0], ref clothingnot);
        //    UpdateBraClothingNots(clothinfo[2], ref clothingnot);

        //    ForceClothDataUpdate = false;
        //}

        private void UpdateTopClothingNots(ListInfoBase infoBase, ref bool[] clothingnot)
        {
            if (infoBase == null)
            {
                return;
            }
            Hooks.ClothingNotPatch.IsshortsCheck = false;
            var ListInfoResult = Hooks.ClothingNotPatch.ListInfoResult;

            var key = ChaListDefine.KeyType.Coordinate;
            infoBase.GetInfo(key);
            var notbot = ListInfoResult[key] == 2; //only in ChangeClothesTopAsync

            key = ChaListDefine.KeyType.NotBra;
            infoBase.GetInfo(key);
            var notbra = ListInfoResult[key] == 1; //only in ChangeClothesTopAsync

            clothingnot[0] = clothingnot[0] || notbot;
            clothingnot[1] = clothingnot[1] || notbra;
        }

        private void UpdateBraClothingNots(ListInfoBase infoBase, ref bool[] clothingnot)
        {
            if (infoBase == null)
            {
                return;
            }
            Hooks.ClothingNotPatch.IsshortsCheck = true;

            var ListInfoResult = Hooks.ClothingNotPatch.ListInfoResult;
            var key = ChaListDefine.KeyType.Coordinate;

            infoBase.GetInfo(key);//kk uses coordinate to hide shorts

            var notShorts = ListInfoResult[key] == 2; //only in ChangeClothesBraAsync

            clothingnot[2] = clothingnot[2] || notShorts;
        }

        internal void ClothingTypeChange(int kind, int index)
        {
            if (index != 0)
                return;
            //switch (kind)
            //{
            //    case 1:
            //        if (ClothNotData[0])
            //            return;
            //        break;
            //    case 2:
            //        if (ClothNotData[1])
            //            return;
            //        break;
            //    case 3:
            //        if (ClothNotData[2])
            //            return;
            //        break;
            //    case 7:
            //        if (ChaControl.nowCoordinate.clothes.parts[8].id == 0)
            //        {
            //            RemoveClothingBinding(9);
            //        }
            //        break;
            //    case 8:
            //        if (ChaControl.nowCoordinate.clothes.parts[7].id == 0)
            //        {
            //            RemoveClothingBinding(9);
            //        }
            //        break;
            //    default:
            //        break;
            //}
            RemoveClothingBinding(kind);
            SaveCoordinateData();
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
