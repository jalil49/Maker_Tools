using BepInEx.Logging;
using HarmonyLib;
using KKAPI.Maker;
using System.Collections.Generic;
#if Parents
namespace Accessory_Parents
#elif States
namespace Accessory_States
#elif Themes
namespace Accessory_Themes
#elif ACI
namespace Additional_Card_Info
#endif
{
    internal static partial class Hooks
    {
        static ManualLogSource Logger;

        public static void Init(ManualLogSource Setting_Logger)
        {
            Logger = Setting_Logger;
            Harmony.CreateAndPatchAll(typeof(Hooks));
            if (TryfindPluginInstance("madevil.kk.MovUrAcc"))
                Harmony.CreateAndPatchAll(typeof(MovUrACC));
            if (TryfindPluginInstance("com.deathweasel.bepinex.moreoutfits"))
                Harmony.CreateAndPatchAll(typeof(MoreOutfits));
        }

        private static bool TryfindPluginInstance(string pluginName)
        {
            return BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(pluginName, out _); ;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeAccessory), typeof(int), typeof(int), typeof(int), typeof(string), typeof(bool))]
        private static void ChangeAccessory(ChaControl __instance, int slotNo, int type)
        {
            __instance.GetComponent<CharaEvent>().Slot_ACC_Change(slotNo, type);
        }

        internal static class MovUrACC
        {
            [HarmonyPostfix]
            [HarmonyPatch("MovUrAcc.MovUrAcc, KK_MovUrAcc", "ProcessQueue")]
            internal static void MovPatch(List<QueueItem> Queue)
            {
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().MovIt(Queue);
            }
        }

        internal static class MoreOutfits
        {
            [HarmonyPostfix, HarmonyPatch("KK_Plugins.MoreOutfits.CharaController+AddCoordinateSlot, KK_MoreOutfits")]
            internal static void AddOutfitHook(ChaControl chaControl)
            {
                chaControl.GetComponent<CharaEvent>().AddOutfitEvent();
            }

            [HarmonyPostfix, HarmonyPatch("KK_Plugins.MoreOutfits.CharaController+RemoveCoordinateSlot, KK_MoreOutfits")]
            internal static void RemoveOutfitHook(ChaControl chaControl)
            {
                chaControl.GetComponent<CharaEvent>().RemoveOutfitEvent();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaCustom.CvsClothes), nameof(ChaCustom.CvsClothes.UpdateSelectClothes))]
        public static void Hook_ChangeClothType(ChaCustom.CvsClothes __instance, int index)
        {
#if ACI
            __instance.chaCtrl.GetComponent<CharaEvent>().UpdateClothingNots();
#elif States
            __instance.chaCtrl.GetComponent<CharaEvent>().ClothingTypeChange(__instance.clothesType, index);
#endif
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
