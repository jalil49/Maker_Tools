using Extensions.GUI_Classes;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;


namespace Accessory_States
{
    //What does this window display?:
    //  it should display bindings on current slot
    //  it should display bindings to add to slot

    internal class MakerGUI : WindowGUI
    {
        private CharaEvent CharaEvent;
        private int SelectedDropDown;
        private int slot;
        public MakerGUI()
        {
            WindowID = 1;
            Init();
        }

        public override void Init()
        {
            CharaEvent = KKAPI.Maker.MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
        }

        public override void WindowDraw(int id)
        {
            if (SelectedDropDown != KKAPI.Maker.AccessoriesApi.SelectedMakerAccSlot)
            {
                slot = KKAPI.Maker.AccessoriesApi.SelectedMakerAccSlot;
                Text = "Slot " + slot;
            }
            BindingData bData = null;
            if (CharaEvent.SlotBindingData.TryGetValue(slot, out var slotData))
            {
                if (SelectedDropDown >= slotData.bindingDatas.Count) SelectedDropDown = 0;
                if (slotData.bindingDatas.Count > 0)
                {
                    bData = slotData.bindingDatas[SelectedDropDown];
                }
            }

            GUILayout.BeginHorizontal();
            {
                var dropDownName = "None";
                if (bData != null)
                {
                    dropDownName = bData.NameData.Name;
                }
                if (Button("<") && slotData.bindingDatas.Count > 0) SelectedDropDown = SelectedDropDown == 0 ? slotData.bindingDatas.Count - 1 : SelectedDropDown--;
                Button(dropDownName);
                if (Button(">") && slotData.bindingDatas.Count > 0) SelectedDropDown = SelectedDropDown == slotData.bindingDatas.Count - 1 ? 0 : SelectedDropDown++;
            }
            GUILayout.EndHorizontal();

            Rect = KKAPI.Utilities.IMGUIUtils.DragResizeEatWindow(WindowID, Rect);
        }
    }
}
