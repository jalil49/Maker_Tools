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

        internal Dictionary<int, SlotData> SlotInfo = new Dictionary<int, SlotData>();

        internal Dictionary<string, List<KeyValuePair<int, SlotData>>> ParentedNameDictionary = new Dictionary<string, List<KeyValuePair<int, SlotData>>>();

        #region Properties
        private ChaFileCoordinate NowCoordinate => ChaControl.nowCoordinate;

        private ChaFileAccessory.PartsInfo[] PartsArray => ChaControl.nowCoordinate.accessory.parts;

        internal readonly List<NameData> Names = new List<NameData>();

        //internal bool[] ClothNotData
        //{
        //    get { return NowCoordinateData.ClothNotData; }
        //    set { NowCoordinateData.ClothNotData = value; }
        //}

        //internal bool ForceClothDataUpdate
        //{
        //    get { return NowCoordinateData.ForceClothNotUpdate; }
        //    set { NowCoordinateData.ForceClothNotUpdate = value; }
        //}

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
            if (SlotInfo.TryGetValue(slot, out var slotdata))
            {
                SaveSlotData(slot, slotdata);
                return;
            }
            SetAccessoryExtData(null, slot);
        }

        private void SaveSlotData(int slot, SlotData slotData)
        {
            if (slot >= PartsArray.Length || slotData.ShouldSave())
            {
                return;
            }
            var savedata = new PluginData() { version = 2 };
            savedata.data[Constants.AccessoryKey] = slotData.Serialize();
            PartsArray[slot].SetExtendedDataById(Settings.GUID, savedata);
            SetAccessoryExtData(savedata, slot);
        }

        private void SaveCoordinateData()
        {
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
            var extendedData = GetAccessoryExtData(slot);
            if (extendedData != null)
            {
                if (extendedData.version > 2)
                {
                    Settings.Logger.LogMessage($"{ChaControl.fileParam.fullname}: New version of Accessory States detected");
                }

                if (extendedData.data.TryGetValue(Constants.AccessoryKey, out var bytearray) && bytearray != null)
                {
                    var slotdata = SlotInfo[slot] = MessagePackSerializer.Deserialize<SlotData>((byte[])bytearray);
                    foreach (var item in slotdata.bindingDatas)
                    {
                        var binding = item.GetBinding();
                        if (binding < 0)
                        {
                            //re-value binding reference
                            var nameDataReference = Names.FirstOrDefault(x => x.Equals(item.NameData));

                            if (nameDataReference == null)
                            {
                                Names.Add(item.NameData);
                            }
                            else
                            {
                                nameDataReference.MergeStatesWith(item.NameData);
                                item.NameData = nameDataReference;
                            }
                            item.SetBinding(Constants.ClothingLength + Names.IndexOf(nameDataReference));
                            continue;
                        }

                        //use constant 

                    }
                }
            }
        }

        private List<StateInfo> GetMasterList()
        {
            var masterList = new List<StateInfo>();
            foreach (var item in SlotInfo)
            {

            }
            return masterList;
        }
    }
}
