using System;
using System.Collections.Generic;
using System.Linq;
using KKAPI.Chara;
using KKAPI.Maker;
using UnityEngine.Serialization;

#if KK
using Heroine = SaveData.Heroine;
#elif KKS
using SaveData;
#endif

namespace Accessory_States
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        internal static bool AssExists;

        internal static List<Heroine> FreeHHeroines = new List<Heroine>();

        [FormerlySerializedAs("NowCoordinate")]
        public CoordinateData nowCoordinate = new CoordinateData();

        public readonly Dictionary<string, List<KeyValuePair<int, SlotData>>> NowParentedNameDictionary =
            new Dictionary<string, List<KeyValuePair<int, SlotData>>>();

        private ChaFile _chaFile;

        private byte _lastKnownShoeType;

        private bool _showSub = true;

        private Dictionary<int, CoordinateData> _coordinate = new Dictionary<int, CoordinateData>();

        private ChaFileAccessory.PartsInfo[] Parts => ChaControl.nowCoordinate.accessory.parts;

        public static event EventHandler<CoordinateLoadedEventArg> CoordinateLoaded;

        public void UpdateNowCoordinate()
        {
            var coordinateValue = (int)CurrentCoordinate.Value;
            if (!_coordinate.TryGetValue(coordinateValue, out var coordinateData))
            {
                coordinateData = new CoordinateData();
                _coordinate[coordinateValue] = coordinateData;
            }

            if (MakerAPI.InsideMaker)
                nowCoordinate = coordinateData;
            else
                nowCoordinate = new CoordinateData(coordinateData);
            Update_Parented_Name();
        }

        public void UpdateNowCoordinate(int outfitNum)
        {
            nowCoordinate = new CoordinateData(_coordinate[outfitNum]);
            Update_Parented_Name();
        }

        public void Update_Parented_Name()
        {
            NowParentedNameDictionary.Clear();
            var shoeType = ChaControl.fileStatus.shoesType;
            var parentedList = nowCoordinate.SlotInfo.Where(x =>
                x.Value.parented && (x.Value.ShoeType == 2 || x.Value.ShoeType == shoeType));
            foreach (var item in parentedList)
            {
                var parentKey = Parts[item.Key].parentKey;
                if (!NowParentedNameDictionary.TryGetValue(parentKey, out var list))
                {
                    list = new List<KeyValuePair<int, SlotData>>();
                    NowParentedNameDictionary[parentKey] = list;
                }

                list.Add(item);
            }
        }

        public void Clear_Now_Coordinate()
        {
            nowCoordinate.Clear();
            NowParentedNameDictionary.Clear();
        }

        public void Clear()
        {
            for (int i = 0, n = _coordinate.Count; i < n; i++) _coordinate[i].Clear();
            NowParentedNameDictionary.Clear();
        }

        public void ClearOutfit(int key)
        {
            _coordinate[key].Clear();
        }

        public void CreateOutfit(int key)
        {
            if (!_coordinate.ContainsKey(key)) _coordinate[key] = new CoordinateData();
        }

        public void MoveOutfit(int dest, int src)
        {
            _coordinate[dest] = new CoordinateData(_coordinate[src]);
        }

        public void RemoveOutfit(int key)
        {
            _coordinate.Remove(key);
        }

        #region Properties

        //private Dictionary<int, CoordinateData> Coordinate
        //{
        //    get { return ThisCharactersData.Coordinate; }
        //    set { ThisCharactersData.Coordinate = value; }
        //}

        //private CoordinateData NowCoordinate
        //{
        //    get { return ThisCharactersData.NowCoordinate; }
        //    set { ThisCharactersData.NowCoordinate = value; }
        //}

        internal Dictionary<int, SlotData> SlotInfo
        {
            get => nowCoordinate.SlotInfo;
            set => nowCoordinate.SlotInfo = value;
        }

        internal Dictionary<int, NameData> Names
        {
            get => nowCoordinate.Names;
            set => nowCoordinate.Names = value;
        }

        internal bool[] ClothNotData
        {
            get => nowCoordinate.ClothNotData;
            set => nowCoordinate.ClothNotData = value;
        }

        private bool ForceClothDataUpdate
        {
            get => nowCoordinate.forceClothNotUpdate;
            set => nowCoordinate.forceClothNotUpdate = value;
        }

        public static bool StopMakerLoop { get; internal set; }

        #endregion
    }
}