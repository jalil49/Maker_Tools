using KKAPI.Chara;
using System;
using System.Collections.Generic;


namespace Accessory_States
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        internal static bool ASS_Exists;

        public Data ThisCharactersData;

        private ChaFile chafile;

        public static event EventHandler<CoordinateLoadedEventARG> Coordloaded;

        private byte lastknownshoetype = 0;

        private bool ShowSub = true;

        #region Properties

        private Dictionary<int, CoordinateData> Coordinate
        {
            get { return ThisCharactersData.Coordinate; }
            set { ThisCharactersData.Coordinate = value; }
        }

        private CoordinateData NowCoordinate
        {
            get { return ThisCharactersData.NowCoordinate; }
            set { ThisCharactersData.NowCoordinate = value; }
        }

        private Dictionary<int, Slotdata> Slotinfo
        {
            get { return NowCoordinate.Slotinfo; }
            set { NowCoordinate.Slotinfo = value; }
        }

        private Dictionary<int, NameData> Names
        {
            get { return NowCoordinate.Names; }
            set { NowCoordinate.Names = value; }
        }

        private bool[] ClothNotData
        {
            get { return NowCoordinate.ClothNotData; }
            set { NowCoordinate.ClothNotData = value; }
        }

        private bool ForceClothDataUpdate
        {
            get { return NowCoordinate.ForceClothNotUpdate; }
            set { NowCoordinate.ForceClothNotUpdate = value; }
        }

        public static bool StopMakerLoop { get; internal set; }

        #endregion
    }
}
