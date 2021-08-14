using KKAPI.Chara;
using System.Collections.Generic;
using UnityEngine;

namespace Accessory_Themes
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private DataStruct data = new DataStruct();

        private Additional_Card_Info.CharaEvent ACI_Ref;

        private List<int> HairAcc = new List<int>();

        #region Properties

        private Dictionary<int, CoordinateData> Coordinate
        {
            get { return data.Coordinate; }
            set { data.Coordinate = value; }
        }

        private CoordinateData NowCoordinate
        {
            get { return data.NowCoordinate; }
            set { data.NowCoordinate = value; }
        }

        private List<ThemeData> Themes
        {
            get { return NowCoordinate.Themes; }
            set { NowCoordinate.Themes = value; }
        }

        private Dictionary<int, int> Theme_Dict
        {
            get { return NowCoordinate.Theme_Dict; }
            set { NowCoordinate.Theme_Dict = value; }
        }

        private Dictionary<int, List<int[]>> Relative_ACC_Dictionary
        {
            get { return NowCoordinate.Relative_ACC_Dictionary; }
            set { NowCoordinate.Relative_ACC_Dictionary = value; }
        }

        private Stack<Queue<Color>> UndoACCSkew
        {
            get { return NowCoordinate.UndoACCSkew; }
            set { NowCoordinate.UndoACCSkew = value; }
        }

        private Stack<Queue<Color>> ClothsUndoSkew
        {
            get { return NowCoordinate.ClothsUndoSkew; }
            set { NowCoordinate.ClothsUndoSkew = value; }
        }

        #endregion

    }
}
