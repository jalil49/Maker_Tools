using Extensions;
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
        public Dictionary<int, Slotdata> Slotinfo { get; set; }

        [Key("_names")]
        public Dictionary<int, NameData> Names { get; set; }

        [Key("_clothnotdata")]
        public bool[] ClothNotData { get; set; }

        [Key("_forceclothnotupdate")]
        public bool ForceClothNotUpdate = true;

        public CoordinateData() { NullCheck(); }

        public CoordinateData(CoordinateData _copy) => CopyData(_copy);

        public void CleanUp()
        {
            var removelist = Slotinfo.Where(x => x.Value.Binding == -1 && !x.Value.Parented).Select(x => x.Key).ToList();
            foreach (var item in removelist)
            {
                Slotinfo.Remove(item);
            }
            removelist = Names.Where(x => !Slotinfo.Any(y => y.Value.Binding == x.Key)).Select(x => x.Key).ToList();
            foreach (var item in removelist)
            {
                Names.Remove(item);
            }

            foreach (var item in Names)
            {
                var max = MaxState(item.Key);
                var statenames = item.Value.Statenames;
                removelist = statenames.Keys.Where(x => x > max).ToList();
                foreach (var key in removelist)
                {
                    statenames.Remove(key);
                }
            }
        }

        public void Clear()
        {
            Slotinfo.Clear();
            Names.Clear();
            ClothNotData = new bool[3];
            ForceClothNotUpdate = true;
        }

        private void NullCheck()
        {
            if (Slotinfo == null) Slotinfo = new Dictionary<int, Slotdata>();
            if (Names == null) Names = new Dictionary<int, NameData>();
            if (ClothNotData == null) ClothNotData = new bool[3];
        }

        public void CopyData(CoordinateData _copy) => CopyData(_copy.Slotinfo, _copy.Names, _copy.ClothNotData, _copy.ForceClothNotUpdate);

        public void CopyData(Dictionary<int, Slotdata> _slotinfo, Dictionary<int, NameData> _names, bool[] _clothnotdata, bool _forceclothnotupdate)
        {
            Slotinfo = _slotinfo.ToNewDictionary();
            Names = _names.ToNewDictionary();
            ClothNotData = _clothnotdata.ToNewArray(3);
            ForceClothNotUpdate = _forceclothnotupdate;
            NullCheck();
        }

        public void Accstatesyncconvert(int outfitnum, out List<AccStateSync.TriggerProperty> TriggerProperty, out List<AccStateSync.TriggerGroup> TriggerGroup)
        {
            TriggerProperty = new List<AccStateSync.TriggerProperty>();
            TriggerGroup = new List<AccStateSync.TriggerGroup>();

            foreach (var slotinfo in Slotinfo)
            {
                var statelist = slotinfo.Value.States;
                var binding = slotinfo.Value.Binding;
                int shoetype = slotinfo.Value.Shoetype;

                if (binding < 0)
                {
                    continue;
                }

                var maxstate = MaxState(binding) + 1;

                bool prioritycheck;
                int priority;
                bool visible;

                for (int state = 0; state < maxstate; state++) //default conversion
                {
                    visible = ShowState(state, statelist);

                    prioritycheck = binding < 4 && state % 3 == 0;
                    priority = 1;

                    if (prioritycheck) priority = 2;

                    TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slotinfo.Key, binding, state, visible, priority));
                }

                bool shoebinding = binding == 7 || binding == 8;

                if (shoetype != 2 && !shoebinding) //binded to indoor or outdoor state only make false priorty to not show
                {
                    var othershoe = (shoetype == 0) ? 8 : 7;
                    for (int state = 0; state < maxstate; state++)
                    {
                        TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slotinfo.Key, othershoe, state, false, 3));
                    }
                }

                if (shoebinding && shoetype == 2) //binded to a shoe, but other shoe needs same work
                {
                    int othershoe = (binding == 7) ? 8 : 7;
                    for (int state = 0; state < maxstate; state++)
                    {
                        visible = ShowState(state, statelist);

                        prioritycheck = binding < 4 && state % 3 == 0;

                        priority = 1;

                        if (prioritycheck) priority = 2;

                        TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slotinfo.Key, othershoe, state, visible, priority));
                    }
                }

                if (binding > 3)
                {
                    continue;
                }

                switch (binding)//using clothing nots to fill in space
                {
                    case 0:
                        if (ClothNotData[0])
                        {
                            TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 1));
                        }
                        if (ClothNotData[1])
                        {
                            TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 2));
                        }
                        break;
                    case 1:
                        if (ClothNotData[0])
                        {
                            TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 0));
                        }
                        break;
                    case 2:
                        if (ClothNotData[2])
                        {
                            TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 3));
                        }
                        if (ClothNotData[1])
                        {
                            TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 0));
                        }
                        break;
                    case 3:
                        if (ClothNotData[2])
                        {
                            TriggerProperty.AddRange(ClothingNotConversion(outfitnum, slotinfo.Key, statelist, 2));
                        }
                        break;
                }
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
            var triglistbyslot = new List<List<AccStateSync.TriggerProperty>>();

            foreach (var slot in TriggerProperty.Distinct(x => x.Slot).Select(x => x.Slot))
            {
                triglistbyslot.Add(TriggerProperty.Where(x => slot == x.Slot).OrderBy(x => x.RefKind).ThenBy(x => x.RefState).ToList());
            }
            for (int index = 0, trigdictcount = triglistbyslot.Count; index < trigdictcount; index++)
            {
                var list = triglistbyslot[index];

                var slot = list[0].Slot;
                bool inner = list.Any(x => x.RefKind == 7 && x.Visible);
                bool outer = list.Any(x => x.RefKind == 8 && x.Visible);
                var refkindlist = list.Distinct(x => x.RefKind).Select(x => x.RefKind).ToList();
                var PriorityDict = new Dictionary<int, int>();
                var statedict = new Dictionary<int, List<int[]>>();

                bool bothshoes = inner && outer || !inner && !outer;

                byte shoetype;
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

                for (int shoe = 7; shoe <= 8; shoe++)
                {
                    if (refkindlist.Contains(shoe))
                    {
                        if (refkindlist.Any(x => x < shoe))
                        {
                            refkindlist.Remove(shoe);
                        }
                    }
                }

                foreach (var kind in refkindlist)
                {
                    var visiblelist = list.Where(x => x.Visible && x.RefKind == kind).ToList();
                    if (visiblelist.Count == 0) continue;
                    PriorityDict[kind] = visiblelist.Count;
                    var statelist = statedict[kind] = new List<int[]>();
                    for (int i = 0, n = visiblelist.Count; i < n; i++)
                    {
                        var j = i;
                        while (j + 1 < n && visiblelist[j + 1].RefState == j + 1)
                        {
                            j++;
                        }
                        statelist.Add(new int[2] { i, j });
                        i = j;
                    }
                }

                var selectedrefkind = PriorityDict.OrderByDescending(x => x.Value).First().Key;

                Slotinfo[slot] = new Slotdata(selectedrefkind, statedict[selectedrefkind], shoetype, false);
            }

            foreach (var item in TriggerGroup)
            {
                Names[item.Kind] = new NameData(item.Label, item.States);
            }
            NullCheck();
        }

        private List<AccStateSync.TriggerProperty> ClothingNotConversion(int outfitnum, int slot, List<int[]> statelist, int clothingkind)
        {
            var TriggerProperty = new List<AccStateSync.TriggerProperty>();
            bool prioritycheck;
            for (int state = 0; state < 4; state++)
            {
                prioritycheck = state % 3 == 0;
                TriggerProperty.Add(new AccStateSync.TriggerProperty(outfitnum, slot, clothingkind, state, (prioritycheck) ? ShowClothNotState(state, statelist) : false, (prioritycheck) ? 2 : 0));
            }
            return TriggerProperty;
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

        private static bool ShowState(int state, List<int[]> list)
        {
            return list.Any(x => x[0] <= state && state <= x[1]);
        }

        private static bool ShowClothNotState(int state, List<int[]> list)
        {
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
        public List<int[]> States { get; set; }

        [Key("_shoetype")]
        public byte Shoetype { get; set; } = 2;

        [Key("_parented")]
        public bool Parented;

        public Slotdata() { NullCheck(); }

        public Slotdata(Slotdata slotdata) => CopyData(slotdata);

        public void CopyData(Slotdata slotdata) => CopyData(slotdata.Binding, slotdata.States, slotdata.Shoetype, slotdata.Parented);

        public Slotdata(int _binding, List<int[]> _state, byte _shoetype, bool _parented) => CopyData(_binding, _state, _shoetype, _parented);

        public void CopyData(int _binding, List<int[]> _state, byte _shoetype, bool _parented)
        {
            Binding = _binding;
            States = _state.ToNewList(new int[] { 0, 3 });
            Shoetype = _shoetype;
            Parented = _parented;
            //if (States == null) States = new List<int[]>() {  };
        }

        public string Print()
        {
            string print = $"Binding {Binding}\t shoetype {Shoetype} \n States: ";
            foreach (var item in States)
            {
                print += $" {item[0]} {item[1]} ";
            }
            return print;
        }

        private void NullCheck()
        {
            if (States == null) States = new List<int[]>() { new int[] { 0, 3 } };
        }
    }

    [Serializable]
    [MessagePackObject]
    public class NameData
    {
        [Key("_name")]
        public string Name { get; set; }

        [Key("_statenames")]
        public Dictionary<int, string> Statenames { get; set; }

        public NameData() { NullCheck(); }

        public NameData(NameData slotdata) => CopyData(slotdata);

        public void CopyData(NameData slotdata) => CopyData(slotdata.Name, slotdata.Statenames);

        public NameData(string _name, Dictionary<int, string> _statenames) => CopyData(_name, _statenames);

        public void CopyData(string _name, Dictionary<int, string> _statenames)
        {
            Name = _name;
            Statenames = _statenames.ToNewDictionary();
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

        public Dictionary<string, List<KeyValuePair<int, Slotdata>>> Now_Parented_Name_Dictionary = new Dictionary<string, List<KeyValuePair<int, Slotdata>>>();

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
            if (KKAPI.Maker.MakerAPI.InsideMaker)
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
            var shoetype = Controller.ChaControl.fileStatus.shoesType;
            var ParentedList = NowCoordinate.Slotinfo.Where(x => x.Value.Parented && (x.Value.Shoetype == 2 || x.Value.Shoetype == shoetype));
#if !KKS
            Controller.Update_More_Accessories();
#endif
            foreach (var item in ParentedList)
            {
                var parentkey = Controller.Accessorys_Parts[item.Key].parentKey;
                if (!Now_Parented_Name_Dictionary.TryGetValue(parentkey, out var list))
                {
                    list = new List<KeyValuePair<int, Slotdata>>();
                    Now_Parented_Name_Dictionary[parentkey] = list;
                }
                list.Add(item);
            }
        }

        public void Clear_Now_Coordinate()
        {
            NowCoordinate.Clear();
            Now_Parented_Name_Dictionary.Clear();
        }

        public void Clear()
        {
            for (int i = 0, n = Coordinate.Count; i < n; i++)
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
