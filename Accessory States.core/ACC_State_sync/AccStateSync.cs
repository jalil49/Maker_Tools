using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

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

            [SerializationConstructor]
            public TriggerProperty(int coordinate, int slot, int refkind, int refstate, bool visible, int priority) // this shit is here to avoid msgpack fucking error
            {
                Coordinate = coordinate;
                Slot = slot;
                RefKind = refkind;
                RefState = refstate;
                Visible = visible;
                Priority = priority;
            }

            public TriggerProperty(int coordinate, int slot, int refkind, int refstate)
            {
                Coordinate = coordinate;
                Slot = slot;
                RefKind = refkind;
                RefState = refstate;
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

        internal static void InitConstants()
        {
            _cordNames = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).ToList();

            foreach (var _key in Enum.GetValues(typeof(ChaAccessoryDefine.AccessoryParentKey)))
                _accessoryParentNames[_key.ToString()] = ChaAccessoryDefine.dictAccessoryParent[(int)_key];
        }
    }
}
