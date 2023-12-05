using System;

namespace Accessory_States
{
    public sealed class CoordinateLoadedEventArg : EventArgs
    {
        public CoordinateLoadedEventArg(ChaControl character /*, ChaFileCoordinate _Coordinate*/)
        {
            //Coordinate = _Coordinate;
            Character = character;
        }

        public ChaControl Character { get; }
        //public ChaFileCoordinate Coordinate { get; }
    }

    public sealed class OnClickCoordinateChange : EventArgs
    {
        public OnClickCoordinateChange(int female, int coordinate)
        {
            Female = female;
            Coordinate = coordinate;
        }

        public int Coordinate { get; }
        public int Female { get; }
    }
}