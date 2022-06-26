using Extensions.GUI_Classes;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace Accessory_States.Settings
{
    //What does this window display?:
    //  it should display bindings on current slot
    //  it should display bindings to add to slot

    internal class MakerGUI : WindowGUI
    {
        private CharaEvent CharaEvent;
        public MakerGUI()
        {
            Init();
        }

        public override void Init()
        {
            CharaEvent = KKAPI.Maker.MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
        }

        public override void WindowDraw(int id)
        {

        }
    }
}
