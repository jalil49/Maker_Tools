using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Accessory_Parents
{
    public static class Hooks
    {
        static ManualLogSource Logger;
        public static void Init()
        {
            Harmony.CreateAndPatchAll(typeof(Hooks));
            Logger = Settings.Logger;
        }

        public static event EventHandler<Acc_modifier_Event_ARG> ACC_Position_Change;
        public static event EventHandler<Acc_modifier_Event_ARG> ACC_Rotation_Change;
        public static event EventHandler<Acc_modifier_Event_ARG> ACC_Scale_Change;
        public static event EventHandler<Slot_ACC_Change_ARG> Slot_ACC_Change;
        internal static event EventHandler<MovUrAcc_Event> MovIt;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetAccessoryPos))]
        private static void PositionPatch(ChaControl __instance, int slotNo, int correctNo, float value, bool add, int flags)
        {
            var args = new Acc_modifier_Event_ARG(__instance, slotNo, correctNo, value, add, flags);
            if (ACC_Position_Change == null || ACC_Position_Change.GetInvocationList().Length == 0)
            {
                return;
            }
            try
            {
                ACC_Position_Change?.Invoke(null, args);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Subscriber crash in {nameof(Accessory_Parents.Hooks)}.{nameof(ACC_Position_Change)} - {ex}");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetAccessoryScl))]
        private static void ScalePatch(ChaControl __instance, int slotNo, int correctNo, float value, bool add, int flags)
        {
            var args = new Acc_modifier_Event_ARG(__instance, slotNo, correctNo, value, add, flags);
            if (ACC_Scale_Change == null || ACC_Scale_Change.GetInvocationList().Length == 0)
            {
                return;
            }
            try
            {
                ACC_Scale_Change?.Invoke(null, args);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Subscriber crash in {nameof(Accessory_Parents.Hooks)}.{nameof(ACC_Scale_Change)} - {ex}");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetAccessoryRot))]
        private static void RotationPatch(ChaControl __instance, int slotNo, int correctNo, float value, bool add, int flags)
        {
            var args = new Acc_modifier_Event_ARG(__instance, slotNo, correctNo, value, add, flags);
            if (ACC_Rotation_Change == null || ACC_Rotation_Change.GetInvocationList().Length == 0)
            {
                return;
            }
            try
            {
                ACC_Rotation_Change?.Invoke(null, args);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Subscriber crash in {nameof(Accessory_Parents.Hooks)}.{nameof(ACC_Rotation_Change)} - {ex}");
            }
        }

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
                Logger.LogError($"Subscriber crash in {nameof(Accessory_Parents.Hooks)}.{nameof(Slot_ACC_Change)} - {ex}");
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
