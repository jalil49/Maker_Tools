using ChaCustom;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Accessory_States
{
    internal static partial class Hooks
    {
        public static event EventHandler<OnClickCoordinateChange> HcoordChange;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetClothesState))]
        public static void Hook_SetClothesState(ChaControl __instance, int clothesKind, byte state)
        {
            __instance.GetComponent<CharaEvent>().SetClothesState(clothesKind, state);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetAccessoryStateCategory))]
        public static void Hook_SetAccessoryStateCategory(ChaControl __instance, int cateNo, bool show)
        {
            if (cateNo == 1)
                __instance.GetComponent<CharaEvent>().SubChanged(show);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnClickCoordinateChange), new Type[] { typeof(int) })]
        public static void Hook_OnClickCoordinateChange(int _coordinate)
        {
            OnClickCoordinateChangeEvent(0, _coordinate);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomChangeMainMenu), nameof(CustomChangeMainMenu.ChangeWindowSetting))]
        public static void Hook_ChangeWindowSetting(int no)
        {
            CharaEvent.StopMakerLoop = no == 3;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnClickCoordinateChange), new Type[] { typeof(int), typeof(int) })]
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
