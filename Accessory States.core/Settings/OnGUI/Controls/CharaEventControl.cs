using Accessory_States.Classes.PresetStorage;
using System.Collections.Generic;

namespace Accessory_States.OnGUI
{
    public class CharaEventControl
    {
        public CharaEvent CharaEvent;
        public readonly Dictionary<NameData, NameDataControl> NameControls = new Dictionary<NameData, NameDataControl>();
        public readonly Dictionary<StateInfo, StateInfoControl> StateControls = new Dictionary<StateInfo, StateInfoControl>();
        public readonly Dictionary<PresetData, PresetContol> SingleControls = new Dictionary<PresetData, PresetContol>();
        public readonly Dictionary<PresetFolder, PresetFolderContol> PresetFolderControls = new Dictionary<PresetFolder, PresetFolderContol>();
        public int SelectedSlot;
        public int SelectedDropDown;
        public CharaEventControl(CharaEvent charaEvent)
        {
            CharaEvent = charaEvent;
        }
    }
}
