using System;
using UnityEngine;
using static Extensions.OnGUIExtensions;

namespace Extensions.GUI_Classes
{
    public class IntTextFieldGUI
    {
        public Action<int> Action;
        public GUILayoutOption[] LayoutOptions;
        public GUIStyle Style;
        public string Text = "0";

        public IntTextFieldGUI(string text, params GUILayoutOption[] gUILayoutOptions)
        {
            Style = TextFieldStyle;
            Text = text;
            LayoutOptions = gUILayoutOptions;
        }

        public void Draw()
        {
            var newText = GUILayout.TextField(Text, Style, LayoutOptions);
            if (newText != Text && int.TryParse(newText, out var value))
            {
                Text = newText;
                if (Action == null)
                    return;
                Action.Invoke(value);
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