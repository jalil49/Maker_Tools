using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;


namespace Extensions.GUI_Classes
{
    public class ToggleGUI
    {
        public bool Value = false;
        public string Text = "Default Text";
        public bool expandWidth = false;
        public Action<bool> action;
        public void Draw()
        {
            if (Toggle(Value, Text, expandWidth) ^ Value)//xor operator
            {
                Value = !Value;
                if (action == null) return;
                action.Invoke(Value);
            }
        }

    }
}
