using System;
using System.Collections.Generic;

namespace Accessory_Parents
{
    public sealed class Acc_modifier_Event_ARG : EventArgs
    {
        public Acc_modifier_Event_ARG(ChaControl _Character, int _slotNo, int _correctNo, float _value, bool _add, int _flags)
        {
            Character = _Character;
            SlotNo = _slotNo;
            CorrectNo = _correctNo;
            Value = _value;
            Add = _add;
            Flags = _flags;
        }
        public ChaControl Character { get; }
        public int SlotNo { get; }
        public int CorrectNo { get; }
        public float Value { get; }
        public bool Add { get; }
        public int Flags { get; }
    }
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
    //public sealed class Parent_Change_ARG : EventArgs
    //{
    //    public Parent_Change_ARG(ChaControl _Character, int _slotNo, string _Parent)
    //    {
    //        Character = _Character;
    //        SlotNo = _slotNo;
    //        Parent = _Parent;
    //    }
    //    public ChaControl Character { get; }
    //    public int SlotNo { get; }
    //    public string Parent { get; }
    //}
    internal sealed class MovUrAcc_Event : EventArgs
    {
        internal MovUrAcc_Event(List<QueueItem> Queue)
        {
            this.Queue = Queue;
        }
        public List<QueueItem> Queue { get; }
    }
}
