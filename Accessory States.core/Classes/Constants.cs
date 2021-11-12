using System.Collections.Generic;

namespace Accessory_States
{
    public static class Constants
    {
        public static Dictionary<int, string> ConstantOutfitNames = new Dictionary<int, string>()
        {
            [-1] = "None",
            [0] = "Top",
            [1] = "Bottom",
            [2] = "Bra",
            [3] = "Panties",
            [4] = "Gloves",
            [5] = "Pantyhose",
            [6] = "Socks",
            [7] = "Inner Shoes",
            [8] = "Outer Shoes",
        };

        public const string CoordinateKey = "CoordinateData";
        public const string AccessoryKey = "SlotData";
        public const int SaveVersion = 2;

#if KK || KKS
        public const int ClothingLength = 9;
#endif
    }
}
