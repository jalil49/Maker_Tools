using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Extensions.GUI_Classes.Config;
using KKAPI.Utilities;
using UnityEngine;
using static Extensions.OnGUIExtensions;
using GL = UnityEngine.GUILayout;

namespace Extensions.GUI_Classes
{
    public class WindowGUI
    {
        public static List<WindowGUI> WindowGuIs = new List<WindowGUI>();
        private static int _windowCount;
        internal static bool SaveEvent;

        private readonly ScrollGUI[] _scrollGuis;

        //Default settings save on change, therefore dont write if values dont actually change
        private readonly ConfigEntry<WindowConfig> _configEntry;
        private readonly GUIContent _content;
        private readonly GUIStyle _style;
        private readonly Func<WindowReturn> _windowFunction;
        private readonly int _windowID;
        private readonly string _windowName;
        private Texture2D _windowTexture;

        private WindowGUI(ConfigFile config, string section, string windowName, Rect rect, float transparency,
            Func<WindowReturn> windowFunction, GUIContent content)
        {
            _windowID = ++_windowCount;
            _windowName = windowName;
            _configEntry = WindowConfig.GetConfigEntry(config, section, windowName + " window",
                new WindowConfig(rect, transparency));
            _content = content;
            _windowFunction = windowFunction;

            _style = new GUIStyle(WindowStyle);
            _windowTexture = _style.normal.background;

            TransparencyBuild(Transparency);
            WindowGuIs.Add(this);
            SaveEvent = false;
        }

        public WindowGUI(ConfigFile config, string section, string windowName, Rect rect, float transparency,
            Func<WindowReturn> windowFunction, GUIContent content, ScrollGUI scrollGUI = null) : this(
            config, section, windowName, rect, transparency, windowFunction, content)
        {
            _scrollGuis = scrollGUI != null ? new[] { scrollGUI } : new ScrollGUI[0];
        }

        public WindowGUI(ConfigFile config, string section, string windowName, Rect rect, float transparency,
            Func<WindowReturn> windowFunction, GUIContent content, ScrollGUI[] scrollGuis) : this(config,
            section, windowName, rect, transparency, windowFunction, content)
        {
            _scrollGuis = scrollGuis;
        }

        public Rect Rect
        {
            get => _configEntry.Value.WindowRect;
            private set
            {
                if (Rect.Equals(value)) return;

                value = KeepWithinWindowBounds(value);
                _configEntry.Value.WindowRect = value;
                SaveEvent = true;
            }
        }

        public bool Show { get; private set; }

        public float Transparency
        {
            get => _configEntry.Value.Transparency;
            set
            {
                if (Transparency == value) return;

                TransparencyBuild(value);
                _configEntry.Value.Transparency = value;
                SaveEvent = true;
            }
        }

        public void TransparencyBuild(float value)
        {
            var height = _windowTexture.height;
            var width = _windowTexture.width;
            var newWindowTexture = new Texture2D(width, height);
            for (var i = 0; i < width; i++)
            for (var j = 0; j < height; j++)
            {
                var test = _windowTexture.GetPixel(i, j);
                test.a = value;
                newWindowTexture.SetPixel(i, j, test);
            }

            newWindowTexture.Apply();
            _style.normal.background = newWindowTexture;
            _windowTexture = newWindowTexture;
        }

        public void Draw()
        {
            if (!Show) return;

            Rect = GL.Window(_windowID, Rect, DrawCall, string.Empty, _style, GL.ExpandWidth(true));
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
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                GL.Label(_content, LabelStyle, GL.ExpandWidth(false));
                GL.FlexibleSpace();
            }

            GL.EndHorizontal();

            var windowReturn = _windowFunction();

            if (_scrollGuis.Length > 0)
            {
                if (_scrollGuis.Length == 1)
                    _scrollGuis[0].Draw();
                else
                    _scrollGuis[windowReturn.SelectedScrollGui].Draw();
            }

            GL.FlexibleSpace();
            Label(GUI.tooltip);
            ConfigSave();
            Rect = IMGUIUtils.DragResizeEatWindow(_windowID, Rect);
        }

        //only save when a config value reaches final value for dragable events
        private void ConfigSave()
        {
            if (SaveEvent && Event.current.type == (EventType)1)
            {
                SaveEvent = false;
                _configEntry.ConfigFile.Save();
            }
        }

        private Rect KeepWithinWindowBounds(Rect modifiedRect)
        {
            //just incase window somehow is bigger than screen size from resizing
            if (modifiedRect.height > Screen.height) modifiedRect.height = Screen.height * 0.9f;

            if (modifiedRect.width > Screen.width) modifiedRect.width = Screen.width * 0.9f;

            var axisAdjust = modifiedRect.width * 0.95f;

            //**Horizontal adjustments**//
            //too far to the right
            var adjustValue = modifiedRect.max.x - axisAdjust;
            if (adjustValue > Screen.width) modifiedRect.x -= adjustValue - Screen.width;

            //too far to the left
            adjustValue = modifiedRect.min.x + axisAdjust;
            if (adjustValue < 0) modifiedRect.x -= adjustValue;

            //**Vertical adjustments**//
            axisAdjust = modifiedRect.height * 0.9f;

            //too far to the bottom
            adjustValue = modifiedRect.max.y - axisAdjust;
            if (adjustValue > Screen.height) modifiedRect.y -= adjustValue - Screen.height;

            adjustValue = modifiedRect.min.y + axisAdjust;
            //too far to the top
            if (adjustValue < 0) modifiedRect.y -= adjustValue;

            return modifiedRect;
        }

        internal void SetWindowName(string newWindowName)
        {
            _content.text = newWindowName;
        }

        internal void TransparencyDraw()
        {
            GL.BeginHorizontal();
            {
                Label(_windowName + " Transparency:", expandWidth: false);
                Transparency = HorizontalSlider(Transparency, 0f, 1f); //Captures inputs can't trigger autosave
            }

            GL.EndHorizontal();
        }

        internal static void ManualSaveDraw()
        {
            //if button is not drawn before horizontal slider, slider won't be dragable
            if (Button(SaveEvent ? "Manual Save" : string.Empty, "Setting without Auto Save was changed", SaveEvent) &&
                WindowGuIs.Count > 0)
            {
                SaveEvent = false;
                WindowGuIs[0]._configEntry.ConfigFile.Save();
            }
        }
    }

    public struct WindowReturn
    {
        public int SelectedScrollGui;

        public WindowReturn(int scrollValue)
        {
            SelectedScrollGui = scrollValue;
        }
    }
}