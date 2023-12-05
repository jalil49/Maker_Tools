using System;
using System.Collections.Generic;
using System.Linq;
using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
using UniRx;
using UnityEngine;

namespace Accessory_Themes
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (!MakerAPI.InsideMaker) return;

            for (var i = 0; i < ChaFileControl.coordinate.Length; i++)
                if (Coordinate.ContainsKey(i))
                    Clearoutfit(i);
                else
                    Createoutfit(i);
            for (int i = ChaFileControl.coordinate.Length, n = Coordinate.Keys.Max() + 1; i < n; i++) Removeoutfit(i);

            CurrentCoordinate.Subscribe(x =>
            {
                _showCustomGui = false;
                if (!Coordinate.ContainsKey((int)x))
                    Createoutfit((int)x);

                UpdateNowCoordinate();
                StartCoroutine(WaitForSlots());
            });

            var myData = GetExtendedData();
            if (myData != null)
            {
                switch (myData.version)
                {
                    case 0:
                        Migrator.MigrateV0(myData, ref _data);
                        break;
                    case 1:
                        if (myData.data.TryGetValue("CoordinateData", out var byteData) && byteData != null)
                            _data.Coordinate =
                                MessagePackSerializer.Deserialize<Dictionary<int, CoordinateData>>((byte[])byteData);
                        break;
                    default:
                        Settings.Logger.LogWarning("New version of plugin detected please update");
                        break;
                }

                StartCoroutine(WaitForSlots());
            }

            _aciRef = ChaControl.GetComponent<Additional_Card_Info.CharaEvent>();
            UpdateNowCoordinate();
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            if (!MakerAPI.InsideMaker)
            {
                var data = GetExtendedData();
                if (data != null) SetExtendedData(data);
                return;
            }

            var myData = new PluginData { version = 1 };

            _data.CleanUp();

            var nulldata = Coordinate.All(x => x.Value.themes.Count == 0);

            myData.data.Add("CoordinateData", MessagePackSerializer.Serialize(Coordinate));
            SetExtendedData(nulldata ? null : myData);
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            var myData = new PluginData { version = 1 };
            NowCoordinate.CleanUp();
            var nulldata = Themes.Count == 0;
            myData.data.Add("CoordinateData", MessagePackSerializer.Serialize(NowCoordinate));
            SetCoordinateExtendedData(coordinate, nulldata ? null : myData);
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            if (!MakerAPI.InsideMaker) return;

            NowCoordinate.Clear();
            var myData = GetCoordinateExtendedData(coordinate);
            if (myData != null)
                switch (myData.version)
                {
                    case 0:
                        NowCoordinate = Migrator.CoordinateMigrateV0(myData);
                        break;
                    case 1:
                        if (myData.data.TryGetValue("CoordinateData", out var byteData) && byteData != null)
                            NowCoordinate = MessagePackSerializer.Deserialize<CoordinateData>((byte[])byteData);
                        break;
                    default:
                        Settings.Logger.LogWarning("New version detected please update");
                        break;
                }

            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
                Coordinate[(int)CurrentCoordinate.Value] = NowCoordinate;
            StartCoroutine(WaitForSlots());
        }

        private void UpdateNowCoordinate()
        {
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            {
                _data.NowCoordinate = _data.Coordinate[(int)CurrentCoordinate.Value];
                return;
            }

            _data.NowCoordinate = new CoordinateData(_data.Coordinate[(int)CurrentCoordinate.Value]);
        }

        private void Theme_Changed()
        {
            if (!MakerAPI.InsideAndLoaded) return;
            var themeNum = _themesDropDownSetting.Value - 1;

            Color[] colordata;
            if (themeNum < 0)
            {
                _isThemeRelativeBool.SetValue(false, false);
                colordata = new[] { new Color(), new Color(), new Color(), new Color() };
            }
            else
            {
                var theme = Themes[themeNum];
                colordata = theme.Colors;
                _isThemeRelativeBool.SetValue(theme.IsRelative, false);
            }

            for (var i = 0; i < 4; i++) AccGuIsliders[i].SetValue(colordata[i], false);
        }

        private bool ChangeAccColor(int slot, int theme)
        {
            if (!_hairAcc.Contains(slot) && theme != 0 /* && !RelativeThemeBool[theme]*/)
            {
                var partinfo = AccessoriesApi.GetPartsInfo(slot);
                var colors = Themes[theme].Colors;
                var newColor = new[] { colors[0], colors[1], colors[2], colors[3] };
                partinfo.color = newColor;
                if (slot < 20)
                    ChaControl.chaFile.coordinate[(int)CurrentCoordinate.Value].accessory.parts[slot].color = newColor;
                ChaControl.ChangeAccessoryColor(slot);
                return true;
            }

            return false;
        }

        private void Copy_ACC_Color(bool isPersonal = false)
        {
            if (int.TryParse(_copyTextbox.Value, out var slot))
            {
                slot--;
                if (slot >= Parts.Length || slot < 0) return;
                var info = AccessoriesApi.GetPartsInfo(slot);

                if (isPersonal)
                {
                    //for (int i = 0; i < 4; i++)
                    //{
                    //    Personal_GUIsliders[i].SetValue(info.color[i]);
                    //}
                    //return;
                }

                for (var i = 0; i < 4; i++) AccGuIsliders[i].SetValue(info.color[i]);
            }
        }

        private void ColorSetByParent(bool simple = false)
        {
            var themeNum = _themesDropDownSetting.Value - 1;
            if (themeNum < 0) return;
            string[] comparison;
            if (simple)
                comparison = Constants.Inclusion[_simpleParentDropdown.Value].ToArray();
            else
                comparison = new[] { _parentDropdown.Options[_parentDropdown.Value] };
            _hairAcc = _aciRef.HairAcc;
            var themedslots = Themes[themeNum].ThemedSlots;
            for (int slot = 0, n = Parts.Length; slot < n; slot++)
            {
                var slotInfo = AccessoriesApi.GetPartsInfo(slot);
                if (slotInfo.type == 120 || ThemeDict.ContainsKey(slot)) continue;
                var parentKey = slotInfo.parentKey;
                if (comparison.Contains(parentKey))
                    if (ChangeAccColor(slot, themeNum))
                    {
                        themedslots.Add(slot);
                        ThemeDict[slot] = themeNum;
                    }
            }
        }

        private void UpdateSliderColor(int colorNum, Color value /*, bool IsPersonal = false*/)
        {
            var themeNum = _themesDropDownSetting.Value - 1;
            if (themeNum < 0) return;

            var theme = Themes[themeNum];
            theme.Colors[colorNum] = value;
            _hairAcc = _aciRef.HairAcc;

            foreach (var item in theme.ThemedSlots) ChangeAccColor(item, themeNum);
        }

        private bool ColorComparison(Color c1, Color c2)
        {
            if (float.TryParse(_tolerance, out var value))
                return Math.Abs(c1.r - c2.r) <= value && Math.Abs(c1.g - c2.g) <= value &&
                       Math.Abs(c1.b - c2.b) <= value && Math.Abs(c1.a - c2.a) <= value;
            return false;
        }

        private bool ColorComparison(Color[] c1, Color[] c2)
        {
            for (var i = 0; i < 4; i++)
                if (!ColorComparison(c1[i], c2[i]))
                    return false;
            return true;
        }

        private void FindRelativeColors()
        {
            var check = new List<Color[]>();
            var relative = new List<bool>();
            foreach (var themeData in Themes)
            {
                check.Add(themeData.Colors);
                relative.Add(themeData.IsRelative);
            }

            var exclude = new List<Color>();
            var excludetemp = new List<Color>();
            var input = new List<int[]>();
            RelativeAccDictionary.Clear();

            for (int c1Theme = 1, n = check.Count; c1Theme < n; c1Theme++)
            {
                if (relative[c1Theme]) continue;
                for (var c1Color = 0; c1Color < 4; c1Color++)
                {
                    excludetemp.Clear();
                    excludetemp.Add(check[c1Theme][c1Color]);
                    if (exclude.Contains(check[c1Theme][c1Color])) continue;
                    input.Add(new[] { c1Theme, c1Color });
                    for (var c2Theme = c1Theme + 1; c2Theme < check.Count; c2Theme++)
                    {
                        if (relative[c2Theme]) continue;
                        for (var c2Color = 0; c2Color < 4; c2Color++)
                            if (ColorComparison(check[c1Theme][c1Color], check[c2Theme][c2Color]))
                            {
                                excludetemp.Add(check[c2Theme][c2Color]);
                                input.Add(new[] { c2Theme, c2Color });
                            }
                    }

                    if (input.Count > 0) RelativeAccDictionary.Add(RelativeAccDictionary.Count, input);
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
            if (!MakerAPI.InsideAndLoaded) return;
            var list = RelativeAccDictionary[_relativeDropdown.Value];
            //var clothes = ChaControl.chaFile.coordinate[(int)CurrentCoordinate.Value].clothes.parts;
            //var clothes2 = ChaControl.nowCoordinate.clothes.parts;
            _hairAcc = _aciRef.HairAcc;
            for (int i = 0, listlength = list.Count; i < listlength; i++)
            {
                var themeNum = list[i][0];
                var theme = Themes[themeNum];
                theme.Colors[list[i][1]] = input;
                //if (list[i].Length < 3)
                //{
                var update = theme.ThemedSlots;
                for (int j = 0, n = update.Count; j < n; j++) ChangeAccColor(update[j], themeNum);
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
            if (RelativeAccDictionary.Count != 0)
            {
                var intarray = RelativeAccDictionary[input][0];
                _makerColorSimilar.SetValue(Themes[intarray[0]].Colors[intarray[1]], false);
            }
            else
            {
                _makerColorSimilar.SetValue(new Color(), false);
                _relativeDropdown.SetValue(0, false);
            }
        }

        private void RelativeSkew(bool undo = false)
        {
            if (RelativeAccDictionary.Count == 0) return;
            var input = _relativeSkewColor.Value;

            Color.RGBToHSV(input, out var inHue, out var _, out var _);
            var inS = _saturationSlider.Value;
            var inV = _valuesSlider.Value;
            var list = RelativeAccDictionary;
            var undoAccQueue = new Queue<Color>();
            var clothesUndoQueue = new Queue<Color>();
            if (undo)
            {
                undoAccQueue = UndoAccSkew.Pop();
                clothesUndoQueue = ClothsUndoSkew.Pop();
            }

            _hairAcc = _aciRef.HairAcc;

            for (int i = 0, iN = list.Count; i < iN; i++)
            {
                var list2 = list[i];

                for (int j = 0, jn = list2.Count; j < jn; j++)
                {
                    var themeNum = list2[j][0];
                    var colornum = list2[j][1];
                    var theme = Themes[themeNum];
                    var update = theme.ThemedSlots;
                    var color = theme.Colors[colornum];

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

                    theme.Colors[colornum] = color;
                    for (int k = 0, kn = update.Count; k < kn; k++) ChangeAccColor(update[k], themeNum);
                }
            }

            var clothes = ChaControl.chaFile.coordinate[(int)CurrentCoordinate.Value].clothes.parts;
            var clothes2 = ChaControl.nowCoordinate.clothes.parts;
            for (var i = 0; i < clothes.Length; i++)
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
                        new HsvColor(Math.Abs(hue % 1f) * 360, Mathf.Clamp(s, 0f, 1f), Mathf.Clamp(v, 0f, 1f)), temp.a);
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
                        new HsvColor(Math.Abs(hue % 1f) * 360, Mathf.Clamp(s, 0f, 1f), Mathf.Clamp(v, 0f, 1f)), temp.a);
                }

                clothes[i].colorInfo[j].patternColor = temp;
                clothes2[i].colorInfo[j].patternColor = temp;
            }

            ChaControl.ChangeClothes();
            if (!undo)
            {
                UndoAccSkew.Push(undoAccQueue);
                ClothsUndoSkew.Push(clothesUndoQueue);
            }
        }

        private void Clearoutfit(int key)
        {
            _data.Clearoutfit(key);
        }

        private void Createoutfit(int key)
        {
            _data.Createoutfit(key);
        }

        private void Moveoutfit(int dest, int src)
        {
            _data.Moveoutfit(dest, src);
        }

        private void Removeoutfit(int key)
        {
            _data.Removeoutfit(key);
        }
    }
}