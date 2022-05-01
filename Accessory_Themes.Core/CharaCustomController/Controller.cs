﻿using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
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
            var plugindata = GetExtendedData();
            if (plugindata != null)
            {
                Migrator.StandardCharaMigrate(ChaControl, plugindata);
            }
            ACI_Ref = ChaControl.GetComponent<Additional_Card_Info.CharaEvent>();
            UpdatePluginData();
            CurrentCoordinate.Subscribe(x =>
            {
                ShowCustomGui = false;

                UpdatePluginData();
                WaitForSlots();
            });
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode) => SetExtendedData(new PluginData() { version = Constants.MasterSaveVersion });

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate) => SetCoordinateExtendedData(coordinate, new PluginData() { version = Constants.MasterSaveVersion });

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            var MyData = GetCoordinateExtendedData(coordinate);
            if (MyData != null)
            {
                Migrator.StandardCoordMigrator(coordinate, MyData);
            }
            UpdatePluginData();
            WaitForSlots();
        }

        private void UpdatePluginData()
        {
            Clear();
            for (var i = 0; i < Parts.Length; i++)
            {
                LoadSlot(i);
            }
            WaitForSlots();
        }

        private void Theme_Changed()
        {
            if (!MakerAPI.InsideAndLoaded)
            {
                return;
            }
            var themenum = ThemesDropDown_Setting.Value - 1;

            Color[] colordata;
            if (themenum < 0)
            {
                IsThemeRelativeBool.SetValue(false, false);
                colordata = new Color[] { new Color(), new Color(), new Color(), new Color() };
            }
            else
            {
                var theme = Themes[themenum];
                colordata = theme.Colors;
                IsThemeRelativeBool.SetValue(theme.Isrelative, false);
            }
            for (var i = 0; i < 4; i++)
            {
                ACC_GUIsliders[i].SetValue(colordata[i], false);
            }
        }

        private bool ChangeACCColor(int slot, int theme)
        {
            if ((!HairAcc.TryGetValue(slot, out var ACISlotInfo) || ACISlotInfo.KeepState != Additional_Card_Info.KeepState.HairKeep) && theme != 0/* && !RelativeThemeBool[theme]*/)
            {
                var Partinfo = AccessoriesApi.GetPartsInfo(slot);
                var Colors = Themes[theme].Colors;
                var New_Color = new Color[] { Colors[0], Colors[1], Colors[2], Colors[3] };
                Partinfo.color = New_Color;
                ChaControl.ChangeAccessoryColor(slot);
                return true;
            }
            return false;
        }

        private void Copy_ACC_Color(bool IsPersonal = false)
        {
            if (Int32.TryParse(CopyTextbox.Value, out var slot))
            {
                slot--;
                if (slot >= Parts.Length || slot < 0)
                {
                    return;
                }
                var info = AccessoriesApi.GetPartsInfo(slot);

                if (IsPersonal)
                {

                    //for (int i = 0; i < 4; i++)
                    //{
                    //    Personal_GUIsliders[i].SetValue(info.color[i]);
                    //}
                    //return;
                }

                for (var i = 0; i < 4; i++)
                {
                    ACC_GUIsliders[i].SetValue(info.color[i]);
                }
            }
        }

        private void ColorSetByParent(bool Simple = false)
        {
            var themenum = ThemesDropDown_Setting.Value - 1;
            if (themenum < 0)
            {
                return;
            }
            string[] comparison;
            if (Simple)
            {
                comparison = Constants.Inclusion[SimpleParentDropdown.Value].ToArray();
            }
            else
            {
                comparison = new string[] { ParentDropdown.Options[ParentDropdown.Value] };
            }
            var themedslots = Themes[themenum].ThemedSlots;
            for (int Slot = 0, n = Parts.Length; Slot < n; Slot++)
            {
                var slotinfo = Parts[Slot];
                if (slotinfo.type == 120 || Theme_Dict.ContainsKey(Slot))
                {
                    continue;
                }
                var ParentKey = slotinfo.parentKey;
                if (comparison.Contains(ParentKey))
                {
                    if (ChangeACCColor(Slot, themenum))
                    {
                        themedslots.Add(Slot);
                        Theme_Dict[Slot] = themenum;
                    }
                }
            }
        }

        private void UpdateSliderColor(int ColorNum, Color value/*, bool IsPersonal = false*/)
        {
            var themenum = ThemesDropDown_Setting.Value - 1;
            if (themenum < 0)
            {
                return;
            }

            var theme = Themes[themenum];
            theme.Colors[ColorNum] = value;

            foreach (var item in theme.ThemedSlots)
            {
                ChangeACCColor(item, themenum);
            }
        }

        private bool ColorComparison(Color C1, Color C2)
        {
            if (float.TryParse(Tolerance, out var value))
            {
                return Math.Abs(C1.r - C2.r) <= value && Math.Abs(C1.g - C2.g) <= value && Math.Abs(C1.b - C2.b) <= value && Math.Abs(C1.a - C2.a) <= value;
            }
            return false;
        }

        private bool ColorComparison(Color[] C1, Color[] C2)
        {
            for (var i = 0; i < 4; i++)
            {
                if (!ColorComparison(C1[i], C2[i]))
                    return false;
            }
            return true;
        }

        private void FindRelativeColors()
        {
            var Check = new List<Color[]>();
            var Relative = new List<bool>();
            foreach (var themeData in Themes)
            {
                Check.Add(themeData.Colors);
                Relative.Add(themeData.Isrelative);
            }
            var exclude = new List<Color>();
            var excludetemp = new List<Color>();
            var input = new List<int[]>();
            Relative_ACC_Dictionary.Clear();

            for (int C1_Theme = 1, n = Check.Count; C1_Theme < n; C1_Theme++)
            {
                if (Relative[C1_Theme])
                {
                    continue;
                }
                for (var C1_color = 0; C1_color < 4; C1_color++)
                {
                    excludetemp.Clear();
                    excludetemp.Add(Check[C1_Theme][C1_color]);
                    if (exclude.Contains(Check[C1_Theme][C1_color]))
                    {
                        continue;
                    }
                    input.Add(new int[] { C1_Theme, C1_color });
                    for (var C2_Theme = C1_Theme + 1; C2_Theme < Check.Count; C2_Theme++)
                    {
                        if (Relative[C2_Theme])
                        {
                            continue;
                        }
                        for (var C2_color = 0; C2_color < 4; C2_color++)
                        {
                            if (ColorComparison(Check[C1_Theme][C1_color], Check[C2_Theme][C2_color]))
                            {
                                excludetemp.Add(Check[C2_Theme][C2_color]);
                                input.Add(new int[] { C2_Theme, C2_color });
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
            //var clothes = ChaControl.chaFile.coordinate[CoordinateNum].clothes.parts;
            //for (int i = 0; i < clothes.Length; i++)
            //{
            //    var colorInfo = clothes[i].colorInfo;
            //    for (int j = 0; j < colorInfo.Length; j++)
            //    {
            //        bool Found = false;
            //        for (int k = 0; k < Relative_ACC_Dictionary.Count; k++)
            //        {
            //            int[] comp = Relative_ACC_Dictionary[k][0];
            //            if (ColorComparison(colorInfo[j].baseColor, Check[comp[0]][comp[1]]))
            //            {
            //                Relative_ACC_Dictionary[k].Add(new int[] { comp[0], j, i, 0 });
            //                Found = true;
            //                break;
            //            }
            //        }
            //        if (!Found)
            //        {
            //            int index = ThemeNames[CoordinateNum].IndexOf($"{(ChaFileDefine.ClothesKind)i}_Base");
            //            Relative_ACC_Dictionary.Add(Relative_ACC_Dictionary.Count, new List<int[]> { new int[] { index, j, i, 0 } });
            //        }
            //        for (int k = 0; k < Relative_ACC_Dictionary.Count; k++)
            //        {
            //            int[] comp = Relative_ACC_Dictionary[k][0];
            //            if (ColorComparison(colorInfo[j].patternColor, Check[comp[0]][comp[1]]))
            //            {
            //                Relative_ACC_Dictionary[k].Add(new int[] { comp[0], j, i, 1 });
            //                Found = true;
            //                break;
            //            }
            //        }
            //        if (!Found)
            //        {
            //            int index = ThemeNames[CoordinateNum].IndexOf($"{(ChaFileDefine.ClothesKind)i}_Pattern");
            //            Relative_ACC_Dictionary.Add(Relative_ACC_Dictionary.Count, new List<int[]> { new int[] { index, j, i, 1 } });
            //        }
            //    }
            //}
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
                theme.Colors[list[i][1]] = input;
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
                makerColorSimilar.SetValue(Themes[intarray[0]].Colors[intarray[1]], false);
            }
            else { makerColorSimilar.SetValue(new Color(), false); RelativeDropdown.SetValue(0, false); }
        }

        private void RelativeSkew(bool undo = false)
        {
            if (Relative_ACC_Dictionary.Count == 0)
            {
                return;
            }
            var input = RelativeSkewColor.Value;

            Color.RGBToHSV(input, out var In_Hue, out var _, out var _);
            var In_S = SaturationSlider.Value;
            var In_V = ValuesSlider.Value;
            var list = Relative_ACC_Dictionary;
            var UndoACCQueue = new Queue<Color>();
            var ClothesUndoQueue = new Queue<Color>();
            if (undo)
            {
                UndoACCQueue = UndoACCSkew.Pop();
                ClothesUndoQueue = ClothsUndoSkew.Pop();
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
                    var color = theme.Colors[colornum];

                    Color.RGBToHSV(color, out var T_Hue, out var T_S, out var T_V);

                    if (undo)
                    {
                        color = UndoACCQueue.Dequeue();
                    }
                    else
                    {
                        UndoACCQueue.Enqueue(new Color(color.r, color.g, color.b, color.a));
                        T_Hue += In_Hue;
                        T_S += In_S;
                        T_V += In_V;
                        color = HsvColor.ToRgba(new HsvColor(Math.Abs(T_Hue % 1f) * 360, Mathf.Clamp(T_S, 0f, 1f), Mathf.Clamp(T_V, 0f, 1f)), color.a);
                    }

                    theme.Colors[colornum] = color;
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
                    Color.RGBToHSV(temp, out var T_Hue, out var T_S, out var T_V);
                    if (undo)
                    {
                        temp = ClothesUndoQueue.Dequeue();
                    }
                    else
                    {
                        ClothesUndoQueue.Enqueue(new Color(temp.r, temp.g, temp.b, temp.a));
                        T_Hue += In_Hue;
                        T_S += In_S;
                        T_V += In_V;
                        temp = HsvColor.ToRgba(new HsvColor(Math.Abs(T_Hue % 1f) * 360, Mathf.Clamp(T_S, 0f, 1f), Mathf.Clamp(T_V, 0f, 1f)), temp.a);
                    }
                    clothes[i].colorInfo[j].baseColor = temp;
                    clothes2[i].colorInfo[j].baseColor = temp;

                    temp = clothes[i].colorInfo[j].patternColor;
                    Color.RGBToHSV(temp, out T_Hue, out T_S, out T_V);
                    if (undo)
                    {
                        temp = ClothesUndoQueue.Dequeue();
                    }
                    else
                    {
                        ClothesUndoQueue.Enqueue(new Color(temp.r, temp.g, temp.b, temp.a));
                        T_Hue += In_Hue;
                        T_S += In_S;
                        T_V += In_V;
                        temp = HsvColor.ToRgba(new HsvColor(Math.Abs(T_Hue % 1f) * 360, Mathf.Clamp(T_S, 0f, 1f), Mathf.Clamp(T_V, 0f, 1f)), temp.a);
                    }
                    clothes[i].colorInfo[j].patternColor = temp;
                    clothes2[i].colorInfo[j].patternColor = temp;
                }
            }
            ChaControl.ChangeClothes();
            if (!undo)
            {
                UndoACCSkew.Push(UndoACCQueue);
                ClothsUndoSkew.Push(ClothesUndoQueue);
            }
        }
    }
}
