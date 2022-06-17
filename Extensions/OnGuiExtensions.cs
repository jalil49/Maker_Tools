using UnityEngine;

namespace GUIHelper
{
    public static class OnGuiExtensions
    {
        public static GUIStyle LabelStyle { get; set; }
        public static GUIStyle ButtonStyle { get; set; }
        public static GUIStyle FieldStyle { get; set; }
        public static GUIStyle ToggleStyle { get; set; }
        public static GUIStyle SliderStyle { get; set; }
        public static GUIStyle SliderThumbStyle { get; set; }

        /// <summary>
        /// Initialize all files and set a default font size
        /// </summary>
        public static void InitializeStyles()
        {
            if (LabelStyle == null)
            {
                LabelStyle = new GUIStyle(GUI.skin.label);
                ButtonStyle = new GUIStyle(GUI.skin.button);
                FieldStyle = new GUIStyle(GUI.skin.textField);
                ToggleStyle = new GUIStyle(GUI.skin.toggle);
                SliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
                SliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
                ButtonStyle.hover.textColor = Color.red;
                ButtonStyle.onNormal.textColor = Color.red;

                SetFontSize(Screen.height / 108);
            }
        }

        /// <summary>
        /// Creates a toggle with default style
        /// </summary>
        /// <returns>value of GUI element</returns>
        public static bool Toggle(bool value, string text, bool expandwidth = true)
        {
            return GUILayout.Toggle(value, text, ToggleStyle, GUILayout.ExpandWidth(expandwidth));
        }

        /// <summary>
        /// Creates a Button with default style
        /// </summary>
        /// <returns>value of GUI element</returns>
        public static bool Button(string text, bool expandwidth = true)
        {
            return GUILayout.Button(text, ButtonStyle, GUILayout.ExpandWidth(expandwidth));
        }

        /// <summary>
        /// Creates a TextField with default style
        /// </summary>
        /// <returns>value of GUI element</returns>
        public static string TextField(string text, bool expandwidth = true)
        {
            return GUILayout.TextField(text, FieldStyle, GUILayout.ExpandWidth(expandwidth));
        }

        /// <summary>
        /// Creates a HorizontalSlider with default style
        /// </summary>
        /// <returns>value of GUI element</returns>
        public static int HorizontalSlider(int value, int start, int stop, bool expandwidth = true)
        {
            return Mathf.RoundToInt(GUILayout.HorizontalSlider(value, start, stop, SliderStyle, SliderThumbStyle, GUILayout.ExpandWidth(expandwidth)));
        }

        /// <summary>
        /// Creates a Label with default style
        /// </summary>
        /// <returns>value of GUI element</returns>
        public static void Label(string text, bool expandwidth = true)
        {
            GUILayout.Label(text, LabelStyle, GUILayout.ExpandWidth(expandwidth));
        }

        /// <summary>
        /// Sets default font size
        /// </summary>
        public static void SetFontSize(int size)
        {
            LabelStyle.fontSize = size;
            ButtonStyle.fontSize = size;
            FieldStyle.fontSize = size;
            ToggleStyle.fontSize = size;
            SliderStyle.fontSize = size;
            SliderThumbStyle.fontSize = size;
        }
    }
}
