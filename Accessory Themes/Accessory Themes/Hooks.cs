using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Accessory_Themes
{
    internal static class Hooks
    {
        public static event EventHandler<Slot_ACC_Change_ARG> Slot_ACC_Change;
        internal static event EventHandler<MovUrAcc_Event> MovIt;

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
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MovUrAcc.MovUrAcc), "ProcessQueue")]
        private static void MovPatch(List<QueueItem> Queue)
        {
            var args = new MovUrAcc_Event(Queue);
            if (MovIt == null || MovIt.GetInvocationList().Length == 0)
            {
                return;
            }
            try
            {
                MovIt?.Invoke(null, args);
            }
            catch (Exception ex)
            {
                Settings.Logger.LogError($"Subscriber crash in {nameof(Hooks)}.{nameof(MovIt)} - {ex}");
            }
        }
    }
    internal class QueueItem
    {
        public int srcSlot { get; set; }

        public int dstSlot { get; set; }

        public QueueItem(int src, int dst)
        {
            srcSlot = src;
            dstSlot = dst;
        }
    }
}
