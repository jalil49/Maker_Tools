using KKAPI;
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

namespace Accessory_Parents
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        static MakerDropdown Parent_DropDown;

        static MakerText ParentText;
        static MakerText ChildText;

        static MakerButton Save_Relative_Button;

        static MakerButton Child_Button;
        static MakerButton Gui_Button;

        static bool MakerEnabled = false;

        static CharaEvent GetController => MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

        static bool Retrospect = false;
        static bool RecursiveStop = false;

        public static void MakerAPI_MakerExiting(object sender, EventArgs e)
        {
            AccessoriesApi.AccessoryKindChanged -= AccessoriesApi_AccessoryKindChanged;

            MakerAPI.ReloadCustomInterface -= MakerAPI_ReloadCustomInterface;
            MakerAPI.MakerExiting -= MakerAPI_MakerExiting;

            Retrospect = false;
            RecursiveStop = false;
            MakerEnabled = false;
            ShowCustomGui = false;
            showreplace = false;
            showdelete = false;
        }

        public static void MakerAPI_MakerStartedLoading(object sender, RegisterCustomControlsEvent e)
        {
            MakerEnabled = Settings.Enable.Value;
            if (!MakerEnabled)
            {
                return;
            }
            MakerAPI.MakerExiting += MakerAPI_MakerExiting;

            AccessoriesApi.AccessoryKindChanged += AccessoriesApi_AccessoryKindChanged;
            MakerAPI.ReloadCustomInterface += MakerAPI_ReloadCustomInterface;
        }

        internal void RemoveOutfitEvent()
        {
            Removeoutfit(Parent_Data.Keys.Max());
        }

        internal void AddOutfitEvent()
        {
            for (int i = Parent_Data.Keys.Max(); i < ChaFileControl.coordinate.Length; i++)
                Createoutfit(i);
        }

        public static void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            MakerEnabled = Settings.Enable.Value;
            if (!MakerEnabled)
            {
                return;
            }
            var owner = Settings.Instance;
            var category = new MakerCategory("", "");

            ParentText = MakerAPI.AddAccessoryWindowControl(new MakerText("Not Parent", category, owner), true);
            ChildText = MakerAPI.AddAccessoryWindowControl(new MakerText("Not Child", category, owner), true);
            var Dropdown = new MakerDropdown("Parent", new string[] { "None" }, category, 0, owner);
            Parent_DropDown = MakerAPI.AddAccessoryWindowControl(Dropdown, true);

            Child_Button = MakerAPI.AddAccessoryWindowControl(new MakerButton("Make Child", category, owner), true);
            Child_Button.OnClick.AddListener(delegate ()
            {
                GetController.MakeChild();
            });
            Save_Relative_Button = MakerAPI.AddAccessoryWindowControl(new MakerButton("Save Position", category, owner), true);
            Save_Relative_Button.OnClick.AddListener(delegate ()
            {
                GetController.Save_Relative_Data(AccessoriesApi.SelectedMakerAccSlot);
            });

            Gui_Button = MakerAPI.AddAccessoryWindowControl(new MakerButton("Parent GUI", category, owner), true);
            Gui_Button.OnClick.AddListener(delegate ()
            {
                GUI_Toggle();
            });

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            Parent_DropDown.GroupingID = GroupingID;
            ParentText.GroupingID = GroupingID;
            Child_Button.GroupingID = GroupingID;
            Save_Relative_Button.GroupingID = GroupingID;
            ChildText.GroupingID = GroupingID;
            Gui_Button.GroupingID = GroupingID;
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
            if (Child.TryGetValue(e.SlotIndex, out var ParentKey))
            {
                Keep_Last_Data(e.SlotIndex, ParentKey);
            }
            else if (Old_Parent.ContainsKey(e.SlotIndex) && Relative_Data.ContainsKey(e.SlotIndex))
            {
                Keep_Last_Data(e.SlotIndex);
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
            var find = Parent_Groups.Where(x => x.ParentSlot == slot).ToArray();
            if (find.Length > 0)
            {
                output += " is parent of ";
                for (int i = 0, n = find.Length; i < n; i++)
                {
                    if (i < n - 2)
                    {
                        output += find[i].Name + ", ";
                    }
                    else if (i == n - 2)
                    {
                        output += find[i].Name + " and ";
                    }
                    else
                    {
                        output += find[i].Name;
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
            if (Child.TryGetValue(slot, out int value))
            {
                output2 = "Child of " + Parent_Groups.First(x => x.ParentSlot == value).Name;
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
                            ChaControl.chaFile.coordinate[CoordinateNum].accessory.parts[item].addMove = ChaControl.nowCoordinate.accessory.parts[item].addMove;
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
                            ChaControl.chaFile.coordinate[CoordinateNum].accessory.parts[item].addMove = ChaControl.nowCoordinate.accessory.parts[item].addMove;
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
                            ChaControl.chaFile.coordinate[CoordinateNum].accessory.parts[item].addMove = ChaControl.nowCoordinate.accessory.parts[item].addMove;
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
                            ChaControl.chaFile.coordinate[CoordinateNum].accessory.parts[item].addMove = ChaControl.nowCoordinate.accessory.parts[item].addMove;
                        }
                    }

                }
            }
        }
#endif
        internal void Slot_ACC_Change(int slotNo, int type)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker || !MakerEnabled)
            {
                return;
            }

            if (type == 120)
            {
                var unbindlist = Parent_Groups.Where(x => x.ParentSlot == slotNo);
                for (int i = 0, n = unbindlist.Count(); i < n; i++)
                {
                    unbindlist.ElementAt(i).ParentSlot = -1;
                }
                Relative_Data.Remove(slotNo);
                UpdateRelations();
                Update_Drop_boxes();
            }
            else
            {
                if (Child.TryGetValue(slotNo, out var ParentKey))
                {
                    Keep_Last_Data(slotNo, ParentKey);
                }
                else if (Old_Parent.ContainsKey(slotNo) && Relative_Data.ContainsKey(slotNo))
                {
                    Keep_Last_Data(slotNo);
                }
            }
        }

        private void Save_Relative_Data(int slot)
        {
            var partsinfo = AccessoriesApi.GetPartsInfo(slot);
            if (partsinfo.type == 120)
            {
                return;
            }
            if (!Relative_Data.TryGetValue(slot, out var relative))
            {
                relative = new Vector3[3];
                for (int i = 0; i < 2; i++)
                {
                    relative[0] = Vector3.zero;
                    relative[1] = Vector3.zero;
                    relative[2] = Vector3.one;
                }
                Relative_Data[slot] = relative;
            }

            var temp = partsinfo.addMove;
            {
                int i = 0;
                for (int j = 0; j < 3; j++)
                {
                    relative[j] = new Vector3(temp[i, j].x, temp[i, j].y, temp[i, j].z);
                }
            }

            if (TryChildListBySlot(slot, out var ParentList))
            {
                ParentList = ParentList.Distinct().ToList();

                foreach (var item in ParentList)
                {
                    Save_Relative_Data(item);
                }
            }
        }

        private void Update_Drop_boxes()
        {
            StartCoroutine(Wait());
            IEnumerator Wait()
            {
                if (!MakerAPI.InsideMaker || !MakerEnabled)
                {
                    yield break;
                }

                while (!MakerAPI.InsideAndLoaded)
                {
                    yield return null;
                }


                var ControlObjects = Parent_DropDown.ControlObjects;
                List<TMP_Dropdown.OptionData> Options = new List<TMP_Dropdown.OptionData>(Parent_DropDown.ControlObject.GetComponentInChildren<TMP_Dropdown>().options);
                if (Options.Count > 1)
                {
                    Options.RemoveRange(1, Options.Count - 1);
                }
                foreach (var item in Parent_Groups)
                {
                    Options.Add(new TMP_Dropdown.OptionData(item.Name));
                }
                foreach (var item in ControlObjects)
                {
                    item.GetComponentInChildren<TMP_Dropdown>().options = Options;
                }
                Update_Old_Parents();
            }
        }

        private void Move_To_Parent_Postion(int slot, int ParentKey)
        {
            var partsinfo = AccessoriesApi.GetPartsInfo(ParentKey);
            var Original = partsinfo.addMove;
            var Parent_Name = partsinfo.parentKey;
            ChaControl.ChangeAccessoryParent(slot, Parent_Name);
            //set position
            ChaControl.SetAccessoryPos(slot, 0, Original[0, 0].x, false, 1);
            ChaControl.SetAccessoryPos(slot, 0, Original[0, 0].y, false, 2);
            ChaControl.SetAccessoryPos(slot, 0, Original[0, 0].z, false, 4);
            //set rotation
            ChaControl.SetAccessoryRot(slot, 0, Original[0, 1].x, false, 1);
            ChaControl.SetAccessoryRot(slot, 0, Original[0, 1].y, false, 2);
            ChaControl.SetAccessoryRot(slot, 0, Original[0, 1].z, false, 4);
            //set scale
            ChaControl.SetAccessoryScl(slot, 0, Original[0, 2].x, false, 1);
            ChaControl.SetAccessoryScl(slot, 0, Original[0, 2].y, false, 2);
            ChaControl.SetAccessoryScl(slot, 0, Original[0, 2].z, false, 4);

            UpdateAccessoryInfo(slot);
        }

        private void Keep_Last_Data(int Slot, int ParentKey)
        {
            var Parent_Name = AccessoriesApi.GetPartsInfo(ParentKey).parentKey;
            if (!Relative_Data.TryGetValue(Slot, out Vector3[] Original))
            {
                for (int i = 0; i < 2; i++)
                {
                    Original[0] = Vector3.zero;
                    Original[1] = Vector3.zero;
                    Original[2] = Vector3.one;
                }
            }

            ChaControl.ChangeAccessoryParent(Slot, Parent_Name);

            for (int i = 0; i < 3; i++)
            {
                var flag = (int)Math.Pow(2, i);
                //set position
                ChaControl.SetAccessoryPos(Slot, 0, Original[0][i], false, flag);
                //set rotation
                ChaControl.SetAccessoryRot(Slot, 0, Original[1][i], false, flag);
                //set scale
                ChaControl.SetAccessoryScl(Slot, 0, Original[2][i], false, flag);
            }
        }

        private void Keep_Last_Data(int Slot)
        {
            Vector3[] Original = Relative_Data[Slot];
            string Parent_Name = Old_Parent[Slot];

            ChaControl.ChangeAccessoryParent(Slot, Parent_Name);
            for (int i = 0; i < 3; i++)
            {
                var flag = (int)Math.Pow(2, i);
                //set position
                ChaControl.SetAccessoryPos(Slot, 0, Original[0][i], false, flag);
                //set rotation
                ChaControl.SetAccessoryRot(Slot, 0, Original[1][i], false, flag);
                //set scale
                ChaControl.SetAccessoryScl(Slot, 0, Original[2][i], false, flag);
            }
        }

        private void Update_Old_Parents()
        {
            Old_Parent.Clear();
            foreach (var item in Parent_Groups)
            {
                int slot = item.ParentSlot;
                if (slot < 0 || Child.ContainsKey(slot))
                {
                    continue;
                }
                string ParentKey = AccessoriesApi.GetPartsInfo(slot).parentKey;
                Old_Parent[slot] = string.Copy(ParentKey);
            }
        }

        private void MakeChild()
        {
            var slot = AccessoriesApi.SelectedMakerAccSlot;

            if (AccessoriesApi.GetPartsInfo(slot).type == 120)
                return;

            if (Parent_DropDown.Value == 0)
            {
                return;
            }

            var NameData = Parent_Groups[Parent_DropDown.Value - 1];
            if (RelatedNames[NameData.ParentSlot].Contains(slot))
            {
                return;
            }

            if (!NameData.ChildSlots.Contains(slot))
                NameData.ChildSlots.Add(slot);

            MakeChild(slot, NameData.ParentSlot);
        }

        private void MakeChild(int slot, int ParentSlot)
        {
            if (ParentSlot < 0)
            {
                return;
            }

            if (!Retrospect)
            {
                Move_To_Parent_Postion(slot, ParentSlot);
            }

            UpdateRelations();
            Save_Relative_Data(slot);
            Update_Old_Parents();
            Update_Text();
        }

        private bool ContainsCustomNameSlot(int slot)
        {
            return Parent_Groups.Any(x => x.ParentSlot == slot);
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
                if (Input.GetKeyDown(KeyCode.F) && AccessoriesApi.GetPartsInfo(AccessoriesApi.SelectedMakerAccSlot).type != 120)
                {
                    AddTheme(true);
                    MakeChild();
                    Parent_DropDown.SetValue(Parent_Groups.Count + 1, false);
                    return;
                }
            }
        }

        private bool TryChildListBySlot(int slot, out List<int> childlist, bool recursive = false)
        {
            childlist = new List<int>();

            foreach (var item in Parent_Groups.Where(x => x.ParentSlot == slot))
            {
                childlist.AddRange(item.ChildSlots);
                if (recursive)
                {
                    foreach (var item2 in item.ChildSlots)
                    {
                        if (TryChildListBySlot(item2, out List<int> temp2list, true))
                        {
                            childlist.AddRange(temp2list);
                        }
                    }
                }
            }
            return childlist.Count > 0;
        }

        private bool TryChildListBySlot(int outfitnum, int slot, out List<int> childlist, bool recursive = false)
        {
            childlist = new List<int>();

            foreach (var item in Parent_Data[outfitnum].Parent_Groups.Where(x => x.ParentSlot == slot))
            {
                childlist.AddRange(item.ChildSlots);
                if (recursive)
                {
                    foreach (var item2 in item.ChildSlots)
                    {
                        if (TryChildListBySlot(outfitnum, item2, out List<int> temp2list, true))
                        {
                            temp2list.AddRange(temp2list);
                        }
                    }
                }
            }
            return childlist.Count > 0;
        }

        internal void MovIt(List<QueueItem> queue)
        {
            var Names = Parent_Groups;
            var RelativeData = Relative_Data;
            foreach (var item in queue)
            {
                var Handover = Names.Where(x => x.ParentSlot == item.SrcSlot);
                for (int i = 0, n = Handover.Count(); i < n; i++)
                {
                    Handover.ElementAt(i).ParentSlot = item.DstSlot;
                }
                if (RelativeData.TryGetValue(item.SrcSlot, out var RelativeVector))
                {
                    RelativeData[item.DstSlot] = RelativeVector;
                }
                RelativeData.Remove(item.SrcSlot);
            }
            UpdateRelations();
            Update_Old_Parents();
            Update_Text();
        }

        private void ChangePosition(int slot, float move, int kind, bool reset, bool fullreset)
        {
            List<int> childlist = new List<int>();
            if (!RecursiveStop)
                TryChildListBySlot(slot, out childlist, true);
            childlist.Add(slot);

            childlist = childlist.Distinct().ToList();

            if (reset)
            {
                VectorExtraction(slot, 0, out var listvector);

                if (!Relative_Data.TryGetValue(slot, out var relativedata))
                {
                    relativedata = new Vector3[3];
                    for (int i = 0; i < 2; i++)
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
                    if (!Relative_Data.TryGetValue(item, out var relativedata))
                    {
                        relativedata = new Vector3[3];
                        for (int i = 0; i < 2; i++)
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

            List<int> childlist = new List<int>();
            if (!RecursiveStop)
                TryChildListBySlot(slot, out childlist, true);

            childlist.Add(slot);

            childlist = childlist.Distinct().ToList();

            Vector3 rot = Vector3.zero;
            var flag = (int)Math.Pow(2, kind);

            if (reset)
            {
                if (!Relative_Data.TryGetValue(slot, out var originalrelativedata))
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

                Vector3 New_pos;

                if (fullreset)
                {
                    rot = Vector3.zero;
                    if (!Relative_Data.TryGetValue(slot, out var relativedata))
                    {
                        relativedata = new Vector3[3];
                        relativedata[0] = Vector3.zero;
                        relativedata[1] = Vector3.zero;
                        relativedata[2] = Vector3.one;
                    }
                    rot[kind] = relativedata[1][kind] - listvectors[1][kind];

                    New_pos = listvectors[0] - originalvectors[0]; //offset with parent as origin
                    New_pos = Quaternion.Euler(rot) * New_pos; //rotate offset
                    New_pos += originalvectors[0]; //Undo offset

                    for (int i = 0; i < 3; i++)
                    {
                        ChaControl.SetAccessoryPos(item, 0, New_pos[i], false, (int)Math.Pow(2, i));
                    }

                    ChaControl.SetAccessoryRot(item, 0, relativedata[1][kind], false, flag);
                    UpdateAccessoryInfo(item);
                    continue;
                }

                New_pos = listvectors[0] - originalvectors[0]; //offset with parent as origin
                New_pos = Quaternion.Euler(rot) * New_pos; //rotate offset
                New_pos += originalvectors[0]; //Undo offset

                for (int i = 0; i < 3; i++)
                {
                    ChaControl.SetAccessoryPos(item, 0, New_pos[i], false, (int)Math.Pow(2, i));
                }

                ChaControl.SetAccessoryRot(item, 0, rotate + listvectors[1][kind], false, flag);
                UpdateAccessoryInfo(item);
            }
        }

        private void ChangeScale(int slot, float scale, int kind, bool reset, bool fullreset)
        {
            List<int> childlist = new List<int>();
            if (!RecursiveStop)
                TryChildListBySlot(slot, out childlist, true);
            childlist.Add(slot);
            childlist = childlist.Distinct().ToList();

            if (reset)
            {
                VectorExtraction(slot, 2, out var listvector);

                if (!Relative_Data.TryGetValue(slot, out var relativedata))
                {
                    relativedata = new Vector3[3];
                    for (int i = 0; i < 2; i++)
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
                    if (!Relative_Data.TryGetValue(item, out var relativedata))
                    {
                        relativedata = new Vector3[3];
                        for (int i = 0; i < 2; i++)
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
            for (int i = 0; i < 3; i++)
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
            {
                ChaControl.chaFile.coordinate[(int)CurrentCoordinate.Value].accessory.parts[slot].addMove = ChaControl.nowCoordinate.accessory.parts[slot].addMove;
            }
        }
    }
}
