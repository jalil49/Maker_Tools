using System;
using UnityEngine;
using static Extensions.OnGUIExtensions;

namespace Extensions.GUI_Classes
{
    public class TextAreaGUI
    {
        public Action<string> Action;
        public string ButtonText = "Rename";
        public GUILayoutOption[] LayoutOptions;
        private string _newText;
        public GUIStyle Style;
        public string Text = "Default Text";

        public TextAreaGUI(string text, params GUILayoutOption[] gUILayoutOptions)
        {
            Style = TextAreaStyle;
            Text = text;
            _newText = text;
            LayoutOptions = gUILayoutOptions;
        }

        public void ActiveDraw()
        {
            var newText = GUILayout.TextArea(Text, Style, LayoutOptions);
            if (newText != Text)
            {
                Text = newText;
                if (Action == null)
                    return;
                Action.Invoke(Text);
            }
        }

        public void ConfirmDraw()
        {
            _newText = GUILayout.TextArea(_newText, Style, LayoutOptions);
            if (_newText != Text && Button(ButtonText, expandwidth: false))
            {
                Text = _newText;
                if (Action == null)
                    return;
                Action.Invoke(Text);
            }
        }

        internal void ManuallySetNewText(string text)
        {
            _newText = text;
        }
    }
}