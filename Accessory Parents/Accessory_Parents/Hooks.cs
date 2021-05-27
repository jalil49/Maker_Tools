using BepInEx.Logging;
using HarmonyLib;
using KKAPI.Maker;
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetAccessoryPos))]
        private static void PositionPatch(ChaControl __instance, int slotNo, int correctNo, float value, bool add, int flags)
        {
            __instance.GetComponent<CharaEvent>().Position_Change(slotNo, correctNo, value, add, flags);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetAccessoryScl))]
        private static void ScalePatch(ChaControl __instance, int slotNo, int correctNo, float value, bool add, int flags)
        {
            __instance.GetComponent<CharaEvent>().Scale_Change(slotNo, correctNo, value, add, flags);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetAccessoryRot))]
        private static void RotationPatch(ChaControl __instance, int slotNo, int correctNo, float value, bool add, int flags)
        {
            __instance.GetComponent<CharaEvent>().Rotation_Change(slotNo, correctNo, value, add, flags);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeAccessory), typeof(int), typeof(int), typeof(int), typeof(string), typeof(bool))]
        private static void ChangeAccessory(ChaControl __instance, int slotNo, int type)
        {
            __instance.GetComponent<CharaEvent>().Slot_ACC_Change(slotNo, type);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MovUrAcc.MovUrAcc), "ProcessQueue")]
        private static void MovPatch(List<QueueItem> Queue)
        {
            MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().MovIt(Queue);
        }
    }
    internal class QueueItem
    {
        public int SrcSlot { get; set; }

        public int DstSlot { get; set; }

        public QueueItem(int src, int dst)
        {
            SrcSlot = src;
            DstSlot = dst;
        }
    }
}
