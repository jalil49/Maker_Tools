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
        public static List<WindowGUI> windowGUIs = new List<WindowGUI>();
        private static int WindowCount;
        internal static bool SaveEvent;

        private readonly ScrollGUI[] _scrollGuis;

        //Default settings save on change, therefore dont write if values dont actually change
        private readonly ConfigEntry<WindowConfig> ConfigEntry;
        private readonly GUIContent Content;
        private readonly GUIStyle Style;
        private readonly Func<WindowReturn> WindowFunction;
        private readonly int WindowID;
        private readonly string WindowName;
        private Texture2D WindowTexture;

        private WindowGUI(ConfigFile config, string section, string windowName, Rect rect, float transparency,
                          Func<WindowReturn> windowFunction, GUIContent content)
        {
            WindowID = ++WindowCount;
            WindowName = windowName;
            ConfigEntry = WindowConfig.GetConfigEntry(config, section, windowName + " window",
                new WindowConfig(rect, transparency));
            Content = content;
            WindowFunction = windowFunction;

            Style = new GUIStyle(WindowStyle);
            WindowTexture = Style.normal.background;

            TransparencyBuild(Transparency);
            windowGUIs.Add(this);
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
            section, windowName, rect, transparency, windowFunction, content) => _scrollGuis = scrollGuis;

        public Rect Rect
        {
            get => ConfigEntry.Value.WindowRect;
            private set
            {
                if (Rect.Equals(value))
                {
                    return;
                }

                value = KeepWithinWindowBounds(value);
                ConfigEntry.Value.WindowRect = value;
                SaveEvent = true;
            }
        }

        public bool Show { get; private set; }

        public float Transparency
        {
            get => ConfigEntry.Value.Transparency;
            set
            {
                if (Transparency == value)
                {
                    return;
                }

                TransparencyBuild(value);
                ConfigEntry.Value.Transparency = value;
                SaveEvent = true;
            }
        }

        public void TransparencyBuild(float value)
        {
            var height = WindowTexture.height;
            var width = WindowTexture.width;
            var newWindowTexture = new Texture2D(width, height);
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var test = WindowTexture.GetPixel(i, j);
                    test.a = value;
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
            {
                return;
            }

            Rect = GL.Window(WindowID, Rect, DrawCall, string.Empty, Style, GL.ExpandWidth(true));
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
                GL.Label(Content, LabelStyle, GL.ExpandWidth(false));
                GL.FlexibleSpace();
            }

            GL.EndHorizontal();

            var windowReturn = WindowFunction();

            if (_scrollGuis.Length > 0)
            {
                if (_scrollGuis.Length == 1)
                {
                    _scrollGuis[0].Draw();
                }
                else
                {
                    _scrollGuis[windowReturn.SelectedScrollGui].Draw();
                }
            }

            GL.FlexibleSpace();
            Label(GUI.tooltip);
            ConfigSave();
            Rect = IMGUIUtils.DragResizeEatWindow(WindowID, Rect);
        }

        //only save when a config value reaches final value for dragable events
        private void ConfigSave()
        {
            if (SaveEvent && Event.current.type == (EventType)1)
            {
                SaveEvent = false;
                ConfigEntry.ConfigFile.Save();
            }
        }

        private Rect KeepWithinWindowBounds(Rect modifiedRect)
        {
            //just incase window somehow is bigger than screen size from resizing
            if (modifiedRect.height > Screen.height)
            {
                modifiedRect.height = Screen.height * 0.9f;
            }

            if (modifiedRect.width > Screen.width)
            {
                modifiedRect.width = Screen.width * 0.9f;
            }

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

        internal void TransparencyDraw()
        {
            GL.BeginHorizontal();
            {
                Label(WindowName + " Transparency:", expandWidth: false);
                Transparency = HorizontalSlider(Transparency, 0f, 1f); //Captures inputs can't trigger autosave
            }

            GL.EndHorizontal();
        }

        internal static void ManualSaveDraw()
        {
            //if button is not drawn before horizontal slider, slider won't be dragable
            if (Button(SaveEvent ? "Manual Save" : string.Empty, "Setting without Auto Save was changed", SaveEvent) &&
                windowGUIs.Count > 0)
            {
                SaveEvent = false;
                windowGUIs[0].ConfigEntry.ConfigFile.Save();
            }
        }
    }

    public struct WindowReturn
    {
        public int SelectedScrollGui;

        public WindowReturn(int scrollValue) => SelectedScrollGui = scrollValue;
    }
}