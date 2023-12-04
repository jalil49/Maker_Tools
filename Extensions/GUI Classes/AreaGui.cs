using System;
using UnityEngine;

namespace Extensions.GUI_Classes
{
    public abstract class AreaGUI
    {
        private GUILayoutOption[] _layoutOptions;
        public Rect Rect { get; set; }

        public void Draw(Action<bool> action)
        {
            GUILayout.BeginArea(Rect);
            action.Invoke(true);
            GUILayout.EndArea();
        }
    }
}