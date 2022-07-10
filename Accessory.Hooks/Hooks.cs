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
        //TODO: Remove
        static ManualLogSource Logger;

        public static void Init(ManualLogSource Setting_Logger)
        {
            Logger = Setting_Logger;
            Harmony.CreateAndPatchAll(typeof(Hooks));
            if (TryfindPluginInstance("madevil.kk.MovUrAcc"))
                Harmony.CreateAndPatchAll(typeof(MovUrACC));
#if ACI || States
            ClothingNotPatch.Init();
#endif
        }

        private static bool TryfindPluginInstance(string pluginName)
        {
            return BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(pluginName, out _);
            ;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeAccessory), typeof(int), typeof(int), typeof(int), typeof(string), typeof(bool))]
        private static void ChangeAccessory(ChaControl __instance, int slotNo, int type)
        {
#if States
            Maker.SlotAccTypeChange(slotNo, type);
#else
            __instance.GetComponent<CharaEvent>().Slot_ACC_Change(slotNo, type);
#endif
        }

#if ACI || States
        internal static class ClothingNotPatch
        {
            internal static void Init()
            {
                Harmony.CreateAndPatchAll(typeof(ClothingNotPatch));
            }

#if States
            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaCustom.CvsClothes), nameof(ChaCustom.CvsClothes.UpdateSelectClothes))]
            public static void Hook_ChangeClothType(int index)
            {
                if (index < 4)
                    Maker.ClothingTypeChange();
            }
#endif
        }
#endif

        internal static class MovUrACC
        {
            [HarmonyPostfix]
            [HarmonyPatch("MovUrAcc.MovUrAcc, KK_MovUrAcc", "ProcessQueue")]
            internal static void MovPatch(List<QueueItem> Queue)
            {
#if States
                Maker.MovIt(Queue);
#else
                KKAPI.Maker.MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().MovIt(Queue);
#endif
            }
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
