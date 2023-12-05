namespace Accessory_Parents
{
    internal static partial class Hooks
    {
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetAccessoryPos))]
        //private static void PositionPatch(ChaControl __instance, int slotNo, int correctNo, float value, bool add, int flags)
        //{
        //    //__instance.GetComponent<CharaEvent>().Position_Change(slotNo, correctNo, value, add, flags);
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetAccessoryScl))]
        //private static void ScalePatch(ChaControl __instance, int slotNo, int correctNo, float value, bool add, int flags)
        //{
        //    //__instance.GetComponent<CharaEvent>().Scale_Change(slotNo, correctNo, value, add, flags);
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetAccessoryRot))]
        //private static void RotationPatch(ChaControl __instance, int slotNo, int correctNo, float value, bool add, int flags)
        //{
        //    //__instance.GetComponent<CharaEvent>().Rotation_Change(slotNo, correctNo, value, add, flags);
        //}
    }
}