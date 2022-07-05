using HarmonyLib;

namespace Accessory_States
{
    internal static partial class Hooks
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetClothesState))]
        public static void Hook_SetClothesState(ChaControl __instance)
        {
            __instance.GetComponent<CharaEvent>().SetClothesState();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetAccessoryStateCategory))]
        public static void Hook_SetAccessoryStateCategory(ChaControl __instance, int cateNo, bool show)
        {
            __instance.GetComponent<CharaEvent>().AccessoryCategoryChange(cateNo, show);
        }
    }
}
