using MessagePack;
using System;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject(true)]
    public class StateInfo
    {
        //unused saved directly on slot
        public int Slot { get; set; }

        //No ASS equiv
        public byte ShoeType { get; set; }

        public int Priority { get; set; }

        // use ASS custom keys
        // seems to be unused with NameData reference focus
        /// <summary>
        /// Should be recalculated on load in case accessories are programatically moved
        /// </summary>
        [Key("RefKind")]
        public int Binding { get; set; }

        [Key("RefState")]
        public int State { get; set; }

        [Key("Visible")]
        public bool Show { get; set; }

        //Use to assume original ShoeType if inner or outer if reimplemented
        public byte? ConvertedShoeType { get; set; }

        public StateInfo()
        {
            Slot = 0;
            Binding = -1;
            State = -1;
            Priority = 0;
            ShoeType = 2;
            Show = true;
        }
    }
}
