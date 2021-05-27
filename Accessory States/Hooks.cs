using BepInEx.Logging;
using HarmonyLib;
using KKAPI.Maker;
using System;
using System.Collections.Generic;

namespace Accessory_States
{
    public static class Hooks
    {
        static ManualLogSource Logger;

        public static event EventHandler<OnClickCoordinateChange> HcoordChange;

        public static void Init()
        {
            Logger = Settings.Logger;
            Harmony.CreateAndPatchAll(typeof(Hooks));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), "SetClothesState")]
        public static void Hook_SetClothesState(ChaControl __instance, int clothesKind, byte state)
        {
            __instance.GetComponent<CharaEvent>().Settings_SetClothesState(clothesKind, state);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), "ChangeCoordinateType", new Type[] { typeof(ChaFileDefine.CoordinateType), typeof(bool) })]
        public static void Hook_ChangeCoordinateType(ChaControl __instance)
        {
            __instance.GetComponent<CharaEvent>().ChangedCoord();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSprite), "OnClickCoordinateChange", new Type[] { typeof(int) })]
        public static void Hook_OnClickCoordinateChange(int _coordinate)
        {
            OnClickCoordinateChangeEvent(0, _coordinate);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSprite), "OnClickCoordinateChange", new Type[] { typeof(int), typeof(int) })]
        public static void Hook_OnClickCoordinateChange2(int _female, int _coordinate)
        {
            OnClickCoordinateChangeEvent(_female, _coordinate);
        }

        private static void OnClickCoordinateChangeEvent(int _female, int _coordinate)
        {
            var args = new OnClickCoordinateChange(_female, _coordinate);
            if (HcoordChange == null || HcoordChange.GetInvocationList().Length == 0)
            {
                return;
            }
            try
            {
                HcoordChange?.Invoke(null, args);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Subscriber crash in {nameof(Hooks)}.{nameof(HcoordChange)} - {ex}");
            }

        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(ChaFileCoordinate), "LoadFile", new Type[] { typeof(string) })]
        //public static void Hook_LoadFile_string2(ChaFileCoordinate __instance)
        //{
        //    var args = new CoordinateLoadedEventARG(__instance);
        //    if (coordloaded == null || coordloaded.GetInvocationList().Length == 0)
        //    {
        //        return;
        //    }
        //    try
        //    {
        //        coordloaded?.Invoke(null, args);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError($"Subscriber crash in {nameof(Hooks)}.{nameof(coordloaded)} - {ex}");
        //    }
        //}

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
