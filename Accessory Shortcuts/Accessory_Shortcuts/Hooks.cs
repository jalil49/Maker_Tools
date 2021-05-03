using BepInEx.Logging;
using ChaCustom;
using HarmonyLib;
using System;

namespace Accessory_Shortcuts
{
    public static class Hooks
    {
        static ManualLogSource Logger;
        //public static CustomAcsSelectKind CustomAcsSelectKind_Reference;
        //public static CustomAcsChangeSlot CustomAcsChangeSlot_Reference;
        public static void Init()
        {
            Harmony.CreateAndPatchAll(typeof(Hooks));
            Logger = Settings.Logger;
        }

        public static event EventHandler<Slot_ACC_Change_ARG> Pre_Slot_ACC_Change;
        public static event EventHandler<Slot_ACC_Change_ARG> Post_Slot_ACC_Change;

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeAccessory), typeof(int), typeof(int), typeof(int), typeof(string), typeof(bool))]
        private static void PostChangeAccessory(ChaControl __instance, int slotNo, int type, int id, string parentKey)
        {
            var args = new Slot_ACC_Change_ARG(__instance, slotNo, type, id, parentKey);
            if (Post_Slot_ACC_Change == null || Post_Slot_ACC_Change.GetInvocationList().Length == 0)
            {
                return;
            }
            try
            {
                Post_Slot_ACC_Change?.Invoke(null, args);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Subscriber crash in {nameof(Accessory_Shortcuts.Hooks)}.{nameof(Post_Slot_ACC_Change)} - {ex}");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeAccessory), typeof(int), typeof(int), typeof(int), typeof(string), typeof(bool))]
        private static void PreAccessory(ChaControl __instance, int slotNo, int type, int id, string parentKey)
        {
            var args = new Slot_ACC_Change_ARG(__instance, slotNo, type, id, parentKey);
            if (Pre_Slot_ACC_Change == null || Pre_Slot_ACC_Change.GetInvocationList().Length == 0)
            {
                return;
            }
            try
            {
                Pre_Slot_ACC_Change?.Invoke(null, args);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Subscriber crash in {nameof(Accessory_Shortcuts.Hooks)}.{nameof(Pre_Slot_ACC_Change)} - {ex}");
            }
        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(CustomAcsChangeSlot), "Start")]
        //private static void CustomAcsChangeSlot_Patch_Start(CustomAcsChangeSlot __instance)
        //{
        //    CustomAcsChangeSlot_Reference = __instance;
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(CustomAcsSelectKind), "Start")]
        //private static void CustomAcsSelectKind_Patch_Start(CustomAcsSelectKind __instance)
        //{
        //    CustomAcsSelectKind_Reference = __instance;
        //}

    }
}
