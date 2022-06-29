using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;


namespace Extensions.GUI_Classes
{
    public class ToolbarGUI
    {
        public int Value = 0;
        public GUIContent[] Text;
        public GUILayoutOption[] layoutOptions;
        public GUIStyle style;
        public Action<int> onToggle;
        public ToolbarGUI(GUIContent[] _text, params GUILayoutOption[] options)
        {
            Text = _text;
            style = ButtonStyle;
            layoutOptions = options;
        }

        public void Draw()
        {
            var newValue = GUILayout.Toolbar(Value, Text, style, layoutOptions);
            if (newValue != Value)//xor operator
            {
                Value = newValue;
                if (onToggle == null)
                    return;
                onToggle.Invoke(Value);
            }
        }
    }
}
