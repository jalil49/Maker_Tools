﻿using BepInEx.Logging;
using ChaCustom;
using HarmonyLib;
using KKAPI;

namespace Accessory_Shortcuts
{
    public static class Hooks
    {
        static ManualLogSource Logger;
        public static void Init()
        {
            var harmony = Harmony.CreateAndPatchAll(typeof(Hooks));
            harmony.PatchAll(typeof(CustomAcsChangeSlot_KKS_Start_Patches));
            Logger = Settings.Logger;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeAccessory), typeof(int), typeof(int), typeof(int), typeof(string), typeof(bool))]
        private static void PostChangeAccessory(ChaControl __instance, int slotNo, int type, int id, string parentKey)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                return;
            }
            __instance.GetComponent<CharaEvent>().Change_To_Stored_Accessory(slotNo, type, id, parentKey);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeAccessory), typeof(int), typeof(int), typeof(int), typeof(string), typeof(bool))]
        private static void PreAccessory(ChaControl __instance, int slotNo, int type, int id, string parentKey)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            {
                return;
            }
            __instance.GetComponent<CharaEvent>().Update_Stored_Accessory(slotNo, type, id, parentKey);
        }

        [HarmonyPatch(typeof(CustomAcsChangeSlot), nameof(CustomAcsChangeSlot.Start))]
        internal static class CustomAcsChangeSlot_KKS_Start_Patches
        {
            private static void Postfix(CustomAcsChangeSlot __instance)
            {
                CharaEvent.CustomAcs = __instance;
            }
        }
    }
}
