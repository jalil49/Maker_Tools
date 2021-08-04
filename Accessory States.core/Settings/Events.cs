using System;

namespace Accessory_States
{
    public sealed class CoordinateLoadedEventARG : EventArgs
    {
        public CoordinateLoadedEventARG(ChaControl _Character/*, ChaFileCoordinate _Coordinate*/)
        {
            //Coordinate = _Coordinate;
            Character = _Character;
        }
        public ChaControl Character { get; }
        //public ChaFileCoordinate Coordinate { get; }
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
}
