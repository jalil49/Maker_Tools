using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;


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
            GUILayout.Label(Text, style, layoutOptions);
        }
    }
}
