using ChaCustom;
using KKAPI.Chara;
using KKAPI.Maker;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Accessory_Shortcuts
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        static bool Skip = false;
        public static CustomAcsChangeSlot CustomAcs { get; internal set; }

        internal void Update_Stored_Accessory(int slotNo, int type, int id, string parentKey)
        {
            if (type == 120 || Skip)
            {
                return;
            }

            var partsInfo = Parts[slotNo];

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
                CustomBase.Instance.SetUpdateCvsAccessory(slotNo, true);
                ChaControl.chaFile.coordinate[(int)CurrentCoordinate.Value].accessory.parts[slotNo] = ChaControl.nowCoordinate.accessory.parts[slotNo];
            }
        }

        public void PrevSlot(int slot)
        {
            CustomAcs.cvsAccessory[Math.Max(slot - 1, 0)].transform.parent.GetComponent<Toggle>().isOn = true;
        }

        public void NextSlot(int slot)
        {
            CustomAcs.cvsAccessory[Math.Min(slot + 1, Parts.Length - 1)].transform.parent.GetComponent<Toggle>().isOn = true;
        }

        protected override void Update()
        {
            if (Input.anyKeyDown && AccessoriesApi.AccessoryCanvasVisible)
            {
                var slot = AccessoriesApi.SelectedMakerAccSlot;
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    PrevSlot(slot);
                    return;
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    NextSlot(slot);
                    return;
                }
                var Emptyandvalid = slot < Parts.Length && Parts[slot].type == 120;
                if (Emptyandvalid)
                {
                    Skip = true;

                    var CVS_Slot = CustomAcs.cvsAccessory[slot];
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
