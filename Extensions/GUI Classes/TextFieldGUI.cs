using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;


namespace Extensions.GUI_Classes
{
    public class TextFieldGUI : IDraw<string>
    {
        public string Text = "Default Text";
        public GUILayoutOption[] layoutOptions;
        public GUIStyle style;

        public TextFieldGUI()
        {
            style = FieldStyle;
        }

        public void Draw(Action<string> action)
        {
            var newText = GUILayout.TextField(Text, style, layoutOptions);
            if (newText != Text)
            {
                Text = newText;
                if (action == null) return;
                action.Invoke(Text);
            }
        }
    }
}
