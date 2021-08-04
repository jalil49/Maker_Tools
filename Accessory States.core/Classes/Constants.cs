using System.Collections.Generic;

namespace Accessory_States
{
    public static class Constants
    {
        public static List<Data> CharacterInfo = new List<Data>();
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
    }
}
