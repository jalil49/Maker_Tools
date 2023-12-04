using System;
using System.Collections.Generic;
using MessagePack;

namespace Accessory_States.Migration.Version1
{
    [Serializable]
    [MessagePackObject]
    public class NameDataV1 : IMessagePackSerializationCallbackReceiver
    {
        public NameDataV1()
        {
            Name = "Default Name";
            Statenames = new Dictionary<int, string>();
        }

        [Key("_name")] public string Name { get; set; }

        [Key("_statenames")] public Dictionary<int, string> Statenames { get; set; }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            NullCheck();
        }

        private void NullCheck()
        {
            Statenames = Statenames ?? new Dictionary<int, string>();
            Name = Name ?? "Default Name";
        }

        public NameData ToNewNameData()
        {
            var nameData = new NameData { Name = Name, StateNames = Statenames };
            nameData.NullCheck();
            return nameData;
        }
    }
}