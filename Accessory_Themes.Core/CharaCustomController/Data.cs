using System.Collections.Generic;
using KKAPI.Chara;
using UnityEngine;

namespace Accessory_Themes
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private Additional_Card_Info.CharaEvent _aciRef;
        private DataStruct _data = new DataStruct();

        private List<int> _hairAcc = new List<int>();

        #region Properties

        private Dictionary<int, CoordinateData> Coordinate
        {
            get => _data.Coordinate;
            set => _data.Coordinate = value;
        }

        private CoordinateData NowCoordinate
        {
            get => _data.NowCoordinate;
            set => _data.NowCoordinate = value;
        }

        private List<ThemeData> Themes
        {
            get => NowCoordinate.themes;
            set => NowCoordinate.themes = value;
        }

        private Dictionary<int, int> ThemeDict
        {
            get => NowCoordinate.ThemeDict;
            set => NowCoordinate.ThemeDict = value;
        }

        private Dictionary<int, List<int[]>> RelativeAccDictionary
        {
            get => NowCoordinate.RelativeAccDictionary;
            set => NowCoordinate.RelativeAccDictionary = value;
        }

        private Stack<Queue<Color>> UndoAccSkew
        {
            get => NowCoordinate.UndoAccSkew;
            set => NowCoordinate.UndoAccSkew = value;
        }

        private Stack<Queue<Color>> ClothsUndoSkew
        {
            get => NowCoordinate.ClothsUndoSkew;
            set => NowCoordinate.ClothsUndoSkew = value;
        }

        #endregion
    }
}