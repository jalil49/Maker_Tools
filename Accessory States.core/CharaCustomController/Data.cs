using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using MessagePack;
using MoreAccessoriesKOI;
using System;
using System.Collections.Generic;
using System.Linq;
using ToolBox;
using UniRx;
using UnityEngine;

namespace Accessory_States
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        internal static bool ASS_Exists;

        public Data ThisCharactersData;
        private ChaFile chafile;

        public static event EventHandler<CoordinateLoadedEventARG> Coordloaded;

        internal List<ChaFileAccessory.PartsInfo> Accessorys_Parts = new List<ChaFileAccessory.PartsInfo>();

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

        private Dictionary<int, bool> Parented
        {
            get { return NowCoordinate.Parented; }
            set { NowCoordinate.Parented = value; }
        }

        #endregion
    }
}
