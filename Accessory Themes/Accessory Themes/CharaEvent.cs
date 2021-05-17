using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
using MoreAccessoriesKOI;
using System;
using System.Collections.Generic;
using System.Linq;
using ToolBox;
using UniRx;
using UnityEngine;

namespace Accessory_Themes
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private List<string>[] ThemeNames = new List<string>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
        private List<bool>[] RelativeThemeBool = new List<bool>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
        private List<Color[]>[] colors = new List<Color[]>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
        private Dictionary<int, int>[] ACC_Theme_Dictionary = new Dictionary<int, int>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
        private Dictionary<int, bool>[] ColorRelativity = new Dictionary<int, bool>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
        private Dictionary<int, List<int[]>>[] Relative_ACC_Dictionary = new Dictionary<int, List<int[]>>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];

        private int CoordinateNum = 0;

        private static readonly string[] Parentlist = Enum.GetNames(typeof(ChaAccessoryDefine.AccessoryParentKey));

        private readonly Stack<Queue<Color>>[] UndoACCSkew = new Stack<Queue<Color>>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
        private readonly Stack<Queue<Color>>[] ClothsUndoSkew = new Stack<Queue<Color>>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];

        private Color[] PersonalColorSkew = new Color[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];

        private Additional_Card_Info.CharaEvent ACI_Ref;

        private List<int> HairAcc = new List<int>();

        List<ChaFileAccessory.PartsInfo> ACCData = new List<ChaFileAccessory.PartsInfo>();

        public CharaEvent()
        {
            for (int i = 0; i < ThemeNames.Length; i++)
            {
                ColorRelativity[i] = new Dictionary<int, bool>();
                ACC_Theme_Dictionary[i] = new Dictionary<int, int>();
                ThemeNames[i] = new List<string> { "None" };
                UndoACCSkew[i] = new Stack<Queue<Color>>();
                ClothsUndoSkew[i] = new Stack<Queue<Color>>();
                Relative_ACC_Dictionary[i] = new Dictionary<int, List<int[]>>();
                RelativeThemeBool[i] = new List<bool>
                {
                    false
                };
                PersonalColorSkew[i] = Color.white;
                colors[i] = new List<Color[]> { new Color[] { new Color(), new Color(), new Color(), new Color() } };
            }
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (currentGameMode != GameMode.Maker)
            {
                return;
            }

            for (int i = 0; i < ThemeNames.Length; i++)
            {
                ColorRelativity[i].Clear();
                ThemeNames[i].Clear();
                ThemeNames[i].Add("None");
                ACC_Theme_Dictionary[i].Clear();
                colors[i].Clear();
                colors[i].Add(new Color[] { new Color(), new Color(), new Color(), new Color() });
                UndoACCSkew[i].Clear();
                ClothsUndoSkew[i].Clear();
                Relative_ACC_Dictionary[i].Clear();
                RelativeThemeBool[i].Clear();
                RelativeThemeBool[i].Add(false);
                PersonalColorSkew[i] = Color.white;
            }
            CurrentCoordinate.Subscribe(x =>
            {
                CoordinateNum = (int)x;
                GetALLACC();
                StartCoroutine(WaitForSlots());
            });
            GetALLACC();

            makerColorSimilar.SetValue(new Color(), false);

            var MyData = GetExtendedData();
            if (MyData != null)
            {
                if (MyData.data.TryGetValue("Theme_Names", out var ByteData) && ByteData != null)
                {
                    ThemeNames = MessagePackSerializer.Deserialize<List<string>[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Theme_dic", out ByteData) && ByteData != null)
                {
                    ACC_Theme_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Color_Theme_dic", out ByteData) && ByteData != null)
                {
                    colors = MessagePackSerializer.Deserialize<List<Color[]>[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Color_Relativity", out ByteData) && ByteData != null)
                {
                    ColorRelativity = MessagePackSerializer.Deserialize<Dictionary<int, bool>[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Color_Skews", out ByteData) && ByteData != null)
                {
                    PersonalColorSkew = MessagePackSerializer.Deserialize<Color[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Relative_Theme_Bools", out ByteData) && ByteData != null)
                {
                    RelativeThemeBool = MessagePackSerializer.Deserialize<List<bool>[]>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Relative_ACC_Dictionary", out ByteData) && ByteData != null)
                {
                    Relative_ACC_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, List<int[]>>[]>((byte[])ByteData);
                }
                StartCoroutine(WaitForSlots());
            }
            GetALLACC();
            ACI_Ref = ChaControl.GetComponent<Additional_Card_Info.CharaEvent>();
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            PluginData MyData = new PluginData();
            MyData.data.Add("Theme_Names", MessagePackSerializer.Serialize(ThemeNames));
            MyData.data.Add("Theme_dic", MessagePackSerializer.Serialize(ACC_Theme_Dictionary));
            MyData.data.Add("Color_Theme_dic", MessagePackSerializer.Serialize(colors));
            MyData.data.Add("Color_Relativity", MessagePackSerializer.Serialize(ColorRelativity));
            MyData.data.Add("Relative_Theme_Bools", MessagePackSerializer.Serialize(RelativeThemeBool));
            MyData.data.Add("Color_Skews", MessagePackSerializer.Serialize(PersonalColorSkew));
            MyData.data.Add("Relative_ACC_Dictionary", MessagePackSerializer.Serialize(Relative_ACC_Dictionary));

            SetExtendedData(MyData);
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            PluginData MyData = new PluginData();
            //clothing to keep
            //Do not keep accessory
            //is colorable
            MyData.data.Add("Theme_Names", MessagePackSerializer.Serialize(ThemeNames[CoordinateNum]));
            MyData.data.Add("Theme_dic", MessagePackSerializer.Serialize(ACC_Theme_Dictionary[CoordinateNum]));
            MyData.data.Add("Color_Theme_dic", MessagePackSerializer.Serialize(colors[CoordinateNum]));
            MyData.data.Add("Color_Relativity", MessagePackSerializer.Serialize(ColorRelativity[CoordinateNum]));
            MyData.data.Add("Relative_Theme_Bools", MessagePackSerializer.Serialize(RelativeThemeBool[CoordinateNum]));
            MyData.data.Add("Relative_ACC_Dictionary", MessagePackSerializer.Serialize(Relative_ACC_Dictionary[CoordinateNum]));
            //Items to not color
            SetCoordinateExtendedData(coordinate, MyData);
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                return;
            }
            ThemeNames[CoordinateNum].Clear();
            ThemeNames[CoordinateNum].Add("None");
            ACC_Theme_Dictionary[CoordinateNum].Clear();
            colors[CoordinateNum].Clear();
            colors[CoordinateNum].Add(new Color[] { new Color(), new Color(), new Color(), new Color() });
            ColorRelativity[CoordinateNum].Clear();
            Relative_ACC_Dictionary[CoordinateNum].Clear();
            RelativeThemeBool[CoordinateNum].Clear();
            var MyData = GetCoordinateExtendedData(coordinate);
            if (MyData != null)
            {
                if (MyData.data.TryGetValue("Theme_Names", out var ByteData) && ByteData != null)
                {
                    ThemeNames[CoordinateNum] = MessagePackSerializer.Deserialize<List<string>>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Theme_dic", out ByteData) && ByteData != null)
                {
                    ACC_Theme_Dictionary[CoordinateNum] = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Color_Theme_dic", out ByteData) && ByteData != null)
                {
                    colors[CoordinateNum] = MessagePackSerializer.Deserialize<List<Color[]>>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Color_Relativity", out ByteData) && ByteData != null)
                {
                    ColorRelativity[CoordinateNum] = MessagePackSerializer.Deserialize<Dictionary<int, bool>>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Relative_Theme_Bools", out ByteData) && ByteData != null)
                {
                    RelativeThemeBool[CoordinateNum] = MessagePackSerializer.Deserialize<List<bool>>((byte[])ByteData);
                }
                if (MyData.data.TryGetValue("Relative_ACC_Dictionary", out ByteData) && ByteData != null)
                {
                    Relative_ACC_Dictionary[CoordinateNum] = MessagePackSerializer.Deserialize<Dictionary<int, List<int[]>>>((byte[])ByteData);
                }
            }
            StartCoroutine(WaitForSlots());
        }

        private void Theme_Changed()
        {
            if (!MakerAPI.InsideAndLoaded)
            {
                return;
            }
            //Settings.Logger.LogWarning($"Coordinate num {CoordinateNum}, drop {ThemeNamesDropdown.Value}");
            ACC_GUIslider1.SetValue(colors[CoordinateNum][ThemeNamesDropdown.Value][0], false);
            ACC_GUIslider2.SetValue(colors[CoordinateNum][ThemeNamesDropdown.Value][1], false);
            ACC_GUIslider3.SetValue(colors[CoordinateNum][ThemeNamesDropdown.Value][2], false);
            ACC_GUIslider4.SetValue(colors[CoordinateNum][ThemeNamesDropdown.Value][3], false);
            IsThemeRelativeBool.SetValue(RelativeThemeBool[CoordinateNum][ThemeNamesDropdown.Value], false);
        }

        private bool ChangeACCColor(int slot, int theme)
        {
            if (!HairAcc.Contains(slot) && theme != 0/* && !RelativeThemeBool[theme]*/)
            {
                Color[] New_Color = new Color[] { colors[CoordinateNum][theme][0], colors[CoordinateNum][theme][1], colors[CoordinateNum][theme][2], colors[CoordinateNum][theme][3] };
                ACCData[slot].color = New_Color;
                if (slot < 20)
                {
                    ChaControl.nowCoordinate.accessory.parts[slot].color = New_Color;
                    ChaControl.chaFile.coordinate[CoordinateNum].accessory.parts[slot].color = New_Color;
                }
                ChaControl.ChangeAccessoryColor(slot);
                return true;
            }
            return false;
        }

        private void GetALLACC()
        {
            WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();
            if (_accessoriesByChar.TryGetValue(ChaFileControl, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
                _accessoriesByChar.Add(ChaFileControl, data);
            }
            ACCData = new List<ChaFileAccessory.PartsInfo>();
            ACCData.AddRange(ChaFileControl.coordinate[CoordinateNum].accessory.parts);
            ACCData.AddRange(data.nowAccessories);
        }

        private void Copy_ACC_Color(bool IsPersonal = false)
        {
            if (Int32.TryParse(CopyTextbox.Value, out int numvalue))
            {
                numvalue--;
                if (numvalue >= ACCData.Count || numvalue < 0)
                {
                    return;
                }
                if (IsPersonal)
                {
                    //Personal_GUIslider1.Value = ACCData[numvalue].color[0];
                    //Personal_GUIslider2.Value = ACCData[numvalue].color[1];
                    //Personal_GUIslider3.Value = ACCData[numvalue].color[2];
                    //Personal_GUIslider4.Value = ACCData[numvalue].color[3];
                }
                else
                {
                    ACC_GUIslider1.SetValue(ACCData[numvalue].color[0]);
                    ACC_GUIslider2.SetValue(ACCData[numvalue].color[1]);
                    ACC_GUIslider3.SetValue(ACCData[numvalue].color[2]);
                    ACC_GUIslider4.SetValue(ACCData[numvalue].color[3]);
                }
            }
        }

        private void ColorSetByParent(bool Simple = false)
        {
            string[] comparison;
            if (Simple)
            {
                comparison = Constants.Inclusion[SimpleParentDropdown.Value].ToArray();
            }
            else
            {
                comparison = new string[] { Parentlist[ParentDropdown.Value] };
            }
            HairAcc = ACI_Ref.HairAcc[CoordinateNum];
            for (int slot = 0; slot < ACCData.Count; slot++)
            {
                if (comparison.Contains(ACCData[slot].parentKey))
                {
                    if (ChangeACCColor(slot, ThemeNamesDropdown.Value))
                    {
                        ACC_Theme_Dictionary[CoordinateNum][slot] = ThemeNamesDropdown.Value;
                    }
                }
            }
        }

        private void UpdateSliderColor(int ColorNum, Color value/*, bool IsPersonal = false*/)
        {
            colors[CoordinateNum][ThemeNamesDropdown.Value][ColorNum] = value;
            HairAcc = ACI_Ref.HairAcc[CoordinateNum];
            GetALLACC();
            foreach (var item in ACC_Theme_Dictionary[CoordinateNum].Where(x => x.Value == ThemeNamesDropdown.Value))
            {
                ChangeACCColor(item.Key, ThemeNamesDropdown.Value);
            }
        }

        protected override void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                foreach (var item in ACC_Theme_Dictionary[CoordinateNum])
                {
                    Settings.Logger.LogWarning($"Key {item.Key}, value {item.Value}");
                }
            }
            base.Update();
        }

        private void AddThemeValueToList(int slot = 0, bool generated = false)
        {
            string Value = ThemeText.Value;
            if (!generated)
            {
                slot = AccessoriesApi.SelectedMakerAccSlot;
            }
            GetALLACC();
            if (Value.Length == 0 || radio.Value == 1 && ThemeNames[CoordinateNum].Contains(Value) || Value == "None" || slot == -1 || ACCData[slot].id == 120)
            {
                return;
            }
            if (radio.Value == 0)
            {
                ThemeNames[CoordinateNum].Add(Value);
                RelativeThemeBool[CoordinateNum].Add(false);
                Color[] current;
                if (slot < 20)
                {
                    current = ChaControl.nowCoordinate.accessory.parts[slot].color;
                }
                else { current = ACCData[slot].color; }

                colors[CoordinateNum].Add(current);
                ACC_Theme_Dictionary[CoordinateNum][slot] = ThemeNames[CoordinateNum].Count - 1;
                Update_ACC_Dropbox();
                Themes.SetValue(slot, ThemeNames[CoordinateNum].Count - 1);
                for (slot = 0; slot < ACCData.Count; slot++)
                {
                    if (ACC_Theme_Dictionary[CoordinateNum].ContainsKey(slot) || ACCData[slot].id == 120)
                    {
                        continue;
                    }

                    if (slot < 20)
                    {
                        if (ColorComparison(current, ChaControl.nowCoordinate.accessory.parts[slot].color))
                        {
                            ACC_Theme_Dictionary[CoordinateNum][slot] = ThemeNames[CoordinateNum].Count - 1;
                            Themes.SetValue(slot, ThemeNames[CoordinateNum].Count - 1);
                        }
                    }
                    else if (ColorComparison(current, ACCData[slot].color))
                    {
                        ACC_Theme_Dictionary[CoordinateNum][slot] = ThemeNames[CoordinateNum].Count - 1;
                        Themes.SetValue(slot, ThemeNames[CoordinateNum].Count - 1);
                    }
                }
                Update_ACC_Dropbox();
                return;
            }
            else if (radio.Value == 1)
            {
                int index = ThemeNames[CoordinateNum].IndexOf(Value);
                if (index < 1)
                    return;
                ThemeNames[CoordinateNum].RemoveAt(index);
                colors[CoordinateNum].RemoveAt(index);
                RelativeThemeBool[CoordinateNum].RemoveAt(index);
                var removeindex = ACC_Theme_Dictionary[CoordinateNum].Where(x => x.Value == index).ToArray();
                foreach (var item in removeindex)
                {
                    ACC_Theme_Dictionary[CoordinateNum].Remove(item.Key);
                    Themes.SetValue(item.Key, 0);
                }
                removeindex = ACC_Theme_Dictionary[CoordinateNum].Where(x => x.Value > index).ToArray();
                foreach (var item in removeindex)
                {
                    Themes.SetValue(item.Key, ThemeNames[CoordinateNum].Count - 1);
                    ACC_Theme_Dictionary[CoordinateNum][item.Key] = item.Value - 1;
                }
            }
            else
            {
                if (ACC_Theme_Dictionary[CoordinateNum].TryGetValue(slot, out int index))
                {
                    if (index < 1)
                    {
                        return;
                    }
                    ThemeNames[CoordinateNum][index] = Value;
                }
            }
            Update_ACC_Dropbox();
        }

        private bool ColorComparison(Color C1, Color C2)
        {
            if (float.TryParse(Tolerance.Value, out float value))
            {
                return Math.Abs(C1.r - C2.r) <= value && Math.Abs(C1.g - C2.g) <= value && Math.Abs(C1.b - C2.b) <= value && Math.Abs(C1.a - C2.a) <= value;
            }
            return false;
        }

        private bool ColorComparison(Color[] C1, Color[] C2)
        {
            for (int i = 0; i < 4; i++)
            {
                if (!ColorComparison(C1[i], C2[i]))
                    return false;
            }
            return true;
        }

        private void FindRelativeColors()
        {
            List<Color[]> Check = colors[CoordinateNum];
            List<Color> exclude = new List<Color>();
            List<Color> excludetemp = new List<Color>();
            List<int[]> input = new List<int[]>();
            Relative_ACC_Dictionary[CoordinateNum].Clear();

            for (int C1_Theme = 1; C1_Theme < Check.Count; C1_Theme++)
            {
                if (RelativeThemeBool[CoordinateNum][C1_Theme])
                {
                    continue;
                }
                for (int C1_color = 0; C1_color < 4; C1_color++)
                {
                    //Color.RGBToHSV(Check[C1_Theme][C1_color], out _, out var Saturation, out var Value);
                    excludetemp.Clear();
                    excludetemp.Add(Check[C1_Theme][C1_color]);
                    if (exclude.Contains(Check[C1_Theme][C1_color]))
                    {
                        continue;
                    }
                    input.Add(new int[] { C1_Theme, C1_color });
                    for (int C2_Theme = C1_Theme + 1; C2_Theme < Check.Count; C2_Theme++)
                    {
                        if (RelativeThemeBool[CoordinateNum][C2_Theme])
                        {
                            continue;
                        }
                        for (int C2_color = 0; C2_color < 4; C2_color++)
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
                        Relative_ACC_Dictionary[CoordinateNum].Add(Relative_ACC_Dictionary[CoordinateNum].Count, input);
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
            List<int[]> list = Relative_ACC_Dictionary[CoordinateNum][SimilarDropdown.Value];
            var clothes = ChaControl.chaFile.coordinate[CoordinateNum].clothes.parts;
            var clothes2 = ChaControl.nowCoordinate.clothes.parts;
            HairAcc = ACI_Ref.HairAcc[CoordinateNum];
            for (int i = 0; i < list.Count; i++)
            {
                colors[CoordinateNum][list[i][0]][list[i][1]] = input;
                //if (list[i].Length < 3)
                //{
                var update = ACC_Theme_Dictionary[CoordinateNum].Where(x => x.Value == list[i][0]).ToArray();
                for (int j = 0; j < update.Length; j++)
                {
                    ChangeACCColor(update[j].Key, update[j].Value);
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
            if (Relative_ACC_Dictionary[CoordinateNum].Count != 0)
            {
                makerColorSimilar.SetValue(colors[CoordinateNum][Relative_ACC_Dictionary[CoordinateNum][input][0][0]][Relative_ACC_Dictionary[CoordinateNum][input][0][1]], false);
            }
            else { makerColorSimilar.SetValue(new Color(), false); SimilarDropdown.SetValue(0, false); }
        }

        private void RelativeSkew(bool undo = false)
        {
            if (Relative_ACC_Dictionary[CoordinateNum].Count == 0)
            {
                return;
            }
            Color input = RelativeSkewColor.Value;

            Color.RGBToHSV(input, out float In_Hue, out float In_S, out float In_V);
            In_S = SaturationSlider.Value;
            In_V = ValuesSlider.Value;
            var list = Relative_ACC_Dictionary[CoordinateNum];
            var UndoACCQueue = new Queue<Color>();
            var ClothesUndoQueue = new Queue<Color>();
            if (undo)
            {
                UndoACCQueue = UndoACCSkew[CoordinateNum].Pop();
                ClothesUndoQueue = ClothsUndoSkew[CoordinateNum].Pop();
            }
            HairAcc = ACI_Ref.HairAcc[CoordinateNum];

            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list[i].Count; j++)
                {
                    Color temp = colors[CoordinateNum][list[i][j][0]][list[i][j][1]];
                    Color.RGBToHSV(temp, out float T_Hue, out float T_S, out float T_V);
                    if (undo)
                    {
                        temp = UndoACCQueue.Dequeue();
                    }
                    else
                    {
                        UndoACCQueue.Enqueue(new Color(temp.r, temp.g, temp.b, temp.a));
                        T_Hue += In_Hue;
                        T_S += In_S;
                        T_V += In_V;
                        temp = HsvColor.ToRgba(new HsvColor(Math.Abs(T_Hue % 1f) * 360, Mathf.Clamp(T_S, 0f, 1f), Mathf.Clamp(T_V, 0f, 1f)), temp.a);
                    }
                    colors[CoordinateNum][list[i][j][0]][list[i][j][1]] = temp;
                    var update = ACC_Theme_Dictionary[CoordinateNum].Where(x => x.Value == list[i][j][0]).ToArray();
                    for (int k = 0; k < update.Length; k++)
                    {
                        ChangeACCColor(update[k].Key, update[k].Value);
                    }
                }
            }
            var clothes = ChaControl.chaFile.coordinate[CoordinateNum].clothes.parts;
            var clothes2 = ChaControl.nowCoordinate.clothes.parts;
            for (int i = 0; i < clothes.Length; i++)
            {
                for (int j = 0; j < clothes[i].colorInfo.Length; j++)
                {
                    Color temp = clothes[i].colorInfo[j].baseColor;
                    Color.RGBToHSV(temp, out float T_Hue, out float T_S, out float T_V);
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
                UndoACCSkew[CoordinateNum].Push(UndoACCQueue);
                ClothsUndoSkew[CoordinateNum].Push(ClothesUndoQueue);
            }
        }

        private void AutoTheme()
        {
            GetALLACC();
            var data = ACCData;
            var themed = ACC_Theme_Dictionary[CoordinateNum];
            if (Clearthemes.Value)
            {
                ACC_Theme_Dictionary[CoordinateNum].Clear();
                ThemeNames[CoordinateNum].Clear();
                ThemeNames[CoordinateNum].Add("None");
                colors[CoordinateNum].Clear();
                colors[CoordinateNum].Add(new Color[] { new Color(), new Color(), new Color(), new Color() });
            }
            radio.SetValue(0);
            for (int Slot = 0; Slot < data.Count; Slot++)
            {
                if (themed.ContainsKey(Slot) || ACCData[Slot].id == 120 || ACCData[Slot].id == 0)
                {
                    continue;
                }
                ThemeText.SetValue($"Gen_Slot{(Slot + 1):000}", false);
                AddThemeValueToList(Slot, true);
            }
        }
    }
}
