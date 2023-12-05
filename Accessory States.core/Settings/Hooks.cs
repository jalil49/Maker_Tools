using System;
using ChaCustom;
using HarmonyLib;

namespace Accessory_States
{
    internal static partial class Hooks
    {
        public static event EventHandler<OnClickCoordinateChange> HCoordinateChange;

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
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnClickCoordinateChange), typeof(int))]
        public static void Hook_OnClickCoordinateChange(int coordinate)
        {
            OnClickCoordinateChangeEvent(0, coordinate);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomChangeMainMenu), nameof(CustomChangeMainMenu.ChangeWindowSetting))]
        public static void Hook_ChangeWindowSetting(int no)
        {
            CharaEvent.StopMakerLoop = no == 3;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnClickCoordinateChange), typeof(int), typeof(int))]
        public static void Hook_OnClickCoordinateChange2(int female, int coordinate)
        {
            OnClickCoordinateChangeEvent(female, coordinate);
        }

        private static void OnClickCoordinateChangeEvent(int female, int coordinate)
        {
            var args = new OnClickCoordinateChange(female, coordinate);
            if (HCoordinateChange == null || HCoordinateChange.GetInvocationList().Length == 0) return;
            try
            {
                HCoordinateChange?.Invoke(null, args);
            }
            catch (Exception ex)
            {
                Settings.Logger.LogError($"Subscriber crash in {nameof(Hooks)}.{nameof(HCoordinateChange)} - {ex}");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.SetState))]
        internal static void LoadSetHook(HSceneProc __instance)
        {
            if (__instance.flags.isFreeH)
                CharaEvent.FreeHHeroines = __instance.flags.lstHeroine;
        }
    }
}