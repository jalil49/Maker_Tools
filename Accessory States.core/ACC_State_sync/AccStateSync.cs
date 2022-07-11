using ExtensibleSaveFormat;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using static ExtensibleSaveFormat.Extensions;

namespace Accessory_States
{
    public static partial class AccStateSync
    {
        [Serializable]
        [MessagePackObject]
        public class TriggerProperty
        {
            [IgnoreMember]
            public const string SerializeKey = "TriggerPropertyList";

            [Key("Coordinate")]
            public int Coordinate { get; set; } = -1;

            [Key("Slot")]
            public int Slot { get; set; } = -1;

            [Key("RefKind")]
            public int RefKind { get; set; } = -1;

            [Key("RefState")]
            public int RefState { get; set; } = -1;

            [Key("Visible")]
            public bool Visible { get; set; } = true;

            [Key("Priority")]
            public int Priority { get; set; } = 0;

            [Key("RenderShow")]
            public HashSet<string> RenderShow;

            [Key("RenderHide")]
            public HashSet<string> RenderHide;

            [SerializationConstructor]
            public TriggerProperty(int coordinate, int slot, int refkind, int refstate, bool visible, int priority, HashSet<string> rendershow = null, HashSet<string> renderhide = null) // this shit is here to avoid msgpack fucking error
            {
                Coordinate = coordinate;
                Slot = slot;
                RefKind = refkind;
                RefState = refstate;
                Visible = visible;
                Priority = priority;
                RenderShow = rendershow ?? new HashSet<string>();
                RenderHide = renderhide ?? new HashSet<string>();
            }

            public TriggerProperty(StateInfo stateInfo, int coordinate, int slot)
            {
                Coordinate = coordinate;
                Slot = slot;
                RefKind = stateInfo.Binding;
                RefState = stateInfo.State;
                Visible = stateInfo.Show;
                Priority = stateInfo.Priority;
            }

            public StateInfo ToStateInfo()
            {
                return new StateInfo()
                {
                    Priority = Priority,
                    State = RefState,
                    Show = Visible,
                    Slot = Slot,
                    ShoeType = 2,
                    Binding = RefKind
                };
            }
        }

        [Serializable]
        [MessagePackObject]
        public class TriggerGroup
        {
            [IgnoreMember]
            public const string SerializeKey = "TriggerGroupList";

            [Key("Coordinate")]
            public int Coordinate { get; set; } = -1;

            [Key("Kind")]
            public int Kind { get; set; } // TriggerProperty RefGroup

            [Key("State")]
            public int State { get; set; } // Current RefState

            [Key("States")]
            public Dictionary<int, string> States { get; set; }

            [Key("Label")]
            public string Label { get; set; } = "";

            [Key("Startup")]
            public int Startup { get; set; } = 0;

            [Key("Secondary")]
            public int Secondary { get; set; } = -1;

            [Key("GUID")]
            public string GUID
            {
                get
                {
                    if (_guid == null)
                    { _guid = Guid.NewGuid().ToString("D").ToUpper(); }
                    return _guid;
                }
                set { _guid = value; }
            }

            private string _guid;

            [SerializationConstructor]
            public TriggerGroup(int coordinate, int kind, string label, int state, int startup, int secondary)
            {
                Coordinate = coordinate;
                Kind = kind;
                State = state;
                if (label.Trim().IsNullOrEmpty())
                    label = $"Custom {kind - 8}";
                Label = label;
                Startup = startup;
                Secondary = secondary;

                States = new Dictionary<int, string>() { [0] = "State 1", [1] = "State 2" };
            }
            public TriggerGroup(int coordinate, int kind, string label, int startup, int secondary) : this(coordinate, kind, label, 0, startup, secondary) { }
            public TriggerGroup(int coordinate, int kind, string label = "") : this(coordinate, kind, label, 0, 0, -1) { }
            public TriggerGroup(NameData nameData, int coord)
            {
                Coordinate = coord;
                Kind = nameData.Binding;
                State = nameData.CurrentState;
                States = nameData.StateNames;
                Label = nameData.Name;
                Startup = nameData.DefaultState;
            }

            public NameData ToNameData()
            {
                return new NameData
                {
                    Name = Label,
                    Binding = Kind,
                    StateNames = States,
                    CurrentState = Startup,
                    DefaultState = State
                };
            }
        }

        internal static List<string> _cordNames = new List<string>();
#if KK
        internal static List<string> _clothesNames = new List<string>() { "Top", "Bottom", "Bra", "Underwear", "Gloves", "Pantyhose", "Legwear", "Indoors", "Outdoors" };
#else
        internal static List<string> _clothesNames = new List<string>() { "Top", "Bottom", "Bra", "Underwear", "Gloves", "Pantyhose", "Legwear", "Shoes" };
#endif
        internal static Dictionary<int, string> _statesNames = new Dictionary<int, string>() { [0] = "Full", [1] = "Half 1", [2] = "Half 2", [3] = "Undressed" };
        internal static Dictionary<string, string> _accessoryParentNames = new Dictionary<string, string>();
        public static void ConvertCoordinateToAss(int coord, int AssShoePreference, ChaFileCoordinate coordinate, ref List<TriggerProperty> triggers, ref List<TriggerGroup> groups)
        {
            var localNames = Constants.GetNameDataList();
            var slot = 0;
            foreach (var part in coordinate.accessory.parts)
            {
                GetSlotData(slot, AssShoePreference, part, ref localNames, coord, ref triggers);
                slot++;
            }

            foreach (var item in localNames)
            {
                groups.Add(new TriggerGroup(item, coord));
            }
        }

        public static SlotData GetSlotData(int slot, int AssShoePreference, ChaFileAccessory.PartsInfo part, ref List<NameData> localNames, int coord, ref List<TriggerProperty> triggers)
        {
            if (!part.TryGetExtendedDataById(Settings.GUID, out var extendedData) || extendedData == null || extendedData.version > 2 || !extendedData.data.TryGetValue(Constants.AccessoryKey, out var bytearray) || bytearray == null)
            {
                return null;
            }

            var slotdata = MessagePackSerializer.Deserialize<SlotData>((byte[])bytearray);
            foreach (var item in slotdata.bindingDatas)
            {
                item.SetSlot(slot);
                var binding = item.GetBinding();

                if (binding < 0)
                {
                    if (item.NameData == null)
                        continue;
                    //re-value binding reference
                    var nameDataReference = localNames.FirstOrDefault(x => x.Equals(item.NameData));

                    if (nameDataReference == null)
                    {
                        localNames.Add(item.NameData);
                        item.NameData.Binding = Constants.ClothingLength + localNames.IndexOf(nameDataReference);
                    }
                    else
                    {
                        nameDataReference.MergeStatesWith(item.NameData);
                        item.NameData = nameDataReference;
                    }
                    foreach (var state in item.States)
                    {
                        triggers.Add(new TriggerProperty(state, coord, slot));
                    }
                    continue;
                }

                if (binding < Constants.ClothingLength)
                {
                    item.NameData = localNames.First(x => x.Binding == binding);
                }

                foreach (var state in item.States)
                {
                    if (state.ShoeType == AssShoePreference || state.ShoeType == 2)
                        triggers.Add(new TriggerProperty(state, coord, slot));
                }
            }
            return slotdata;
        }

        public static void FullAssCardSave(ChaFileCoordinate[] coordinates, int AssShoePreference, ref List<TriggerProperty> triggers, ref List<TriggerGroup> groups)
        {
            var coord = 0;
            foreach (var coordinate in coordinates)
            {
                ConvertCoordinateToAss(coord, AssShoePreference, coordinate, ref triggers, ref groups);
                coord++;
            }
        }

        public static void FullAssCardLoad(ChaFileCoordinate[] coordinates, ChaFileCoordinate nowCoordinate, int currentCoord, List<TriggerProperty> triggers, List<TriggerGroup> groups)
        {
            for (var i = 0; i < coordinates.Length; i++)
            {
                ConvertAssCoordinate(coordinates[i], triggers.FindAll(x => x.Coordinate == i), groups.FindAll(x => x.Coordinate == i));
                if (i == currentCoord)
                {
                    ConvertAssCoordinate(nowCoordinate, triggers.FindAll(x => x.Coordinate == i), groups.FindAll(x => x.Coordinate == i));
                }
            }
        }

        public static void ConvertAssCoordinate(ChaFileCoordinate coordinate, List<TriggerProperty> triggers, List<TriggerGroup> groups)
        {
            var localNames = Constants.GetNameDataList();
            var max = localNames.Max(x => x.Binding) + 1;
            //dictionary<slot, refkind, listoftriggers>
            var slotDict = new Dictionary<int, Dictionary<int, List<TriggerProperty>>>();
            var groupRelation = new Dictionary<TriggerGroup, NameData>();
            foreach (var item in groups)
            {
                var newNameData = item.ToNameData();
                var reference = localNames.FirstOrDefault(x => x.Binding == newNameData.Binding || x.Equals(newNameData));
                if (reference != null)
                {
                    newNameData = reference;
                }
                if (newNameData.Binding >= Constants.ClothingLength)
                {
                    newNameData.Binding = max;
                    max++;
                }
                groupRelation[item] = newNameData;
            }

            foreach (var item in triggers)
            {
                if (!slotDict.TryGetValue(item.Slot, out var subDict))
                {
                    subDict = slotDict[item.Slot] = new Dictionary<int, List<TriggerProperty>>();
                }

                if (!subDict.TryGetValue(item.RefKind, out var subRefKindList))
                {
                    slotDict[item.RefKind] = new Dictionary<int, List<TriggerProperty>>();
                }
            }

            var parts = coordinate.accessory.parts;
            foreach (var slotReference in slotDict)
            {
                var part = parts[slotReference.Key];

                if (part.TryGetExtendedDataById(Settings.GUID, out var pluginData) && pluginData != null)
                {
                    continue;
                }
                var slotData = new SlotData();

                foreach (var bindingReference in slotReference.Value)
                {
                    if (bindingReference.Key >= parts.Length)
                        continue;


                    var bindingData = new BindingData() { NameData = groupRelation[groups.First(x => x.Kind == bindingReference.Key)] };
                    slotData.bindingDatas.Add(bindingData);
                    foreach (var state in bindingReference.Value)
                    {
                        bindingData.States.Add(state.ToStateInfo());
                    }
                    bindingData.SetBinding();
                    bindingData.SetSlot(slotReference.Key);
                }

                part.SetExtendedDataById(Settings.GUID, slotData.Serialize());
            }
        }
    }
}
