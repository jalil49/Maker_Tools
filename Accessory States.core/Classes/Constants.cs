using System.Collections.Generic;

namespace Accessory_States
{
    public static class Constants
    {
        static Constants()
        {
            var states = new Dictionary<int, string>()
            {
                [0] = "Full",
                [1] = "Half-off 1",
                [2] = "Half-off 2",
                [3] = "Naked",
            };
            for (var i = -1; i < ClothingLength; i++)
            {
                KnownNameData[i] = new NameData() { Name = GetClothingName(i), StateNames = states };
            }
            UnknownNameData = new NameData() { Name = "Unknown", StateNames = states };
        }
        public static string GetClothingName(int clothNum)
        {
            switch (clothNum)
            {
                case -1: return "None";
                case 0: return "Top";
                case 1: return "Bottom";
                case 2: return "Bra";
                case 3: return "Panties";
                case 4: return "Gloves";
                case 5: return "Pantyhose";
                case 6: return "Socks";
#if KK || KKS
                case 7: return "Inner Shoes";
                case 8: return "Outer Shoes";
#else
                case 7: return "Shoes";
#endif
                default: return "Unknown";
            }
        }
        public static NameData GetClothingNameData(int clothNum)
        {
            if (KnownNameData.TryGetValue(clothNum, out var nameData)) return nameData;
            return UnknownNameData;
        }

        public const string CoordinateKey = "CoordinateData";
        public const string AccessoryKey = "SlotData";
        public const int SaveVersion = 2;
        public readonly static Dictionary<int, NameData> KnownNameData = new Dictionary<int, NameData>();
        public readonly static NameData UnknownNameData;
#if KK || KKS
        public const int ClothingLength = 9;
#else
        public const int ClothingLength = 8;
#endif
    }
}
