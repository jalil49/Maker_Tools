using ExtensibleSaveFormat;
using MessagePack;
using System;
using System.Collections.Generic;


namespace Additional_Card_Info
{
    [Serializable]
    [MessagePackObject(true)]
    public class CardInfo : IMessagePackSerializationCallbackReceiver
    {
        public bool CosplayReady;

        public bool AdvancedDirectory;

        public bool[] PersonalClothingBools;

        public string SimpleFolderDirectory;

        public Dictionary<string, string> AdvancedFolderDirectory;//for folder reference to external card directory not saved on coordinates

        public CardInfo() { NullCheck(); }

        internal CardInfo(Migrator.CardInfoV1 oldinfo)
        {
            CosplayReady = oldinfo.CosplayReady;
            AdvancedDirectory = oldinfo.AdvancedDirectory;
            PersonalClothingBools = oldinfo.PersonalClothingBools;
            SimpleFolderDirectory = oldinfo.SimpleFolderDirectory;
            AdvancedFolderDirectory = oldinfo.AdvancedFolderDirectory;
            foreach (var item in Constants.Coordinates)
            {
                AdvancedFolderDirectory.Remove(item);
            }
        }

        public void Clear()
        {
            CosplayReady = false;
            PersonalClothingBools = new bool[9];
        }

        private void NullCheck()
        {
            SimpleFolderDirectory = SimpleFolderDirectory ?? "";

            PersonalClothingBools = PersonalClothingBools ?? new bool[9];

            AdvancedFolderDirectory = AdvancedFolderDirectory ?? new Dictionary<string, string>();
        }

        public PluginData Serialize() => new PluginData { version = Constants.MasterSaveVersion, data = new Dictionary<string, object>() { [Constants.CardKey] = MessagePackSerializer.Serialize(this) } };

        public static CardInfo Deserialize(object bytearray) => MessagePackSerializer.Deserialize<CardInfo>((byte[])bytearray);

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() { NullCheck(); }
    }
}
