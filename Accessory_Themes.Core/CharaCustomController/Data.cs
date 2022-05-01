using KKAPI.Chara;
using System.Collections.Generic;
using UnityEngine;
using static ExtensibleSaveFormat.Extensions;

namespace Accessory_Themes
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        #region ACI reference
        private Additional_Card_Info.CharaEvent ACI_Ref;

        private Dictionary<int, Additional_Card_Info.SlotInfo> HairAcc => ACI_Ref.SlotInfo;
        #endregion

        private readonly List<ThemeData> Themes = new List<ThemeData>();

        private readonly Dictionary<int, SlotData> SlotDataDict = new Dictionary<int, SlotData>();

        private readonly Dictionary<int, int> Theme_Dict = new Dictionary<int, int>();

        private readonly Dictionary<int, List<int[]>> Relative_ACC_Dictionary = new Dictionary<int, List<int[]>>();

        private readonly Stack<Queue<Color>> UndoACCSkew = new Stack<Queue<Color>>();

        private readonly Stack<Queue<Color>> ClothsUndoSkew = new Stack<Queue<Color>>();

        private void Clear()
        {
            Themes.Clear();
            SlotDataDict.Clear();
            Theme_Dict.Clear();
            Relative_ACC_Dictionary.Clear();
            UndoACCSkew.Clear();
            ClothsUndoSkew.Clear();
        }

        private void SaveSlot()
        {
            var slot = KKAPI.Maker.AccessoriesApi.SelectedMakerAccSlot;
            if (slot < 0 || slot >= Parts.Length)
            {
                return;
            }
            SaveSlot(slot);
        }

        private void SaveSlot(int slot)
        {
            if (!SlotDataDict.TryGetValue(slot, out var slotInfo))
            {
                Parts[slot].SetExtendedDataById(Settings.GUID, null);
                return;
            }
            Parts[slot].SetExtendedDataById(Settings.GUID, slotInfo.Serialize());
        }

        private void LoadSlot(int slot)
        {
            if (slot >= Parts.Length)
            {
                return;
            }
            if (Parts[slot].TryGetExtendedDataById(Settings.GUID, out var pluginData))
            {
                var slotdata = Migrator.SlotDataMigrate(pluginData);
                if (slotdata != null)
                {
                    SlotDataDict[slot] = slotdata;
                    var themeindex = Themes.FindIndex(x => x.ThemeName == slotdata.ThemeName);
                    if (themeindex < 0)
                    {
                        themeindex = Themes.Count;
                        Themes.Add(new ThemeData(slotdata.ThemeName));
                    }
                    Themes[themeindex].ThemedSlots.Add(slot);
                    Theme_Dict[slot] = themeindex;
                    return;
                }
            }
            SlotDataDict.Remove(slot);
            foreach (var item in Themes)
            {
                item.ThemedSlots.Remove(slot);
            }
            Theme_Dict.Remove(slot);
            Relative_ACC_Dictionary.Remove(slot);
        }
    }
}
