using Extensions.GUI_Classes;
using KKAPI.Studio.UI;
using UnityEngine;

namespace Additional_Card_Info.Controls
{
    public class SlotDataControls
    {
        public ToggleGUI<KeepState> Keep;
        public ToolbarGUI KeepState;
        public ToggleGUI<ShowHIde> HideAccessoryButton;
        public ToggleGUI<Always> AlwaysVisibleAccessory;

        public SlotDataControls(SlotData slotData)
        {
            Keep = new ToggleGUI<KeepState>(slotData.keep, "");
            KeepState = new ToolbarGUI((int)slotData.keepState,KeepsExtension.KeepsGUIContents(), (i, i1) => slotData.keepState = (KeepState)i1);
        }
        public static class KeepsExtension
        {
            public static GUIContent[] KeepsGUIContents()
            {
                return new[]
                {
                    new GUIContent("Don't Keep", "Don't keep accessory when overriding outfit"),
                    new GUIContent("Accessory Keep", "Accessory should be kept when overriding outfit"),
                    new GUIContent("Hair Keep", "Accessory should be kept and be treated as hair")
                };
            }
        }
    }
    
    public enum KeepState
    {
        DontKeep = -1,
        NonHairKeep = 0,
        HairKeep = 1
    }
}