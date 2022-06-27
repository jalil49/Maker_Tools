using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;


namespace Extensions.GUI_Classes
{
    public class WindowGUI
    {
        public int WindowID;
        public Rect Rect;
        public bool Show;
        public GUI.WindowFunction WindowFunction;
        public GUIContent content;
        public void Draw()
        {
            Rect = GUILayout.Window(WindowID, Rect, DrawCall, content);
        }

        public void ToggleShow()
        {
            Show = !Show;
        }

        public void EatDragResize()
        {
            Rect = KKAPI.Utilities.IMGUIUtils.DragResizeEatWindow(WindowID, Rect);
        }

        private void DrawCall(int id)
        {
            WindowFunction(id);
            EatDragResize();
        }
    }
}
