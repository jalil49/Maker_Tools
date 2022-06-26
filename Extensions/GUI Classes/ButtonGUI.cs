using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;


namespace Extensions.GUI_Classes
{
    public class ButtonGUI<T>
    {
        public string Text = "Default Text";
        public GUILayoutOption[] layoutOptions;
        public GUIStyle style;
        public Action<T> action;
        public ButtonGUI(string _text, params GUILayoutOption[] options)
        {
            style = ButtonStyle;
            Text = _text;
            layoutOptions = options;
        }

        public void Draw(T invoke)
        {
            if (GUILayout.Button(Text, style, layoutOptions))
            {
                if (action == null) return;
                action.Invoke(invoke);
            }
        }
    }
}
