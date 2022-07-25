using System;
using UnityEngine;
using static Extensions.OnGUIExtensions;

namespace Extensions.GUI_Classes
{
    public class TextFieldGUI
    {
        public GUIContent GUIContent;
        public string ButtonText = "Rename";
        private string newText;
        public GUILayoutOption[] layoutOptions;
        public GUIStyle style;
        public Action<string, string> OnValueChange;
        public TextFieldGUI(GUIContent _text, params GUILayoutOption[] gUILayoutOptions)
        {
            style = TextFieldStyle;
            GUIContent = _text;
            layoutOptions = gUILayoutOptions;
            newText = _text.text;
        }

        public void ActiveDraw()
        {
            var newText = GUILayout.TextField(GUIContent.text, style, layoutOptions);
            if(newText != GUIContent.text)
            {
                if(OnValueChange != null)
                {
                    OnValueChange.Invoke(GUIContent.text, newText);
                }

                GUIContent.text = newText;
            }
        }

        public void ConfirmDraw()
        {
            newText = GUILayout.TextField(newText, style, layoutOptions);

            if(newText != GUIContent.text && Button(ButtonText, expandwidth: false))
            {
                if(OnValueChange != null)
                {
                    OnValueChange.Invoke(GUIContent.text, newText);
                }

                GUIContent.text = newText;
            }
        }

        internal void ManuallySetNewText(string text)
        {
            newText = text;
        }
    }
}
