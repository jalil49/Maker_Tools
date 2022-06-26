using Extensions.GUI_Classes;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace Accessory_States.Settings
{
    internal class StatesGUI : WindowGUI
    {
        public ToggleGUI ShowDelete;
        public ScrollGUI Scroll;
        private CharaEvent CharaEvent => KKAPI.Maker.MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
        public StatesGUI()
        {
            Init();
        }

        public override void Init()
        {
            ShowDelete = new ToggleGUI("Show Delete", ExpandWidth(false));
            Scroll = new ScrollGUI();

        }

        public override void WindowDraw(int id)
        {

        }
    }
}
