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
        public Action<string, string> OnValueChange;
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
                if (OnValueChange != null)
                {
                    OnValueChange.Invoke(Text, newText);
                }
                Text = newText;
            }
        }

        public void ConfirmDraw()
        {
            newText = GUILayout.TextField(newText, style, layoutOptions);

            if (newText != Text && Button(ButtonText, expandwidth: false))
            {
                if (OnValueChange != null)
                {
                    OnValueChange.Invoke(Text, newText);
                }

                Text = newText;
            }
        }

        internal void ManuallySetNewText(string text)
        {
            newText = text;
        }
    }
}
