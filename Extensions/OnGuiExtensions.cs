using UnityEngine;

namespace Extensions
{
    public static class OnGUIExtensions
    {
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
            if (LabelStyle == null)
            {

                LabelStyle = new GUIStyle(GUI.skin.label)
                {
                    wordWrap = true
                };
                ButtonStyle = new GUIStyle(GUI.skin.button);
                TextFieldStyle = new GUIStyle(GUI.skin.textField);
                TextAreaStyle = new GUIStyle(GUI.skin.textArea);
                ToggleStyle = new GUIStyle(GUI.skin.toggle);
                SliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
                SliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
                WindowStyle = new GUIStyle(GUI.skin.window);

                ButtonStyle.hover.textColor = Color.red;
                ButtonStyle.onNormal.textColor = Color.red;

                SetFontSize(Screen.height / 108);
            }
        }

        /// <summary>
        /// Creates a toggle with default style
        /// </summary>
        /// <returns>value of GUI element</returns>
        public static bool Toggle(bool value, string text, string tooltip, bool expandwidth = true)
        {
            return GUILayout.Toggle(value, new GUIContent(text, tooltip), ToggleStyle, GUILayout.ExpandWidth(expandwidth));
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
        public static int HorizontalSlider(int value, int start, int stop, bool expandwidth = true)
        {
            return Mathf.RoundToInt(GUILayout.HorizontalSlider(value, start, stop, SliderStyle, SliderThumbStyle, GUILayout.ExpandWidth(expandwidth)));
        }

        /// <summary>
        /// Creates a Label with default style
        /// </summary>
        /// <returns>value of GUI element</returns>
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
            SetFontSize(LabelStyle.fontSize + 1);
        }
        public static void DecrementFontSize()
        {
            SetFontSize(System.Math.Max(0, LabelStyle.fontSize - 1));
        }

        private static void DarkenPixels(Texture2D texture)
        {
            var width = texture.width;
            var height = texture.height;
            var colorVal = 0.4f;
            var reference = new Color(colorVal, colorVal, colorVal, 0);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var pixel = texture.GetPixel(x, y);
                    pixel -= reference;
                    texture.SetPixel(x, y, pixel);
                }
            }
            texture.Apply();
        }
    }
}
