using UnityEngine;
using static KKAPI.Utilities.IMGUIUtils;

namespace Extensions
{
    public static class OnGUIExtensions
    {
        public static int FontSize { get; set; } = 16;
        public static GUIStyle LabelStyle { get; set; }
        public static GUIStyle ButtonStyle { get; set; }
        public static GUIStyle TextFieldStyle { get; set; }
        public static GUIStyle TextAreaStyle { get; set; }
        public static GUIStyle ToggleStyle { get; set; }
        public static GUIStyle SliderStyle { get; set; }
        public static GUIStyle SliderThumbStyle { get; set; }

        public static GUIStyle WindowStyle { get; set; }

        /// <summary>
        /// Initialize all files and set a default font size
        /// </summary>
        public static void InitializeStyles()
        {
            if(LabelStyle == null)
            {
                LabelStyle = new GUIStyle(SolidBackgroundGuiSkin.label)
                {
                    wordWrap = true
                };
                ButtonStyle = new GUIStyle(SolidBackgroundGuiSkin.button);
                TextFieldStyle = new GUIStyle(SolidBackgroundGuiSkin.textField);
                TextAreaStyle = new GUIStyle(SolidBackgroundGuiSkin.textArea);
                ToggleStyle = new GUIStyle(SolidBackgroundGuiSkin.toggle);
                SliderStyle = new GUIStyle(SolidBackgroundGuiSkin.horizontalSlider);
                SliderThumbStyle = new GUIStyle(SolidBackgroundGuiSkin.horizontalSliderThumb);
                WindowStyle = new GUIStyle(SolidBackgroundGuiSkin.window);
                ButtonStyle.hover.textColor = Color.red;
                ButtonStyle.onNormal.textColor = Color.red;

                SetFontSize(FontSize);
            }
        }

        /// <summary>
        /// Creates a toggle with default style
        /// </summary>
        /// <returns>value of GUI element</returns>
        public static bool Toggle(bool value, string text, string tooltip, bool expandwidth = true)
        {
            GUILayout.BeginHorizontal();
            {
                if(Button(value ? "Enabled" : "Disabled", expandwidth: false))
                {
                    value = !value;
                }

                Label(text, tooltip, expandwidth);
            }

            GUILayout.EndHorizontal();
            return value;
        }

        /// <summary>
        /// Creates a Button with default style
        /// </summary>
        /// <returns>value of GUI element</returns>
        public static bool Button(string text, string tooltip = " ", bool expandwidth = true)
        {
            return GUILayout.Button(new GUIContent(text, tooltip), ButtonStyle, GUILayout.ExpandWidth(expandwidth));
        }

        /// <summary>
        /// Creates a TextField with default style
        /// </summary>
        /// <returns>value of GUI element</returns>
        public static string TextField(string text, bool expandwidth = true)
        {
            return GUILayout.TextField(text, TextFieldStyle, GUILayout.ExpandWidth(expandwidth), GUILayout.MinWidth(10));
        }

        /// <summary>
        /// Creates a HorizontalSlider with default style
        /// </summary>
        /// <returns>value of GUI element</returns>
        public static int HorizontalSlider(int value, int start, int stop, bool expandwidth = true, int minWidth = 100)
        {
            return Mathf.RoundToInt(GUILayout.HorizontalSlider(value, start, stop, SliderStyle, SliderThumbStyle, GUILayout.ExpandWidth(expandwidth), GUILayout.MinWidth(minWidth)));
        }

        /// <summary>
        /// Creates a HorizontalSlider with default style
        /// </summary>
        /// <returns>value of GUI element</returns>
        public static float HorizontalSlider(float value, float start, float stop, bool expandwidth = true, int minWidth = 100)
        {
            return GUILayout.HorizontalSlider(value, start, stop, SliderStyle, SliderThumbStyle, GUILayout.ExpandWidth(expandwidth), GUILayout.MinWidth(minWidth));
        }

        /// <summary>
        /// Creates a Label with default style
        /// </summary>
        /// <returns>value of GUI element</returns>
        /// 
        public static void Label(string text, string tooltip = "", bool expandwidth = true)
        {
            GUILayout.Label(new GUIContent(text, tooltip), LabelStyle, GUILayout.ExpandWidth(expandwidth));
        }

        /// <summary>
        /// Sets default font size
        /// </summary>
        public static void SetFontSize(int size)
        {
            LabelStyle.fontSize = size;
            ButtonStyle.fontSize = size;
            TextFieldStyle.fontSize = size;
            ToggleStyle.fontSize = size;
            SliderStyle.fontSize = size;
            SliderThumbStyle.fontSize = size;
            WindowStyle.fontSize = size;
            TextAreaStyle.fontSize = size;
        }

        public static void IncrementFontSize()
        {
            SetFontSize(FontSize + 1);
        }
        public static void DecrementFontSize()
        {
            SetFontSize(System.Math.Max(1, FontSize - 1));
        }
    }
}
