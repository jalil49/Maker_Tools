using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Extensions.OnGUIExtensions;


namespace Extensions.GUI_Classes
{
    public class ToggleGUI
    {
        public bool Value = false;
        public string Text = "Default Text";
        public GUILayoutOption[] layoutOptions;
        public GUIStyle style;
        public Action<bool> onToggle;
        public ToggleGUI(string _text, params GUILayoutOption[] options)
        {
            Text = _text;
            style = ToggleStyle;
            layoutOptions = options;
        }

        public void Draw()
        {
            if (GUILayout.Toggle(Value, Text, style, layoutOptions) ^ Value)//xor operator
            {
                Value = !Value;
                if (onToggle == null)
                    return;
                onToggle.Invoke(Value);
            }
        }
    }
}
