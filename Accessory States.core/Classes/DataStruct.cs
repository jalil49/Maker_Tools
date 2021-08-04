using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject]
    public class CoordinateData
    {
        [Key("_slotinfo")]
        public Dictionary<int, Slotdata> Slotinfo { get; set; } = new Dictionary<int, Slotdata>();

        [Key("_names")]
        public Dictionary<int, NameData> Names { get; set; } = new Dictionary<int, NameData>();

        [Key("_parented")]
        public Dictionary<int, bool> Parented { get; set; } = new Dictionary<int, bool>();

        public CoordinateData() { }

        public CoordinateData(CoordinateData _copy) => CopyData(_copy);

        public void CleanUp()
        {
            NullCheck();
            var removeslots = Slotinfo.Where(x => x.Value.Binding == -1).ToList();
            foreach (var item in removeslots)
            {
                Slotinfo.Remove(item.Key);
            }
            var removenames = Names.Where(x => !Slotinfo.Any(y => y.Value.Binding == x.Key)).ToList();
            foreach (var item in removenames)
            {
                Names.Remove(item.Key);
            }
            var removeparents = Parented.Where(x => !x.Value).ToList();
            foreach (var item in removeparents)
            {
                Parented.Remove(item.Key);
            }
        }

        public void Clear()
        {
            NullCheck();
            Slotinfo.Clear();
            Names.Clear();
            Parented.Clear();
        }

        private void NullCheck()
        {
            if (Slotinfo == null) Slotinfo = new Dictionary<int, Slotdata>();
            if (Names == null) Names = new Dictionary<int, NameData>();
            if (Parented == null) Parented = new Dictionary<int, bool>();
        }

        public void CopyData(CoordinateData _copy)
        {
            Slotinfo = new Dictionary<int, Slotdata>(_copy.Slotinfo);
            Names = new Dictionary<int, NameData>(_copy.Names);
            Parented = new Dictionary<int, bool>(_copy.Parented);
        }

        public void Accstatesyncconvert(int outfitnum, out List<AccStateSync.TriggerProperty> TriggerProperty, out List<AccStateSync.TriggerGroup> TriggerGroup)
        {
            TriggerProperty = new List<AccStateSync.TriggerProperty>();
            TriggerGroup = new List<AccStateSync.TriggerGroup>();

            foreach (var slotinfo in Slotinfo)
            {
                var statelist = slotinfo.Value.States;
                var binding = slotinfo.Value.Binding;
                bool singlestate = 3 < binding && binding < 9;
                int shoetype = slotinfo.Value.Shoetype;

                for (int j = 0, nn = MaxState(binding) + 1; j < nn; j++)
                {
                    TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slotinfo.Key, binding, j, ShowState(j, statelist, singlestate), 0));
                }

                bool shoebinding = binding == 7 || binding == 8;

                if (shoetype != 2 && !shoebinding)
                {
                    var shoe = shoetype + 7;
                    for (int j = 0, nn = MaxState(binding) + 1; j < nn; j++)
                    {
                        TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slotinfo.Key, shoe, j, ShowState(j, statelist, singlestate), 1));
                    }
                    continue;
                }

                if (shoebinding && shoetype == 2)
                {
                    int othershoe = (binding == 7) ? 8 : 7;
                    for (int j = 0, nn = MaxState(binding) + 1; j < nn; j++)
                    {
                        TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slotinfo.Key, othershoe, j, ShowState(j, statelist, singlestate), 0));
                    }
                }
            }


            for (int i = 0, n = AccStateSync._clothesNames.Count; i < n; i++)
            {
                TriggerGroup.Add(new AccStateSync.TriggerGroup(outfitnum, i, AccStateSync._clothesNames[i]) { States = AccStateSync._statesNames });
            }

            foreach (var NameData in Names)
            {
                var states = NameData.Value.Statenames;
                for (int i = 0, n = MaxState(NameData.Key) + 1; i < n; i++)
                {
                    if (states.ContainsKey(i))
                    {
                        continue;
                    }
                    states[i] = $"State {i}";
                }

                TriggerGroup.Add(new AccStateSync.TriggerGroup(outfitnum, NameData.Key, NameData.Value.Name) { States = states.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value) });
            }
        }

        public void Accstatesyncconvert(List<AccStateSync.TriggerProperty> TriggerProperty, List<AccStateSync.TriggerGroup> TriggerGroup)
        {
            var Converted_Dictionary = new Dictionary<int, int>();
            var Converted_array = new Dictionary<int, List<int[]>>();
            var trigdict = new Dictionary<int, List<AccStateSync.TriggerProperty>>();

            foreach (var item in TriggerProperty)
            {
                if (trigdict.TryGetValue(item.Slot, out var list))
                {
                    list.Add(item);
                    continue;
                }
                trigdict[item.Slot] = new List<AccStateSync.TriggerProperty>() { item };
            }

            foreach (var list in trigdict.Values)
            {
                byte shoetype;
                var slot = list[0].Slot;
                bool inner = list.Any(x => x.RefKind == 7);
                bool outer = list.Any(x => x.RefKind == 8);
                var PriorityDict = new Dictionary<int, int>();
                var refkindlist = new List<int>();
                var statedict = new Dictionary<int, List<int[]>>();
                var bothshoes = inner && outer || !(inner && outer);

                if (bothshoes)
                {
                    shoetype = 2;
                }
                else if (inner)
                {
                    shoetype = 0;
                }
                else
                {
                    shoetype = 1;
                }

                foreach (var item in list.Distinct(x => x.RefKind))
                {
                    refkindlist.Add(item.RefKind);
                }

                foreach (var kind in refkindlist)
                {
                    var sortedlist = list.Where(x => x.Visible && x.RefKind == kind).OrderBy(x => x.RefState).ToList();
                    PriorityDict[kind] = sortedlist.First(x => x.RefKind == kind).Priority;
                    var statelist = statedict[kind] = new List<int[]>();
                    for (int i = 0, n = sortedlist.Count; i < n; i++)
                    {
                        var j = i;
                        while (j + 1 < n && sortedlist[j + 1].RefState == j + 1)
                        {
                            j++;
                        }
                        statelist.Add(new int[2] { i, j });
                        i = j;
                    }
                }

                var selectedrefkind = PriorityDict.OrderByDescending(x => x.Value).First().Key;

                Slotinfo[slot] = new Slotdata(selectedrefkind, statedict[selectedrefkind], shoetype);
            }

            foreach (var slot in Converted_Dictionary)
            {
                List<int[]> stateslist = new List<int[]>();
                List<int> triggerstates = new List<int>();
                TriggerProperty.ForEach(x =>
                {
                    if (x.Visible)
                    {
                        triggerstates.Add(x.RefState);
                    }
                });
                var max = triggerstates.Max() + 1;
                bool lastvis = false;
                int[] addstate = new int[2];
                if (slot.Value < 5 || 8 < slot.Value)
                {
                    for (int i = 0; i < max; i++)
                    {
                        bool show = triggerstates.Contains(i);
                        if (show != lastvis)
                        {
                            if (show)
                            {
                                addstate[0] = i;
                                lastvis = show;
                                continue;
                            }
                            addstate[1] = i - 1;
                            stateslist.Add(addstate);
                            addstate = new int[2];
                        }
                        lastvis = show;
                    }
                }
                else
                {
                    addstate[0] = (triggerstates.Contains(0)) ? 0 : 1;
                    addstate[1] = (triggerstates.Contains(3)) ? 1 : 0;
                    stateslist.Add(addstate);
                    addstate = new int[2];
                }
                Converted_array[slot.Key] = stateslist;
            }

            for (int i = 0, n = Converted_Dictionary.Count; i < n; i++)
            {
                var element = Converted_Dictionary.ElementAt(i);
                if (!Converted_array.TryGetValue(element.Key, out var states))
                {
                    continue;
                }
                Slotinfo[element.Key] = new Slotdata(element.Value, states, 2);
            }

            foreach (var item in TriggerGroup)
            {
                Names[item.Kind] = new NameData(item.Label, item.States);
            }
        }

        private int MaxState(int binding)
        {
            if (binding < 9)
            {
                return 3;
            }
            int max = 0;
            var bindinglist = Slotinfo.Values.Where(x => x.Binding == binding);
            foreach (var item in bindinglist)
            {
                item.States.ForEach(x => max = Math.Max(x[1], max));
            }
            return max;
        }

        private static bool ShowState(int state, List<int[]> list, bool singlestate)
        {
            if (singlestate)
            {
                for (int i = 0, n = list.Count; i < n; i++)
                {
                    var single = list[i];
                    for (int j = 0, nn = single.Length; j < nn; j++)
                    {
                        if (single[j] != 0)
                        {
                            single[j] = 3;
                        }
                    }
                }
            }
            return list.Any(x => x[0] <= state && state <= x[1]);
        }
    }

    [Serializable]
    [MessagePackObject]
    public class Slotdata
    {
        [Key("_binding")]
        public int Binding { get; set; } = -1;

        [Key("_state")]
        public List<int[]> States { get; set; } = new List<int[]>() { new int[] { 0, 3 } };

        [Key("_shoetype")]
        public byte Shoetype { get; set; } = 2;

        public Slotdata() { }

        public Slotdata(Slotdata slotdata) => CopyData(slotdata);

        public void CopyData(Slotdata slotdata) => CopyData(slotdata.Binding, slotdata.States, slotdata.Shoetype);

        public Slotdata(int _binding, List<int[]> _state, byte _shoetype) => CopyData(_binding, _state, _shoetype);

        public void CopyData(int _binding, List<int[]> _state, byte _shoetype)
        {
            Binding = _binding;
            States = _state;
            Shoetype = _shoetype;
            if (States == null) States = new List<int[]>() { new int[] { 0, 3 } };
        }
    }

    [Serializable]
    [MessagePackObject]
    public class NameData
    {
        [Key("_name")]
        public string Name { get; set; } = "";

        [Key("_statenames")]
        public Dictionary<int, string> Statenames { get; set; } = new Dictionary<int, string>();

        public NameData() { NullCheck(); }

        public NameData(NameData slotdata) => CopyData(slotdata);

        public void CopyData(NameData slotdata) => CopyData(slotdata.Name, slotdata.Statenames);

        public NameData(string _name, Dictionary<int, string> _statenames) => CopyData(_name, _statenames);

        public void CopyData(string _name, Dictionary<int, string> _statenames)
        {
            Name = _name;
            Statenames = _statenames;
            NullCheck();
        }

        private void NullCheck()
        {
            if (Statenames == null) Statenames = new Dictionary<int, string>();
            if (Name == null) Name = "";
        }
    }

    public class Data
    {
        internal int Personality;
        internal string BirthDay;
        internal string FullName;
        internal CharaEvent Controller;
        internal bool processed = false;

        public Dictionary<int, CoordinateData> Coordinate = new Dictionary<int, CoordinateData>();

        public CoordinateData NowCoordinate = new CoordinateData();

        public Dictionary<string, List<int>> Now_Parented_Name_Dictionary = new Dictionary<string, List<int>>();

        public Data(int _Personality, string _BirthDay, string _FullName, CharaEvent controller)
        {
            Personality = _Personality;
            BirthDay = _BirthDay;
            FullName = _FullName;
            Controller = controller;
        }

        public void Update_Now_Coordinate()
        {
            var outfitnum = (int)Controller.CurrentCoordinate.Value;
            if (!Coordinate.TryGetValue(outfitnum, out var coordinateData))
            {
                coordinateData = new CoordinateData();
                Coordinate[outfitnum] = coordinateData;
            }
            //Settings.Logger.LogWarning((ChaFileDefine.CoordinateType)outfitnum);
            if (KKAPI.KoikatuAPI.GetCurrentGameMode() == KKAPI.GameMode.Maker)
            {
                NowCoordinate = coordinateData;
            }
            else
            {
                NowCoordinate = new CoordinateData(coordinateData);
            }
            Update_Parented_Name();
        }

        public void Update_Now_Coordinate(int outfitnum)
        {
            NowCoordinate = new CoordinateData(Coordinate[outfitnum]);
            Update_Parented_Name();
        }

        public void Update_Parented_Name()
        {
            Now_Parented_Name_Dictionary.Clear();
            var ParentedList = NowCoordinate.Parented.Where(x => x.Value);
            Controller.Update_More_Accessories();
            foreach (var item in ParentedList)
            {
                var parentkey = Controller.Accessorys_Parts[item.Key].parentKey;
                if (!Now_Parented_Name_Dictionary.TryGetValue(parentkey, out var list))
                {
                    list = new List<int>();
                    Now_Parented_Name_Dictionary[parentkey] = list;
                }
                list.Add(item.Key);
            }
        }

        public void Clear_Now_Coordinate()
        {
            NowCoordinate.Clear();
            Now_Parented_Name_Dictionary.Clear();
        }

        public void Clear()
        {
            for (int i = 0, n = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length; i < n; i++)
            {
                Coordinate[i].Clear();
            }
            Now_Parented_Name_Dictionary.Clear();
        }

        public void Clearoutfit(int key)
        {
            Coordinate[key].Clear();
        }

        public void Createoutfit(int key)
        {
            if (!Coordinate.ContainsKey(key))
            {
                Coordinate[key] = new CoordinateData();
            }
        }

        public void Moveoutfit(int dest, int src)
        {
            Coordinate[dest] = new CoordinateData(Coordinate[src]);
        }

        public void Removeoutfit(int key)
        {
            Coordinate.Remove(key);
        }
    }
}
