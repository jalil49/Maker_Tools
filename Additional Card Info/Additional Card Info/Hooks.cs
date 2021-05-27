using HarmonyLib;
using KKAPI.Maker;
using System;
using System.Collections.Generic;

namespace Additional_Card_Info
{
    internal static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeAccessory), typeof(int), typeof(int), typeof(int), typeof(string), typeof(bool))]
        private static void ChangeAccessory(ChaControl __instance, int slotNo, int type)
        {
            __instance.GetComponent<CharaEvent>().Hooks_Slot_ACC_Change(slotNo, type);
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
