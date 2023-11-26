using System;
using UnityEngine;
using static Extensions.OnGUIExtensions;

namespace Extensions.GUI_Classes
{
    public class MultiToolbarGUI
    {
        private readonly GUILayoutOption[] _layoutOptions;
        private readonly Action<int, int> _onValueChange;
        private readonly GUIStyle _style;
        private readonly GUIContent[] _text;
        private readonly int _xCount;

        public MultiToolbarGUI(int defaultValue, GUIContent[] text, Action<int, int> onValueChange, int xCount,
                               params GUILayoutOption[] options)
        {
            Value = defaultValue;
            _text = text;
            _onValueChange = onValueChange;
            _xCount = xCount;
            _style = ButtonStyle;
            _layoutOptions = options;
        }

        public int Value { get; private set; }

        public void Draw()
        {
            var newValue = GUILayout.SelectionGrid(Value, _text, _xCount, _style, _layoutOptions);
            if (newValue != Value)
            {
                if (_onValueChange != null)
                {
                    _onValueChange.Invoke(Value, newValue);
                }

                Value = newValue;
            }
        }
    }
}