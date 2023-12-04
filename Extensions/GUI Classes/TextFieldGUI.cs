using System;
using UnityEngine;
using static Extensions.OnGUIExtensions;

namespace Extensions.GUI_Classes
{
    public class TextFieldGUI
    {
        private readonly GUILayoutOption[] _layoutOptions;
        private readonly Action<string, string> _onValueChange;
        private readonly GUIStyle _style;
        public readonly GUIContent GUIContent;
        private string _newText;
        public string ButtonText = "Rename";

        public TextFieldGUI(GUIContent text, Action<string, string> onValueChange,
            params GUILayoutOption[] gUILayoutOptions)
        {
            _style = TextFieldStyle;
            GUIContent = text;
            _onValueChange = onValueChange;
            _layoutOptions = gUILayoutOptions;
            _newText = text.text;
        }

        public void ActiveDraw()
        {
            var textField = GUILayout.TextField(GUIContent.text, _style, _layoutOptions);
            if (textField == GUIContent.text) return;

            _onValueChange?.Invoke(GUIContent.text, textField);

            GUIContent.text = textField;
        }

        public void ConfirmDraw()
        {
            _newText = GUILayout.TextField(_newText, _style, _layoutOptions);

            if (_newText != GUIContent.text && Button(ButtonText, expandwidth: false))
            {
                if (_onValueChange != null) _onValueChange.Invoke(GUIContent.text, _newText);

                GUIContent.text = _newText;
            }
        }

        internal void ManuallySetNewText(string text)
        {
            _newText = text;
        }
    }
}