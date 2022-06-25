using MessagePack;
using System;
using System.Runtime.Serialization;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject(true)]
    public class StateInfo
    {
        public int Slot { get; set; }
        public byte ShoeType { get; set; }

        public int Priority { get; set; }

        // use ASS custom keys
        /// <summary>
        /// Should be recalculated on load
        /// currently serialized to be -1 for accessories groups
        /// </summary>
        [Key("RefKind")]

        public int Binding { get; set; }

        [Key("RefState")]
        public int State { get; set; }

        [Key("Visible")]
        public bool Show { get; set; }

        public StateInfo()
        {
            Slot = 0;
            Binding = -1;
            State = -1;
            Priority = 0;
            ShoeType = 2;
            Show = true;
        }

        [OnSerializing()]
        internal void OnSerializingMethod(StreamingContext context)
        {
            if (Binding >= Constants.ClothingLength)
                Binding = -1;
        }
    }
}
