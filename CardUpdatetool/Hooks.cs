using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Sideloader;
using Sideloader.AutoResolver;

namespace CardUpdateTool
{
    internal static class Hooks
    {
        internal static bool WaitForSave;

        internal static bool Outdated;
        internal static readonly List<string> OutdatedList = new List<string>();

        internal static bool Missing;
        internal static readonly List<string> MissingList = new List<string>();

        internal static bool Migrateable;

        internal static bool Resolverinprogress;
        internal static bool Waitforresolver;

        private static bool _compatibilitinprogressy;
        private static bool _migratedatainprogress;

        internal static void Init()
        {
            Harmony.CreateAndPatchAll(typeof(Hooks));
            Harmony.CreateAndPatchAll(typeof(ResolveHook));
            Harmony.CreateAndPatchAll(typeof(CoordinateSaveHook));
            Harmony.CreateAndPatchAll(typeof(CompatibilityResolve));
            Harmony.CreateAndPatchAll(typeof(MigrateData));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UniversalAutoResolver), "ShowGUIDError")]
        internal static void ErrorHook(string guid)
        {
            if (UniversalAutoResolver.LoadedResolutionInfo.Any(x => x.GUID == guid))
                //possibly Usable but missing something
            {
                Outdated = true;
                if (!OutdatedList.Contains(guid))
                    OutdatedList.Add(guid);
                return;
            }

            //Not Usable
            Missing = true;
            if (!MissingList.Contains(guid))
                MissingList.Add(guid);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Sideloader.Sideloader), nameof(Sideloader.Sideloader.GetManifest))]
        internal static void GetManifestHook(Manifest __result)
        {
            if (!_migratedatainprogress) return;
            if (__result != null)
                //Usable
                Migrateable = true;
            //Not Usable
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UniversalAutoResolver), nameof(UniversalAutoResolver.TryGetResolutionInfo), typeof(int),
            typeof(string), typeof(ChaListDefine.CategoryNo), typeof(string))]
        internal static void MigrationHook(ResolveInfo __result)
        {
            if (__result == null || !Resolverinprogress || !_compatibilitinprogressy) return;
            Migrateable = true;
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch(typeof(ChaFile), nameof(ChaFile.SaveFile), typeof(BinaryWriter), typeof(bool))]
        internal static void CharaSaveFinishHook()
        {
            WaitForSave = false;
        }


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
                _compatibilitinprogressy = true;
            }

            internal static void Postfix()
            {
                _compatibilitinprogressy = false;
            }
        }

        [HarmonyPatch(typeof(UniversalAutoResolver), "MigrateData")]
        internal class MigrateData
        {
            internal static void Prefix()
            {
                _migratedatainprogress = true;
            }

            internal static void Postfix()
            {
                _migratedatainprogress = false;
            }
        }
    }
}