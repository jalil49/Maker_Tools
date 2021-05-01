using HarmonyLib;
using System;

namespace Additional_Card_Info
{
    internal static class Hooks
    {
        public static event EventHandler<Slot_ACC_Change_ARG> Slot_ACC_Change;

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
                Settings.Logger.LogError($"Subscriber crash in {nameof(Hooks)}.{nameof(Slot_ACC_Change)} - {ex}");
            }
        }

    }
}
