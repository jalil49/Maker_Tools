using System.Collections.Generic;
using Accessory_States.Classes.PresetStorage;

namespace Accessory_States.OnGUI
{
    public class CharaEventControl
    {
        public readonly CharaEvent CharaEvent;

        public readonly Dictionary<NameData, NameDataControl>
            NameControls = new Dictionary<NameData, NameDataControl>();

        public readonly Dictionary<PresetFolder, PresetFolderControl> PresetFolderControls =
            new Dictionary<PresetFolder, PresetFolderControl>();

        public readonly Dictionary<PresetData, PresetContol>
            SingleControls = new Dictionary<PresetData, PresetContol>();

        public readonly Dictionary<StateInfo, StateInfoControl> StateControls =
            new Dictionary<StateInfo, StateInfoControl>();

        public int SelectedDropDown;
        public int SelectedSlot;

        public CharaEventControl(CharaEvent charaEvent) => CharaEvent = charaEvent;
    }
}