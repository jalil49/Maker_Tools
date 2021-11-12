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
}
