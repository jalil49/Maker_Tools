using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExtensibleSaveFormat;
using MessagePack;

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
            ClothNotData = new bool[3];
            CoordinateSaveBools = new bool[9];
            AdvancedFolder = string.Empty;
            CreatorNames.Clear();
            SetNames = string.Empty;
            SubSetNames = string.Empty;
            RestrictionData = new RestrictionData();
        }

        internal void CleanUp()
        {
            var invalidPath = Path.GetInvalidPathChars();

            foreach (var item in invalidPath)
            {
                SetNames = SetNames.Replace(item, '_');
                SubSetNames = SubSetNames.Replace(item, '_');
            }
        }

        private void NullCheck()
        {
            ClothNotData = ClothNotData ?? new bool[3];

            CoordinateSaveBools = CoordinateSaveBools ?? new bool[9];

            AdvancedFolder = AdvancedFolder ?? string.Empty;

            CreatorNames = CreatorNames ?? new List<string>();

            SetNames = SetNames ?? string.Empty;

            SubSetNames = SubSetNames ?? string.Empty;

            RestrictionData = RestrictionData ?? new RestrictionData();
        }

        public PluginData Serialize(string creatorName = null)
        {
            if (!creatorName.IsNullOrEmpty() && (CreatorNames.Count == 0 || CreatorNames.Last() != creatorName))
            {
                CreatorNames.Add(creatorName);
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

        public bool MakeUpKeep;

        public bool[] ClothNotData;

        public bool[] CoordinateSaveBools;

        public List<string> CreatorNames;

        public string SetNames;

        public string SubSetNames;

        public string AdvancedFolder;

        public RestrictionData RestrictionData;

        #endregion
    }
}