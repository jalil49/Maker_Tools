﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;


namespace Extensions.GUI_Classes
{
    public class ToggleGUI : IDraw<bool>
    {
        public bool Value = false;
        public string Text = "Default Text";
        public GUILayoutOption[] layoutOptions;
        public GUIStyle style;

        public ToggleGUI(string _text, params GUILayoutOption[] options)
        {
            Text = _text;
            style = ToggleStyle;
            layoutOptions = options;
        }

        public void Draw(Action<bool> action)
        {
            if (GUILayout.Toggle(Value, Text, ToggleStyle, layoutOptions) ^ Value)//xor operator
            {
                Value = !Value;
                if (action == null) return;
                action.Invoke(Value);
            }
        }
    }
}
