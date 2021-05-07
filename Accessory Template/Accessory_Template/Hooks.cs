using BepInEx.Logging;
using HarmonyLib;
using System;

namespace Template_Accessories
{
    public static class Hooks
    {
        static ManualLogSource Logger;

        public static void Init()
        {
            Harmony.CreateAndPatchAll(typeof(Hooks));
            Logger = Settings.Logger;
        }

        public static event EventHandler<Slot_ACC_Change_ARG> Slot_ACC_Change;

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetAccessoryPos))]
        //private static void PositionPatch(ChaControl __instance, int slotNo, int correctNo, float value, bool add, int flags)
        //{
        //    var args = new Acc_modifier_Event_ARG(__instance, slotNo, correctNo, value, add, flags);
        //    if (ACC_Position_Change == null || ACC_Position_Change.GetInvocationList().Length == 0)
        //    {
        //        return;
        //    }
        //    try
        //    {
        //        ACC_Position_Change?.Invoke(null, args);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError($"Subscriber crash in {nameof(Accessory_Parents.Hooks)}.{nameof(ACC_Position_Change)} - {ex}");
        //    }
        //}

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeAccessory), typeof(int), typeof(int), typeof(int), typeof(string), typeof(bool))]
        private static void ChangeAccessory(ChaControl __instance, int slotNo, int type)
        {
            var args = new Slot_ACC_Change_ARG(__instance, slotNo, type);
            if (Slot_ACC_Change == null || Slot_ACC_Change.GetInvocationList().Length == 0)
            {
                return;
            }
            try
            {
                Slot_ACC_Change?.Invoke(null, args);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Subscriber crash in {nameof(Hooks)}.{nameof(Slot_ACC_Change)} - {ex}");
            }
        }
    }
}
