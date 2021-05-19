using System;
using System.Collections.Generic;

namespace Accessory_Themes
{
    public sealed class Slot_ACC_Change_ARG : EventArgs
    {
        public Slot_ACC_Change_ARG(ChaControl _Character, int _slotNo, int _Type)
        {
            Character = _Character;
            SlotNo = _slotNo;
            Type = _Type;
        }
        public ChaControl Character { get; }
        public int SlotNo { get; }
        public int Type { get; }
    }
    internal sealed class MovUrAcc_Event : EventArgs
    {
        internal MovUrAcc_Event(List<QueueItem> Queue)
        {
            this.Queue = Queue;
        }
        public List<QueueItem> Queue { get; }
    }
}
