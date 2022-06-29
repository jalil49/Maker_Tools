using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;


namespace Extensions.GUI_Classes
{
    public class TextFieldGUI
    {
        public string Text = "Default Text";
        public string ButtonText = "Rename";
        private string newText;
        public GUILayoutOption[] layoutOptions;
        public GUIStyle style;
        public Action<string> action;
        public TextFieldGUI(string _text, params GUILayoutOption[] gUILayoutOptions)
        {
            style = FieldStyle;
            Text = _text;
            layoutOptions = gUILayoutOptions;
            newText = _text;
        }

        public void ActiveDraw()
        {
            var newText = GUILayout.TextField(Text, style, layoutOptions);
            if (newText != Text)
            {
                Text = newText;
                if (action == null)
                    return;
                action.Invoke(Text);
            }
        }
        public void ConfirmDraw()
        {
            newText = GUILayout.TextField(newText, style, layoutOptions);
            if (newText != Text && Button(ButtonText, expandwidth: false))
            {
                Text = newText;
                if (action == null)
                    return;
                action.Invoke(Text);
            }
        }
    }
}
