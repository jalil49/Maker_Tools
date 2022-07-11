﻿using Extensions;
using MessagePack;
using System.Collections.Generic;

namespace Accessory_States.Classes.PresetStorage
{
    [MessagePackObject(true)]
    public class PresetFolder
    {
        public string FileName
        {
            set
            {
                if (_fileName.IsNullOrWhiteSpace())
                    _fileName = Name;
                if (_fileName.Length == 0)
                    _fileName = this.GetHashCode().ToString();

                _fileName = string.Concat(value.Split(System.IO.Path.GetInvalidFileNameChars())).Trim();
            }
            get
            {
                if (_fileName.IsNullOrWhiteSpace())
                    _fileName = Name;
                if (_fileName.Length == 0)
                    _fileName = this.GetHashCode().ToString();

                return _fileName;
            }
        }
        private string _fileName;
        public string Name;
        public List<PresetData> PresetDatas;
        public string Description;

        public const string SerializeKey = "Folder";

        [IgnoreMember]
        public bool SavedOnDisk;

        public PresetFolder()
        {
            PresetDatas = new List<PresetData>();
            Name = "Default Folder Name";
            FileName = "Default File Name";
            Description = "Default Description";
            SavedOnDisk = false;
        }
        public bool Filter(string filter)
        {

            if (Name.Contains(filter, System.StringComparison.OrdinalIgnoreCase) || FileName.Contains(filter, System.StringComparison.OrdinalIgnoreCase) || Description.Contains(filter, System.StringComparison.OrdinalIgnoreCase))
                return false;

            foreach (var item in PresetDatas)
            {
                if (!item.Filter(filter))
                    return false;
            }

            return true;
        }

        public void SaveFile()
        {
            Presets.SaveFile(FileName, MessagePackSerializer.Serialize(Serialize()));
            SavedOnDisk = true;
        }

        public static PresetFolder Deserialize(byte[] data)
        {
            return MessagePackSerializer.Deserialize<PresetFolder>(data);
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