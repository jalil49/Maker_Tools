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

        private ChaFile chafile;

        public static event EventHandler<CoordinateLoadedEventARG> Coordloaded;

        private byte lastknownshoetype = 0;

        private bool ShowSub = true;
        private bool ShowMain = true;

        internal CoordinateData NowCoordinateData = new CoordinateData();

        internal Dictionary<int, SlotData> SlotInfo = new Dictionary<int, SlotData>();

        internal Dictionary<string, List<KeyValuePair<int, SlotData>>> ParentedNameDictionary = new Dictionary<string, List<KeyValuePair<int, SlotData>>>();

        #region Properties
        private ChaFileCoordinate NowCoordinate => ChaControl.nowCoordinate;

        private ChaFileAccessory.PartsInfo[] PartsArray => ChaControl.nowCoordinate.accessory.parts;

        internal List<NameData> Names
        {
            get { return NowCoordinateData.Names; }
            set { NowCoordinateData.Names = value; }
        }

        internal bool[] ClothNotData
        {
            get { return NowCoordinateData.ClothNotData; }
            set { NowCoordinateData.ClothNotData = value; }
        }

        internal bool ForceClothDataUpdate
        {
            get { return NowCoordinateData.ForceClothNotUpdate; }
            set { NowCoordinateData.ForceClothNotUpdate = value; }
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

            StartCoroutine(WaitForSlots());
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
            var shoetype = ChaControl.fileStatus.shoesType;
            var ParentedList = SlotInfo.Where(x => x.Value.Parented && (x.Value.ShoeType == 2 || x.Value.ShoeType == shoetype));
            foreach (var item in ParentedList)
            {
                var parentkey = PartsArray[item.Key].parentKey;
                if (ParentedNameDictionary.TryGetValue(parentkey, out var keyValuePairs))
                {
                    keyValuePairs.Add(item);
                    continue;
                }
                ParentedNameDictionary[parentkey] = new List<KeyValuePair<int, SlotData>>() { item };
            }
        }

        internal void ClearNowCoordinate()
        {
            NowCoordinateData.Clear();
            ParentedNameDictionary.Clear();
            SlotInfo.Clear();
        }

        private void SaveSlotData(int slot)
        {
            if (slot >= PartsArray.Length)
            {
                return;
            }
            if (SlotInfo.TryGetValue(slot, out var slotdata) && !(slotdata.Binding < 0 && !slotdata.Parented))
            {
                SaveSlotData(slot, slotdata);
                return;
            }
            PartsArray[slot].SetExtendedDataById(Settings.GUID, null);
        }

        private void SaveSlotData(int slot, SlotData slotData)
        {
            if (slot >= PartsArray.Length || slotData.Binding < 0 && !slotData.Parented)
            {
                return;
            }
            var savedata = new PluginData() { version = 2 };
            savedata.data[Constants.AccessoryKey] = slotData.Serialize();
            PartsArray[slot].SetExtendedDataById(Settings.GUID, savedata);
        }

        private void SaveCoordinateData()
        {
            NowCoordinateData.CleanUp(SlotInfo);
            var plugin = new PluginData() { version = 2 };
            plugin.data[Constants.CoordinateKey] = NowCoordinateData.Serialize();
            ChaControl.nowCoordinate.accessory.SetExtendedDataById(Settings.GUID, plugin);
        }

        private void LoadSlotData(int slot)
        {
            if (slot >= PartsArray.Length)
            {
                return;
            }
            if (PartsArray[slot].TryGetExtendedDataById(Settings.GUID, out var extendeddata) && extendeddata.version == 2 && extendeddata.data.TryGetValue(Constants.AccessoryKey, out var bytearray) && bytearray != null)
            {
                var slotdata = SlotInfo[slot] = MessagePackSerializer.Deserialize<SlotData>((byte[])bytearray);
                if (!Names.Any(x => x.Name == slotdata.GroupName))
                {
                    Names.Add(new NameData() { Name = slotdata.GroupName });
                }
                //confirm binding with name index
                if (slotdata.Binding >= Constants.ClothingLength)
                    slotdata.Binding = Constants.ClothingLength + Names.FindIndex(x => x.Name == slotdata.GroupName);
            }
        }
    }
}
