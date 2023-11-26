using System;
using UnityEngine;
using static Extensions.OnGUIExtensions;

namespace Extensions.GUI_Classes
{
    public class ToggleGUI
    {
        private readonly GUILayoutOption[] _layoutOptions;
        private readonly GUIStyle _style;
        public Action<bool> OnToggle;
        public string Text;

        public ToggleGUI(bool value, string text, params GUILayoutOption[] options)
        {
            Value = value;
            Text = text;
            _style = ToggleStyle;
            _layoutOptions = options;
        }

        public bool Value { get; set; }

        public void Draw()
        {
            if (GUILayout.Toggle(Value, Text, _style, _layoutOptions) ^ Value) //xor operator
            {
                Value = !Value;
                OnToggle?.Invoke(Value);
            }
        }
    }
}