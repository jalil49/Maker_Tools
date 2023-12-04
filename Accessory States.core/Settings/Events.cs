using System;

namespace Accessory_States
{
    public sealed class CoordinateLoadedEventArg : EventArgs
    {
        //TODO: Remove Event with Get GameEvent Object.
        public CoordinateLoadedEventArg(ChaControl character /*, ChaFileCoordinate _Coordinate*/)
        {
            //Coordinate = _Coordinate;
            Character = character;
        }

        public ChaControl Character { get; }
        //public ChaFileCoordinate Coordinate { get; }
    }
}