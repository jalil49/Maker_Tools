using BepInEx.Logging;
using HarmonyLib;
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
            //if (TryfindPluginInstance("com.deathweasel.bepinex.moreoutfits"))
            //    Harmony.CreateAndPatchAll(typeof(MoreOutfits));
#if ACI || States
            ClothingNotPatch.Init();
#endif
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

#if ACI || States
        internal static class ClothingNotPatch
        {
            internal static bool IsshortsCheck = false;
            internal static Dictionary<ChaListDefine.KeyType, int> ListInfoResult { get; set; } = new Dictionary<ChaListDefine.KeyType, int>() { [ChaListDefine.KeyType.NotBra] = 0, [ChaListDefine.KeyType.Coordinate] = 0 };

            internal static void Init()
            {
                Harmony.CreateAndPatchAll(typeof(ClothingNotPatch));
            }
            //TODO: Check That this doesn't break anything
#if States

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaCustom.CvsClothes), nameof(ChaCustom.CvsClothes.UpdateSelectClothes))]
            public static void Hook_ChangeClothType(ChaCustom.CvsClothes __instance, int index)
            {
                var charaevent = __instance.chaCtrl.GetComponent<CharaEvent>();
                charaevent.ClothingTypeChange(__instance.clothesType, index);

            }
#endif
            [HarmonyPostfix]
            [HarmonyPriority(Priority.HigherThanNormal)]
            [HarmonyPatch(typeof(ListInfoBase), nameof(ListInfoBase.GetInfo))]
            internal static void Hook_GetInfo(ChaListDefine.KeyType keyType, string __result)
            {
                ClothingNotEvent(keyType, __result);
            }

            private static void ClothingNotEvent(ChaListDefine.KeyType keyType, string result)
            {
                if (keyType != ChaListDefine.KeyType.NotBra && keyType != ChaListDefine.KeyType.Coordinate || !int.TryParse(result, out var value))
                {
                    return;
                }
                ListInfoResult[keyType] = value;
            }
        }
#endif
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
