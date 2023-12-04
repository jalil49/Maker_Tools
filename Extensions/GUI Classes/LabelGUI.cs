using UnityEngine;
using static Extensions.OnGUIExtensions;
using static UnityEngine.GUILayout;

namespace Extensions.GUI_Classes
{
    public class LabelGUI
    {
        public GUILayoutOption[] LayoutOptions;
        public GUIStyle Style;
        public string Text = "Default Text";

        public LabelGUI()
        {
            Style = LabelStyle;
        }

        public void Draw()
        {
            BeginVertical();
            Label(Text, Style, LayoutOptions);
            EndVertical();
        }
    }
}