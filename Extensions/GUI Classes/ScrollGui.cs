using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;


namespace Extensions.GUI_Classes
{
    public class ScrollGUI<T>
    {
        public GUILayoutOption[] layoutOptions;
        private static Vector2 Scrolling = new Vector2();
        public Action<T> action;

        public void Draw(T actionObj)
        {
            Scrolling = GUILayout.BeginScrollView(Scrolling, layoutOptions);
            action.Invoke(actionObj);
            GUILayout.EndScrollView();
        }
    }
}
