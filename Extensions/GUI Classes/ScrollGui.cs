using System;
using UnityEngine;


namespace Extensions.GUI_Classes
{
    public class ScrollGUI
    {
        public GUILayoutOption[] layoutOptions;
        public Action action;

        private Vector2 Scrolling = new Vector2();

        public void Draw()
        {
            Scrolling = GUILayout.BeginScrollView(Scrolling, layoutOptions);
            action.Invoke();
            GUILayout.EndScrollView();
        }
    }
}
