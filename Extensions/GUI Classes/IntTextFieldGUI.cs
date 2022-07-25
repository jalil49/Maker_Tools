using System;
using UnityEngine;
using static Extensions.OnGUIExtensions;

namespace Extensions.GUI_Classes
{
    public class IntTextFieldGUI
    {
        public string Text = "0";
        public GUILayoutOption[] layoutOptions;
        public GUIStyle style;
        public Action<int> action;
        public IntTextFieldGUI(string _text, params GUILayoutOption[] gUILayoutOptions)
        {
            style = TextFieldStyle;
            Text = _text;
            layoutOptions = gUILayoutOptions;
        }

        public void Draw()
        {
            var newText = GUILayout.TextField(Text, style, layoutOptions);
            if(newText != Text && int.TryParse(newText, out var value))
            {
                Text = newText;
                if(action == null)
                    return;
                action.Invoke(value);
            }
        }

        public void Draw(int value)
        {
            Text = value.ToString();
            Draw();
        }

        public int GetValue()
        {
            return int.Parse(Text);
        }
    }
}
