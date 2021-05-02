using System;

namespace Accessory_Shortcuts
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
        public Slot_ACC_Change_ARG(ChaControl _Character, int _slotNo, int _Type, int _id, string _parentKey)
        {
            Character = _Character;
            SlotNo = _slotNo;
            Type = _Type;
            Id = _id;
            ParentKey = _parentKey;
        }
        public ChaControl Character { get; }
        public int SlotNo { get; }
        public int Type { get; }
        public string ParentKey { get; }
        public int Id { get; }
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
}
