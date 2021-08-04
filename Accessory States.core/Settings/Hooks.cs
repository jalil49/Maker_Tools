using HarmonyLib;
using System;

namespace Accessory_States
{
    internal static partial class Hooks
    {
        public static event EventHandler<OnClickCoordinateChange> HcoordChange;


        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), "SetClothesState")]
        public static void Hook_SetClothesState(ChaControl __instance, int clothesKind, byte state)
        {
            __instance.GetComponent<CharaEvent>().SetClothesState(clothesKind, state);
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
    }
}
