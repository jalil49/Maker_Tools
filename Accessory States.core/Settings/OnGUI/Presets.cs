using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Accessory_States.Classes.PresetStorage;
using BepInEx;
using MessagePack;

namespace Accessory_States
{
    internal static class Presets
    {
        private const string Extenstion = ".Presets";

        static Presets()
        {
            SetCachePath();
        }

        internal static string CachePath { get; private set; }

        private static void SetCachePath()
        {
            var sep = Path.DirectorySeparatorChar;
            CachePath = Paths.CachePath + sep + Settings.GUID + sep;
        }

        public static void OpenPresetFolder()
        {
            CreateCacheFolder();
            try
            {
                Process.Start("explorer.exe", $"\"{Path.GetFullPath(CachePath)}\"");
            }
            catch (Exception e)
            {
                Settings.Logger.LogError(e);
                Settings.Logger.LogMessage("Failed to open the folder - " + e.Message);
            }
        }

        internal static bool CreateCacheFolder()
        {
            if (!Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
                return true;
            }

            return false;
        }

        private static bool CreateFile(string save)
        {
            CreateCacheFolder();
            if (!File.Exists(save))
            {
                File.Create(save).Dispose();
                return true;
            }

            return false;
        }

        private static string[] GetAllPresetFiles()
        {
            if (CreateCacheFolder())
            {
                return new string[0];
            }

            return Directory.GetFiles(CachePath);
        }

        public static void LoadAllPresets(out List<PresetData> presetDatas, out List<PresetFolder> presetFolders)
        {
            presetDatas = new List<PresetData>();
            presetFolders = new List<PresetFolder>();
            foreach (var item in GetAllPresetFiles())
            {
                Settings.Logger.LogWarning(item);

                if (TryReadFile(item, out var presetData, out var presetFolder))
                {
                    if (presetData != null)
                    {
                        presetDatas.Add(presetData);
                    }

                    if (presetFolder != null)
                    {
                        presetFolders.Add(presetFolder);
                    }
                }
            }
        }

        public static List<PresetData> LoadAllSinglePresets()
        {
            var presets = new List<PresetData>();
            foreach (var item in GetAllPresetFiles())
            {
                if (TryReadFile(item, out var presetData, out _) && presetData != null)
                {
                    presets.Add(presetData);
                }
            }

            return presets;
        }

        internal static void Rename(string originalFileName, string newFileName)
        {
            var originalFilePath = CachePath + originalFileName + Extenstion;
            var newFilePath = CachePath + newFileName + Extenstion;
            if (!File.Exists(originalFilePath))
            {
                Settings.Logger.LogMessage(
                    $"File not found: Unable to rename file from \"{originalFileName}\" to \"{newFileName}\"");
                return;
            }

            Settings.Logger.LogMessage($"Renaming file from \"{originalFileName}\" to \"{newFileName}\"");
            File.Move(originalFilePath, newFilePath);
        }

        internal static void Delete(string fileName)
        {
            var filePath = CachePath + fileName + Extenstion;
            if (!File.Exists(filePath))
            {
                Settings.Logger.LogWarning($"File not found: Unable to delete file: {fileName}");
                return;
            }

            File.Delete(filePath);
        }

        public static List<PresetFolder> LoadAllFolderPresets()
        {
            var presets = new List<PresetFolder>();
            foreach (var item in GetAllPresetFiles())
            {
                if (TryReadFile(item, out var _, out var presetFolder) && presetFolder != null)
                {
                    presets.Add(presetFolder);
                }
            }

            return presets;
        }

        private static bool TryReadFile(string saveFile, out PresetData presetData, out PresetFolder presetFolder)
        {
            presetData = null;
            presetFolder = null;
            var data = File.ReadAllBytes(saveFile);
            if (data == null || data.Length == 0)
            {
                return false;
            }

            try
            {
                var serializeddict = MessagePackSerializer.Deserialize<KeyValuePair<string, byte[]>>(data);
                if (serializeddict.Key.IsNullOrWhiteSpace())
                {
                    return false;
                }

                switch (serializeddict.Key)
                {
                    case PresetFolder.SerializeKey:
                        presetFolder = PresetFolder.Deserialize(serializeddict.Value);
                        presetFolder.FileName =
                            Path.GetFileNameWithoutExtension(saveFile); //if FileName is manually changed reflect it
                        presetFolder.SavedOnDisk = true;
                        return true;

                    case PresetData.SerializeKey:
                        presetData = PresetData.Deserialize(serializeddict.Value);
                        presetData.FileName =
                            Path.GetFileNameWithoutExtension(saveFile); //if FileName is manually changed reflect it
                        presetData.SavedOnDisk = true;
                        return true;

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Settings.Logger.LogError("Failed to read file " + ex);
            }

            return false;
        }

        internal static void SaveFile(string fileName, byte[] data)
        {
            var filepath = CachePath + fileName + Extenstion;
            CreateFile(filepath);
            File.WriteAllBytes(filepath, data);
        }

        internal static void DeleteCache()
        {
            Directory.Delete(CachePath, true);
        }
    }
}