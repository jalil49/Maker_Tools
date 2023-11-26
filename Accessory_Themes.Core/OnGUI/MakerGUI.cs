using Extensions;
using Extensions.GUI_Classes;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Accessory_Themes.OnGUI
{
    public class MakerGUI
    {
        public static MakerGUI Instance { get; private set; }

        private ChaControl ChaControl => MakerAPI.GetCharacterControl();

        private CharaEvent CharaEvent => ChaControl.GetComponent<CharaEvent>();

        private Dictionary<int, Additional_Card_Info.SlotData> HairAcc => ChaControl.GetComponent<Additional_Card_Info.CharaEvent>().SlotData;

        private ChaFileAccessory.PartsInfo[] Parts => ChaControl.nowCoordinate.accessory.parts;

        private readonly Dictionary<CharaEvent, CharaEventControl> CharaEventControls;

        private static bool _makerEnabled;

        private readonly Stack<Queue<Color>> UndoACCSkew = new Stack<Queue<Color>>();

        private readonly Stack<Queue<Color>> ClothsUndoSkew = new Stack<Queue<Color>>();

        private readonly int[] bulkRange = { 0, 1 };

        private float Tolerance = 0.1F;
        private float SaturationSlider;
        private float ValuesSlider;
        private int SimpleParent;
        private Color RelativeSkewColor;
        private readonly HashSet<string> SelectedParents = new HashSet<string>();
        private readonly WindowGUI SlotWindow;

        private Dictionary<int, SlotData> SlotDataDict => CharaEvent.SlotDataDict;

        private MakerGUI(RegisterSubCategoriesEvent e)
        {

            CharaEventControls = new Dictionary<CharaEvent, CharaEventControl>();

            MakerAPI.MakerExiting += MakerAPI_MakerExiting;
            AccessoriesApi.SelectedMakerAccSlotChanged += AccessoriesApi_SelectedMakerAccSlotChanged;
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;

            var owner = Settings.Instance;
            var category = new MakerCategory(null, null);

            var guiButton = new MakerButton("Themes GUI", category, owner);
            MakerAPI.AddAccessoryWindowControl(guiButton, true);
            guiButton.OnClick.AddListener(SlotWindow.ToggleShow);

            var groupingID = "Maker_Tools_" + Settings.NamingID.Value;
            guiButton.GroupingID = groupingID;
        }

        private SlotData GetSlotData()
        {
            return GetSlotData(AccessoriesApi.SelectedMakerAccSlot);
        }

        private SlotData GetSlotData(int slotNum)
        {
            if(SlotDataDict.TryGetValue(slotNum, out var slotData))
                return slotData;
            return SlotDataDict[slotNum] = new SlotData();
        }

        private void ToggleSlotWindow()
        {
            SlotWindow.ToggleShow();
        }

        private void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            var controller = CharaEvent;
            controller.LoadSlot(e.DestinationSlotIndex);
        }

        private void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            var charaEvent = CharaEvent;
            if(e.CopyDestination != charaEvent.CurrentCoordinate.Value)
                return;

            foreach(var item in e.CopiedSlotIndexes)
            {
                charaEvent.LoadSlot(item);
            }
        }

        private void AccessoriesApi_SelectedMakerAccSlotChanged(object sender, AccessorySlotEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void MakerAPI_MakerExiting(object sender, System.EventArgs e)
        {
            MakerAPI.MakerExiting -= MakerAPI_MakerExiting;
            AccessoriesApi.SelectedMakerAccSlotChanged -= AccessoriesApi_SelectedMakerAccSlotChanged;
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;
        }

        public static void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            _makerEnabled = Settings.Enable.Value;
            if(!_makerEnabled)
            {
                return;
            }

            Instance = new MakerGUI(e);
        }

        private void BulkProcess(ThemeData theme)
        {
            if(theme == null)
                return;
            var slotData = SlotDataDict;
            var small = Math.Max(Math.Min(bulkRange[0], bulkRange[1]), 0);
            var large = Math.Min(Math.Max(bulkRange[0], bulkRange[1]), 0);

            for(int slot = small, n = large; slot < n; slot++)
            {
                if(!slotData.TryGetValue(slot, out var data))
                {
                    slotData[slot] = new SlotData();
                }

                if(data.Theme != null || Parts[slot].type == 120)
                {
                    continue;
                }

                data.Theme = theme;

                ChangeACCColor(slot, theme);
            }
        }

        private void AutoTheme()
        {
            CharaEvent.Clear();

            for(int slot = 0, n = Parts.Length; slot < n; slot++)
            {
                AddThemeValueToList(slot);
            }
        }

        private void AddThemeValueToList()
        {

        }

        private void AddThemeValueToList(int slot, bool generated = true)
        {
            if(Parts[slot].type < 121)
            {
                return;
            }

            var slotData = GetSlotData(slot);

            var BaseColor = Parts[slot].color;

            var theme = new ThemeData($"Gen_Slot {(slot + 1):000}", BaseColor);
            CharaEvent.Themes.Add(theme);

            slotData.Theme = theme;

            if(!generated)
            {
                slot = 0;
            }

            for(var i = slot; i < Parts.Length; i++)
            {

                if(Parts[i].type <= 120)
                {
                    continue;
                }

                if(ColorComparison(BaseColor, Parts[i].color))
                    GetSlotData(i).Theme = theme;
            }
        }

        private void AssignThemeToRange(ThemeData theme, int lower, int upper)
        {
            for(var i = lower; lower < upper; i++)
            {

                if(Parts[i].type <= 120)
                {
                    continue;
                }

                GetSlotData(i).Theme = theme;
            }
        }

        private bool ChangeACCColor(int slot, ThemeData theme)
        {
            if((!HairAcc.TryGetValue(slot, out var ACISlotInfo) || ACISlotInfo.KeepState != Additional_Card_Info.KeepState.HairKeep))
            {
                var colors = theme.BaseColors;
                Parts[slot].color = new Color[] { colors[0], colors[1], colors[2], colors[3] };
                ChaControl.ChangeAccessoryColor(slot);
                return true;
            }

            return false;
        }

        internal void MovIt(List<QueueItem> _)
        {
            CharaEvent.UpdatePluginData();
        }

        private void ColorSetByParent(ThemeData theme, bool Simple = false)
        {
            //TODO: Fix

            string[] comparison;

            if(Simple)
            {
                comparison = Constants.Inclusion[SimpleParent].ToArray();
            }
            else
            {
                comparison = SelectedParents.ToArray();
            }

            for(int slot = 0, n = Parts.Length; slot < n; slot++)
            {
                var partData = Parts[slot];

                if(partData.type == 120 || !comparison.Contains(partData.parentKey))
                {
                    continue;
                }

                if(!SlotDataDict.TryGetValue(slot, out var slotData))
                {
                    SlotDataDict[slot] = slotData = new SlotData();
                }

                if(ChangeACCColor(slot, theme))
                {
                    slotData.Theme = theme;
                }
            }
        }

        private bool ColorComparison(Color C1, Color C2)
        {
            return Math.Abs(C1.r - C2.r) <= Tolerance && Math.Abs(C1.g - C2.g) <= Tolerance && Math.Abs(C1.b - C2.b) <= Tolerance && Math.Abs(C1.a - C2.a) <= Tolerance;
        }

        private bool ColorComparison(Color[] C1, Color[] C2)
        {
            for(var i = 0; i < 4; i++)
            {
                if(!ColorComparison(C1[i], C2[i]))
                    return false;
            }

            return true;
        }

        private void RelativeSkew(bool undo = false)
        {

            var input = RelativeSkewColor;

            Color.RGBToHSV(input, out var In_Hue, out var _, out var _);
            var In_S = SaturationSlider;
            var In_V = ValuesSlider;
            var list = Relative_ACC_Dictionary;
            var UndoACCQueue = new Queue<Color>();
            var ClothesUndoQueue = new Queue<Color>();
            if(undo)
            {
                UndoACCQueue = UndoACCSkew.Pop();
                ClothesUndoQueue = ClothsUndoSkew.Pop();
            }

            for(int i = 0, iN = list.Count; i < iN; i++)
            {
                var list2 = list[i];

                for(int j = 0, jn = list2.Count; j < jn; j++)
                {
                    var themenum = list2[j][0];
                    var colornum = list2[j][1];
                    var theme = Themes[themenum];
                    var update = theme.ThemedSlots;
                    var color = theme.BaseColors[colornum];

                    Color.RGBToHSV(color, out var T_Hue, out var T_S, out var T_V);

                    if(undo)
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

                    theme.BaseColors[colornum] = color;
                    for(int k = 0, kn = update.Count; k < kn; k++)
                    {
                        ChangeACCColor(update[k], themenum);
                    }
                }
            }

            var clothes = ChaControl.chaFile.coordinate[(int)CharaEvent.CurrentCoordinate.Value].clothes.parts;
            var clothes2 = ChaControl.nowCoordinate.clothes.parts;
            for(var i = 0; i < clothes.Length; i++)
            {
                for(var j = 0; j < clothes[i].colorInfo.Length; j++)
                {
                    var temp = clothes[i].colorInfo[j].baseColor;
                    Color.RGBToHSV(temp, out var T_Hue, out var T_S, out var T_V);
                    if(undo)
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
                    if(undo)
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
            if(!undo)
            {
                UndoACCSkew.Push(UndoACCQueue);
                ClothsUndoSkew.Push(ClothesUndoQueue);
            }
        }

        private void FindRelativeColors()
        {
            var check = new List<Color[]>();
            var themes = CharaEvent.Themes;
            //TODO: Check why I had excluded is relative
            foreach(var themeData in themes)
            {
                check.Add(themeData.BaseColors);
            }

            var relativeDictionary = new Dictionary<Color, HashSet<RelativeColor>>();
            var skipThemes = new List<ThemeData>();
            for(var t1 = 0; t1 < themes.Count; t1++)
            {
                var themeData = themes[t1];
                for(var c1 = 0; c1 < themeData.BaseColors.Length; c1++)
                {
                    var color1 = themeData.BaseColors[c1];
                    if(relativeDictionary.Any(x => ColorComparison(x.Key, color1)))
                        continue;
                    var relativeColors = new HashSet<RelativeColor>() { new RelativeColor(themeData, c1) };
                    for(var t2 = t1; t2 < themes.Count; t2++)
                    {
                        var themeDate2 = themes[t2];
                        for(var c2 = t1 == t2 ? c1 + 1 : 0; c2 < themeDate2.BaseColors.Length; c2++)
                        {
                            if(!ColorComparison(themeDate2.BaseColors[c2], color1))
                                continue;
                            relativeColors.Add(new RelativeColor(themeDate2, c2));
                        }
                    }

                    relativeDictionary[color1] = relativeColors;
                }
            }
        }

        private void RelativeAssignColors(Color input)
        {
            if(!MakerAPI.InsideAndLoaded)
            {
                return;
            }

            var list = Relative_ACC_Dictionary[RelativeDropdown.Value];

            for(int i = 0, listlength = list.Count; i < listlength; i++)
            {
                var themenum = list[i][0];
                var theme = Themes[themenum];
                theme.BaseColors[list[i][1]] = input;

                var update = theme.ThemedSlots;
                for(int j = 0, n = update.Count; j < n; j++)
                {
                    ChangeACCColor(update[j], themenum);
                }
            }
        }

        internal class RelativeColor
        {
            public ThemeData Theme { get; set; }
            public int ColorNum { get; set; }

            public RelativeColor(ThemeData theme, int colorNum)
            {
                Theme = theme;
                this.ColorNum = colorNum;
            }

            public override bool Equals(object obj)
            {
                return obj is RelativeColor color &&
                       EqualityComparer<ThemeData>.Default.Equals(Theme, color.Theme) &&
                       ColorNum == color.ColorNum;
            }

            public override int GetHashCode()
            {
                var hashCode = -1395371734;
                hashCode = hashCode * -1521134295 + EqualityComparer<ThemeData>.Default.GetHashCode(Theme);
                hashCode = hashCode * -1521134295 + ColorNum.GetHashCode();
                return hashCode;
            }
        }
    }
}
