using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using TMPro;
using UnityEngine;

namespace Accessory_Parents
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private static MakerDropdown _parentDropDown;

        private static MakerText _parentText;
        private static MakerText _childText;

        private static MakerButton _saveRelativeButton;

        private static MakerButton _childButton;
        private static MakerButton _guiButton;

        private static bool _makerEnabled;

        private static CharaEvent GetController => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        private static bool _retrospect;
        private static bool _recursiveStop;

        public static void MakerAPI_MakerExiting(object sender, EventArgs e)
        {
            AccessoriesApi.AccessoryKindChanged -= AccessoriesApi_AccessoryKindChanged;

            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;
            MakerAPI.MakerExiting -= MakerAPI_MakerExiting;

            _retrospect = false;
            _recursiveStop = false;
            _makerEnabled = false;
            _showCustomGui = false;
            _showreplace = false;
            _showDelete = false;
        }

        public static void MakerAPI_MakerStartedLoading(object sender, RegisterCustomControlsEvent e)
        {
            _makerEnabled = Settings.Enable.Value;
            if (!_makerEnabled) return;
            MakerAPI.MakerExiting += MakerAPI_MakerExiting;

            AccessoriesApi.AccessoryKindChanged += AccessoriesApi_AccessoryKindChanged;
            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;
        }

        internal void RemoveOutfitEvent()
        {
            Removeoutfit(_parentData.Keys.Max());
        }

        internal void AddOutfitEvent()
        {
            for (var i = _parentData.Keys.Max(); i < ChaFileControl.coordinate.Length; i++)
                Createoutfit(i);
        }

        public static void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            _makerEnabled = Settings.Enable.Value;
            if (!_makerEnabled) return;
            var owner = Settings.Instance;
            var category = new MakerCategory("", "");

            _parentText = MakerAPI.AddAccessoryWindowControl(new MakerText("Not Parent", category, owner), true);
            _childText = MakerAPI.AddAccessoryWindowControl(new MakerText("Not Child", category, owner), true);
            var dropdown = new MakerDropdown("Parent", new[] { "None" }, category, 0, owner);
            _parentDropDown = MakerAPI.AddAccessoryWindowControl(dropdown, true);

            _childButton = MakerAPI.AddAccessoryWindowControl(new MakerButton("Make Child", category, owner), true);
            _childButton.OnClick.AddListener(delegate { GetController.MakeChild(); });
            _saveRelativeButton =
                MakerAPI.AddAccessoryWindowControl(new MakerButton("Save Position", category, owner), true);
            _saveRelativeButton.OnClick.AddListener(delegate
            {
                GetController.Save_Relative_Data(AccessoriesApi.SelectedMakerAccSlot);
            });

            _guiButton = MakerAPI.AddAccessoryWindowControl(new MakerButton("Parent GUI", category, owner), true);
            _guiButton.OnClick.AddListener(delegate { GUI_Toggle(); });

            var groupingID = "Maker_Tools_" + Settings.NamingID.Value;
            _parentDropDown.GroupingID = groupingID;
            _parentText.GroupingID = groupingID;
            _childButton.GroupingID = groupingID;
            _saveRelativeButton.GroupingID = groupingID;
            _childText.GroupingID = groupingID;
            _guiButton.GroupingID = groupingID;
        }

        public static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            GetController.Update_Drop_boxes();
        }

        private static void AccessoriesApi_AccessoryKindChanged(object sender, AccessorySlotEventArgs e)
        {
            ControllerGet.AccessoryKindChanged(e);
        }

        private void AccessoryKindChanged(AccessorySlotEventArgs e)
        {
            if (Child.TryGetValue(e.SlotIndex, out var parentKey))
                Keep_Last_Data(e.SlotIndex, parentKey);
            else if (OldParent.ContainsKey(e.SlotIndex) && RelativeData.ContainsKey(e.SlotIndex))
                Keep_Last_Data(e.SlotIndex);
        }

        private void Update_Text()
        {
            var slot = AccessoriesApi.SelectedMakerAccSlot;
            if (_parentText.ControlObjects.Count() <= slot) return;
            var output = "Slot " + (slot + 1);
            var find = ParentGroups.Where(x => x.ParentSlot == slot).ToArray();
            if (find.Length > 0)
            {
                output += " is parent of ";
                for (int i = 0, n = find.Length; i < n; i++)
                    if (i < n - 2)
                        output += find[i].Name + ", ";
                    else if (i == n - 2)
                        output += find[i].Name + " and ";
                    else
                        output += find[i].Name;
            }
            else
            {
                output += " is not Parent";
            }

            foreach (var item in _parentText.ControlObjects)
                item.GetComponentInChildren<TextMeshProUGUI>().text = output;
            string output2;
            if (Child.TryGetValue(slot, out var value))
                output2 = "Child of " + ParentGroups.First(x => x.ParentSlot == value).Name;
            else
                output2 = "Is not a Child";
            foreach (var item in _childText.ControlObjects)
                item.GetComponentInChildren<TextMeshProUGUI>().text = output2;
        }
#if false
        internal void Scale_Change(int slotNo, int correctNo, float value, bool add, int flags)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker || !MakerEnabled || RecursiveStop)
            {
                return;
            }
            if (!add && value == 1f && ContainsCustomNameSlot(slotNo))
            {
                float Value;
                if (Relative_Data.TryGetValue(slotNo, out var vectors))
                {
                    if (flags == 1)
                    {
                        Value = vectors[0, 2].x;
                    }
                    else if (flags == 2)
                    {
                        Value = vectors[0, 2].y;
                    }
                    else if (flags == 4)
                    {
                        Value = vectors[0, 2].z;
                    }
                    else
                    {
                        Value = 1f;
                    }
                }
                else
                {
                    Value = 1f;
                }
                if (Value != 1f)
                {
                    ChaControl.SetAccessoryScl(slotNo, correctNo, Value, add, flags);
                    return;
                }
            }

            if (TryChildListBySlot(slotNo, out var parentList))
            {
                if (add)
                {
                    foreach (var item in parentList)
                    {
                        ChaControl.SetAccessoryScl(item, correctNo, value, add, flags);
                    }
                }
                else
                {
                    foreach (var item in parentList)
                    {
                        float Value;
                        if (Relative_Data.TryGetValue(item, out var vectors))
                        {
                            if (flags == 1)
                            {
                                Value = vectors[0, 2].x;
                            }
                            else if (flags == 2)
                            {
                                Value = vectors[0, 2].y;
                            }
                            else if (flags == 4)
                            {
                                Value = vectors[0, 2].z;
                            }
                            else
                            {
                                Value = 1;
                            }
                        }
                        else
                        {
                            Value = 1;
                        }
                        ChaControl.SetAccessoryScl(item, correctNo, Value, add, flags);
                    }
                }
            }
        }

        internal void Rotation_Change(int slotNo, int correctNo, float value, bool add, int flags)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker || !MakerEnabled || RecursiveStop)
            {
                return;
            }

            if (!add && value == 0 && ContainsCustomNameSlot(slotNo))
            {
                float Value;
                if (Relative_Data.TryGetValue(slotNo, out var vectors))
                {
                    if (flags == 1)
                    {
                        Value = vectors[0, 1].x;
                    }
                    else if (flags == 2)
                    {
                        Value = vectors[0, 1].y;
                    }
                    else if (flags == 4)
                    {
                        Value = vectors[0, 1].z;
                    }
                    else
                    {
                        Value = 0f;
                    }
                }
                else
                {
                    Value = 0f;
                }
                if (Value != 0f)
                {
                    ChaControl.SetAccessoryRot(slotNo, correctNo, Value, add, flags);
                    return;
                }
            }

            if (TryChildListBySlot(slotNo, out var parentList))
            {
                Vector3 Rot = Vector3.zero;

                if (flags == 1)
                {
                    Rot.x = value;
                }
                else if (flags == 2)
                {
                    Rot.y = value;
                }
                else if (flags == 4)
                {
                    Rot.z = value;
                }

                var store = AccessoriesApi.GetPartsInfo(slotNo).addMove;
                Vector3 original_pos;
                original_pos = new Vector3(store[0, 0].x, store[0, 0].y, store[0, 0].z);

                if (add)
                {
                    Vector3 New_pos;
                    foreach (var item in parentList)
                    {
                        ChaControl.SetAccessoryRot(item, correctNo, value, add, flags);
                        var store2 = AccessoriesApi.GetPartsInfo(slotNo).addMove;
                        New_pos = new Vector3(store2[0, 0].x, store2[0, 0].y, store2[0, 0].z) - original_pos;
                        //Logger.LogWarning($"Old X: {New_pos.x}, Old Y: {New_pos.y}, Old Z: {New_pos.z}");
                        New_pos = Quaternion.Euler(Rot) * New_pos;
                        New_pos -= new Vector3(store2[0, 0].x, store2[0, 0].y, store2[0, 0].z) - original_pos;
                        //Logger.LogWarning($"New X: {New_pos.x}, New Y: {New_pos.y}, New Z: {New_pos.z}");
                        for (int i = 0; i < 3; i++)
                        {
                            int flag;
                            switch (i)
                            {
                                case 0:
                                    flag = 1;
                                    break;
                                case 1:
                                    flag = 2;
                                    break;
                                default:
                                    flag = 4;
                                    break;
                            }
                            ChaControl.SetAccessoryPos(item, correctNo, New_pos[i], true, flag);
                        }
                        if (item < 20)
                        {
                            ChaControl.chaFile.coordinate[CoordinateNum].accessory.parts[item].addMove =
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   ChaControl.nowCoordinate.accessory.parts[item].addMove;
                        }
                    }
                }
                else
                {
                    //Vector3 New_pos;
                    float Value;
                    foreach (var item in parentList)
                    {
                        //Vector3 Rot2 = Vector3.zero;
                        var store2 = AccessoriesApi.GetPartsInfo(item).addMove;

                        if (Relative_Data.TryGetValue(item, out var vectors))
                        {
                            if (flags == 1)
                            {
                                Value = vectors[0, 1].x;
                                //Rot2.x = vectors[0, 1].x - store2[CorrectNo, 1].x;
                            }
                            else if (flags == 2)
                            {
                                Value = vectors[0, 1].y;

                                //Rot2.y = vectors[0, 1].y - store2[CorrectNo, 1].y;
                            }
                            else if (flags == 4)
                            {
                                Value = vectors[0, 1].z;
                                //Rot2.z = vectors[0, 1].z - store2[CorrectNo, 1].z;
                            }
                            else
                            {
                                Value = 0;
                            }
                        }
                        else
                        {
                            Value = 0;
                        }
                        ChaControl.SetAccessoryRot(item, correctNo, Value, add, flags);

                        //New_pos = new Vector3(store2[0, 0].x, store2[0, 0].y, store2[0, 0].z) - original_pos;
                        //New_pos = Quaternion.Euler(Rot2) * New_pos;
                        //New_pos -= new Vector3(store2[0, 0].x, store2[0, 0].y, store2[0, 0].z) - original_pos;

                        //for (int i = 0; i < 3; i++)
                        //{
                        //    int flag;
                        //    switch (i)
                        //    {
                        //        case 0:
                        //            flag = 1;
                        //            break;
                        //        case 1:
                        //            flag = 2;
                        //            break;
                        //        default:
                        //            flag = 4;
                        //            break;
                        //    }
                        //    ChaControl.SetAccessoryPos(item, CorrectNo, New_pos[i], true, flag);
                        //}


                        if (item < 20)
                        {
                            ChaControl.chaFile.coordinate[CoordinateNum].accessory.parts[item].addMove =
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   ChaControl.nowCoordinate.accessory.parts[item].addMove;
                        }
                    }
                }

            }
        }

        internal void Position_Change(int slotNo, int correctNo, float value, bool add, int flags)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker || !MakerEnabled || RecursiveStop)
            {
                return;
            }

            if (!add && value == 0 && ContainsCustomNameSlot(slotNo))
            {
                float Value;
                if (Relative_Data.TryGetValue(slotNo, out var vectors))
                {
                    if (flags == 1)
                    {
                        Value = vectors[0, 0].x;
                    }
                    else if (flags == 2)
                    {
                        Value = vectors[0, 0].y;
                    }
                    else if (flags == 4)
                    {
                        Value = vectors[0, 0].z;
                    }
                    else
                    {
                        Value = 0f;
                    }
                }
                else
                {
                    Value = 0f;
                }
                if (Value != 0f)
                {
                    ChaControl.SetAccessoryPos(slotNo, correctNo, Value, add, flags);
                    return;
                }
            }

            if (TryChildListBySlot(slotNo, out var parentList))
            {
                if (add)
                {
                    foreach (var item in parentList)
                    {
                        ChaControl.SetAccessoryPos(item, correctNo, value, add, flags);
                        if (item < 20)
                        {
                            ChaControl.chaFile.coordinate[CoordinateNum].accessory.parts[item].addMove =
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      ChaControl.nowCoordinate.accessory.parts[item].addMove;
                        }
                    }
                }
                else
                {
                    float Value;
                    foreach (var item in parentList)
                    {
                        if (Relative_Data.TryGetValue(item, out var vectors))
                        {
                            if (flags == 1)
                            {
                                Value = vectors[0, 0].x;
                            }
                            else if (flags == 2)
                            {
                                Value = vectors[0, 0].y;
                            }
                            else if (flags == 4)
                            {
                                Value = vectors[0, 0].z;
                            }
                            else
                            {
                                Value = 0;
                            }
                        }
                        else
                        {
                            Value = 0;
                        }
                        ChaControl.SetAccessoryPos(item, correctNo, Value, false, flags);
                        if (item < 20)
                        {
                            ChaControl.chaFile.coordinate[CoordinateNum].accessory.parts[item].addMove =
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      ChaControl.nowCoordinate.accessory.parts[item].addMove;
                        }
                    }

                }
            }
        }
#endif
        internal void Slot_ACC_Change(int slotNo, int type)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker || !_makerEnabled) return;

            if (type == 120)
            {
                var unbindlist = ParentGroups.Where(x => x.ParentSlot == slotNo);
                for (int i = 0, n = unbindlist.Count(); i < n; i++) unbindlist.ElementAt(i).ParentSlot = -1;
                RelativeData.Remove(slotNo);
                UpdateRelations();
                Update_Drop_boxes();
            }
            else
            {
                if (Child.TryGetValue(slotNo, out var parentKey))
                    Keep_Last_Data(slotNo, parentKey);
                else if (OldParent.ContainsKey(slotNo) && RelativeData.ContainsKey(slotNo)) Keep_Last_Data(slotNo);
            }
        }

        private void Save_Relative_Data(int slot)
        {
            var partsinfo = AccessoriesApi.GetPartsInfo(slot);
            if (partsinfo.type == 120) return;
            if (!RelativeData.TryGetValue(slot, out var relative))
            {
                relative = new Vector3[3];
                for (var i = 0; i < 2; i++)
                {
                    relative[0] = Vector3.zero;
                    relative[1] = Vector3.zero;
                    relative[2] = Vector3.one;
                }

                RelativeData[slot] = relative;
            }

            var temp = partsinfo.addMove;
            {
                const int i = 0;
                for (var j = 0; j < 3; j++) relative[j] = new Vector3(temp[i, j].x, temp[i, j].y, temp[i, j].z);
            }

            if (TryChildListBySlot(slot, out var parentList))
            {
                parentList = parentList.Distinct().ToList();

                foreach (var item in parentList) Save_Relative_Data(item);
            }
        }

        private void Update_Drop_boxes()
        {
            StartCoroutine(Wait());

            IEnumerator Wait()
            {
                if (!MakerAPI.InsideMaker || !_makerEnabled) yield break;

                while (!MakerAPI.InsideAndLoaded) yield return null;


                var controlObjects = _parentDropDown.ControlObjects;
                var options = new List<TMP_Dropdown.OptionData>(_parentDropDown.ControlObject
                    .GetComponentInChildren<TMP_Dropdown>().options);
                if (options.Count > 1) options.RemoveRange(1, options.Count - 1);
                foreach (var item in ParentGroups) options.Add(new TMP_Dropdown.OptionData(item.Name));
                foreach (var item in controlObjects) item.GetComponentInChildren<TMP_Dropdown>().options = options;
                Update_Old_Parents();
            }
        }

        private void Move_To_Parent_Postion(int slot, int parentKey)
        {
            var partsinfo = AccessoriesApi.GetPartsInfo(parentKey);
            var original = partsinfo.addMove;
            var parentName = partsinfo.parentKey;
            ChaControl.ChangeAccessoryParent(slot, parentName);
            //set position
            ChaControl.SetAccessoryPos(slot, 0, original[0, 0].x, false, 1);
            ChaControl.SetAccessoryPos(slot, 0, original[0, 0].y, false, 2);
            ChaControl.SetAccessoryPos(slot, 0, original[0, 0].z, false, 4);
            //set rotation
            ChaControl.SetAccessoryRot(slot, 0, original[0, 1].x, false, 1);
            ChaControl.SetAccessoryRot(slot, 0, original[0, 1].y, false, 2);
            ChaControl.SetAccessoryRot(slot, 0, original[0, 1].z, false, 4);
            //set scale
            ChaControl.SetAccessoryScl(slot, 0, original[0, 2].x, false, 1);
            ChaControl.SetAccessoryScl(slot, 0, original[0, 2].y, false, 2);
            ChaControl.SetAccessoryScl(slot, 0, original[0, 2].z, false, 4);

            UpdateAccessoryInfo(slot);
        }

        private void Keep_Last_Data(int slot, int parentKey)
        {
            var parentName = AccessoriesApi.GetPartsInfo(parentKey).parentKey;
            if (!RelativeData.TryGetValue(slot, out var original))
                for (var i = 0; i < 2; i++)
                {
                    original[0] = Vector3.zero;
                    original[1] = Vector3.zero;
                    original[2] = Vector3.one;
                }

            ChaControl.ChangeAccessoryParent(slot, parentName);

            for (var i = 0; i < 3; i++)
            {
                var flag = (int)Math.Pow(2, i);
                //set position
                ChaControl.SetAccessoryPos(slot, 0, original[0][i], false, flag);
                //set rotation
                ChaControl.SetAccessoryRot(slot, 0, original[1][i], false, flag);
                //set scale
                ChaControl.SetAccessoryScl(slot, 0, original[2][i], false, flag);
            }
        }

        private void Keep_Last_Data(int slot)
        {
            var original = RelativeData[slot];
            var parentName = OldParent[slot];

            ChaControl.ChangeAccessoryParent(slot, parentName);
            for (var i = 0; i < 3; i++)
            {
                var flag = (int)Math.Pow(2, i);
                //set position
                ChaControl.SetAccessoryPos(slot, 0, original[0][i], false, flag);
                //set rotation
                ChaControl.SetAccessoryRot(slot, 0, original[1][i], false, flag);
                //set scale
                ChaControl.SetAccessoryScl(slot, 0, original[2][i], false, flag);
            }
        }

        private void Update_Old_Parents()
        {
            OldParent.Clear();
            foreach (var item in ParentGroups)
            {
                var slot = item.ParentSlot;
                if (slot < 0 || Child.ContainsKey(slot)) continue;
                var parentKey = AccessoriesApi.GetPartsInfo(slot).parentKey;
                OldParent[slot] = string.Copy(parentKey);
            }
        }

        private void MakeChild()
        {
            var slot = AccessoriesApi.SelectedMakerAccSlot;

            if (AccessoriesApi.GetPartsInfo(slot).type == 120)
                return;

            if (_parentDropDown.Value == 0) return;

            var nameData = ParentGroups[_parentDropDown.Value - 1];
            if (RelatedNames[nameData.ParentSlot].Contains(slot)) return;

            if (!nameData.childSlots.Contains(slot))
                nameData.childSlots.Add(slot);

            MakeChild(slot, nameData.ParentSlot);
        }

        private void MakeChild(int slot, int parentSlot)
        {
            if (parentSlot < 0) return;

            if (!_retrospect) Move_To_Parent_Postion(slot, parentSlot);

            UpdateRelations();
            Save_Relative_Data(slot);
            Update_Old_Parents();
            Update_Text();
        }

        internal void Update()
        {
            if (Input.anyKeyDown && AccessoriesApi.AccessoryCanvasVisible)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    MakeChild();
                    return;
                }

                if (Input.GetKeyDown(KeyCode.F) &&
                    AccessoriesApi.GetPartsInfo(AccessoriesApi.SelectedMakerAccSlot).type != 120)
                {
                    AddTheme(true);
                    MakeChild();
                    _parentDropDown.SetValue(ParentGroups.Count + 1, false);
                }
            }
        }

        private bool TryChildListBySlot(int slot, out List<int> childlist, bool recursive = false)
        {
            childlist = new List<int>();

            foreach (var item in ParentGroups.Where(x => x.ParentSlot == slot))
            {
                childlist.AddRange(item.childSlots);
                if (recursive)
                    foreach (var item2 in item.childSlots)
                        if (TryChildListBySlot(item2, out var temp2List, true))
                            childlist.AddRange(temp2List);
            }

            return childlist.Count > 0;
        }

        private bool TryChildListBySlot(int outfitNum, int slot, out List<int> childlist, bool recursive = false)
        {
            childlist = new List<int>();

            foreach (var item in _parentData[outfitNum].parentGroups.Where(x => x.ParentSlot == slot))
            {
                childlist.AddRange(item.childSlots);
                if (recursive)
                    foreach (var item2 in item.childSlots)
                        if (TryChildListBySlot(outfitNum, item2, out var temp2List, true))
                            temp2List.AddRange(temp2List);
            }

            return childlist.Count > 0;
        }

        internal void MovIt(List<QueueItem> queue)
        {
            var names = ParentGroups;
            var relativeData = RelativeData;
            foreach (var item in queue)
            {
                var handover = names.Where(x => x.ParentSlot == item.SrcSlot);
                for (int i = 0, n = handover.Count(); i < n; i++) handover.ElementAt(i).ParentSlot = item.DstSlot;
                if (relativeData.TryGetValue(item.SrcSlot, out var relativeVector))
                    relativeData[item.DstSlot] = relativeVector;
                relativeData.Remove(item.SrcSlot);
            }

            UpdateRelations();
            Update_Old_Parents();
            Update_Text();
        }

        private void ChangePosition(int slot, float move, int kind, bool reset, bool fullreset)
        {
            var childlist = new List<int>();
            if (!_recursiveStop)
                TryChildListBySlot(slot, out childlist, true);
            childlist.Add(slot);

            childlist = childlist.Distinct().ToList();

            if (reset)
            {
                VectorExtraction(slot, 0, out var listvector);

                if (!RelativeData.TryGetValue(slot, out var relativedata))
                {
                    relativedata = new Vector3[3];
                    for (var i = 0; i < 2; i++)
                    {
                        relativedata[0] = Vector3.zero;
                        relativedata[1] = Vector3.zero;
                        relativedata[2] = Vector3.one;
                    }
                }

                move = relativedata[0][kind] - listvector[kind];
            }

            var flag = (int)Math.Pow(2, kind);
            foreach (var item in childlist)
            {
                if (fullreset)
                {
                    if (!RelativeData.TryGetValue(item, out var relativedata))
                    {
                        relativedata = new Vector3[3];
                        for (var i = 0; i < 2; i++)
                        {
                            relativedata[0] = Vector3.zero;
                            relativedata[1] = Vector3.zero;
                            relativedata[2] = Vector3.one;
                        }
                    }

                    ChaControl.SetAccessoryPos(item, 0, relativedata[0][kind], false, flag);
                    UpdateAccessoryInfo(item);
                    continue;
                }

                VectorExtraction(item, 0, out var listvector);
                listvector[kind] += move;
                ChaControl.SetAccessoryPos(item, 0, listvector[kind], false, flag);
                UpdateAccessoryInfo(item);
            }
        }

        private void ChangeRotation(int slot, float rotate, int kind, bool reset, bool fullreset)
        {
            VectorExtraction(slot, out var originalvectors);

            var childlist = new List<int>();
            if (!_recursiveStop)
                TryChildListBySlot(slot, out childlist, true);

            childlist.Add(slot);

            childlist = childlist.Distinct().ToList();

            var rot = Vector3.zero;
            var flag = (int)Math.Pow(2, kind);

            if (reset)
            {
                if (!RelativeData.TryGetValue(slot, out var originalrelativedata))
                {
                    originalrelativedata = new Vector3[3];
                    {
                        originalrelativedata[0] = Vector3.zero;
                        originalrelativedata[1] = Vector3.zero;
                        originalrelativedata[2] = Vector3.one;
                    }
                }

                rotate = originalrelativedata[1][kind] - originalvectors[1][kind];
            }

            rot[kind] = rotate;

            foreach (var item in childlist)
            {
                VectorExtraction(item, out var listvectors);

                Vector3 newPos;

                if (fullreset)
                {
                    rot = Vector3.zero;
                    if (!RelativeData.TryGetValue(slot, out var relativedata))
                    {
                        relativedata = new Vector3[3];
                        relativedata[0] = Vector3.zero;
                        relativedata[1] = Vector3.zero;
                        relativedata[2] = Vector3.one;
                    }

                    rot[kind] = relativedata[1][kind] - listvectors[1][kind];

                    newPos = listvectors[0] - originalvectors[0]; //offset with parent as origin
                    newPos = Quaternion.Euler(rot) * newPos; //rotate offset
                    newPos += originalvectors[0]; //Undo offset

                    for (var i = 0; i < 3; i++)
                        ChaControl.SetAccessoryPos(item, 0, newPos[i], false, (int)Math.Pow(2, i));

                    ChaControl.SetAccessoryRot(item, 0, relativedata[1][kind], false, flag);
                    UpdateAccessoryInfo(item);
                    continue;
                }

                newPos = listvectors[0] - originalvectors[0]; //offset with parent as origin
                newPos = Quaternion.Euler(rot) * newPos; //rotate offset
                newPos += originalvectors[0]; //Undo offset

                for (var i = 0; i < 3; i++) ChaControl.SetAccessoryPos(item, 0, newPos[i], false, (int)Math.Pow(2, i));

                ChaControl.SetAccessoryRot(item, 0, rotate + listvectors[1][kind], false, flag);
                UpdateAccessoryInfo(item);
            }
        }

        private void ChangeScale(int slot, float scale, int kind, bool reset, bool fullreset)
        {
            var childlist = new List<int>();
            if (!_recursiveStop)
                TryChildListBySlot(slot, out childlist, true);
            childlist.Add(slot);
            childlist = childlist.Distinct().ToList();

            if (reset)
            {
                VectorExtraction(slot, 2, out var listvector);

                if (!RelativeData.TryGetValue(slot, out var relativedata))
                {
                    relativedata = new Vector3[3];
                    for (var i = 0; i < 2; i++)
                    {
                        relativedata[0] = Vector3.zero;
                        relativedata[1] = Vector3.zero;
                        relativedata[2] = Vector3.one;
                    }
                }

                scale = relativedata[2][kind] - listvector[kind];
            }

            var flag = (int)Math.Pow(2, kind);

            foreach (var item in childlist)
            {
                if (fullreset)
                {
                    if (!RelativeData.TryGetValue(item, out var relativedata))
                    {
                        relativedata = new Vector3[3];
                        for (var i = 0; i < 2; i++)
                        {
                            relativedata[0] = Vector3.zero;
                            relativedata[1] = Vector3.zero;
                            relativedata[2] = Vector3.one;
                        }
                    }

                    ChaControl.SetAccessoryScl(item, 0, relativedata[2][kind], false, flag);
                    UpdateAccessoryInfo(item);
                    continue;
                }

                VectorExtraction(item, 2, out var listvector);

                listvector[kind] += scale;
                ChaControl.SetAccessoryScl(item, 0, listvector[kind], false, flag);
                UpdateAccessoryInfo(item);
            }
        }

        private void VectorExtraction(int slot, out Vector3[] vectorarray)
        {
            var partinfo = AccessoriesApi.GetPartsInfo(slot);
            vectorarray = new Vector3[3];
            for (var i = 0; i < 3; i++)
            {
                var vector = partinfo.addMove[0, i];
                vectorarray[i] = new Vector3(vector.x, vector.y, vector.z);
            }
        }

        private void VectorExtraction(int slot, int kind, out Vector3 vector)
        {
            var partinfo = AccessoriesApi.GetPartsInfo(slot);

            var addmove = partinfo.addMove[0, kind];
            vector = new Vector3(addmove.x, addmove.y, addmove.z);
        }

        private void UpdateAccessoryInfo(int slot)
        {
            if (slot < 20)
                ChaControl.chaFile.coordinate[(int)CurrentCoordinate.Value].accessory.parts[slot].addMove =
                    ChaControl.nowCoordinate.accessory.parts[slot].addMove;
        }
    }
}