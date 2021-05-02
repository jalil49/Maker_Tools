using ChaCustom;
using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Maker;
using System;
using UnityEngine;

namespace Accessory_Shortcuts
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        Traverse Get_CVS_ACCESSORY;
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

            Get_CVS_ACCESSORY = Traverse.Create(MoreAccessoriesKOI.MoreAccessories._self);
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
                CvsAccessory CVS_Slot = Get_CVS_ACCESSORY.Method("GetCvsAccessory", new object[] { e.SlotNo }).GetValue<CvsAccessory>();
                CVS_Slot.UpdateCustomUI();
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
                    CvsAccessory CVS_Slot = Get_CVS_ACCESSORY.Method("GetCvsAccessory", new object[] { Slot }).GetValue<CvsAccessory>();
                    if (int.TryParse(Input.inputString, out var kind) && kind < 10)
                    {
                        CVS_Slot.UpdateSelectAccessoryType(kind);
                        CVS_Slot.UpdateCustomUI();
                    }
                    else if (Input.GetKeyDown(KeyCode.Minus))
                    {
                        CVS_Slot.UpdateSelectAccessoryType(10);
                        CVS_Slot.UpdateCustomUI();
                    }
                    Skip = false;
                }
            }
            //if (Input.GetKeyDown(KeyCode.N))
            //{
            //    Constants.Print_Dict();
            //}
            base.Update();
        }
    }
}
