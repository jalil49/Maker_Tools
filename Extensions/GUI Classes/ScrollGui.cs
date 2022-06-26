using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;


namespace Extensions.GUI_Classes
{
    public class ScrollGUI
    {
        public GUILayoutOption[] layoutOptions;
        private static Vector2 Scrolling = new Vector2();
        public Action<bool> action;



        public void Draw()
        {
            Scrolling = GUILayout.BeginScrollView(Scrolling, layoutOptions);
            action.Invoke(true);
            GUILayout.EndScrollView();
        }
    }
}
