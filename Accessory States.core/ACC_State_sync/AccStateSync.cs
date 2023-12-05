﻿using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace Accessory_States
{
    public static partial class AccStateSync
    {
        internal static List<string> CordNames = new List<string>();

        internal static List<string> ClothesNames = new List<string>
            { "Top", "Bottom", "Bra", "Underwear", "Gloves", "Pantyhose", "Leg wear", "Indoors", "Outdoors" };

        internal static Dictionary<int, string> StatesNames = new Dictionary<int, string>
            { [0] = "Full", [1] = "Half 1", [2] = "Half 2", [3] = "Undressed" };

        internal static readonly Dictionary<string, string> AccessoryParentNames = new Dictionary<string, string>();

        internal static void InitConstants()
        {
            CordNames = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).ToList();

            foreach (var key in Enum.GetValues(typeof(ChaAccessoryDefine.AccessoryParentKey)))
                AccessoryParentNames[key.ToString()] = ChaAccessoryDefine.dictAccessoryParent[(int)key];
        }

        [Serializable]
        [MessagePackObject]
        public class TriggerProperty
        {
            [SerializationConstructor]
            public TriggerProperty(int coordinate, int slot, int refkind, int refstate, bool visible,
                int priority) // this shit is here to avoid msgpack fucking error
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

            [Key("Coordinate")] public int Coordinate { get; set; } = -1;

            [Key("Slot")] public int Slot { get; set; } = -1;

            [Key("RefKind")] public int RefKind { get; set; } = -1;

            [Key("RefState")] public int RefState { get; set; } = -1;

            [Key("Visible")] public bool Visible { get; set; } = true;

            [Key("Priority")] public int Priority { get; set; }
        }

        [Serializable]
        [MessagePackObject]
        public class TriggerGroup
        {
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

                States = new Dictionary<int, string> { [0] = "State 1", [1] = "State 2" };
            }

            public TriggerGroup(int coordinate, int kind, string label, int startup, int secondary) : this(coordinate,
                kind, label, 0, startup, secondary)
            {
            }

            public TriggerGroup(int coordinate, int kind, string label = "") : this(coordinate, kind, label, 0, 0, -1)
            {
            }

            [Key("Coordinate")] public int Coordinate { get; set; } = -1;

            [Key("Kind")] public int Kind { get; set; } // TriggerProperty RefGroup

            [Key("State")] public int State { get; set; } // Current RefState

            [Key("States")] public Dictionary<int, string> States { get; set; }

            [Key("Label")] public string Label { get; set; } = "";

            [Key("Startup")] public int Startup { get; set; }

            [Key("Secondary")] public int Secondary { get; set; } = -1;

            [Key("GUID")] public string Guid { get; set; } = System.Guid.NewGuid().ToString("D").ToUpper();

            public void Rename(string label)
            {
                if (label.Trim().IsNullOrEmpty())
                    label = $"Custom {Kind - 8}";
                Label = label;
            }

            public void RenameState(int state, string label)
            {
                if (!States.ContainsKey(state))
                    return;
                if (label.Trim().IsNullOrEmpty())
                    label = $"State {state + 1}";
                States[state] = label;
            }

            public int GetNewStateID()
            {
                return States.OrderByDescending(x => x.Key).FirstOrDefault().Key + 1;
            }

            public int AddNewState()
            {
                var state = States.OrderByDescending(x => x.Key).FirstOrDefault().Key + 1;
                return AddNewState(state);
            }

            public int AddNewState(int state)
            {
                var label = $"State {state + 1}";
                States[state] = label;
                return state;
            }
        }
    }
}