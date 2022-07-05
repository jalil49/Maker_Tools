using System;
using UnityEngine;
using static Extensions.OnGUIExtensions;

namespace Extensions.GUI_Classes
{
    public class TextFieldGUI
    {
        public string Text = "Default Text";
        public string ButtonText = "Rename";
        private string newText;
        public GUILayoutOption[] layoutOptions;
        public GUIStyle style;
        public Action<string> newAction;
        public Action<string, string> oldAction;
        public TextFieldGUI(string _text, params GUILayoutOption[] gUILayoutOptions)
        {
            style = TextFieldStyle;
            Text = _text;
            layoutOptions = gUILayoutOptions;
            newText = _text;
        }

        public void ActiveDraw()
        {
            var newText = GUILayout.TextField(Text, style, layoutOptions);
            if (newText != Text)
            {
                if (oldAction != null)
                {
                    oldAction.Invoke(Text, newText);
                }
                Text = newText;
                if (newAction != null)
                {
                    newAction.Invoke(Text);
                }
            }
        }

        public void ConfirmDraw()
        {
            newText = GUILayout.TextField(newText, style, layoutOptions);

            if (newText != Text && Button(ButtonText, expandwidth: false))
            {
                if (oldAction != null)
                {
                    oldAction.Invoke(Text, newText);
                }

                Text = newText;

                if (newAction != null)
                {
                    newAction.Invoke(Text);
                }
            }
        }

        internal void ManuallySetNewText(string text)
        {
            newText = text;
        }
    }
}
