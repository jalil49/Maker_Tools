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
        public List<ThemeData> Themes = new List<ThemeData>();

        [IgnoreMember]
        public Dictionary<int, int> Theme_Dict = new Dictionary<int, int>();

        [IgnoreMember]
        public Dictionary<int, List<int[]>> Relative_ACC_Dictionary = new Dictionary<int, List<int[]>>();

        [IgnoreMember]
        public Stack<Queue<Color>> UndoACCSkew = new Stack<Queue<Color>>();

        [IgnoreMember]
        public Stack<Queue<Color>> ClothsUndoSkew = new Stack<Queue<Color>>();

        public CoordinateData() { }

        public CoordinateData(List<ThemeData> _themes)
        {
            Themes = _themes;
        }

        public CoordinateData(CoordinateData _copy) => CopyData(_copy.Themes, _copy.Relative_ACC_Dictionary);

        public void CopyData(CoordinateData _copy) => CopyData(_copy.Themes, _copy.Relative_ACC_Dictionary);

        public void CopyData(List<ThemeData> _themes, Dictionary<int, List<int[]>> _relativedict)
        {
            Themes = _themes;
            Relative_ACC_Dictionary = _relativedict;
            NullCheck();
        }

        public void CleanUp()
        {
            NullCheck();
            Themes.RemoveAll(x => x.ThemedSlots.Count == 0);
        }

        public void Clear()
        {
            NullCheck();
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
        public bool Isrelative { get; set; } = false;

        [Key("_colors")]
        public Color[] Colors { get; set; } = new Color[] { new Color(), new Color(), new Color(), new Color() };

        [Key("_slots")]
        public List<int> ThemedSlots { get; set; } = new List<int>();

        public ThemeData(string _themename)
        {
            ThemeName = _themename;
            NullCheck();
        }
        public ThemeData(ThemeData _copy, bool partial)
        {
            ThemeName = _copy.ThemeName;
            Isrelative = _copy.Isrelative;
            var _colors = _copy.Colors;
            for (int i = 0; i < _colors.Length; i++)
            {
                var color = _colors[i];
                Colors[i] = new Color(color.r, color.g, color.b, color.a);
            }
            if (partial)
            {
                return;
            }
            ThemedSlots = new List<int>(_copy.ThemedSlots);
            NullCheck();
        }

        public ThemeData(string _themename, Color[] _colors)
        {
            ThemeName = _themename;
            for (int i = 0; i < _colors.Length; i++)
            {
                var color = _colors[i];
                Colors[i] = new Color(color.r, color.g, color.b, color.a);
            }
            NullCheck();
        }

        public ThemeData(string _themename, bool _isrelative, Color[] _colors, List<int> _slots)
        {
            ThemeName = _themename;
            Isrelative = _isrelative;
            ThemedSlots = _slots;
            for (int i = 0; i < _colors.Length; i++)
            {
                var color = _colors[i];
                Colors[i] = new Color(color.r, color.g, color.b, color.a);
            }
            NullCheck();
        }

        private void NullCheck()
        {
            if (Colors == null) Colors = new Color[] { new Color(), new Color(), new Color(), new Color() };
            if (ThemedSlots == null) ThemedSlots = new List<int>();
        }
    }
}
