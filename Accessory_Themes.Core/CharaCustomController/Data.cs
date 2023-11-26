using KKAPI.Chara;
using System.Collections.Generic;
using UnityEngine;
using static ExtensibleSaveFormat.Extensions;

namespace Accessory_Themes
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        internal readonly List<ThemeData> Themes = new List<ThemeData>();

        internal readonly Dictionary<int, SlotData> SlotDataDict = new Dictionary<int, SlotData>();

        internal readonly Dictionary<Color, HashSet<RelativeColor>> RelativeDictionary = new Dictionary<Color, HashSet<RelativeColor>>();

        private ChaFileAccessory.PartsInfo[] Parts => ChaControl.nowCoordinate.accessory.parts;

        internal void Clear()
        {
            Themes.Clear();
            SlotDataDict.Clear();
        }

        internal void SaveSlot()
        {
            var slot = KKAPI.Maker.AccessoriesApi.SelectedMakerAccSlot;
            if(slot < 0 || slot >= Parts.Length)
            {
                return;
            }

            SaveSlot(slot);
        }

        internal void SaveSlot(int slot)
        {
            if(!SlotDataDict.TryGetValue(slot, out var SlotData))
            {
                Parts[slot].SetExtendedDataById(Settings.GUID, null);
                return;
            }

            Parts[slot].SetExtendedDataById(Settings.GUID, SlotData.Serialize());
        }

        internal void LoadSlot(int slot)
        {
            if(slot >= Parts.Length)
            {
                return;
            }

            SlotDataDict.Remove(slot);

            if(Parts[slot].TryGetExtendedDataById(Settings.GUID, out var pluginData))
            {
                var slotdata = Migrator.Migrator.SlotDataMigrate(pluginData);
                if(slotdata != null)
                {
                    SlotDataDict[slot] = slotdata;

                    if(Themes.Find(x => slotdata.Equals(x)) == null)
                    {
                        Themes.Add(slotdata.Theme);
                    }

                    return;
                }
            }
        }
    }
}
