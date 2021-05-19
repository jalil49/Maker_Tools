using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Accessory_States
{
    public static class Hooks
    {
        static ManualLogSource Logger;

        public static event EventHandler<ClothChangeEventArgs> SetClothesState;
        public static event EventHandler<ChangeCoordinateTypeARG> ChangedCoord;
        public static event EventHandler<OnClickCoordinateChange> HcoordChange;
        public static event EventHandler<Slot_ACC_Change_ARG> Slot_ACC_Change;
        internal static event EventHandler<MovUrAcc_Event> MovIt;

        public static void Init()
        {
            Logger = Settings.Logger;
            Harmony.CreateAndPatchAll(typeof(Hooks));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), "SetClothesState")]
        public static void Hook_SetClothesState(ChaControl __instance, int clothesKind, byte state, bool next = true)
        {
            var args = new ClothChangeEventArgs(__instance, clothesKind, state, next);
            if (SetClothesState == null || SetClothesState.GetInvocationList().Length == 0)
            {
                return;
            }
            try
            {
                SetClothesState?.Invoke(null, args);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Subscriber crash in {nameof(Hooks)}.{nameof(SetClothesState)} - {ex}");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), "ChangeCoordinateType", new Type[] { typeof(ChaFileDefine.CoordinateType), typeof(bool) })]
        public static void Hook_ChangeCoordinateType(ChaControl __instance)
        {
            var args = new ChangeCoordinateTypeARG(__instance);
            if (ChangedCoord == null || ChangedCoord.GetInvocationList().Length == 0)
            {
                return;
            }
            try
            {
                ChangedCoord?.Invoke(null, args);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Subscriber crash in {nameof(Hooks)}.{nameof(ChangedCoord)} - {ex}");
            }
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
                Logger.LogError($"Subscriber crash in {nameof(Accessory_States.Hooks)}.{nameof(Slot_ACC_Change)} - {ex}");
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
                Logger.LogError($"Subscriber crash in {nameof(Hooks)}.{nameof(MovIt)} - {ex}");
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
