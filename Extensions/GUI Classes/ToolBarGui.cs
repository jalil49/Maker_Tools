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
        public Action<int, int> OnValueChange;
        public ToolbarGUI(int defaultValue, GUIContent[] _text, params GUILayoutOption[] options)
        {
            Value = defaultValue;
            Text = _text;
            style = ButtonStyle;
            layoutOptions = options;
        }

        public void Draw()
        {
            var newValue = GUILayout.Toolbar(Value, Text, style, layoutOptions);
            if(newValue != Value)//xor operator
            {
                if(OnValueChange != null)
                    OnValueChange.Invoke(Value, newValue);
                Value = newValue;
            }
        }
    }
}
