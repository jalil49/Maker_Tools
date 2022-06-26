using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;


namespace Extensions.GUI_Classes
{
    public class ButtonGUI : IDraw<bool>
    {
        public string Text = "Default Text";
        public GUILayoutOption[] layoutOptions;
        public GUIStyle style;

        public ButtonGUI()
        {
            style = ButtonStyle;
        }

        public void Draw(Action<bool> action)
        {
            if (GUILayout.Button(Text, style, layoutOptions))
            {
                if (action == null) return;
                action.Invoke(true);
            }
        }
    }
}
