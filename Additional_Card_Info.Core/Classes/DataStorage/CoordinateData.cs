using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExtensibleSaveFormat;
using MessagePack;
using UnityEngine.Serialization;

namespace Additional_Card_Info
{
    [Serializable]
    [MessagePackObject(true)]
    public class CoordinateInfo : IMessagePackSerializationCallbackReceiver
    {
        public CoordinateInfo() => NullCheck();

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            NullCheck();
        }

        public void Clear()
        {
            clothNotData = new bool[3];
            coordinateSaveBools = new bool[9];
            advancedFolder = string.Empty;
            creatorNames.Clear();
            setNames = string.Empty;
            subSetNames = string.Empty;
            restrictionData = new RestrictionData();
        }

        internal void CleanUp()
        {
            var invalidPath = Path.GetInvalidPathChars();

            foreach (var item in invalidPath)
            {
                setNames = setNames.Replace(item, '_');
                subSetNames = subSetNames.Replace(item, '_');
            }
        }

        private void NullCheck()
        {
            clothNotData = clothNotData ?? new bool[3];

            coordinateSaveBools = coordinateSaveBools ?? new bool[9];

            advancedFolder = advancedFolder ?? string.Empty;

            creatorNames = creatorNames ?? new List<string>();

            setNames = setNames ?? string.Empty;

            subSetNames = subSetNames ?? string.Empty;

            restrictionData = restrictionData ?? new RestrictionData();
        }

        public PluginData Serialize(string creatorName = null)
        {
            if (!creatorName.IsNullOrEmpty() && (creatorNames.Count == 0 || creatorNames.Last() != creatorName))
            {
                creatorNames.Add(creatorName);
            }

            return new PluginData
            {
                version = Constants.MasterSaveVersion,
                data = new Dictionary<string, object>
                    { [Constants.CoordinateKey] = MessagePackSerializer.Serialize(this) }
            };
        }

        public static CoordinateInfo Deserialize(object bytearray) =>
            MessagePackSerializer.Deserialize<CoordinateInfo>((byte[])bytearray);

        #region fields

        [FormerlySerializedAs("MakeUpKeep")] public bool makeUpKeep;

        [FormerlySerializedAs("ClothNotData")] public bool[] clothNotData;

        [FormerlySerializedAs("CoordinateSaveBools")] public bool[] coordinateSaveBools;

        [FormerlySerializedAs("CreatorNames")] public List<string> creatorNames;

        [FormerlySerializedAs("SetNames")] public string setNames;

        [FormerlySerializedAs("SubSetNames")] public string subSetNames;

        [FormerlySerializedAs("AdvancedFolder")] public string advancedFolder;

        [FormerlySerializedAs("RestrictionData")] public RestrictionData restrictionData;

        #endregion
    }
}