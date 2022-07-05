using System;
using UnityEngine;


namespace Extensions.GUI_Classes
{
    public class ScrollGUI<T>
    {
        public GUILayoutOption[] layoutOptions;
        public Action<T> action;

        private Vector2 Scrolling = new Vector2();

        public void Draw(T actionObj)
        {
            Scrolling = GUILayout.BeginScrollView(Scrolling, layoutOptions);
            action.Invoke(actionObj);
            GUILayout.EndScrollView();
        }
    }
}
