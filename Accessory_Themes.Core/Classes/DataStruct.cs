using Extensions;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Accessory_Themes
{
    public class DataStruct
    {
        public Dictionary<int, CoordinateData> Coordinate = new Dictionary<int, CoordinateData>();

        public CoordinateData NowCoordinate = new CoordinateData();

        public int MaxKey => Coordinate.Keys.Max();

        public void CleanUp()
        {
            foreach (var item in Coordinate)
            {
                item.Value.CleanUp();
            }
        }

        public void Clearoutfit(int key)
        {
            if (!Coordinate.ContainsKey(key))
                Coordinate[key] = new CoordinateData();
            Coordinate[key].Clear();
        }

        public void Createoutfit(int key)
        {
            if (!Coordinate.ContainsKey(key))
                Coordinate[key] = new CoordinateData();
        }

        public void Moveoutfit(int dest, int src)
        {
            Coordinate[dest] = new CoordinateData(Coordinate[src]);
        }

        public void Removeoutfit(int key)
        {
            Coordinate.Remove(key);
        }
    }

    [Serializable]
    [MessagePackObject]
    public class CoordinateData
    {
        [Key("_themes")]
        public List<ThemeData> Themes;

        [IgnoreMember]
        public Dictionary<int, int> Theme_Dict;

        [IgnoreMember]
        public Dictionary<int, List<int[]>> Relative_ACC_Dictionary;

        [IgnoreMember]
        public Stack<Queue<Color>> UndoACCSkew;

        [IgnoreMember]
        public Stack<Queue<Color>> ClothsUndoSkew;

        public CoordinateData() { NullCheck(); }

        public CoordinateData(List<ThemeData> _themes)
        {
            Themes = _themes.ToNewList();
            NullCheck();
        }

        public CoordinateData(CoordinateData _copy) => CopyData(_copy);

        public void CopyData(CoordinateData _copy) => CopyData(_copy.Themes, _copy.Relative_ACC_Dictionary);

        public void CopyData(List<ThemeData> _themes, Dictionary<int, List<int[]>> _relativedict)
        {
            Themes = _themes.ToNewList();
            Relative_ACC_Dictionary = _relativedict.ToNewDictionary();
            NullCheck();
        }

        public void CleanUp()
        {
            Themes.RemoveAll(x => x.ThemedSlots.Count == 0);
        }

        public void Clear()
        {
            Themes.Clear();
            Relative_ACC_Dictionary.Clear();
            Theme_Dict.Clear();
            UndoACCSkew.Clear();
            ClothsUndoSkew.Clear();
        }

        private void NullCheck()
        {
            if (Themes == null) Themes = new List<ThemeData>();
            if (Theme_Dict == null) Theme_Dict = new Dictionary<int, int>();
            if (Relative_ACC_Dictionary == null) Relative_ACC_Dictionary = new Dictionary<int, List<int[]>>();
            if (UndoACCSkew == null) UndoACCSkew = new Stack<Queue<Color>>();
            if (ClothsUndoSkew == null) ClothsUndoSkew = new Stack<Queue<Color>>();
        }
    }

    [Serializable]
    [MessagePackObject]
    public class ThemeData
    {
        [Key("_themename")]
        public string ThemeName { get; set; }

        [Key("_isrelative")]
        public bool Isrelative { get; set; }

        [Key("_colors")]
        public Color[] Colors { get; set; }

        [Key("_slots")]
        public List<int> ThemedSlots { get; set; }

        public ThemeData(string _themename)
        {
            ThemeName = _themename;
            NullCheck();
        }

        public ThemeData(ThemeData _copy, bool partial)
        {
            ThemeName = _copy.ThemeName;
            Isrelative = _copy.Isrelative;
            Colors = _copy.Colors.ToNewArray(4);
            if (!partial) ThemedSlots = _copy.ThemedSlots.ToNewList();
            NullCheck();
        }

        public ThemeData(string _themename, Color[] _colors)
        {
            ThemeName = _themename;
            Colors = _colors.ToNewArray(4);
            NullCheck();
        }

        public ThemeData(string _themename, bool _isrelative, Color[] _colors, List<int> _slots) => CopyData(_themename, _isrelative, _colors, _slots);

        public ThemeData(ThemeData _copy) => CopyData(_copy.ThemeName, _copy.Isrelative, _copy.Colors, _copy.ThemedSlots);

        public void CopyData(string _themename, bool _isrelative, Color[] _colors, List<int> _slots)
        {
            ThemeName = _themename;
            Isrelative = _isrelative;
            ThemedSlots = _slots.ToNewList();
            Colors = _colors.ToNewArray(4);
            NullCheck();
        }

        private void NullCheck()
        {
            if (ThemeName == null) ThemeName = "";
            if (Colors == null) Colors = new Color[4];
            for (int i = 0; i < Colors.Length; i++)
            {
                if (Colors[i] == null)
                {
                    Colors[i] = new Color();
                }
            }
            if (ThemedSlots == null) ThemedSlots = new List<int>();
        }
    }
}
