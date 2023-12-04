using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace Accessory_States
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
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

        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeAccessoryParent))]
        public static void Hook_ChangeAccessoryParentPre(ChaControl __instance, int slotNo, string parentStr)
        {
            __instance.GetComponent<CharaEvent>().ParentRemove(slotNo, parentStr);
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.VeryLow)]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeAccessoryParent))]
        public static void Hook_ChangeAccessoryParentPost(ChaControl __instance, int slotNo, string parentStr)
        {
            __instance.GetComponent<CharaEvent>().ParentUpdate(slotNo, parentStr);
        }
    }
}