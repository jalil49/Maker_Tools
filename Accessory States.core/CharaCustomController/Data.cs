using System;
using System.Collections.Generic;
using System.Linq;
using ExtensibleSaveFormat;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;

namespace Accessory_States
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        public static bool DisableRefresh;

        internal static bool AssExists;

        internal readonly List<NameData> NameDataList = new List<NameData>();

        internal CoordinateData NowCoordinateData = new CoordinateData();

        internal readonly Dictionary<string, ParentedData> ParentedNameDictionary = new Dictionary<string, ParentedData>();
        private bool _showMain = true;

        private bool _showSub = true;

        internal readonly Dictionary<int, SlotData> SlotBindingData = new Dictionary<int, SlotData>();

        public static event EventHandler<CoordinateLoadedEventArg> Coordloaded;

        internal void UpdatePluginData()
        {
            ClearNowCoordinate();
            NowCoordinate.accessory.TryGetExtendedDataById(Settings.Guid, out var pluginData);

            if (pluginData != null)
            {
                if (pluginData.version == 2)
                {
                    if (pluginData.data.TryGetValue(Constants.CoordinateKey, out var bytedata) && bytedata != null)
                        NowCoordinateData = MessagePackSerializer.Deserialize<CoordinateData>((byte[])bytedata);
                }
                else
                {
                    Settings.Logger.LogMessage("New version of Accessory States detected, Please Update");
                }
            }

            for (var i = 0; i < PartsArray.Length; i++) LoadSlotData(i);

            UpdateParentedDict();

            var args = new CoordinateLoadedEventArg(ChaControl /*, coordinate*/);
            if (!(Coordloaded == null || Coordloaded.GetInvocationList().Length == 0))
                try
                {
                    Coordloaded?.Invoke(null, args);
                }
                catch (Exception ex)
                {
                    Settings.Logger.LogError(string.Format("Subscriber crash in {0}.{1} - {2}", nameof(Hooks),
                        nameof(Coordloaded), ex));
                }

            RefreshSlots();
        }

        internal void UpdateParentedDict()
        {
            ParentedNameDictionary.Clear();
            foreach (var item in SlotBindingData)
            {
                if (!item.Value.parented)
                    continue;
                if (!ParentedNameDictionary.TryGetValue(PartsArray[item.Key].parentKey, out var parentedData))
                    parentedData = ParentedNameDictionary[PartsArray[item.Key].parentKey] = new ParentedData();

                parentedData.AssociateSlots.Add(item.Key);
            }
        }

        internal void ClearNowCoordinate()
        {
            NowCoordinateData.Clear();
            ParentedNameDictionary.Clear();
            SlotBindingData.Clear();
            NameDataList.Clear();
            NameDataList.AddRange(Constants.GetNameDataList());
        }

        internal void SaveSlotData(int slot)
        {
            if (slot >= PartsArray.Length) return;

            if (SlotBindingData.TryGetValue(slot, out var slotdata))
            {
                SaveSlotData(slot, slotdata);
                return;
            }

            SetAccessoryExtData(null, slot);
        }

        internal void SaveSlotData(int slot, SlotData slotData)
        {
            if (slot >= PartsArray.Length) return;

            var pluginData = PartsArray[slot].type != 120 ? slotData.Serialize() : null;
            if (MakerAPI.InsideAndLoaded) SetAccessoryExtData(pluginData, slot, (int)CurrentCoordinate.Value);

            SetAccessoryExtData(pluginData, slot);
        }

        internal void SaveCoordinateData()
        {
            var pluginData = NowCoordinateData.Serialize();
            ChaControl.nowCoordinate.accessory.SetExtendedDataById(Settings.Guid, pluginData);
            if (MakerAPI.InsideAndLoaded)
                ChaFileControl.coordinate[(int)CurrentCoordinate.Value].accessory
                    .SetExtendedDataById(Settings.Guid, pluginData);
        }

        internal void LoadSlotData(int slot)
        {
            if (slot >= PartsArray.Length || PartsArray[slot].type == 120) return;

            var extendedData = GetAccessoryExtData(slot);
            if (extendedData == null)
            {
                Settings.Logger.LogWarning("No data in slot " + slot);
                return;
            }

            if (extendedData.version > 2)
            {
                Settings.Logger.LogMessage(
                    $"{ChaControl.fileParam.fullname}: New version of Accessory States detected");
                return;
            }

            if (extendedData.data.TryGetValue(Constants.AccessoryKey, out var bytearray) && bytearray != null)
            {
                Settings.Logger.LogWarning("Loaded Data slot " + slot);
                var slotdata = SlotBindingData[slot] = MessagePackSerializer.Deserialize<SlotData>((byte[])bytearray);
                foreach (var item in slotdata.bindingDatas)
                {
                    item.SetSlot(slot);
                    var binding = item.GetBinding();
                    if (binding < 0)
                    {
                        if (item.NameData == null)
                            continue;
                        //re-value binding reference
                        var nameDataReference = NameDataList.FirstOrDefault(x => x.Equals(item.NameData, true));

                        if (nameDataReference == null)
                        {
                            NameDataList.Add(item.NameData);
                            item.NameData.binding = Constants.ClothingLength + NameDataList.IndexOf(nameDataReference);
                        }
                        else
                        {
                            nameDataReference.MergeStatesWith(item.NameData);
                            item.NameData = nameDataReference;
                        }

                        nameDataReference.AssociatedSlots.Add(slot);
                        continue;
                    }

                    if (binding < Constants.ClothingLength)
                    {
                        item.NameData = NameDataList.First(x => x.binding == binding);
                        item.NameData.AssociatedSlots.Add(slot);
                    }
                }
            }
        }

        internal void ParentRemove(int slotNo, string parentStr)
        {
            if (!SlotBindingData.TryGetValue(slotNo, out var slotData) || !slotData.parented)
                return;

            if (!ParentedNameDictionary.TryGetValue(parentStr, out var nameData))
                ParentedNameDictionary[parentStr] = nameData = new ParentedData();

            nameData.AssociateSlots.Remove(slotNo);

            if (nameData.AssociateSlots.Count == 0) ParentedNameDictionary.Remove(parentStr);
        }

        internal void ParentUpdate(int slotNo, string parentStr)
        {
            if (!SlotBindingData.TryGetValue(slotNo, out var slotData) || !slotData.parented)
                return;

            if (!ParentedNameDictionary.TryGetValue(parentStr, out var nameData))
                ParentedNameDictionary[parentStr] = nameData = new ParentedData();

            nameData.AssociateSlots.Add(slotNo);
        }

        public class ParentedData
        {
            public readonly HashSet<int> AssociateSlots = new HashSet<int>();
            public bool Show = true;

            public bool Toggle()
            {
                Show = !Show;
                return Show;
            }
        }

        #region Properties

        public bool[] ClothNotData
        {
            get => NowCoordinateData.clothingNotData;
            set => NowCoordinateData.clothingNotData = value;
        }

        internal ChaFileCoordinate NowCoordinate => ChaControl.nowCoordinate;

        internal ChaFileAccessory.PartsInfo[] PartsArray => ChaControl.nowCoordinate.accessory.parts;

        public int AssShoePreference
        {
            get => NowCoordinateData.AssShowPreference;
            set
            {
                NowCoordinateData.AssShowPreference = value;
                SaveCoordinateData();
            }
        }

        #endregion
    }
}