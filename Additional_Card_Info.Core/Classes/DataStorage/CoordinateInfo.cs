using ExtensibleSaveFormat;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Additional_Card_Info
{
    [Serializable]
    [MessagePackObject(true)]
    public class CoordinateInfo : IMessagePackSerializationCallbackReceiver
    {
        #region fields
        public bool MakeUpKeep;

        public bool[] ClothNotData;

        public bool[] CoordinateSaveBools;

        public List<string> CreatorNames;

        public string SetNames;

        public string SubSetNames;

        public string AdvancedFolder;

        public RestrictionInfo RestrictionInfo;
        #endregion

        public CoordinateInfo() { NullCheck(); }

        public void Clear()
        {
            ClothNotData = new bool[3];
            CoordinateSaveBools = new bool[9];
            AdvancedFolder = "";
            CreatorNames.Clear();
            SetNames = "";
            SubSetNames = "";
            RestrictionInfo.Clear();
        }

        internal void CleanUp()
        {
            var invalidpath = System.IO.Path.GetInvalidPathChars();

            foreach (var item in invalidpath)
            {
                SetNames.Replace(item, '_');
                SubSetNames.Replace(item, '_');
            }
            RestrictionInfo.CleanUp();
        }

        private void NullCheck()
        {
            ClothNotData = ClothNotData ?? new bool[3];

            CoordinateSaveBools = CoordinateSaveBools ?? new bool[9];

            AdvancedFolder = AdvancedFolder ?? "";

            CreatorNames = CreatorNames ?? new List<string>();

            SetNames = SetNames ?? "";

            SubSetNames = SubSetNames ?? "";

            RestrictionInfo = RestrictionInfo ?? new RestrictionInfo();
        }

        public PluginData Serialize(string creatorname = null)
        {
            if (!creatorname.IsNullOrEmpty() && (CreatorNames.Count == 0 || CreatorNames.Last() != creatorname))
            {
                CreatorNames.Add(creatorname);
            }

            return new PluginData { version = Constants.MasterSaveVersion, data = new Dictionary<string, object>() { [Constants.CoordinateKey] = MessagePackSerializer.Serialize(this) } };
        }

        public static CoordinateInfo Deserialize(object bytearray) => MessagePackSerializer.Deserialize<CoordinateInfo>((byte[])bytearray);

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() { NullCheck(); }
    }
}
