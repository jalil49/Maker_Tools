using System.Collections.Generic;

namespace Accessory_Shortcuts
{
    public static class Constants
    {
        public static Dictionary<int, Data> Parent = new Dictionary<int, Data>();
        static Constants()
        {
            Default_Dict();
        }
        public static void Default_Dict()
        {
            Settings.Logger.LogWarning("defaulting");
            Parent.Clear();
            Parent.Add(0, new Data(120, 0, ""));
            Parent.Add(1, new Data(121, 0, "a_n_hair_pin"));
            Parent.Add(2, new Data(122, 0, "a_n_headtop"));
            Parent.Add(3, new Data(123, 0, "a_n_megane"));
            Parent.Add(4, new Data(124, 0, "a_n_neck"));
            Parent.Add(5, new Data(125, 0, "a_n_back"));
            Parent.Add(6, new Data(126, 0, "a_n_waist"));
            Parent.Add(7, new Data(127, 0, "a_n_leg_L"));
            Parent.Add(8, new Data(128, 0, "a_n_shoulder_L"));
            Parent.Add(9, new Data(129, 0, "a_n_hand_L"));
            Parent.Add(10, new Data(130, 0, "a_n_kokan"));
        }
        public static void Print_Dict()
        {
            foreach (var item in Parent.Values)
            {
                Settings.Logger.LogWarning($"Type: {item.ACC_Type}, ID: {item.Id}, Parent: {item.ParentKey}");
            }
        }
    }
    public class Data
    {
        public Data(int _Acc_type, int _id, string _ParentKey)
        {
            ACC_Type = _Acc_type;
            Id = _id;
            ParentKey = _ParentKey;
        }
        public Data(Data data, int _id, string _ParentKey)
        {
            ACC_Type = data.ACC_Type;
            Id = _id;
            ParentKey = _ParentKey;
        }
        public int ACC_Type { get; }
        public int Id { get; set; }
        public string ParentKey { get; set; }
        //public static bool ChangingType = false;
    }
}
