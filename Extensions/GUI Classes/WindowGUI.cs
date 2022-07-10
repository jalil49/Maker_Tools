using BepInEx.Configuration;
using UnityEngine;
using static Extensions.OnGUIExtensions;

namespace Extensions.GUI_Classes
{
    public class WindowGUI
    {
        private readonly int WindowID;

        public Rect Rect
        {
            get { return RectRef.Value; }
            private set
            {
                if (Rect.Equals(value))
                {
                    return;
                }
                value = KeepWithinWindowBounds(value);
                RectRef.Value = value;
            }
        }

        //Default settings save on change, therefore dont write if values dont actually change
        private readonly ConfigEntry<Rect> RectRef;
        private readonly ConfigEntry<int> TransparencyRef;

        public bool Show { get; private set; }

        private readonly GUIContent Content;
        private readonly GUI.WindowFunction WindowFunction;
        private Texture2D WindowTexture;
        private readonly GUIStyle Style;
        public int Transparency
        {
            get { return TransparencyRef.Value; }
            set
            {
                if (Transparency == value)
                {
                    return;
                }
                TransparencyBuild(value);
                TransparencyRef.Value = value;
            }
        }

        public WindowGUI(int winID, ConfigEntry<Rect> rectConfigEntry, ConfigEntry<int> transparencyConfig, GUI.WindowFunction windowFunction, GUIContent content)
        {
            WindowID = winID;
            RectRef = rectConfigEntry;
            TransparencyRef = transparencyConfig;
            Content = content;
            WindowFunction = windowFunction;

            Style = new GUIStyle(WindowStyle);
            WindowTexture = Style.normal.background;

            TransparencyBuild(Transparency);
        }

        public void TransparencyBuild(int value)
        {
            var height = WindowTexture.height;
            var width = WindowTexture.width;
            var newWindowTexture = new Texture2D(width, height);
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var test = WindowTexture.GetPixel(i, j);
                    test.a = value / 100f;
                    newWindowTexture.SetPixel(i, j, test);
                }
            }
            newWindowTexture.Apply();
            Style.normal.background = newWindowTexture;
            WindowTexture = newWindowTexture;
        }

        public void Draw()
        {
            if (!Show)
                return;
            Rect = GUILayout.Window(WindowID, Rect, DrawCall, "", Style);
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
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(Content, LabelStyle, GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
            WindowFunction(id);
            GUILayout.FlexibleSpace();
            Label(GUI.tooltip);
            Rect = KKAPI.Utilities.IMGUIUtils.DragResizeEatWindow(WindowID, Rect);
        }

        private Rect KeepWithinWindowBounds(Rect modifiedRect)
        {
            //just incase window somehow is bigger than screen size
            if (modifiedRect.height >= Screen.height)
                modifiedRect.height = Screen.height * 0.95f;

            if (modifiedRect.width >= Screen.width)
                modifiedRect.width = Screen.width * 0.9f;

            var axisAdjust = modifiedRect.width * 0.95f;

            //**Horizontal adjustments**//
            //too far to the right
            var adjustValue = modifiedRect.max.x - axisAdjust;
            if (adjustValue > Screen.width)
            {
                modifiedRect.x -= adjustValue - Screen.width;
            }

            //too far to the left
            adjustValue = modifiedRect.min.x + axisAdjust;
            if (adjustValue < 0)
            {
                modifiedRect.x -= adjustValue;
            }

            //**Vertical adjustments**//
            axisAdjust = modifiedRect.height * 0.9f;

            //too far to the bottom
            adjustValue = modifiedRect.max.y - axisAdjust;
            if (adjustValue > Screen.height)
            {
                modifiedRect.y -= adjustValue - Screen.height;
            }

            adjustValue = modifiedRect.min.y + axisAdjust;
            //too far to the top
            if (adjustValue < 0)
            {
                modifiedRect.y -= adjustValue;
            }

            return modifiedRect;
        }

        internal void SetWindowName(string newWindowName)
        {
            Content.text = newWindowName;
        }
    }
}
