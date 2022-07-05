using System;
using UnityEngine;
using static Extensions.OnGUIExtensions;

namespace Extensions.GUI_Classes
{
    public class ToolbarGUI
    {
        public int Value = 0;
        public GUIContent[] Text;
        public GUILayoutOption[] layoutOptions;
        public GUIStyle style;
        public Action<int> action;
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
                if (action == null)
                    return;
                action.Invoke(Value);
            }
        }
    }
}
