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
        static AccessoryControlWrapper<MakerDropdown, int> GroupSelect;
        static AccessoryControlWrapper<MakerSlider, float> StartState;
        static AccessoryControlWrapper<MakerSlider, float> EndState;
        static MakerWindow makerWindow;
        static MakerButton makerButton;
        static MakerButton gui_button;

        static bool MakerEnabled = false;

        static CharaEvent ControllerGet => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                if (makerWindow == null || makerWindow.IsDestroyed())
                {
                    Settings.Logger.LogWarning($"attempted to create window");

                    makerWindow = MakerWindow.CreateMakerWindow("Test window", false, "04_AccessoryTop", Settings.Instance);
                    //Settings.Logger.LogWarning($"attempted to create window\n{makerWindow.transform.name}");
                }
                if (makerButton == null && (makerWindow != null || makerWindow.IsDestroyed()))
                {
                    for (var i = 0; i < 20; i++)
                    {
                        makerButton = new MakerButton("test button " + i, new MakerCategory(null, null), Settings.Instance);
                        MakerAPI.AddCustomWindowControl(makerWindow, makerButton);
                    }
                }
                if (makerWindow != null)
                {
                    MakerAPI.CreateCustomWindowControls(makerWindow);
                }
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                Settings.Logger.LogWarning("Test warning");
                if (makerWindow != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)makerWindow.transform);
            }
        }

        public static void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            MakerEnabled = Settings.Enable.Value;
            if (!MakerEnabled)
            {
                return;
            }

            var owner = Settings.Instance;
            var category = new MakerCategory(null, null);
            //var category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 3, "Accessory Settings");

            var ACC_app_Dropdown = Constants.ConstantOutfitNames.Values.ToArray();
            //string[] ACC_app_Dropdown = new string[] { "None", "Top", "Bottom", "Bra", "Panty", "Gloves", "Pantyhose", "Socks", "Shoes" };

            GroupSelect = MakerAPI.AddEditableAccessoryWindowControl<MakerDropdown, int>(new MakerDropdown("Clothing\nBind", ACC_app_Dropdown, category, 0, owner), true);
            GroupSelect.ValueChanged += (s, es) => ControllerGet.ACC_Appearance_dropdown_ValueChanged(es.SlotIndex, es.NewValue - 1);

            var Start_Slider = new MakerSlider(category, "Start", 0, 3, 0, owner)
            {
                WholeNumbers = true
            };
            StartState = MakerAPI.AddEditableAccessoryWindowControl<MakerSlider, float>(Start_Slider, true);
            StartState.ValueChanged += (s, es) => ControllerGet.ACC_Appearance_state_ValueChanged(es.SlotIndex, Mathf.RoundToInt(es.NewValue), 0);

            var Stop_Slider = new MakerSlider(category, "Stop", 0, 3, 3, owner)
            {
                WholeNumbers = true
            };
            EndState = MakerAPI.AddEditableAccessoryWindowControl<MakerSlider, float>(Stop_Slider, true);
            EndState.ValueChanged += (s, es) => ControllerGet.ACC_Appearance_state2_ValueChanged(es.SlotIndex, Mathf.RoundToInt(es.NewValue), 0);

            gui_button = new MakerButton("States GUI", category, owner);
            MakerAPI.AddAccessoryWindowControl(gui_button, true);
            gui_button.OnClick.AddListener(delegate () { GUIToggle(); });

            SetupInterface();

            var interfacebutton = e.AddSidebarControl(new SidebarToggle("ACC States", true, owner));
            interfacebutton.ValueChanged.Subscribe(x => ShowToggleInterface = x);

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            GroupSelect.Control.GroupingID = GroupingID;
            StartState.Control.GroupingID = GroupingID;
            EndState.Control.GroupingID = GroupingID;
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

            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;

            AccessoriesApi.MakerAccSlotAdded += AccessoriesApi_MakerAccSlotAdded;
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;
        }

        public static void Maker_Ended()
        {
            MakerAPI.MakerExiting -= (s, e) => Maker_Ended();

            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;

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
            GroupSelect.SetValue(e.DestinationSlotIndex, GroupSelect.GetValue(e.SourceSlotIndex), false);
            StartState.SetValue(e.DestinationSlotIndex, StartState.GetValue(e.SourceSlotIndex), false);
            EndState.SetValue(e.DestinationSlotIndex, EndState.GetValue(e.SourceSlotIndex), false);
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
                StartCoroutine(WaitForSlots());
            }
        }

        private static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            var Controller = ControllerGet;
            Controller.StartCoroutine(Controller.WaitForSlots());
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

        private void ACC_Appearance_state_ValueChanged(int slot, int newvalue, int index)
        {
            if (!SlotInfo.TryGetValue(slot, out var data))
            {
                data = SlotInfo[slot] = new SlotData() { States = new List<int[]> { new int[] { newvalue, 3 } } };
            }

            data.States[index][0] = newvalue;

            if (data.Binding < 0)
            {
                return;
            }

            UpdateAccessoryshow(data, slot);
        }

        private void ACC_Appearance_state2_ValueChanged(int slot, int newvalue, int index)
        {
            if (!SlotInfo.TryGetValue(slot, out var data))
            {
                data = SlotInfo[slot] = new SlotData() { States = new List<int[]> { new int[] { 0, newvalue } } };
            }

            data.States[index][1] = newvalue;

            var slider = StartState.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>();
            var slider2 = EndState.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>();
            if (autoscale && GroupSelect.GetValue(slot) >= Constants.ClothingLength)
            {
                UpdateTogglesGUI();

                if (newvalue == Mathf.RoundToInt(slider2.maxValue))
                {
                    slider2.maxValue = slider.maxValue += 1;
                }
                var maxstate = GUI_Custom_Dict[GroupSelect.GetValue(slot)][1];
                if (slider2.maxValue > maxstate)
                {
                    slider2.maxValue = slider.maxValue = maxstate;
                }
            }
            if (data.Binding < 0)
            {
                return;
            }
            UpdateAccessoryshow(data, slot);
        }

        private void UpdateAccessoryshow(SlotData data, int slot)
        {
            int state;
            var binding = data.Binding;
            if (binding < 9)
            {
                state = ChaControl.fileStatus.clothesState[binding];
            }
            else
            {
                state = GUI_Custom_Dict[binding][0];
            }

            var show = ShowState(state, data.States);
            if (data.ShoeType != 2 && ChaControl.fileStatus.shoesType != data.ShoeType)
            {
                show = false;
            }
            ChaControl.SetAccessoryState(slot, show);
        }

        private void ACC_Appearance_dropdown_ValueChanged(int slot, int newvalue)
        {
            var delete = newvalue < 0;
            var kind = 0;
            if (!delete)
            {
                if (newvalue < Constants.ClothingLength)
                {
                    kind = SlotInfo[slot].Binding = newvalue;
                }
                else
                {
                    kind = SlotInfo[slot].Binding = newvalue - Constants.ClothingLength;
                }
            }

            if (delete)
            {
                //do nothing
            }
            else if (newvalue < 9)
            {
                StartState.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue = 3;
                EndState.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue = 3;
                StartState.SetValue(slot, 0);
                EndState.SetValue(slot, 3);
            }
            else
            {
                var limit = Math.Max(MaxState(kind), minilimit);
                StartState.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue = limit;
                EndState.Control.ControlObjects.ElementAt(slot).GetComponentInChildren<Slider>().maxValue = limit;
                StartState.SetValue(slot, 0);
                EndState.SetValue(slot, limit);
            }

            if (!ASSExists)
            {
                return;
            }

            //if (delete)
            //{
            //    DeleteGroup(kind);
            //}
            //else
            //{
            //    ChangeTriggerProperty(kind);
            //}
        }

        private void UpdateMakerDropboxes()
        {
            var ACC_Appearance_dropdown_Controls = GroupSelect.Control.ControlObjects.ToList();
            var Appearance_Options = new List<TMP_Dropdown.OptionData>(ACC_Appearance_dropdown_Controls[0].GetComponentInChildren<TMP_Dropdown>().options);
            var optioncount = Constants.ClothingLength;
            var namedict = Names;
            var n = namedict.Count;

            if (Appearance_Options.Count > optioncount)
            {
                Appearance_Options.RemoveRange(optioncount, Appearance_Options.Count - (optioncount));
            }

            foreach (var item in namedict)
            {
                var temp = new TMP_Dropdown.OptionData(item.Name);
                Appearance_Options.Add(temp);
            }

            foreach (var item in GroupSelect.Control.ControlObjects)
            {
                item.GetComponentInChildren<TMP_Dropdown>().ClearOptions();
                item.GetComponentInChildren<TMP_Dropdown>().AddOptions(Appearance_Options);
            }

            foreach (var item in SlotInfo)
            {
                if (item.Value.Binding < Constants.ClothingLength)
                {
                    GroupSelect.SetValue(item.Key, item.Value.Binding + 1, false);
                }
                else
                {
                    var index = 0;
                    for (; index < n; index++)
                    {
                        if (index + Constants.ClothingLength == item.Value.Binding)
                        {
                            break;
                        }
                    }
                    GroupSelect.SetValue(item.Key, index + Constants.ClothingLength, false);
                }
            }
        }

        private IEnumerator WaitForSlots()
        {
            if (!MakerAPI.InsideMaker)
            {
                yield break;
            }

            var ACCData = PartsArray.Length;
            do
            {
                yield return null;
            } while (!MakerAPI.InsideAndLoaded || StartState.Control.ControlObjects.Count() < ACCData);
            Refresh();

            UpdateMakerDropboxes();
            var slider1 = StartState.Control.ControlObjects;
            var slider2 = EndState.Control.ControlObjects;
            var slotinfo = SlotInfo;
            for (var i = 0; i < ACCData; i++)
            {
                if (!slotinfo.TryGetValue(i, out var slotdata))
                {
                    slotdata = new SlotData();
                    slotinfo[i] = slotdata;
                }
                var binding = slotdata.Binding;
                var max = MaxState(binding);
                var zerostate = slotdata.States[0];
                slider1.ElementAt(i).GetComponentInChildren<Slider>().maxValue = Math.Max(3, max);

                StartState.SetValue(i, zerostate[0], false);

                slider2.ElementAt(i).GetComponentInChildren<Slider>().maxValue = Math.Max(3, max);

                EndState.SetValue(i, zerostate[1], false);
                if (binding < Constants.ClothingLength)
                {
                    binding += 1;
                }
                else
                {
                    binding = Constants.ClothingLength + Names.FindIndex(x => x.Name == slotdata.GroupName);
                }
                GroupSelect.SetValue(i, binding, false);
            }
            UpdateClothingNots();
            UpdateTogglesGUI();
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
                GroupSelect.SetValue(slotNo, 0, false);
                StartState.SetValue(slotNo, 0, false);
                EndState.SetValue(slotNo, 3, false);
                SaveSlotData(slotNo);
            }
        }

        internal void UpdateClothingNots()
        {
            var clothingnot = ClothNotData;
            for (int i = 0, n = clothingnot.Length; i < n; i++)
            {
                clothingnot[i] = false;
            }

            var clothinfo = ChaControl.infoClothes;

            UpdateTopClothingNots(clothinfo[0], ref clothingnot);
            UpdateBraClothingNots(clothinfo[2], ref clothingnot);

            ForceClothDataUpdate = false;
        }

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
                case 7:
                    if (ChaControl.nowCoordinate.clothes.parts[8].id == 0)
                    {
                        RemoveClothingBinding(9);
                    }
                    break;
                case 8:
                    if (ChaControl.nowCoordinate.clothes.parts[7].id == 0)
                    {
                        RemoveClothingBinding(9);
                    }
                    break;
                default:
                    break;
            }
            RemoveClothingBinding(kind);
            SaveCoordinateData();
        }

        private void RemoveClothingBinding(int kind)
        {
            var slotinfo = SlotInfo;
            var removelist = slotinfo.Where(x => x.Value.Binding == kind).ToList();
            for (int i = 0, n = removelist.Count; i < n; i++)
            {
                slotinfo.Remove(removelist[i].Key);
                SaveSlotData(removelist[i].Key);
            }
        }
    }
}
