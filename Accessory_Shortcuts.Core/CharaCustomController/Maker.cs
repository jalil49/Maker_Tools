using System;
using ChaCustom;
using KKAPI.Chara;
using KKAPI.Maker;
using UnityEngine;

namespace Accessory_Shortcuts
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private static bool _skip;
        public static CustomAcsChangeSlot CustomAcs { get; internal set; }

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

                if (Input.GetKeyDown(KeyCode.E))
                {
                    NextSlot(slot);
                    return;
                }

                var emptyandvalid = slot < Parts.Length && Parts[slot].type == 120;
                if (emptyandvalid)
                {
                    _skip = true;

                    var cvsSlot = CustomAcs.cvsAccessory[slot];
                    if (int.TryParse(Input.inputString, out var kind) && kind < 10 && kind > -1)
                    {
                        if (kind == 0) kind = 10;
                        cvsSlot.UpdateSelectAccessoryType(kind);
                        cvsSlot.UpdateCustomUI();
                        cvsSlot.tglAcsKind.isOn = true;
                    }

                    _skip = false;
                }
            }

            base.Update();
        }

        internal void Update_Stored_Accessory(int slotNo, int type, int id, string parentKey)
        {
            if (type == 120 || _skip) return;

            var partsInfo = Parts[slotNo];

            if (type == partsInfo.type)
            {
                Constants.Parent[partsInfo.type - 120].Id = id;
                Constants.Parent[partsInfo.type - 120].ParentKey = parentKey;
            }
        }

        internal void Change_To_Stored_Accessory(int slotNo, int type, int id, string parentKey)
        {
            if (type == 120) return;

            if (Constants.Parent.TryGetValue(type - 120, out var data) &&
                (id != data.Id || parentKey != data.ParentKey))
            {
                ChaControl.ChangeAccessory(slotNo, type, data.Id, data.ParentKey);
                CustomBase.Instance.SetUpdateCvsAccessory(slotNo, true);
                ChaControl.chaFile.coordinate[(int)CurrentCoordinate.Value].accessory.parts[slotNo] =
                    ChaControl.nowCoordinate.accessory.parts[slotNo];
            }
        }

        public void PrevSlot(int slot)
        {
            CustomAcs.items[Math.Max(slot - 1, 0)].tglItem.isOn = true;
        }

        public void NextSlot(int slot)
        {
            CustomAcs.items[Math.Min(slot + 1, Parts.Length - 1)].tglItem.isOn = true;
        }
    }
}