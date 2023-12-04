using System;
using UnityEngine;

namespace Extensions.GUI_Classes
{
    public class ScrollGUI
    {
        private readonly Action _action;
        private readonly GUILayoutOption[] _layoutOptions;

        private Vector2 _scrolling;

        public ScrollGUI(Action action, GUILayoutOption[] gUILayoutOptions = null)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _layoutOptions = gUILayoutOptions ?? new GUILayoutOption[0];
            this._action = action;
            _scrolling = new Vector2();
        }

        public void Draw()
        {
            _scrolling = GUILayout.BeginScrollView(_scrolling, _layoutOptions);
            _action.Invoke();
            GUILayout.EndScrollView();
        }
    }
}