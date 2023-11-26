using Extensions.GUI_Classes;
using KKAPI.Studio.UI;
using UnityEngine;

namespace Additional_Card_Info.Controls
{
    public class SlotDataControls
    {
        public ToggleGUI Keep;
        public ToolbarGUI KeepState;
        public ToggleGUI HideAccessoryButton;
        public ToggleGUI AlwaysVisibleAccessory;

        public SlotDataControls(SlotData slotData)
        {
            Keep = new ToggleGUI(slotData.Keep, "");
            KeepState = new ToolbarGUI((int)slotData.KeepState,
                new[]
                {
                    new GUIContent("Don't Keep", "Don't keep accessory when overriding outfit"),
                    new GUIContent("Accessory Keep", "Accessory should be kept when overriding outfit"),
                    new GUIContent("Hair Keep", "Accessory should be kept and be treated as hair")
                }, (i, i1) => slotData.KeepState = (KeepState)i1);
        }
    }
}