using BepInEx.Logging;
using HarmonyLib;
using KKAPI;
using System;

namespace Accessory_Shortcuts
{
    public static class Hooks
    {
        static ManualLogSource Logger;
        public static void Init()
        {
            Harmony.CreateAndPatchAll(typeof(Hooks));
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
    }
}
