using ChaCustom;
using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Maker;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Accessory_Shortcuts
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        readonly List<Toggle> Slot_Toggles = new List<Toggle>();
        Transform Slots_Location;
        Traverse More_Acc;
        bool Skip = false;

        private void MakerAPI_MakerExiting(object sender, EventArgs e)
        {
            Hooks.Pre_Slot_ACC_Change -= Hooks_Pre_Slot_ACC_Change;
            Hooks.Post_Slot_ACC_Change -= Hooks_Slot_ACC_Change;
        }

        private void MakerAPI_MakerFinishedLoading(object sender, EventArgs e)
        {
            Hooks.Post_Slot_ACC_Change += Hooks_Slot_ACC_Change;
            Hooks.Pre_Slot_ACC_Change += Hooks_Pre_Slot_ACC_Change;

            More_Acc = Traverse.Create(MoreAccessoriesKOI.MoreAccessories._self);
            Slots_Location = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/04_AccessoryTop/Slots/Viewport/Content").transform;
            UpdateSlots();
        }

        private void Hooks_Pre_Slot_ACC_Change(object sender, Slot_ACC_Change_ARG e)
        {
            if (e.Type == 120 || Skip)
            {
                return;
            }
            ChaFileAccessory.PartsInfo partsInfo;
            var Slot = e.SlotNo;
            if (Slot < 20)
            {
                partsInfo = ChaControl.nowCoordinate.accessory.parts[Slot];
            }
            else
            {
                if (Slot - 20 >= Accessorys_Parts.Count)
                {
                    Update_More_Accessories();
                }
                partsInfo = Accessorys_Parts[Slot - 20];
            }

            if (e.Type == partsInfo.type)
            {
                Constants.Parent[partsInfo.type - 120].Id = e.Id;
                Constants.Parent[partsInfo.type - 120].ParentKey = e.ParentKey;
            }
        }

        private void Hooks_Slot_ACC_Change(object sender, Slot_ACC_Change_ARG e)
        {
            if (e.Type == 120)
            {
                return;
            }
            else if (Constants.Parent.TryGetValue(e.Type - 120, out var data) && (e.Id != data.Id || e.ParentKey != data.ParentKey))
            {
                ChaControl.ChangeAccessory(e.SlotNo, e.Type, data.Id, data.ParentKey);
                CvsAccessory CVS_Slot = More_Acc.Method("GetCvsAccessory", new object[] { e.SlotNo }).GetValue<CvsAccessory>();
                CVS_Slot.UpdateCustomUI();
            }
        }

        private void UpdateSlots()
        {
            Slot_Toggles.Clear();
            for (int i = 0, n = Slots_Location.childCount - 1; i < n; i++)
            {
                Slot_Toggles.Add(Slots_Location.GetChild(i).GetComponent<Toggle>());
            }
        }

        protected override void Update()
        {
            if (Input.anyKeyDown && AccessoriesApi.AccessoryCanvasVisible)
            {
                var Slot = AccessoriesApi.SelectedMakerAccSlot;
                var accessory = MakerAPI.GetCharacterControl().GetAccessoryObject(Slot);
                if (accessory == null)
                {
                    Skip = true;
                    CvsAccessory CVS_Slot = More_Acc.Method("GetCvsAccessory", new object[] { Slot }).GetValue<CvsAccessory>();
                    if (int.TryParse(Input.inputString, out var kind) && kind < 10 && kind > -1)
                    {
                        if (kind == 0)
                        {
                            kind = 10;
                        }
                        CVS_Slot.UpdateSelectAccessoryType(kind);
                        CVS_Slot.UpdateCustomUI();
                    }
                    Skip = false;
                }


                if (Slot + 1 >= Slot_Toggles.Count)
                {
                    UpdateSlots();
                }
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    Slot_Toggles[Math.Max(Slot - 1, 0)].isOn = true;
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    Slot_Toggles[Math.Min(Slot + 1, Slot_Toggles.Count - 1)].isOn = true;
                }
            }
            base.Update();
        }
    }
}
