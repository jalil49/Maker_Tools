using UnityEngine;
using static Extensions.OnGUIExtensions;


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
        private int frameCounter;

        public WindowGUI(bool transparent = false)
        {
            if (WindowTexture == null)
            {
                WindowTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                var colorVal = 0.2f;
                WindowTexture.SetPixel(0, 0, new Color(colorVal, colorVal, colorVal, transparent ? 0.5f : 1f));
                WindowTexture.Apply();
            }
            frameCounter = 0;
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

            if (frameCounter++ >= 60)
                KeepWithinWindowBounds();
        }

        public void ToggleShow()
        {
            Show = !Show;
        }
        public void ToggleShow(bool show)
        {
            Show = show;
        }

        private void DrawCall(int id)
        {
            WindowFunction(id);
            GUILayout.FlexibleSpace();
            Label(GUI.tooltip);
            Rect = KKAPI.Utilities.IMGUIUtils.DragResizeEatWindow(WindowID, Rect);
        }

        private void KeepWithinWindowBounds()
        {
            frameCounter = 0;//reset counter
            var axisAdjust = Rect.width * 0.9f;

            //**Horizontal adjustments**//

            //too far to the right
            var adjustValue = Rect.max.x - axisAdjust;
            if (adjustValue > Screen.width)
                Rect.x -= adjustValue - Screen.width;

            //too far to the left
            adjustValue = Rect.min.x + axisAdjust;
            if (adjustValue < 0)
                Rect.x -= adjustValue;

            //**Vertical adjustments**//
            axisAdjust = Rect.height * 0.9f;

            //too far to the bottom
            adjustValue = Rect.max.y - axisAdjust;
            if (adjustValue > Screen.height)
                Rect.y -= adjustValue - Screen.height;

            adjustValue = Rect.min.y + axisAdjust;
            //too far to the top
            if (adjustValue < 0)
                Rect.y -= adjustValue;
        }
    }
}
