using ExtensibleSaveFormat;
using KKAPI.Chara;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accessory_States
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        public static bool DisableRefresh;

        internal static bool ASSExists;

        public static event EventHandler<CoordinateLoadedEventARG> Coordloaded;

        private byte lastknownshoetype = 0;

        private bool ShowSub = true;
        private bool ShowMain = true;

        internal CoordinateData NowCoordinateData = new CoordinateData();

        internal Dictionary<int, SlotData> SlotBindingData = new Dictionary<int, SlotData>();

        internal Dictionary<string, bool> ParentedNameDictionary = new Dictionary<string, bool>();


        #region Properties
        internal ChaFileCoordinate NowCoordinate => ChaControl.nowCoordinate;

        internal ChaFileAccessory.PartsInfo[] PartsArray => ChaControl.nowCoordinate.accessory.parts;

        internal readonly List<NameData> Names = new List<NameData>();

        internal bool[] ClothNotData
        {
            get { return NowCoordinateData.ClothingNotData; }
            set { NowCoordinateData.ClothingNotData = value; }
        }

        public static bool StopMakerLoop { get; internal set; }

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
        }
        internal void UpdateParentedDict()
        {
            ParentedNameDictionary.Clear();
            foreach (var item in SlotBindingData)
            {
                if (!item.Value.Parented) continue;
                ParentedNameDictionary[PartsArray[item.Key].parentKey] = true;
            }
        }

        internal void ClearNowCoordinate()
        {
            NowCoordinateData.Clear();
            ParentedNameDictionary.Clear();
            SlotBindingData.Clear();
            Names.Clear();
            Names.AddRange(Constants.GetNameDataList());
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
            SetAccessoryExtData(slotData.Serialize(), slot);
        }

        private void SaveCoordinateData()
        {
            ChaControl.nowCoordinate.accessory.SetExtendedDataById(Settings.GUID, NowCoordinateData.Serialize());
        }

        internal void LoadSlotData(int slot)
        {
            if (slot >= PartsArray.Length)
            {
                return;
            }

            var extendedData = GetAccessoryExtData(slot);
            if (extendedData == null) return;

            if (extendedData.version > 2)
            {
                Settings.Logger.LogMessage($"{ChaControl.fileParam.fullname}: New version of Accessory States detected");
            }

            if (extendedData.data.TryGetValue(Constants.AccessoryKey, out var bytearray) && bytearray != null)
            {
                var slotdata = SlotBindingData[slot] = MessagePackSerializer.Deserialize<SlotData>((byte[])bytearray);
                foreach (var item in slotdata.bindingDatas)
                {
                    item.SetSlot(slot);
                    var binding = item.GetBinding();
                    if (binding < 0)
                    {
                        if (item.NameData == null) continue;
                        //re-value binding reference
                        var nameDataReference = Names.FirstOrDefault(x => x.Equals(item.NameData));

                        if (nameDataReference == null)
                        {
                            Names.Add(item.NameData);
                            item.NameData.Binding = Constants.ClothingLength + Names.IndexOf(nameDataReference);
                        }
                        else
                        {
                            nameDataReference.MergeStatesWith(item.NameData);
                            item.NameData = nameDataReference;
                        }

                        item.SetBinding();
                        //nameDataReference.AssociatedSlots.Add(slot);
                        continue;
                    }

                    if (binding < Constants.ClothingLength)
                    {
                        item.NameData = Names.First(x => x.Binding == binding);
                        //item.NameData.AssociatedSlots.Add(slot);
                    }
                }
            }
        }
    }
}
