using ChaCustom;
using HarmonyLib;

namespace Accessory_States
{
    internal static partial class Hooks
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetClothesState))]
        public static void Hook_SetClothesState(ChaControl __instance, int clothesKind, byte state)
        {
            __instance.GetComponent<CharaEvent>().SetClothesState(clothesKind, state);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetAccessoryStateCategory))]
        public static void Hook_SetAccessoryStateCategory(ChaControl __instance, int cateNo, bool show)
        {
            __instance.GetComponent<CharaEvent>().AccessoryCategoryChange(cateNo, show);
        }

        //TODO: Check this for crashing
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomChangeMainMenu), nameof(CustomChangeMainMenu.ChangeWindowSetting))]
        public static void Hook_ChangeWindowSetting(int no)
        {
            CharaEvent.StopMakerLoop = no < 4;
        }
    }
}
