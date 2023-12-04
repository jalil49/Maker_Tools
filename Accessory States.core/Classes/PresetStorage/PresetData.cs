using System;
using System.Collections.Generic;
using System.IO;
using Extensions;
using MessagePack;

namespace Accessory_States.Classes.PresetStorage
{
    [MessagePackObject(true)]
    public class PresetData
    {
        public const string SerializeKey = "Single";
        private string _fileName;
        public SlotData Data;
        public string Description;
        public string Name;
        public bool SavedOnDisk;

        public PresetData()
        {
            Name = "Default Preset Name";
            Description = "Default Description";
            FileName = "Default File Name";
            Data = null;
            SavedOnDisk = false;
        }

        public string FileName
        {
            set
            {
                if (_fileName.IsNullOrWhiteSpace())
                    _fileName = Name;
                if (_fileName.Length == 0)
                    _fileName = GetHashCode().ToString();

                _fileName = string.Concat(value.Split(Path.GetInvalidFileNameChars())).Trim();
            }
            get
            {
                if (_fileName.IsNullOrWhiteSpace())
                    _fileName = Name;
                if (_fileName.Length == 0)
                    _fileName = GetHashCode().ToString();

                return _fileName;
            }
        }

        public bool Filter(string filter)
        {
            if (Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                FileName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                Description.Contains(filter, StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
        }

        public void SaveFile()
        {
            Presets.SaveFile(FileName, MessagePackSerializer.Serialize(Serialize()));
            SavedOnDisk = true;
        }

        public static PresetData Deserialize(byte[] data)
        {
            return MessagePackSerializer.Deserialize<PresetData>(data);
        }

        public static PresetData ConvertSlotData(SlotData slotData, int selectedSlot)
        {
            var presetData = new PresetData
            {
                Name = $"Slot {selectedSlot} Preset",
                Data = MessagePackSerializer.Deserialize<SlotData>(MessagePackSerializer.Serialize(slotData))
            };
            return presetData;
        }

        public KeyValuePair<string, byte[]> Serialize()
        {
            return new KeyValuePair<string, byte[]>(SerializeKey, MessagePackSerializer.Serialize(this));
        }

        internal void Delete()
        {
            Presets.Delete(FileName);
        }
    }
}