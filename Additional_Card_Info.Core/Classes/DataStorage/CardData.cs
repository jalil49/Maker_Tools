using System;
using System.Collections.Generic;
using Additional_Card_Info.Classes.Migration;
using ExtensibleSaveFormat;
using MessagePack;
using UnityEngine.Serialization;

namespace Additional_Card_Info
{
    [Serializable]
    [MessagePackObject(true)]
    public class CardData : IMessagePackSerializationCallbackReceiver
    {
        [FormerlySerializedAs("CosplayReady")] public bool cosplayReady;

        [FormerlySerializedAs("AdvancedDirectory")] public bool advancedDirectory;

        [FormerlySerializedAs("PersonalClothingBools")] public bool[] personalClothingBools;

        [FormerlySerializedAs("SimpleFolderDirectory")] public string simpleFolderDirectory;

        public Dictionary<string, string>
            AdvancedFolderDirectory; //for folder reference to external card directory not saved on coordinates

        public CardData() => NullCheck();

        internal CardData(Migrator.CardInfoV1 oldInfo) : this()
        {
            cosplayReady = oldInfo.cosplayReady;
            advancedDirectory = oldInfo.advancedDirectory;
            personalClothingBools = oldInfo.personalClothingBools;
            simpleFolderDirectory = oldInfo.simpleFolderDirectory;
            AdvancedFolderDirectory = oldInfo.AdvancedFolderDirectory;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            NullCheck();
        }

        public void Clear()
        {
            cosplayReady = false;
            personalClothingBools = new bool[9];
        }

        private void NullCheck()
        {
            simpleFolderDirectory = simpleFolderDirectory ?? string.Empty;

            personalClothingBools = personalClothingBools ?? new bool[9];

            if (personalClothingBools.Length < 9)
            {
                personalClothingBools = new bool[9];
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