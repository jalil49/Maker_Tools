using Extensions;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject(true)]
    public class CoordinateData
    {
        public bool[] ClothingNotData;
        public CoordinateData() { NullCheck(); }

        public void Clear()
        {
        }

        internal void NullCheck()
        {
            if (ClothingNotData == null) ClothingNotData = new bool[3] { false, false, false };
        }

        public ExtensibleSaveFormat.PluginData Serialize()
        {
            var data = new ExtensibleSaveFormat.PluginData() { version = Constants.SaveVersion };
            data.data.Add(Constants.CoordinateKey, MessagePackSerializer.Serialize(this));
            return data;
        }
    }
}
