using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Accessory_States
{
    public sealed class ChangeCoordinateTypeARG : EventArgs
    {
        public ChangeCoordinateTypeARG(ChaControl _Character)
        {
            Character = _Character;
        }
        public ChaControl Character { get; }
    }
    public sealed class CoordinateLoadedEventARG : EventArgs
    {
        public CoordinateLoadedEventARG(ChaControl _Character, ChaFileCoordinate _Coordinate)
        {
            Coordinate = _Coordinate;
            Character = _Character;
        }
        public ChaControl Character { get; }
        public ChaFileCoordinate Coordinate { get; }
    }

    public sealed class ClothChangeEventArgs : EventArgs
    {
        public ClothChangeEventArgs(ChaControl _Character, int _clothesKind, byte _state, bool _next)
        {
            Character = _Character;
            clothesKind = _clothesKind;
            state = _state;
            next = _next;
        }
        public ChaControl Character { get; }
        public int clothesKind { get; }
        public byte state;
        public bool next;
    }

    public sealed class OnClickCoordinateChange : EventArgs
    {
        public OnClickCoordinateChange(int _female, int _coordinate)
        {
            Female = _female;
            Coordinate = _coordinate;
        }
        public int Coordinate { get; }
        public int Female { get; }
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

}
