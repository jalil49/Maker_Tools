using System.Collections.Generic;

namespace Accessory_States
{
    public static class Constants
    {
        public static string GetClothingName(int clothNum)
        {
            switch (clothNum)
            {
                case -1:
                    return "None";
                case 0:
                    return "Top";
                case 1:
                    return "Bottom";
                case 2:
                    return "Bra";
                case 3:
                    return "Panties";
                case 4:
                    return "Gloves";
                case 5:
                    return "Pantyhose";
                case 6:
                    return "Socks";
#if KK || KKS
                case 7:
                    return "Shoes";
                //case 7: return "Inner Shoes";
                //case 8: return "Outer Shoes";
#else
                case 7: return "Shoes";
#endif
                default:
                    return "Unknown";
            }
        }

        public static List<NameData> GetNameDataList()
        {
            var states = new Dictionary<int, string>()
            {
                [0] = "Full",
                [1] = "Shift",
                [2] = "Hang",
                [3] = "Naked",
            };

            var list = new List<NameData>();
            for (var i = -1; i < ClothingLength; i++)
                list.Add(new NameData() { Name = GetClothingName(i), StateNames = states, Binding = i });
            return list;
        }

        public const string CoordinateKey = "CoordinateData";
        public const string AccessoryKey = "SlotData";
        public const int SaveVersion = 2;
#if KK || KKS
        public const int ClothingLength = 8;
#else
        public const int ClothingLength = 8;
#endif
    }
}
