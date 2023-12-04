using System;
using UnityEngine;
using static Extensions.OnGUIExtensions;

namespace Extensions.GUI_Classes
{
    public class ButtonGUI<T>
    {
        public Action<T> Action;
        public GUILayoutOption[] LayoutOptions;
        public GUIStyle Style;
        public string Text = "Default Text";

        public ButtonGUI(string text, params GUILayoutOption[] options)
        {
            Style = ButtonStyle;
            Text = text;
            LayoutOptions = options;
        }

        public void Draw(T invoke)
        {
            if (GUILayout.Button(Text, Style, LayoutOptions))
            {
                if (Action == null)
                    return;
                Action.Invoke(invoke);
            }
        }
    }
}