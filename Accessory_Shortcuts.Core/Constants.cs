using System.Collections.Generic;

namespace Accessory_Shortcuts
{
    public static class Constants
    {
        public static Dictionary<int, Data> Parent = new Dictionary<int, Data>();

        public static void Print_Dict()
        {
            foreach (var item in Parent)
            {
                Settings.Logger.LogWarning($"Type: {item.Key}, ID: {item.Value.Id}, Parent: {item.Value.ParentKey}");
            }
        }
    }
    public class Data
    {
        public Data(string _ParentKey)
        {
            Id = 0;
            ParentKey = _ParentKey;
        }
        public int Id { get; set; }
        public string ParentKey { get; set; }
        //public static bool ChangingType = false;
    }
}
