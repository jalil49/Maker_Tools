using System.Collections.Generic;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using ChaCustom;
using HarmonyLib;
using KKAPI.Maker;

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
        private static ManualLogSource _logger;

        public static void Init(ManualLogSource settingLogger)
        {
            _logger = settingLogger;
            Harmony.CreateAndPatchAll(typeof(Hooks));
            if (TryFindPluginInstance("madevil.kk.MovUrAcc"))
                Harmony.CreateAndPatchAll(typeof(MovUrAcc));
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
#if States
            MakerGUI.SlotAccTypeChange(slotNo, type);
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
            [HarmonyPatch(typeof(CvsClothes), nameof(CvsClothes.UpdateSelectClothes))]
            public static void Hook_ChangeClothType(int index)
            {
                if (index < 4)
                    MakerGUI.ClothingTypeChange();
            }
#endif
        }
#endif

        internal static class MovUrAcc
        {
            [HarmonyPostfix]
            [HarmonyPatch("MovUrAcc.MovUrAcc, KK_MovUrAcc", "ProcessQueue")]
            internal static void MovPatch(List<QueueItem> queue)
            {
#if States
                MakerGUI.MovIt(queue);
#else
                MakerAPI.GetCharacterControl().GetComponent<CharaEvent>().MovIt(queue);
#endif
            }
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class QueueItem
    {
        public QueueItem(int src, int dst)
        {
            SrcSlot = src;
            DstSlot = dst;
        }

        public int SrcSlot { get; set; }

        public int DstSlot { get; set; }
    }
}