using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Accessory_States.Migration.Version1
{
    //TODO: Remove if effectively Redundant
    [Serializable]
    [MessagePackObject]
    internal class NameDataV1
    {
        [Key("_name")]
        public string Name { get; set; }

        [Key("_statenames")]
        public Dictionary<int, string> Statenames { get; set; }

        public NameDataV1() { NullCheck(); }

        private void NullCheck()
        {
            if (Statenames == null) Statenames = new Dictionary<int, string>();
            if (Name == null) Name = "";
        }

        public NameData ToNewNameData()
        {
            var nameData = new NameData() { Name = this.Name, StateNames = this.Statenames };
            nameData.NullCheck();
            return nameData;
        }
    }
}
