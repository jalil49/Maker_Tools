using HarmonyLib;
using Sideloader.AutoResolver;
using System;
using System.Collections.Generic;
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
            Harmony.CreateAndPatchAll(typeof(CompatibilityResolve));
            Harmony.CreateAndPatchAll(typeof(MigrateData));
        }

        internal static bool WaitForSave = false;

        internal static bool Outdated = false;
        internal readonly static List<string> OutdatedList = new List<string>();

        internal static bool Missing = false;
        internal readonly static List<string> MissingList = new List<string>();

        internal static bool Migrateable = false;

        internal static bool Resolverinprogress = false;
        internal static bool Waitforresolver = false;

        private static bool Compatibilitinprogressy = false;
        private static bool Migratedatainprogress = false;


        [HarmonyPatch(typeof(ChaFileCoordinate), nameof(ChaFileCoordinate.SaveFile))]
        internal class CoordinateSaveHook
        {
            [HarmonyPriority(Priority.LowerThanNormal)]
            internal static void Prefix(ref string path)
            {
                path = CardUpdateTool.InterceptSave(path);
            }

            internal static void Postfix(string path)
            {
                CardUpdateTool.DeleteIntercept(path);
            }
        }

        [HarmonyPatch(typeof(UniversalAutoResolver), "ResolveStructure")]
        internal class ResolveHook
        {
            internal static void Prefix()
            {
                Resolverinprogress = true;
                Waitforresolver = false;
            }

            internal static void Postfix()
            {
                Resolverinprogress = false;
            }
        }

        [HarmonyPatch(typeof(UniversalAutoResolver), "CompatibilityResolve")]
        internal class CompatibilityResolve
        {
            internal static void Prefix()
            {
                Compatibilitinprogressy = true;
            }

            internal static void Postfix()
            {
                Compatibilitinprogressy = false;
            }
        }

        [HarmonyPatch(typeof(UniversalAutoResolver), "MigrateData")]
        internal class MigrateData
        {
            internal static void Prefix()
            {
                Migratedatainprogress = true;
            }

            internal static void Postfix()
            {
                Migratedatainprogress = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UniversalAutoResolver), "ShowGUIDError")]
        internal static void ErrorHook(string guid)
        {
            if (UniversalAutoResolver.LoadedResolutionInfo.Any(x => x.GUID == guid))
            //possibly Useable but missing something
            {
                Outdated = true;
                if (!OutdatedList.Contains(guid))
                    OutdatedList.Add(guid);
                return;
            }
            //Not Useable
            Missing = true;
            if (!MissingList.Contains(guid))
                MissingList.Add(guid);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Sideloader.Sideloader), nameof(Sideloader.Sideloader.GetManifest))]
        internal static void GetManifestHook(Sideloader.Manifest __result)
        {
            if (!Migratedatainprogress)
            {
                return;
            }
            if (__result != null)
            //Useable
            {
                Migrateable = true;
                return;
            }
            //Not Useable
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UniversalAutoResolver), nameof(UniversalAutoResolver.TryGetResolutionInfo), typeof(int), typeof(string), typeof(ChaListDefine.CategoryNo), typeof(string))]
        internal static void MigrationHook(ResolveInfo __result)
        {
            if (__result == null || !Resolverinprogress || !Compatibilitinprogressy)
            {
                return;
            }
            Migrateable = true;
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch(typeof(ChaFile), nameof(ChaFile.SaveFile), new Type[] { typeof(System.IO.BinaryWriter), typeof(bool) })]
        internal static void CharaSaveFinishHook()
        {
            WaitForSave = false;
        }
    }
}
