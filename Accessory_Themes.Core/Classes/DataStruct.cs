using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using MessagePack;
using UnityEngine;
using UnityEngine.Serialization;

namespace Accessory_Themes
{
    public class DataStruct
    {
        public Dictionary<int, CoordinateData> Coordinate = new Dictionary<int, CoordinateData>();

        public CoordinateData NowCoordinate = new CoordinateData();

        public int MaxKey => Coordinate.Keys.Max();

        public void CleanUp()
        {
            foreach (var item in Coordinate) item.Value.CleanUp();
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
        [FormerlySerializedAs("Themes")] [Key("_themes")]
        public List<ThemeData> themes;

        [IgnoreMember] public Stack<Queue<Color>> ClothsUndoSkew;

        [IgnoreMember] public Dictionary<int, List<int[]>> RelativeAccDictionary;

        [IgnoreMember] public Dictionary<int, int> ThemeDict;

        [IgnoreMember] public Stack<Queue<Color>> UndoAccSkew;

        public CoordinateData()
        {
            NullCheck();
        }

        public CoordinateData(List<ThemeData> copyThemes)
        {
            themes = copyThemes.ToNewList();
            NullCheck();
        }

        public CoordinateData(CoordinateData copy)=> CopyData(copy);
        

        public void CopyData(CoordinateData copy)=> CopyData(copy.themes, copy.RelativeAccDictionary);
        

        public void CopyData(List<ThemeData> copyThemes, Dictionary<int, List<int[]>> relativeDictionary)
        {
            themes = copyThemes.ToNewList();
            RelativeAccDictionary = relativeDictionary.ToNewDictionary();
            NullCheck();
        }

        public void CleanUp()
        {
            themes.RemoveAll(x => x.ThemedSlots.Count == 0);
        }

        public void Clear()
        {
            themes.Clear();
            RelativeAccDictionary.Clear();
            ThemeDict.Clear();
            UndoAccSkew.Clear();
            ClothsUndoSkew.Clear();
        }

        private void NullCheck()
        {
            if (themes == null) themes = new List<ThemeData>();
            if (ThemeDict == null) ThemeDict = new Dictionary<int, int>();
            if (RelativeAccDictionary == null) RelativeAccDictionary = new Dictionary<int, List<int[]>>();
            if (UndoAccSkew == null) UndoAccSkew = new Stack<Queue<Color>>();
            if (ClothsUndoSkew == null) ClothsUndoSkew = new Stack<Queue<Color>>();
        }
    }

    [Serializable]
    [MessagePackObject]
    public class ThemeData
    {
        public ThemeData(string themeName)
        {
            ThemeName = themeName;
            NullCheck();
        }

        public ThemeData(ThemeData copy, bool partial)
        {
            ThemeName = copy.ThemeName;
            IsRelative = copy.IsRelative;
            Colors = copy.Colors.ToNewArray(4);
            if (!partial) ThemedSlots = copy.ThemedSlots.ToNewList();
            NullCheck();
        }

        public ThemeData(string themeName, Color[] colors)
        {
            ThemeName = themeName;
            Colors = colors.ToNewArray(4);
            NullCheck();
        }

        public ThemeData(string themeName, bool isRelative, Color[] colors, List<int> slots)
        {
            CopyData(themeName, isRelative, colors, slots);
        }

        public ThemeData(ThemeData copy)
        {
            CopyData(copy.ThemeName, copy.IsRelative, copy.Colors, copy.ThemedSlots);
        }

        // ReSharper disable once StringLiteralTypo
        [Key("_themename")] public string ThemeName { get; set; }

        // ReSharper disable once StringLiteralTypo
        [Key("_isrelative")] public bool IsRelative { get; set; }

        [Key("_colors")] public Color[] Colors { get; set; }

        [Key("_slots")] public List<int> ThemedSlots { get; set; }

        public void CopyData(string themeName, bool isRelative, Color[] colors, List<int> slots)
        {
            ThemeName = themeName;
            IsRelative = isRelative;
            ThemedSlots = slots.ToNewList();
            Colors = colors.ToNewArray(4);
            NullCheck();
        }

        private void NullCheck()
        {
            if (ThemeName == null) ThemeName = "";
            if (Colors == null) Colors = new Color[4];
            if (ThemedSlots == null) ThemedSlots = new List<int>();
        }
    }
}