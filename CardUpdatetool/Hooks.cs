using HarmonyLib;
using Sideloader.AutoResolver;
using System;
using System.Linq;

namespace CardUpdateTool
{
    internal static class Hooks
    {
        internal static void Init()
        {
            Harmony.CreateAndPatchAll(typeof(Hooks));
            Harmony.CreateAndPatchAll(typeof(ResolveHook));
            Harmony.CreateAndPatchAll(typeof(CoordinateSaveHook));
        }

        [HarmonyPatch(typeof(ChaFileCoordinate), nameof(ChaFileCoordinate.SaveFile))]
        internal class CoordinateSaveHook
        {
            [HarmonyPriority(Priority.LowerThanNormal)]
            internal static void Prefix(ref string path)
            {
                path = CharaEvent.InterceptSave(path);
            }

            internal static void Postfix(string path)
            {
                CharaEvent.DeleteIntercept(path);
            }
        }

        [HarmonyPatch(typeof(UniversalAutoResolver), "ResolveStructure")]
        internal class ResolveHook
        {
            internal static void Prefix()
            {
                CharaEvent.resolverinprogress = true;
                CharaEvent.Waitforresolver = false;
            }

            internal static void Postfix()
            {
                CharaEvent.resolverinprogress = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UniversalAutoResolver), "ShowGUIDError")]
        internal static void ErrorHook(string guid)
        {
            if (UniversalAutoResolver.LoadedResolutionInfo.Any(x => x.GUID == guid))
            //Useable
            {
                CharaEvent.NeedUpdate = true;
            }
            else
            //Not Useable
            {
                CharaEvent.BlackList = true;
            }
        }


        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch(typeof(ChaFile), nameof(ChaFile.SaveFile), new Type[] { typeof(System.IO.BinaryWriter), typeof(bool) })]
        internal static void CharaSaveFinishHook()
        {
            CharaEvent.WaitForSave = false;
        }
    }
}
