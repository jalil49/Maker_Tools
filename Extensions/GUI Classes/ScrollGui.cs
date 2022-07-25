using System;
using UnityEngine;

namespace Extensions.GUI_Classes
{
    public class ScrollGUI
    {
        private GUILayoutOption[] layoutOptions;
        private Action action;

        private Vector2 Scrolling;

        public ScrollGUI(Action action, GUILayoutOption[] gUILayoutOptions = null)
        {
            if(action == null)
                throw new ArgumentNullException("action");

            layoutOptions = gUILayoutOptions ?? new GUILayoutOption[0];
            this.action = action;
            Scrolling = new Vector2();
        }

        public void Draw()
        {
            Scrolling = GUILayout.BeginScrollView(Scrolling, layoutOptions);
            action.Invoke();
            GUILayout.EndScrollView();
        }
    }
}
