using System;
using System.Collections.Generic;
using Additional_Card_Info.Classes.Migration;
using ExtensibleSaveFormat;
using MessagePack;

namespace Additional_Card_Info
{
    [Serializable]
    [MessagePackObject(true)]
    public class CardData : IMessagePackSerializationCallbackReceiver
    {
        public bool CosplayReady;

        public bool AdvancedDirectory;

        public bool[] PersonalClothingBools;

        public string SimpleFolderDirectory;

        public Dictionary<string, string>
            AdvancedFolderDirectory; //for folder reference to external card directory not saved on coordinates

        public CardData() => NullCheck();

        internal CardData(Migrator.CardInfoV1 oldInfo) : this()
        {
            CosplayReady = oldInfo.CosplayReady;
            AdvancedDirectory = oldInfo.AdvancedDirectory;
            PersonalClothingBools = oldInfo.PersonalClothingBools;
            SimpleFolderDirectory = oldInfo.SimpleFolderDirectory;
            AdvancedFolderDirectory = oldInfo.AdvancedFolderDirectory;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            NullCheck();
        }

        public void Clear()
        {
            CosplayReady = false;
            PersonalClothingBools = new bool[9];
        }

        private void NullCheck()
        {
            SimpleFolderDirectory = SimpleFolderDirectory ?? string.Empty;

            PersonalClothingBools = PersonalClothingBools ?? new bool[9];

            if (PersonalClothingBools.Length < 9)
            {
                PersonalClothingBools = new bool[9];
            }

            AdvancedFolderDirectory = AdvancedFolderDirectory ?? new Dictionary<string, string>();
        }

        public PluginData Serialize() =>
            new PluginData
            {
                version = Constants.MasterSaveVersion,
                data = new Dictionary<string, object> { [Constants.CardKey] = MessagePackSerializer.Serialize(this) }
            };

        public static CardData Deserialize(object bytearray) =>
            MessagePackSerializer.Deserialize<CardData>((byte[])bytearray);
    }
}