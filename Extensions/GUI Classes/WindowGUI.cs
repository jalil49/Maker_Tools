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
        public WindowGUI WindowRef;
        public Vector2 RectAdjustVec;
        private bool firstDraw;

        public bool Show;
        public GUI.WindowFunction WindowFunction;
        public GUIContent content;
        private readonly Texture2D WindowTexture;
        private readonly GUIStyle boxStyle;

        public WindowGUI()
        {
            if (WindowTexture == null)
            {
                WindowTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                var colorVal = 0.2f;
                WindowTexture.SetPixel(0, 0, new Color(colorVal, colorVal, colorVal));
                WindowTexture.Apply();
            }
            firstDraw = true;
            boxStyle = new GUIStyle { normal = new GUIStyleState { background = WindowTexture } };
        }

        public void Draw()
        {
            if (!Show)
                return;
            if (firstDraw && WindowRef != null)
            {
                firstDraw = false;
                Rect.x = WindowRef.Rect.x;
                Rect.y = WindowRef.Rect.y;
                Rect.height = WindowRef.Rect.height;
                if (RectAdjustVec.x != 0f)
                    Rect.x += WindowRef.Rect.width + RectAdjustVec.x;
                if (RectAdjustVec.y != 0f)
                    Rect.y += WindowRef.Rect.height + RectAdjustVec.y;
            }

            GUI.Box(Rect, GUIContent.none, boxStyle);
            Rect = GUILayout.Window(WindowID, Rect, DrawCall, content);

            //keep in window

            if (Rect.center.x > Screen.width)
                Rect.x -= Rect.center.x - Screen.width;
            if (Rect.center.x < 0)
                Rect.x -= Rect.center.x;

            if (Rect.center.y > Screen.height)
                Rect.y -= Rect.center.y - Screen.height;
            if (Rect.center.y < 0)
                Rect.y -= Rect.center.y;

            //if (Rect.xMax > Screen.width)
            //    Rect.x -= Rect.xMax - Screen.width;
            //if (Rect.xMin < 0)
            //    Rect.x -= Rect.xMin;

            //if (Rect.yMax > Screen.height)
            //    Rect.y -= Rect.yMax - Screen.height;
            //if (Rect.yMin < 0)
            //    Rect.y -= Rect.yMin;
        }

        public void ToggleShow()
        {
            Show = !Show;
        }

        private void DrawCall(int id)
        {
            WindowFunction(id);
            Label(GUI.tooltip);
            Rect = KKAPI.Utilities.IMGUIUtils.DragResizeEatWindow(WindowID, Rect);
        }
    }
}
