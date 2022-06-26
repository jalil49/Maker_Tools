using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;


namespace Extensions.GUI_Classes
{
    public abstract class WindowGUI
    {
        public string Text = "Default Text";
        public int WindowID;
        public Rect Rect;

        public void Draw()
        {
            Rect = GUILayout.Window(WindowID, Rect, WindowDraw, Text);
        }

        public abstract void Init();

        public abstract void WindowDraw(int id);
    }
}
