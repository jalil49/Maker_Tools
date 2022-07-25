using System;
using UnityEngine;
using static Extensions.OnGUIExtensions;

namespace Extensions.GUI_Classes
{
    public class TextAreaGUI
    {
        public string Text = "Default Text";
        public string ButtonText = "Rename";
        private string newText;
        public GUILayoutOption[] layoutOptions;
        public GUIStyle style;
        public Action<string> action;
        public TextAreaGUI(string _text, params GUILayoutOption[] gUILayoutOptions)
        {
            style = TextAreaStyle;
            Text = _text;
            newText = _text;
            layoutOptions = gUILayoutOptions;
        }

        public void ActiveDraw()
        {
            var newText = GUILayout.TextArea(Text, style, layoutOptions);
            if(newText != Text)
            {
                Text = newText;
                if(action == null)
                    return;
                action.Invoke(Text);
            }
        }
        public void ConfirmDraw()
        {
            newText = GUILayout.TextArea(newText, style, layoutOptions);
            if(newText != Text && Button(ButtonText, expandwidth: false))
            {
                Text = newText;
                if(action == null)
                    return;
                action.Invoke(Text);
            }
        }

        internal void ManuallySetNewText(string text)
        {
            newText = text;
        }
    }
}
