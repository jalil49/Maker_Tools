using System.Collections.Generic;
using BepInEx.Bootstrap;
using HarmonyLib;
using JetBrains.Annotations;
using KKAPI.Maker;
#if !Themes && !Parents
using ChaCustom;
#endif

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
        public static void Init()
        {
            Harmony.CreateAndPatchAll(typeof(Hooks));
            if (TryFindPluginInstance("madevil.kk.MovUrAcc"))
                Harmony.CreateAndPatchAll(typeof(MovUrAcc));
            //if (TryFindPluginInstance("com.deathweasel.bepinex.moreoutfits"))
            //    Harmony.CreateAndPatchAll(typeof(MoreOutfits));
#if ACI || States
            ClothingNotPatch.Init();
#endif
        }

        private static bool TryFindPluginInstance(string pluginName)
        {
            return Chainloader.PluginInfos.TryGetValue(pluginName, out _);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeAccessory), typeof(int), typeof(int), typeof(int),
            typeof(string), typeof(bool))]
        private static void ChangeAccessory(ChaControl __instance, int slotNo, int type)
        {
            __instance.GetComponent<CharaEvent>().Slot_ACC_Change(slotNo, type);
        }

        internal static class MovUrAcc
        {
            [HarmonyPostfix]
            [HarmonyPatch("MovUrAcc.MovUrAcc, KK_MovUrAcc", "ProcessQueue")]
            internal static void MovPatch(List<QueueItem> queue)
            {
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().MovIt(queue);
            }
        }

        internal static class MoreOutfits
        {
            [HarmonyPostfix]
            [HarmonyPatch("KK_Plugins.MoreOutfits.CharaController+AddCoordinateSlot, KK_MoreOutfits")]
            internal static void AddOutfitHook(ChaControl chaControl)
            {
                chaControl.GetComponent<CharaEvent>().AddOutfitEvent();
            }

            [HarmonyPostfix]
            [HarmonyPatch("KK_Plugins.MoreOutfits.CharaController+RemoveCoordinateSlot, KK_MoreOutfits")]
            internal static void RemoveOutfitHook(ChaControl chaControl)
            {
                chaControl.GetComponent<CharaEvent>().RemoveOutfitEvent();
            }
        }

#if ACI || States
        internal static class ClothingNotPatch
        {
            internal static bool IsShortsCheck = false;

            internal static Dictionary<ChaListDefine.KeyType, int> ListInfoResult { get; } =
                new Dictionary<ChaListDefine.KeyType, int>
                    { [ChaListDefine.KeyType.NotBra] = 0, [ChaListDefine.KeyType.Coordinate] = 0 };

            internal static void Init()
            {
                Harmony.CreateAndPatchAll(typeof(ClothingNotPatch));
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CvsClothes), nameof(CvsClothes.UpdateSelectClothes))]
            public static void Hook_ChangeClothType(CvsClothes __instance, int index)
            {
                var clothesType = __instance.clothesType;
                var charaEvent = __instance.chaCtrl.GetComponent<CharaEvent>();
                if (clothesType < 4) charaEvent.UpdateClothingNots();
#if States
                charaEvent.ClothingTypeChange(clothesType, index);
#endif
            }

            [HarmonyPostfix]
            [HarmonyPriority(Priority.HigherThanNormal)]
            [HarmonyPatch(typeof(ListInfoBase), nameof(ListInfoBase.GetInfo))]
            internal static void Hook_GetInfo(ChaListDefine.KeyType keyType, string __result)
            {
                ClothingNotEvent(keyType, __result);
            }

            private static void ClothingNotEvent(ChaListDefine.KeyType keyType, string result)
            {
                if ((keyType != ChaListDefine.KeyType.NotBra && keyType != ChaListDefine.KeyType.Coordinate) ||
                    !int.TryParse(result, out var value)) return;
                ListInfoResult[keyType] = value;
            }
        }
#endif
    }

    [UsedImplicitly]
    internal class QueueItem
    {
        public QueueItem(int src, int dst)
        {
            SrcSlot = src;
            DstSlot = dst;
        }

        public int SrcSlot { get; }

        public int DstSlot { get; }
    }
}