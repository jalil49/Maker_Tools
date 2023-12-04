using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Accessory_Themes
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            var pluginData = GetExtendedData();
            if (pluginData != null)
            {
                Migrator.Migrator.StandardCharaMigrate(ChaControl, pluginData);
            }

            UpdatePluginData();
            CurrentCoordinate.Subscribe(x => { UpdatePluginData(); });
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            SetExtendedData(new PluginData() { version = Constants.MasterSaveVersion });
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            SetCoordinateExtendedData(coordinate, new PluginData() { version = Constants.MasterSaveVersion });
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            var myData = GetCoordinateExtendedData(coordinate);
            if (myData != null)
            {
                Migrator.Migrator.StandardCoordMigrator(coordinate, myData);
            }

            UpdatePluginData();
        }

        internal void UpdatePluginData()
        {
            Clear();
            for (var i = 0; i < Parts.Length; i++)
            {
                LoadSlot(i);
            }
        }

        private void ColorSetByParent(bool simple = false)
        {
            //TODO: Fix
            string[] comparison;
            if (simple)
            {
                comparison = Constants.Inclusion[SimpleParentDropdown.Value].ToArray();
            }
            else
            {
                comparison = new string[] { ParentDropdown.Options[ParentDropdown.Value] };
            }

            for (int slot = 0, n = Parts.Length; slot < n; slot++)
            {
                var partData = Parts[slot];

                if (partData.type == 120)
                {
                    continue;
                }

                SlotDataDict.DefaultIfEmpty<SlotData>(slot);
                if (!SlotDataDict.TryGetValue(slot, out var slotData))
                {
                    SlotDataDict[slot] = new SlotData();
                }

                var parentKey = partData.parentKey;
                if (comparison.Contains(parentKey))
                {
                    if (ChangeACCColor(slot, theme))
                    {
                        SlotDataDict[slot] = theme;
                    }
                }
            }
        }

        private void UpdateSliderColor(int colorNum, Color value /*, bool IsPersonal = false*/)
        {
            var themenum = ThemesDropDown_Setting.Value - 1;
            if (themenum < 0)
            {
                return;
            }

            var theme = Themes[themenum];
            theme.BaseColors[colorNum] = value;

            foreach (var item in theme.ThemedSlots)
            {
                ChangeACCColor(item, themenum);
            }
        }

        private bool ColorComparison(Color c1, Color c2)
        {
            if (float.TryParse(Tolerance, out var value))
            {
                return Math.Abs(c1.r - c2.r) <= value && Math.Abs(c1.g - c2.g) <= value &&
                       Math.Abs(c1.b - c2.b) <= value && Math.Abs(c1.a - c2.a) <= value;
            }

            return false;
        }

        private bool ColorComparison(Color[] c1, Color[] c2)
        {
            for (var i = 0; i < 4; i++)
            {
                if (!ColorComparison(c1[i], c2[i]))
                    return false;
            }

            return true;
        }

        private void FindRelativeColors()
        {
            var check = new List<Color[]>();
            var relative = new List<bool>();
            foreach (var themeData in Themes)
            {
                check.Add(themeData.BaseColors);
                relative.Add(themeData.Isrelative);
            }

            var exclude = new List<Color>();
            var excludetemp = new List<Color>();
            var input = new List<int[]>();
            Relative_ACC_Dictionary.Clear();

            for (int c1Theme = 1, n = check.Count; c1Theme < n; c1Theme++)
            {
                if (relative[c1Theme])
                {
                    continue;
                }

                for (var c1Color = 0; c1Color < 4; c1Color++)
                {
                    excludetemp.Clear();
                    excludetemp.Add(check[c1Theme][c1Color]);
                    if (exclude.Contains(check[c1Theme][c1Color]))
                    {
                        continue;
                    }

                    input.Add(new int[] { c1Theme, c1Color });
                    for (var c2Theme = c1Theme + 1; c2Theme < check.Count; c2Theme++)
                    {
                        if (relative[c2Theme])
                        {
                            continue;
                        }

                        for (var c2Color = 0; c2Color < 4; c2Color++)
                        {
                            if (ColorComparison(check[c1Theme][c1Color], check[c2Theme][c2Color]))
                            {
                                excludetemp.Add(check[c2Theme][c2Color]);
                                input.Add(new int[] { c2Theme, c2Color });
                            }
                        }
                    }

                    if (input.Count > 0)
                    {
                        Relative_ACC_Dictionary.Add(Relative_ACC_Dictionary.Count, input);
                    }

                    input = new List<int[]>();
                    exclude.AddRange(excludetemp);
                }
            }
        }

        //private void AddClothColors()
        //{
        //    var clothes = ChaControl.chaFile.coordinate[CoordinateNum].clothes.parts;
        //    for (int i = 0; i < clothes.Length; i++)
        //    {
        //        if (ThemeNames[CoordinateNum].Contains($"{(ChaFileDefine.ClothesKind)i}_Base"))
        //        {
        //            continue;
        //        }
        //        ThemeNames[CoordinateNum].Add($"{(ChaFileDefine.ClothesKind)i}_Base");
        //        colors[CoordinateNum].Add(new Color[] { clothes[i].colorInfo[0].baseColor, clothes[i].colorInfo[1].baseColor, clothes[i].colorInfo[2].baseColor, clothes[i].colorInfo[3].baseColor });
        //        ThemeNames[CoordinateNum].Add($"{(ChaFileDefine.ClothesKind)i}_Pattern");
        //        colors[CoordinateNum].Add(new Color[] { clothes[i].colorInfo[0].patternColor, clothes[i].colorInfo[1].patternColor, clothes[i].colorInfo[2].patternColor, clothes[i].colorInfo[3].patternColor });
        //    }
        //    Update_ACC_Dropbox();
        //}

        private void RelativeAssignColors(Color input)
        {
            if (!MakerAPI.InsideAndLoaded)
            {
                return;
            }

            var list = Relative_ACC_Dictionary[RelativeDropdown.Value];
            //var clothes = ChaControl.chaFile.coordinate[(int)CurrentCoordinate.Value].clothes.parts;
            //var clothes2 = ChaControl.nowCoordinate.clothes.parts;
            for (int i = 0, listlength = list.Count; i < listlength; i++)
            {
                var themenum = list[i][0];
                var theme = Themes[themenum];
                theme.BaseColors[list[i][1]] = input;
                //if (list[i].Length < 3)
                //{
                var update = theme.ThemedSlots;
                for (int j = 0, n = update.Count; j < n; j++)
                {
                    ChangeACCColor(update[j], themenum);
                }
                //}
                //else
                //{
                //    if (list[i][3] == 0)
                //    {
                //        clothes[list[i][2]].colorInfo[list[i][1]].baseColor = input;
                //        clothes2[list[i][2]].colorInfo[list[i][1]].baseColor = input;
                //    }
                //    else
                //    {
                //        clothes[list[i][2]].colorInfo[list[i][1]].patternColor = input;
                //        clothes2[list[i][2]].colorInfo[list[i][1]].patternColor = input;
                //    }
                //}
            }
            //ChaControl.ChangeClothes();
        }

        private void AssignRelativeColorBox(int input)
        {
            if (Relative_ACC_Dictionary.Count != 0)
            {
                var intarray = Relative_ACC_Dictionary[input][0];
                makerColorSimilar.SetValue(Themes[intarray[0]].BaseColors[intarray[1]], false);
            }
            else
            {
                makerColorSimilar.SetValue(Color.white, false);
                RelativeDropdown.SetValue(0, false);
            }
        }

        private void RelativeSkew(bool undo = false)
        {
            if (Relative_ACC_Dictionary.Count == 0)
            {
                return;
            }

            var input = RelativeSkewColor.Value;

            Color.RGBToHSV(input, out var inHue, out var _, out var _);
            var inS = SaturationSlider.Value;
            var inV = ValuesSlider.Value;
            var list = Relative_ACC_Dictionary;
            var undoAccQueue = new Queue<Color>();
            var clothesUndoQueue = new Queue<Color>();
            if (undo)
            {
                undoAccQueue = UndoACCSkew.Pop();
                clothesUndoQueue = ClothsUndoSkew.Pop();
            }

            for (int i = 0, iN = list.Count; i < iN; i++)
            {
                var list2 = list[i];

                for (int j = 0, jn = list2.Count; j < jn; j++)
                {
                    var themenum = list2[j][0];
                    var colornum = list2[j][1];
                    var theme = Themes[themenum];
                    var update = theme.ThemedSlots;
                    var color = theme.BaseColors[colornum];

                    Color.RGBToHSV(color, out var hue, out var s, out var v);

                    if (undo)
                    {
                        color = undoAccQueue.Dequeue();
                    }
                    else
                    {
                        undoAccQueue.Enqueue(new Color(color.r, color.g, color.b, color.a));
                        hue += inHue;
                        s += inS;
                        v += inV;
                        color = HsvColor.ToRgba(
                            new HsvColor(Math.Abs(hue % 1f) * 360, Mathf.Clamp(s, 0f, 1f), Mathf.Clamp(v, 0f, 1f)),
                            color.a);
                    }

                    theme.BaseColors[colornum] = color;
                    for (int k = 0, kn = update.Count; k < kn; k++)
                    {
                        ChangeACCColor(update[k], themenum);
                    }
                }
            }

            var clothes = ChaControl.chaFile.coordinate[(int)CurrentCoordinate.Value].clothes.parts;
            var clothes2 = ChaControl.nowCoordinate.clothes.parts;
            for (var i = 0; i < clothes.Length; i++)
            {
                for (var j = 0; j < clothes[i].colorInfo.Length; j++)
                {
                    var temp = clothes[i].colorInfo[j].baseColor;
                    Color.RGBToHSV(temp, out var hue, out var s, out var v);
                    if (undo)
                    {
                        temp = clothesUndoQueue.Dequeue();
                    }
                    else
                    {
                        clothesUndoQueue.Enqueue(new Color(temp.r, temp.g, temp.b, temp.a));
                        hue += inHue;
                        s += inS;
                        v += inV;
                        temp = HsvColor.ToRgba(
                            new HsvColor(Math.Abs(hue % 1f) * 360, Mathf.Clamp(s, 0f, 1f), Mathf.Clamp(v, 0f, 1f)),
                            temp.a);
                    }

                    clothes[i].colorInfo[j].baseColor = temp;
                    clothes2[i].colorInfo[j].baseColor = temp;

                    temp = clothes[i].colorInfo[j].patternColor;
                    Color.RGBToHSV(temp, out hue, out s, out v);
                    if (undo)
                    {
                        temp = clothesUndoQueue.Dequeue();
                    }
                    else
                    {
                        clothesUndoQueue.Enqueue(new Color(temp.r, temp.g, temp.b, temp.a));
                        hue += inHue;
                        s += inS;
                        v += inV;
                        temp = HsvColor.ToRgba(
                            new HsvColor(Math.Abs(hue % 1f) * 360, Mathf.Clamp(s, 0f, 1f), Mathf.Clamp(v, 0f, 1f)),
                            temp.a);
                    }

                    clothes[i].colorInfo[j].patternColor = temp;
                    clothes2[i].colorInfo[j].patternColor = temp;
                }
            }

            ChaControl.ChangeClothes();
            if (!undo)
            {
                UndoACCSkew.Push(undoAccQueue);
                ClothsUndoSkew.Push(clothesUndoQueue);
            }
        }
    }
}