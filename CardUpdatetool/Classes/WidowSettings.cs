using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using BepInEx.Configuration;

namespace CardUpdateTool
{
    internal static class Window
    {
        static public ConfigEntry<int> FontSize;
        static public ConfigEntry<int> X;
        static public ConfigEntry<int> Y;
        static public ConfigEntry<int> Width;
        static public ConfigEntry<int> Height;

        public static class Defaults
        {
            public static int fontSize = Screen.height / 108;
            public static int x = (int)(Screen.width * 0.33f);
            public static int y = (int)(Screen.height * 0.09f);
            public static int width = (int)(Screen.width * 0.225);
            public static int height = (int)(int)(Screen.height * 0.273);
        }
    }
}
