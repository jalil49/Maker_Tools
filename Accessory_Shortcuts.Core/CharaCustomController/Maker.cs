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
        static readonly List<Toggle> Slot_Toggles = new List<Toggle>();
        static Transform Slots_Location;
        static Traverse More_Acc;
        static bool Skip = false;
        static bool makerslottriggered = false;

        internal static void MakerAPI_MakerFinishedLoading(object sender, EventArgs e)
        {
            More_Acc = Traverse.Create(MoreAccessoriesKOI.MoreAccessories._self);
#if !KKS
            var Slots_Location_string = "CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/04_AccessoryTop/Slots/Viewport/Content";
#else
            var Slots_Location_string = "CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/04_AccessoryTop/";
#endif
            AccessoriesApi.MakerAccSlotAdded += AccessoriesApi_MakerAccSlotAdded;
            Slots_Location = GameObject.Find(Slots_Location_string).transform;
            Slot_Toggles.Clear();
            UpdateSlots();
        }

        private static void AccessoriesApi_MakerAccSlotAdded(object sender, AccessorySlotEventArgs e)
        {
            if (!makerslottriggered)
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().MakerslotAddedcoroutine();
        }

        private void MakerslotAddedcoroutine()
        {
            StartCoroutine(MakerslotAdded());
        }

        private IEnumerator MakerslotAdded()
        {
            makerslottriggered = true;
            yield return new WaitForSeconds(0.1f);
            UpdateSlots();
            makerslottriggered = false;
        }

        internal void Update_Stored_Accessory(int slotNo, int type, int id, string parentKey)
        {
            if (type == 120 || Skip)
            {
                return;
            }
            Update_More_Accessories();

            var partsInfo = Accessorys_Parts[slotNo];

            if (type == partsInfo.type)
            {
                Constants.Parent[partsInfo.type - 120].Id = id;
                Constants.Parent[partsInfo.type - 120].ParentKey = parentKey;
            }
        }

        internal void Change_To_Stored_Accessory(int slotNo, int type, int id, string parentKey)
        {
            if (type == 120)
            {
                return;
            }
            else if (Constants.Parent.TryGetValue(type - 120, out var data) && (id != data.Id || parentKey != data.ParentKey))
            {
                ChaControl.ChangeAccessory(slotNo, type, data.Id, data.ParentKey);
                CvsAccessory CVS_Slot = More_Acc.Method("GetCvsAccessory", new object[] { slotNo }).GetValue<CvsAccessory>();
                CVS_Slot.UpdateCustomUI();
                if (slotNo < 20)
                {
                    ChaControl.chaFile.coordinate[(int)CurrentCoordinate.Value].accessory.parts[slotNo] = ChaControl.nowCoordinate.accessory.parts[slotNo];
                }
            }
        }

        private static void UpdateSlots()
        {
            for (int i = Slot_Toggles.Count, n = Slots_Location.childCount - 1; i < n; i++)
            {
                if (Slots_Location.GetChild(i).gameObject.activeSelf)
                {
                    Slot_Toggles.Add(Slots_Location.GetChild(i).GetComponent<Toggle>());
                }
                else
                {
                    return;
                }
            }
        }

        public static void NextSlot(int slot)
        {
            if (slot + 1 >= Slot_Toggles.Count)
            {
                UpdateSlots();
            }
            Slot_Toggles[Math.Max(slot - 1, 0)].isOn = true;
        }

        public static void PrevSlot(int slot)
        {
            if (slot + 1 >= Slot_Toggles.Count)
            {
                UpdateSlots();
            }
            Slot_Toggles[Math.Min(slot + 1, Slot_Toggles.Count - 1)].isOn = true;
        }

        protected override void Update()
        {
            if (Input.anyKeyDown && AccessoriesApi.AccessoryCanvasVisible)
            {
                var slot = AccessoriesApi.SelectedMakerAccSlot;
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    NextSlot(slot);
                    return;
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    PrevSlot(slot);
                    return;
                }
                var Emptyandvalid = slot < Slot_Toggles.Count && AccessoriesApi.GetPartsInfo(slot).type == 120;
                if (Emptyandvalid)
                {
                    Skip = true;
                    CvsAccessory CVS_Slot = More_Acc.Method("GetCvsAccessory", new object[] { slot }).GetValue<CvsAccessory>();
                    if (int.TryParse(Input.inputString, out var kind) && kind < 10 && kind > -1)
                    {
                        if (kind == 0)
                        {
                            kind = 10;
                        }
                        CVS_Slot.UpdateSelectAccessoryType(kind);
                        CVS_Slot.UpdateCustomUI();
                        CVS_Slot.tglAcsKind.isOn = true;
                    }
                    Skip = false;
                }
            }
            base.Update();
        }
    }
}
