using KKAPI.Chara;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accessory_States
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        internal static bool ASS_Exists;

        private ChaFile chafile;

        public static event EventHandler<CoordinateLoadedEventARG> Coordloaded;

        private byte lastknownshoetype = 0;

        private bool ShowSub = true;

        internal static List<SaveData.Heroine> FreeHHeroines = new List<SaveData.Heroine>();

        public Dictionary<int, CoordinateData> Coordinate = new Dictionary<int, CoordinateData>();

        public CoordinateData NowCoordinate = new CoordinateData();

        public Dictionary<string, List<KeyValuePair<int, Slotdata>>> Now_Parented_Name_Dictionary = new Dictionary<string, List<KeyValuePair<int, Slotdata>>>();

        public void Update_Now_Coordinate()
        {
            var outfitnum = (int)CurrentCoordinate.Value;
            if (!Coordinate.TryGetValue(outfitnum, out var coordinateData))
            {
                coordinateData = new CoordinateData();
                Coordinate[outfitnum] = coordinateData;
            }
            if (KKAPI.Maker.MakerAPI.InsideMaker)
            {
                NowCoordinate = coordinateData;
            }
            else
            {
                NowCoordinate = new CoordinateData(coordinateData);
            }
            Update_Parented_Name();
        }

        public void Update_Now_Coordinate(int outfitnum)
        {
            NowCoordinate = new CoordinateData(Coordinate[outfitnum]);
            Update_Parented_Name();
        }

        public void Update_Parented_Name()
        {
            Now_Parented_Name_Dictionary.Clear();
            var shoetype = ChaControl.fileStatus.shoesType;
            var ParentedList = NowCoordinate.Slotinfo.Where(x => x.Value.Parented && (x.Value.Shoetype == 2 || x.Value.Shoetype == shoetype));
#if !KKS
            Update_More_Accessories();
#endif
            foreach (var item in ParentedList)
            {
                var parentkey = Accessorys_Parts[item.Key].parentKey;
                if (!Now_Parented_Name_Dictionary.TryGetValue(parentkey, out var list))
                {
                    list = new List<KeyValuePair<int, Slotdata>>();
                    Now_Parented_Name_Dictionary[parentkey] = list;
                }
                list.Add(item);
            }
        }

        public void Clear_Now_Coordinate()
        {
            NowCoordinate.Clear();
            Now_Parented_Name_Dictionary.Clear();
        }

        public void Clear()
        {
            for (int i = 0, n = Coordinate.Count; i < n; i++)
            {
                Coordinate[i].Clear();
            }
            Now_Parented_Name_Dictionary.Clear();
        }

        public void Clearoutfit(int key)
        {
            Coordinate[key].Clear();
        }

        public void Createoutfit(int key)
        {
            if (!Coordinate.ContainsKey(key))
            {
                Coordinate[key] = new CoordinateData();
            }
        }

        public void Moveoutfit(int dest, int src)
        {
            Coordinate[dest] = new CoordinateData(Coordinate[src]);
        }

        public void Removeoutfit(int key)
        {
            Coordinate.Remove(key);
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
