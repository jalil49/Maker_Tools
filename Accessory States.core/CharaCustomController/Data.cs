using ExtensibleSaveFormat;
using KKAPI.Chara;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using static ExtensibleSaveFormat.Extensions;

namespace Accessory_States
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        public static bool DisableRefresh;

        internal static bool ASSExists;

        public static event EventHandler<CoordinateLoadedEventARG> Coordloaded;

        private bool ShowSub = true;
        private bool ShowMain = true;

        internal CoordinateData NowCoordinateData = new CoordinateData();

        internal Dictionary<int, SlotData> SlotBindingData = new Dictionary<int, SlotData>();

        internal Dictionary<string, ParentedData> ParentedNameDictionary = new Dictionary<string, ParentedData>();

        internal readonly List<NameData> NameDataList = new List<NameData>();

        #region Properties
        public bool[] ClothNotData
        {
            get { return NowCoordinateData.ClothingNotData; }
            set { NowCoordinateData.ClothingNotData = value; }
        }

        internal ChaFileCoordinate NowCoordinate => ChaControl.nowCoordinate;

        internal ChaFileAccessory.PartsInfo[] PartsArray => ChaControl.nowCoordinate.accessory.parts;

        public int AssShoePreference
        {
            get { return NowCoordinateData.AssShowPreference; }
            set
            {
                NowCoordinateData.AssShowPreference = value;
                SaveCoordinateData();
            }
        }

        #endregion

        internal void UpdatePluginData()
        {
            ClearNowCoordinate();
            NowCoordinate.accessory.TryGetExtendedDataById(Settings.GUID, out var pluginData);

            if (pluginData != null)
            {
                if (pluginData.version == 2)
                {
                    if (pluginData.data.TryGetValue(Constants.CoordinateKey, out var bytedata) && bytedata != null)
                    {
                        NowCoordinateData = MessagePackSerializer.Deserialize<CoordinateData>((byte[])bytedata);
                    }
                }
                else
                {
                    Settings.Logger.LogMessage("New version of Accessory States detected, Please Update");
                }
            }

            for (var i = 0; i < PartsArray.Length; i++)
            {
                LoadSlotData(i);
            }

            UpdateParentedDict();

            var args = new CoordinateLoadedEventARG(ChaControl/*, coordinate*/);
            if (!(Coordloaded == null || Coordloaded.GetInvocationList().Length == 0))
            {
                try
                {
                    Coordloaded?.Invoke(null, args);
                }
                catch (Exception ex)
                {
                    Settings.Logger.LogError($"Subscriber crash in {nameof(Hooks)}.{nameof(Coordloaded)} - {ex}");
                }
            }

            RefreshSlots();
        }

        internal void UpdateParentedDict()
        {
            ParentedNameDictionary.Clear();
            foreach (var item in SlotBindingData)
            {
                if (!item.Value.Parented)
                    continue;
                if (!ParentedNameDictionary.TryGetValue(PartsArray[item.Key].parentKey, out var parentedData))
                { parentedData = ParentedNameDictionary[PartsArray[item.Key].parentKey] = new ParentedData(); }
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
            if (slot >= PartsArray.Length)
            {
                return;
            }
            if (SlotBindingData.TryGetValue(slot, out var slotdata))
            {
                SaveSlotData(slot, slotdata);
                return;
            }
            SetAccessoryExtData(null, slot);
        }

        internal void SaveSlotData(int slot, SlotData slotData)
        {
            if (slot >= PartsArray.Length)
            {
                return;
            }
            var pluginData = PartsArray[slot].type != 120 ? slotData.Serialize() : null;
            if (KKAPI.Maker.MakerAPI.InsideAndLoaded)
            {
                SetAccessoryExtData(pluginData, slot, (int)CurrentCoordinate.Value);
            }
            SetAccessoryExtData(pluginData, slot);
        }

        internal void SaveCoordinateData()
        {
            var pluginData = NowCoordinateData.Serialize();
            ChaControl.nowCoordinate.accessory.SetExtendedDataById(Settings.GUID, pluginData);
            if (KKAPI.Maker.MakerAPI.InsideAndLoaded)
            {
                ChaFileControl.coordinate[(int)CurrentCoordinate.Value].accessory.SetExtendedDataById(Settings.GUID, pluginData);
            }
        }

        internal void LoadSlotData(int slot)
        {
            if (slot >= PartsArray.Length || PartsArray[slot].type == 120)
            {
                return;
            }

            var extendedData = GetAccessoryExtData(slot);
            if (extendedData == null)
            {
                Settings.Logger.LogWarning("No data in slot " + slot);
                return;
            }

            if (extendedData.version > 2)
            {
                Settings.Logger.LogMessage($"{ChaControl.fileParam.fullname}: New version of Accessory States detected");
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
                        var nameDataReference = NameDataList.FirstOrDefault(x => x.Equals(item.NameData));

                        if (nameDataReference == null)
                        {
                            NameDataList.Add(item.NameData);
                            item.NameData.Binding = Constants.ClothingLength + NameDataList.IndexOf(nameDataReference);
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
                        item.NameData = NameDataList.First(x => x.Binding == binding);
                        item.NameData.AssociatedSlots.Add(slot);
                    }
                }
            }
        }

        public class ParentedData
        {
            public bool Show = true;
            public readonly HashSet<int> AssociateSlots = new HashSet<int>();

            public bool Toggle()
            {
                Show = !Show;
                return Show;
            }
        }

        internal void ParentRemove(int slotNo, string parentStr)
        {
            if (!SlotBindingData.TryGetValue(slotNo, out var slotData) || !slotData.Parented)
                return;

            if (!ParentedNameDictionary.TryGetValue(parentStr, out var nameData))
            {
                ParentedNameDictionary[parentStr] = nameData = new ParentedData();
            }

            nameData.AssociateSlots.Remove(slotNo);

            if (nameData.AssociateSlots.Count == 0)
            {
                ParentedNameDictionary.Remove(parentStr);
            }
        }

        internal void ParentUpdate(int slotNo, string parentStr)
        {
            if (!SlotBindingData.TryGetValue(slotNo, out var slotData) || !slotData.Parented)
                return;

            if (!ParentedNameDictionary.TryGetValue(parentStr, out var nameData))
            {
                ParentedNameDictionary[parentStr] = nameData = new ParentedData();
            }

            nameData.AssociateSlots.Add(slotNo);
        }
    }
}
