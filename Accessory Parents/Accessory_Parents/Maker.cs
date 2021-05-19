﻿using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;


namespace Accessory_Parents
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        static MakerDropdown Parent_DropDown;

        static MakerText ParentText;
        static MakerText ChildText;

        static MakerTextbox textbox;

        static MakerRadioButtons radio;

        static MakerButton Modify_Button;
        static MakerButton Replace_Button;
        static MakerButton Save_Relative_Button;
        static MakerButton Child_Button;

        public static void MakerAPI_MakerExiting(object sender, EventArgs e)
        {
            AccessoriesApi.AccessoriesCopied -= (s, es) => VisibiltyToggle();
            AccessoriesApi.AccessoryTransferred -= (s, es) => VisibiltyToggle();
            AccessoriesApi.MakerAccSlotAdded -= (s, es) => VisibiltyToggle();
            AccessoriesApi.SelectedMakerAccSlotChanged -= (s, es) => VisibiltyToggle();
            MakerAPI.MakerFinishedLoading -= (s, es) => VisibiltyToggle();

            AccessoriesApi.AccessoryKindChanged -= AccessoriesApi_AccessoryKindChanged;
            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;

            Hooks.Slot_ACC_Change -= Hooks_Slot_ACC_Change;
            Hooks.ACC_Position_Change -= Hooks_ACC_Position_Change;
            Hooks.ACC_Rotation_Change -= Hooks_ACC_Rotation_Change;
            Hooks.ACC_Scale_Change -= Hooks_ACC_Scale_Change;
            Hooks.MovIt -= Hooks_MovIt;
        }

        public static void MakerAPI_MakerStartedLoading(object sender, RegisterCustomControlsEvent e)
        {
            AccessoriesApi.AccessoriesCopied += (s, es) => VisibiltyToggle();
            AccessoriesApi.AccessoryTransferred += (s, es) => VisibiltyToggle();
            AccessoriesApi.MakerAccSlotAdded += (s, es) => VisibiltyToggle();
            AccessoriesApi.SelectedMakerAccSlotChanged += (s, es) => VisibiltyToggle();
            MakerAPI.MakerFinishedLoading += (s, es) => VisibiltyToggle();

            AccessoriesApi.AccessoryKindChanged += AccessoriesApi_AccessoryKindChanged;
            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;

            Hooks.Slot_ACC_Change += Hooks_Slot_ACC_Change;
            Hooks.ACC_Position_Change += Hooks_ACC_Position_Change;
            Hooks.ACC_Rotation_Change += Hooks_ACC_Rotation_Change;
            Hooks.ACC_Scale_Change += Hooks_ACC_Scale_Change;
            Hooks.MovIt += Hooks_MovIt;
        }

        public static void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;
            var category = new MakerCategory("", "");

            ParentText = MakerAPI.AddAccessoryWindowControl<MakerText>(new MakerText("Not Parent", category, owner));
            ChildText = MakerAPI.AddAccessoryWindowControl<MakerText>(new MakerText("Not Child", category, owner));

            var Dropdown = new MakerDropdown("Parent", new string[] { "None" }, category, 0, owner);
            Parent_DropDown = MakerAPI.AddAccessoryWindowControl<MakerDropdown>(Dropdown);

            textbox = MakerAPI.AddAccessoryWindowControl<MakerTextbox>(new MakerTextbox(category, "Name", "", owner));

            radio = new MakerRadioButtons(category, owner, "Modify", 0, new string[] { "Add", "Remove", "Rename" });
            MakerAPI.AddAccessoryWindowControl<MakerRadioButtons>(radio).ValueChanged.Subscribe(x => RadioChanged(x));

            Modify_Button = MakerAPI.AddAccessoryWindowControl<MakerButton>(new MakerButton("Modify Parent", category, owner));
            Modify_Button.OnClick.AddListener(delegate ()
            {
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().Modify_Dropdown();
            });
            Replace_Button = MakerAPI.AddAccessoryWindowControl<MakerButton>(new MakerButton("Replace Parent", category, owner));
            Replace_Button.OnClick.AddListener(delegate ()
                {
                    CharaEvent controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
                    var CoordinateNum = controller.CoordinateNum;
                    if (Dropdown.Value == 0)
                    {
                        return;
                    }
                    var options = Dropdown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;
                    var Slot = AccessoriesApi.SelectedMakerAccSlot;
                    var Replace = controller.Custom_Names[CoordinateNum][options[Dropdown.Value].text];
                    var update = controller.Child[CoordinateNum].Where(x => x.Value == Replace).ToArray();
                    foreach (var item in update)
                    {
                        controller.Child[CoordinateNum][item.Key] = Slot;
                    }
                    controller.Child[CoordinateNum].Remove(Slot);
                    controller.Custom_Names[CoordinateNum][options[Dropdown.Value].text] = Slot;
                    controller.Update_Parenting();
                    VisibiltyToggle();
                });
            Child_Button = MakerAPI.AddAccessoryWindowControl<MakerButton>(new MakerButton("Make Child", category, owner));
            Child_Button.OnClick.AddListener(delegate ()
            {
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().MakeChild();
            });
            Save_Relative_Button = MakerAPI.AddAccessoryWindowControl<MakerButton>(new MakerButton("Save Position", category, owner));
            Save_Relative_Button.OnClick.AddListener(delegate ()
            {
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().Save_Relative_data(AccessoriesApi.SelectedMakerAccSlot);
            });

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            Parent_DropDown.GroupingID = GroupingID;
            ParentText.GroupingID = GroupingID;
            Modify_Button.GroupingID = GroupingID;
            Replace_Button.GroupingID = GroupingID;
            textbox.GroupingID = GroupingID;
            radio.GroupingID = GroupingID;
            Child_Button.GroupingID = GroupingID;
            Save_Relative_Button.GroupingID = GroupingID;
            ChildText.GroupingID = GroupingID;
        }

        public static void MakerAPI_ReloadCustomInterface(object sender, EventArgs e)
        {
            MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().Update_DropBox();
        }

        private static void AccessoriesApi_MakerAccSlotAdded(object sender, AccessorySlotEventArgs e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            Controller.StartCoroutine(Wait());
            IEnumerator Wait()
            {
                yield return null;
                Controller.Update_More_Accessories();
                VisibiltyToggle();
            }
        }

        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            VisibiltyToggle();
        }

        private static void AccessoriesApi_AccessoryKindChanged(object sender, AccessorySlotEventArgs e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            var CoordinateNum = Controller.CoordinateNum;
            if (Controller.Child[CoordinateNum].TryGetValue(e.SlotIndex, out var ParentKey))
            {
                Controller.Keep_Last_Data(e.SlotIndex, ParentKey);
            }
            else if (Controller.Old_Parent[CoordinateNum].ContainsKey(e.SlotIndex) && Controller.Relative_Data[CoordinateNum].ContainsKey(e.SlotIndex))
            {

                //foreach (var item in Old_Parent[CoordinateNum])
                //{
                //    Logger.LogWarning($"Kind Changed Slot {item.Key} has parent {item.Value}");
                //}
                Controller.Keep_Last_Data(e.SlotIndex);
            }
            VisibiltyToggle();
        }

        private static void VisibiltyToggle()
        {
            if (!MakerAPI.InsideMaker)
                return;

            var accessory = MakerAPI.GetCharacterControl().GetAccessoryObject(AccessoriesApi.SelectedMakerAccSlot);
            if (accessory == null)
            {
                Parent_DropDown.Visible.OnNext(false);
                ParentText.Visible.OnNext(false);
                Modify_Button.Visible.OnNext(false);
                Replace_Button.Visible.OnNext(false);
                textbox.Visible.OnNext(false);
                radio.Visible.OnNext(false);
                Child_Button.Visible.OnNext(false);
                Save_Relative_Button.Visible.OnNext(false);
                ChildText.Visible.OnNext(false);
            }
            else
            {
                Parent_DropDown.Visible.OnNext(true);
                ParentText.Visible.OnNext(true);
                Modify_Button.Visible.OnNext(true);
                Replace_Button.Visible.OnNext(true);
                textbox.Visible.OnNext(true);
                radio.Visible.OnNext(true);
                Child_Button.Visible.OnNext(true);
                Save_Relative_Button.Visible.OnNext(true);
                ChildText.Visible.OnNext(true);
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().Update_Text();
            }
        }

        private void Update_Text()
        {
            var slot = AccessoriesApi.SelectedMakerAccSlot;
            if (ParentText.ControlObjects.Count() <= slot)
            {
                return;
            }
            string output = "Slot " + (slot + 1).ToString();
            var find = Custom_Names[CoordinateNum].Where(x => x.Value == slot).ToArray();
            if (find.Length > 0)
            {
                output += " is parent of ";
                for (int i = 0, n = find.Length; i < n; i++)
                {
                    if (i < n - 2)
                    {
                        output += find[i].Key + ", ";
                    }
                    else if (i == n - 2)
                    {
                        output += find[i].Key + " and ";
                    }
                    else
                    {
                        output += find[i].Key;
                    }
                }
            }
            else
            {
                output += " is not Parent";
            }
            foreach (var item in ParentText.ControlObjects)
            {
                item.GetComponentInChildren<TextMeshProUGUI>().text = output;
            }
            string output2;
            if (Child[CoordinateNum].TryGetValue(slot, out int value))
            {
                output2 = "Child of " + Custom_Names[CoordinateNum].First(x => x.Value == value).Key;
            }
            else
            {
                output2 = "Is not a Child";
            }
            foreach (var item in ChildText.ControlObjects)
            {
                item.GetComponentInChildren<TextMeshProUGUI>().text = output2;
            }
        }

        private static void Hooks_ACC_Scale_Change(object sender, Acc_modifier_Event_ARG e)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                return;
            }
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            var CoordinateNum = Controller.CoordinateNum;
            if (!e.Add && e.Value == 1f && Controller.Custom_Names[CoordinateNum].ContainsValue(e.SlotNo))
            {
                float Value;
                if (Controller.Relative_Data[CoordinateNum].TryGetValue(e.SlotNo, out var vectors))
                {
                    if (e.Flags == 1)
                    {
                        Value = vectors[0, 2].x;
                    }
                    else if (e.Flags == 2)
                    {
                        Value = vectors[0, 2].y;
                    }
                    else if (e.Flags == 4)
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
                    Controller.ChaControl.SetAccessoryScl(e.SlotNo, e.CorrectNo, Value, e.Add, e.Flags);
                    return;
                }
            }

            if (Controller.Bindings[CoordinateNum].TryGetValue(e.SlotNo, out var parentList))
            {
                if (e.Add)
                {
                    foreach (var item in parentList)
                    {
                        Controller.ChaControl.SetAccessoryScl(item, e.CorrectNo, e.Value, e.Add, e.Flags);
                    }
                }
                else
                {
                    foreach (var item in parentList)
                    {
                        float Value;
                        if (Controller.Relative_Data[CoordinateNum].TryGetValue(item, out var vectors))
                        {
                            if (e.Flags == 1)
                            {
                                Value = vectors[0, 2].x;
                            }
                            else if (e.Flags == 2)
                            {
                                Value = vectors[0, 2].y;
                            }
                            else if (e.Flags == 4)
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
                        Controller.ChaControl.SetAccessoryScl(item, e.CorrectNo, Value, e.Add, e.Flags);
                    }
                }

            }
        }

        private static void Hooks_ACC_Rotation_Change(object sender, Acc_modifier_Event_ARG e)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                return;
            }
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            var CoordinateNum = Controller.CoordinateNum;

            if (!e.Add && e.Value == 0 && Controller.Custom_Names[CoordinateNum].ContainsValue(e.SlotNo))
            {
                float Value;
                if (Controller.Relative_Data[CoordinateNum].TryGetValue(e.SlotNo, out var vectors))
                {
                    if (e.Flags == 1)
                    {
                        Value = vectors[0, 1].x;
                    }
                    else if (e.Flags == 2)
                    {
                        Value = vectors[0, 1].y;
                    }
                    else if (e.Flags == 4)
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
                    Controller.ChaControl.SetAccessoryRot(e.SlotNo, e.CorrectNo, Value, e.Add, e.Flags);
                    return;
                }
            }

            if (Controller.Bindings[CoordinateNum].TryGetValue(e.SlotNo, out var parentList))
            {
                Vector3 Rot = Vector3.zero;

                if (e.Flags == 1)
                {
                    Rot.x = e.Value;
                }
                else if (e.Flags == 2)
                {
                    Rot.y = e.Value;
                }
                else if (e.Flags == 4)
                {
                    Rot.z = e.Value;
                }

                Vector3[,] store;
                if (e.SlotNo < 20)
                {
                    store = Controller.ChaControl.nowCoordinate.accessory.parts[e.SlotNo].addMove;
                }
                else
                {
                    store = Controller.Accessorys_Parts[e.SlotNo - 20].addMove;
                }
                Vector3 original_pos;
                original_pos = new Vector3(store[0, 0].x, store[0, 0].y, store[0, 0].z);

                if (e.Add)
                {
                    Vector3 New_pos;
                    foreach (var item in parentList)
                    {
                        Vector3[,] store2;
                        Controller.ChaControl.SetAccessoryRot(item, e.CorrectNo, e.Value, e.Add, e.Flags);
                        if (item < 20)
                        {
                            store2 = Controller.ChaControl.nowCoordinate.accessory.parts[item].addMove;
                        }
                        else
                        {
                            store2 = Controller.Accessorys_Parts[item - 20].addMove;
                        }
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
                            Controller.ChaControl.SetAccessoryPos(item, e.CorrectNo, New_pos[i], true, flag);
                        }
                        if (item < 20)
                        {
                            Controller.ChaControl.chaFile.coordinate[CoordinateNum].accessory.parts[item].addMove = Controller.ChaControl.nowCoordinate.accessory.parts[item].addMove;
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
                        Vector3[,] store2;
                        if (item < 20)
                        {
                            store2 = Controller.ChaControl.nowCoordinate.accessory.parts[item].addMove;
                        }
                        else
                        {
                            store2 = Controller.Accessorys_Parts[item - 20].addMove;
                        }

                        if (Controller.Relative_Data[CoordinateNum].TryGetValue(item, out var vectors))
                        {
                            if (e.Flags == 1)
                            {
                                Value = vectors[0, 1].x;
                                //Rot2.x = vectors[0, 1].x - store2[e.CorrectNo, 1].x;
                            }
                            else if (e.Flags == 2)
                            {
                                Value = vectors[0, 1].y;

                                //Rot2.y = vectors[0, 1].y - store2[e.CorrectNo, 1].y;
                            }
                            else if (e.Flags == 4)
                            {
                                Value = vectors[0, 1].z;
                                //Rot2.z = vectors[0, 1].z - store2[e.CorrectNo, 1].z;
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
                        Controller.ChaControl.SetAccessoryRot(item, e.CorrectNo, Value, e.Add, e.Flags);

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
                        //    ChaControl.SetAccessoryPos(item, e.CorrectNo, New_pos[i], true, flag);
                        //}


                        if (item < 20)
                        {
                            Controller.ChaControl.chaFile.coordinate[CoordinateNum].accessory.parts[item].addMove = Controller.ChaControl.nowCoordinate.accessory.parts[item].addMove;
                        }
                    }
                }

            }
        }

        private static void Hooks_ACC_Position_Change(object sender, Acc_modifier_Event_ARG e)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                return;
            }
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            var CoordinateNum = Controller.CoordinateNum;

            if (!e.Add && e.Value == 0 && Controller.Custom_Names[CoordinateNum].ContainsValue(e.SlotNo))
            {
                float Value;
                if (Controller.Relative_Data[CoordinateNum].TryGetValue(e.SlotNo, out var vectors))
                {
                    if (e.Flags == 1)
                    {
                        Value = vectors[0, 0].x;
                    }
                    else if (e.Flags == 2)
                    {
                        Value = vectors[0, 0].y;
                    }
                    else if (e.Flags == 4)
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
                    Controller.ChaControl.SetAccessoryPos(e.SlotNo, e.CorrectNo, Value, e.Add, e.Flags);
                    return;
                }
            }

            if (Controller.Bindings[CoordinateNum].TryGetValue(e.SlotNo, out var parentList))
            {
                if (e.Add)
                {
                    foreach (var item in parentList)
                    {
                        Controller.ChaControl.SetAccessoryPos(item, e.CorrectNo, e.Value, e.Add, e.Flags);
                        if (item < 20)
                        {
                            Controller.ChaControl.chaFile.coordinate[CoordinateNum].accessory.parts[item].addMove = Controller.ChaControl.nowCoordinate.accessory.parts[item].addMove;
                        }
                    }
                }
                else
                {
                    float Value;
                    foreach (var item in parentList)
                    {
                        if (Controller.Relative_Data[CoordinateNum].TryGetValue(item, out var vectors))
                        {
                            if (e.Flags == 1)
                            {
                                Value = vectors[0, 0].x;
                            }
                            else if (e.Flags == 2)
                            {
                                Value = vectors[0, 0].y;
                            }
                            else if (e.Flags == 4)
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
                        Controller.ChaControl.SetAccessoryPos(item, e.CorrectNo, Value, false, e.Flags);
                        if (item < 20)
                        {
                            Controller.ChaControl.chaFile.coordinate[CoordinateNum].accessory.parts[item].addMove = Controller.ChaControl.nowCoordinate.accessory.parts[item].addMove;
                        }
                    }

                }
            }
        }

        private static void Hooks_Slot_ACC_Change(object sender, Slot_ACC_Change_ARG e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            var CoordinateNum = Controller.CoordinateNum;

            if (e.Type == 120)
            {
                Controller.Child[CoordinateNum].Remove(e.SlotNo);
                var test = Controller.Custom_Names[CoordinateNum].Where(x => x.Value == e.SlotNo).ToArray();
                for (int i = 0; i < test.Length; i++)
                {
                    Controller.Custom_Names[CoordinateNum].Remove(test[i].Key);
                }
                Controller.Relative_Data[CoordinateNum].Remove(e.SlotNo);
                Parent_DropDown.SetValue(0);
                Controller.Update_Parenting();
                Controller.Update_DropBox();
            }
            else
            {
                if (Controller.Child[CoordinateNum].TryGetValue(e.SlotNo, out var ParentKey))
                {
                    Controller.Keep_Last_Data(e.SlotNo, ParentKey);
                }
                else if (Controller.Old_Parent[CoordinateNum].ContainsKey(e.SlotNo) && Controller.Relative_Data[CoordinateNum].ContainsKey(e.SlotNo))
                {
                    Controller.Keep_Last_Data(e.SlotNo);
                }
            }
            VisibiltyToggle();
        }

        private void Save_Relative_data(int Slot)
        {
            if (!Relative_Data[CoordinateNum].TryGetValue(Slot, out var relative))
            {
                relative = new Vector3[2, 3];
                for (int i = 0; i < 2; i++)
                {
                    relative[i, 0] = Vector3.zero;
                    relative[i, 1] = Vector3.zero;
                    relative[i, 2] = Vector3.one;
                }
            }

            if (Slot < 20)
            {
                var temp = ChaControl.chaFile.coordinate[CoordinateNum].accessory.parts[Slot].addMove;
                for (int i = 0; i < 2; i++)
                {
                    relative[i, 0] = new Vector3(temp[i, 0].x, temp[i, 0].y, temp[i, 0].z);
                    relative[i, 1] = new Vector3(temp[i, 1].x, temp[i, 1].y, temp[i, 1].z);
                    relative[i, 2] = new Vector3(temp[i, 2].x, temp[i, 2].y, temp[i, 2].z);
                }
            }
            else
            {
                Update_More_Accessories();

                var temp = Accessorys_Parts[Slot - 20].addMove;
                for (int i = 0; i < 2; i++)
                {
                    relative[i, 0] = new Vector3(temp[i, 0].x, temp[i, 0].y, temp[i, 0].z);
                    relative[i, 1] = new Vector3(temp[i, 1].x, temp[i, 1].y, temp[i, 1].z);
                    relative[i, 2] = new Vector3(temp[i, 2].x, temp[i, 2].y, temp[i, 2].z);
                }
            }
            Relative_Data[CoordinateNum][Slot] = relative;
            if (Bindings[CoordinateNum].TryGetValue(Slot, out var ParentList))
            {
                foreach (var item in ParentList)
                {
                    Save_Relative_data(item);
                }
            }
        }

        private bool RecursiveParentCheck(int Slot, int Original_Parent)
        {
            if (Slot == Original_Parent)
            {
                //Logger.LogWarning("Tried to make child of self");
                return true;
            }
            if (Child[CoordinateNum].TryGetValue(Slot, out var value))
            {
                bool temp = value == Original_Parent;
                if (temp)
                {
                    return true;
                }
                if (RecursiveParentCheck(value, Original_Parent))
                {
                    return true;
                }
            }
            return false;
        }

        private bool RecursiveChildCheck(int Parent, int Original_Parent)
        {
            if (Parent == Original_Parent)
            {
                return true;
            }
            if (Bindings[CoordinateNum].TryGetValue(Parent, out var ParentList))
            {
                foreach (var item in ParentList)
                {

                    if (RecursiveChildCheck(item, Original_Parent))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void Update_Parenting()
        {
            Bindings[CoordinateNum].Clear();
            foreach (var ParentKey in Custom_Names[CoordinateNum].Values.Distinct())
            {
                List<int> ChildList = new List<int>();
                foreach (var item2 in Child[CoordinateNum].Where(x => x.Value == ParentKey))
                {
                    ChildList.Add(item2.Key);
                }
                Bindings[CoordinateNum][ParentKey] = ChildList;
            }
            Update_Old_Parents();
        }

        private void Update_DropBox()
        {
            StartCoroutine(Wait());
            IEnumerator Wait()
            {
                yield return null;
                VisibiltyToggle();
                if (Parent_DropDown == null || Parent_DropDown.ControlObject == null)
                {
                    yield break;
                }
                var ControlObjects = Parent_DropDown.ControlObjects;
                List<TMP_Dropdown.OptionData> Options = new List<TMP_Dropdown.OptionData>(Parent_DropDown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options);
                if (Options.Count > 1)
                {
                    Options.RemoveRange(1, Options.Count - 1);
                }
                foreach (var item in Custom_Names[CoordinateNum])
                {
                    Options.Add(new TMP_Dropdown.OptionData(item.Key));
                }
                foreach (var item in ControlObjects)
                {
                    item.GetComponentInChildren<TMP_Dropdown>().options = Options;
                }
                Update_More_Accessories();
                Update_Old_Parents();
            }
        }

        private void Move_To_Parent_Postion(int Slot, int ParentKey)
        {
            Vector3[,] Original;
            string Parent_Name;
            if (ParentKey < 20)
            {
                Original = ChaControl.nowCoordinate.accessory.parts[ParentKey].addMove;
                Parent_Name = ChaControl.nowCoordinate.accessory.parts[ParentKey].parentKey;
            }
            else
            {
                Update_More_Accessories();
                Original = Accessorys_Parts[ParentKey - 20].addMove;
                Parent_Name = Accessorys_Parts[ParentKey - 20].parentKey;
            }
            ChaControl.ChangeAccessoryParent(Slot, Parent_Name);
            //set position
            ChaControl.SetAccessoryPos(Slot, 0, Original[0, 0].x, false, 1);
            ChaControl.SetAccessoryPos(Slot, 0, Original[0, 0].y, false, 2);
            ChaControl.SetAccessoryPos(Slot, 0, Original[0, 0].z, false, 4);
            //set rotation
            ChaControl.SetAccessoryRot(Slot, 0, Original[0, 1].x, false, 1);
            ChaControl.SetAccessoryRot(Slot, 0, Original[0, 1].y, false, 2);
            ChaControl.SetAccessoryRot(Slot, 0, Original[0, 1].z, false, 4);
            //set scale
            ChaControl.SetAccessoryScl(Slot, 0, Original[0, 2].x, false, 1);
            ChaControl.SetAccessoryScl(Slot, 0, Original[0, 2].y, false, 2);
            ChaControl.SetAccessoryScl(Slot, 0, Original[0, 2].z, false, 4);
        }

        private void Keep_Last_Data(int Slot, int ParentKey)
        {
            string Parent_Name;
            if (ParentKey < 20)
            {
                Parent_Name = ChaControl.nowCoordinate.accessory.parts[ParentKey].parentKey;
            }
            else
            {
                Update_More_Accessories();
                Parent_Name = Accessorys_Parts[ParentKey - 20].parentKey;
            }
            if (!Relative_Data[CoordinateNum].TryGetValue(Slot, out Vector3[,] Original))
            {
                for (int i = 0; i < 2; i++)
                {
                    Original[i, 0] = Vector3.zero;
                    Original[i, 1] = Vector3.zero;
                    Original[i, 2] = Vector3.one;
                }
            }
            ChaControl.ChangeAccessoryParent(Slot, Parent_Name);
            //set position
            ChaControl.SetAccessoryPos(Slot, 0, Original[0, 0].x, false, 1);
            ChaControl.SetAccessoryPos(Slot, 0, Original[0, 0].y, false, 2);
            ChaControl.SetAccessoryPos(Slot, 0, Original[0, 0].z, false, 4);
            //set rotation
            ChaControl.SetAccessoryRot(Slot, 0, Original[0, 1].x, false, 1);
            ChaControl.SetAccessoryRot(Slot, 0, Original[0, 1].y, false, 2);
            ChaControl.SetAccessoryRot(Slot, 0, Original[0, 1].z, false, 4);
            //set scale
            ChaControl.SetAccessoryScl(Slot, 0, Original[0, 2].x, false, 1);
            ChaControl.SetAccessoryScl(Slot, 0, Original[0, 2].y, false, 2);
            ChaControl.SetAccessoryScl(Slot, 0, Original[0, 2].z, false, 4);
        }

        private void Keep_Last_Data(int Slot)
        {
            Vector3[,] Original = Relative_Data[CoordinateNum][Slot];
            string Parent_Name = Old_Parent[CoordinateNum][Slot];

            ChaControl.ChangeAccessoryParent(Slot, Parent_Name);
            //set position
            ChaControl.SetAccessoryPos(Slot, 0, Original[0, 0].x, false, 1);
            ChaControl.SetAccessoryPos(Slot, 0, Original[0, 0].y, false, 2);
            ChaControl.SetAccessoryPos(Slot, 0, Original[0, 0].z, false, 4);
            //set rotation
            ChaControl.SetAccessoryRot(Slot, 0, Original[0, 1].x, false, 1);
            ChaControl.SetAccessoryRot(Slot, 0, Original[0, 1].y, false, 2);
            ChaControl.SetAccessoryRot(Slot, 0, Original[0, 1].z, false, 4);
            //set scale
            ChaControl.SetAccessoryScl(Slot, 0, Original[0, 2].x, false, 1);
            ChaControl.SetAccessoryScl(Slot, 0, Original[0, 2].y, false, 2);
            ChaControl.SetAccessoryScl(Slot, 0, Original[0, 2].z, false, 4);
        }

        private void Update_Old_Parents()
        {
            Old_Parent[CoordinateNum].Clear();
            foreach (var item in Custom_Names[CoordinateNum].Values.Distinct())
            {
                if (Child[CoordinateNum].ContainsKey(item))
                {
                    continue;
                }
                string ParentKey;
                if (item < 20)
                {
                    ParentKey = ChaControl.chaFile.coordinate[CoordinateNum].accessory.parts[item].parentKey;
                }
                else
                {
                    ParentKey = Accessorys_Parts[item - 20].parentKey;
                }
                Old_Parent[CoordinateNum][item] = string.Copy(ParentKey);
            }
        }

        private void MakeChild()
        {
            if (MakerAPI.GetCharacterControl().GetAccessoryObject(AccessoriesApi.SelectedMakerAccSlot) == null)
            {
                return;
            }
            var Slot = AccessoriesApi.SelectedMakerAccSlot;
            if (Parent_DropDown.Value == 0)
            {
                Child[CoordinateNum].Remove(Slot);
                Update_Parenting();
                Update_Text();
                return;
            }
            var options = Parent_DropDown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options;
            if (Custom_Names[CoordinateNum].TryGetValue(options[Parent_DropDown.Value].text, out var parentKey))
            {
                if (RecursiveParentCheck(Slot, parentKey) || RecursiveChildCheck(Slot, parentKey))
                {
                    Child[CoordinateNum].Remove(Slot);
                    Update_Parenting();
                    Update_Text();
                    return;
                }
                Child[CoordinateNum][Slot] = parentKey;
                Move_To_Parent_Postion(Slot, parentKey);
                if (Slot < 20)
                {
                    ChaControl.chaFile.coordinate[(int)CurrentCoordinate.Value].accessory.parts[Slot] = ChaControl.nowCoordinate.accessory.parts[Slot];
                }
                Save_Relative_data(Slot);
                Update_Parenting();
                Update_Text();
            }
        }

        private void Modify_Dropdown(bool Generic = false)
        {
            if (MakerAPI.GetCharacterControl().GetAccessoryObject(AccessoriesApi.SelectedMakerAccSlot) == null)
            {
                return;
            }
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>(Parent_DropDown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options);
            int index = 0;
            if (Generic)
            {
                radio.SetValue(0);
            }
            switch (radio.Value)
            {
                case 0:
                    var Text = textbox.Value;
                    if (textbox.Value == "" || Generic)
                    {
                        Text = $"Slot{(AccessoriesApi.SelectedMakerAccSlot + 1):000}";
                    }
                    if (options.Any(x => x.text == Text))
                    {
                        return;
                    }
                    options.Add(new TMP_Dropdown.OptionData(Text));
                    Custom_Names[CoordinateNum][Text] = AccessoriesApi.SelectedMakerAccSlot;
                    break;
                case 1:
                    if (textbox.Value == "None")
                    {
                        return;
                    }
                    for (int i = 0; i < options.Count; i++)
                    {
                        if (options[i].text == textbox.Value)
                        {
                            index = i;
                            break;
                        }
                        else if (i == options.Count - 1)
                        {
                            return;
                        }
                    }
                    var childlist = Child[CoordinateNum].Where(x => x.Value == Custom_Names[CoordinateNum][textbox.Value]);
                    for (int i = 0, n = childlist.Count(); i < n; i++)
                    {
                        Child[CoordinateNum].Remove(childlist.ElementAt(i).Key);
                    }

                    Bindings[CoordinateNum].Remove(Custom_Names[CoordinateNum][textbox.Value]);
                    Custom_Names[CoordinateNum].Remove(textbox.Value);
                    options.RemoveAt(index);
                    break;
                case 2:
                    if (Parent_DropDown.Value == 0)
                    {
                        return;
                    }
                    if (options.Any(x => x.text == textbox.Value))
                    {
                        return;
                    }
                    Custom_Names[CoordinateNum][textbox.Value] = Custom_Names[CoordinateNum][options[Parent_DropDown.Value].text];
                    Custom_Names[CoordinateNum].Remove(options[Parent_DropDown.Value].text);
                    options[Parent_DropDown.Value].text = textbox.Value;
                    break;
                default:
                    break;
            }

            foreach (var item in Parent_DropDown.ControlObjects)
            {
                item.GetComponentInChildren<TMP_Dropdown>().options = options;
            }
            Update_Parenting();
            VisibiltyToggle();
        }

        protected override void Update()
        {
            if (Input.anyKeyDown && AccessoriesApi.AccessoryCanvasVisible)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    MakeChild();
                }
                else if (Input.GetKeyDown(KeyCode.F))
                {
                    MakeChild();
                    Modify_Dropdown(true);
                    Parent_DropDown.SetValue(Parent_DropDown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options.Count() - 1);
                }
            }
            base.Update();
        }

        private static void RadioChanged(int toggle)
        {
            foreach (var item in radio.ControlObjects)
            {
                var toggles = item.GetComponentsInChildren<Toggle>();
                for (int i = 0; i < 3; i++)
                {
                    toggles[i].isOn = toggle == i;
                }
            }
        }

        private static void Hooks_MovIt(object sender, MovUrAcc_Event e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            //var Bindings = Controller.Bindings[Controller.CoordinateNum];
            var Names = Controller.Custom_Names[Controller.CoordinateNum];
            var RelativeData = Controller.Relative_Data[Controller.CoordinateNum];
            var Child = Controller.Child[Controller.CoordinateNum];
            foreach (var item in e.Queue)
            {
                var Handover = Names.Where(x => x.Value == item.srcSlot).ToArray();
                for (int i = 0; i < Handover.Length; i++)
                {
                    Names[Handover[i].Key] = item.dstSlot;
                }
                if (RelativeData.TryGetValue(item.srcSlot, out var RelativeVector))
                {
                    RelativeData[item.dstSlot] = RelativeVector;
                    RelativeData.Remove(item.srcSlot);
                }
                if (Child.TryGetValue(item.srcSlot, out var parent))
                {
                    foreach (var sub in e.Queue)
                    {
                        if (sub.srcSlot == parent)
                        {
                            parent = sub.dstSlot;
                            break;
                        }
                    }
                    Child[item.dstSlot] = parent;
                    Child.Remove(item.srcSlot);
                }
            }
            Controller.Update_Parenting();
        }
    }
}
