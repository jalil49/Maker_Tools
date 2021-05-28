using BepInEx.Logging;
using HarmonyLib;
using KKAPI.Maker;
using System;
using System.Collections.Generic;
#if Parents
using Accessory_Parents.Core;
#elif States
using Accessory_States.Core;
#elif Themes
using Accessory_Themes.Core;
#elif ACI
using Additional_Card_Info.Core;
#endif
namespace Hook_Space
{
    internal static class Hooks
    {
        static ManualLogSource Logger;

        public static void Init(ManualLogSource Setting_Logger)
        {
            Logger = Setting_Logger;
            Harmony.CreateAndPatchAll(typeof(Hooks));
        }

#if Parents
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
#endif

#if States
        public static event EventHandler<OnClickCoordinateChange> HcoordChange;


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
            var Controller = __instance.GetComponent<CharaEvent>();
            if (Controller?.ThisCharactersData != null)
            {
                Controller.ChangedCoord();
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
#endif

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
