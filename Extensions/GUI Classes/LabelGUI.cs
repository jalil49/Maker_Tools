using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Extensions.OnGUIExtensions;
using static UnityEngine.GUILayout;

namespace Extensions.GUI_Classes
{
    public class LabelGUI
    {
        public string Text = "Default Text";
        public GUILayoutOption[] layoutOptions;
        public GUIStyle style;

        public LabelGUI()
        {
            style = LabelStyle;
        }

        public void Draw()
        {
            BeginVertical();
            GUILayout.Label(Text, style, layoutOptions);
            EndVertical();
        }
    }
}
