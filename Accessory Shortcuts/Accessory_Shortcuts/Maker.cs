using ChaCustom;
using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Maker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Accessory_Shortcuts
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        List<Toggle> Slot_Toggles = new List<Toggle>();
        //List<CvsAccessory> Slot_CvsAccessory = new List<CvsAccessory>();
        Transform Slots_Location;
        Traverse More_Acc;
        //CustomAcsSelectKind CustomAcsSelectKind_Reference;
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
            //CustomAcsSelectKind_Reference = Hooks.CustomAcsSelectKind_Reference;
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
            //Slot_CvsAccessory.Clear();
            Slot_Toggles.Clear();
            for (int i = 0, n = Slots_Location.childCount - 1; i < n; i++)
            {
                Slot_Toggles.Add(Slots_Location.GetChild(i).GetComponent<Toggle>());
                //Slot_CvsAccessory.Add(Slots_Location.GetChild(i).GetComponentInChildren<CvsAccessory>(true));
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
                    if (int.TryParse(Input.inputString, out var kind) && kind < 10)
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
                    //StartCoroutine(ToggleSlot(Math.Max(Slot - 1, 0)));
                    //CustomAcsSelectKind_Reference.OnSelect(Math.Max(Slot - 1, 0));
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    Slot_Toggles[Math.Min(Slot + 1, Slot_Toggles.Count - 1)].isOn = true;
                    //StartCoroutine(ToggleSlot(Math.Min(Slot + 1, Slot_Toggles.Count - 1)));
                    //CustomAcsSelectKind_Reference.OnSelect(Math.Min(Slot + 1, 19 + Accessorys_Parts.Count));
                }
            }
            //if (Input.GetKeyDown(KeyCode.N))
            //{
            //    Constants.Print_Dict();
            //}
            base.Update();
        }
        private IEnumerator ToggleSlot(int Slot)
        {
            yield return null;
            Logger.LogWarning($"Disabling Slot {AccessoriesApi.SelectedMakerAccSlot} and enabling {Slot}");
            //Slot_Toggles[AccessoriesApi.SelectedMakerAccSlot].isOn = false;
            // Slot_CvsAccessory[AccessoriesApi.SelectedMakerAccSlot].gameObject.SetActive(false);
            Slot_Toggles[Slot].isOn = true;
            //Slot_CvsAccessory[Slot].gameObject.SetActive(true);
        }
    }
}
