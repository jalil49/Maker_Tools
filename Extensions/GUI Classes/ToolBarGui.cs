using System;
using UnityEngine;
using static Extensions.OnGUIExtensions;

namespace Extensions.GUI_Classes
{
    public class ToolbarGUI
    {
        private readonly GUILayoutOption[] _layoutOptions;
        private readonly Action<int, int> _onValueChange;
        private readonly GUIStyle _style;
        private readonly GUIContent[] _text;

        public ToolbarGUI(int defaultValue, GUIContent[] text, Action<int, int> onValueChange = null,
                          params GUILayoutOption[] options)
        {
            Value = defaultValue;
            _text = text;
            _onValueChange = onValueChange;
            _style = ButtonStyle;
            _layoutOptions = options;
        }

        public int Value { get; private set; }

        public void Draw()
        {
            var newValue = GUILayout.Toolbar(Value, _text, _style, _layoutOptions);
            if (newValue == Value)
            {
                return;
            }

            _onValueChange?.Invoke(Value, newValue);
            Value = newValue;
        }
    }
}