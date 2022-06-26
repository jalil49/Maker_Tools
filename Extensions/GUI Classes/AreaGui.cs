using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;


namespace Extensions.GUI_Classes
{
    public abstract class AreaGUI
    {
        public GUILayoutOption[] layoutOptions;
        public Rect Rect { get; set; }
        public void Draw(Action<bool> action)
        {
            GUILayout.BeginArea(Rect);
            action.Invoke(true);
            GUILayout.EndArea();
        }
    }
}
