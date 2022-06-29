﻿using Extensions;
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

        public int AssShowPreference
        {
            get { return _assShowPreference; }
            set
            {
                if (value > 1)
                    value = 1;
                if (value < 0)
                    value = 0;
                _assShowPreference = value;
            }
        }

        private int _assShowPreference;

        public CoordinateData() { NullCheck(); }

        public void Clear()
        {
            ClothingNotData = null;
            NullCheck();
            AssShowPreference = 0;
        }

        internal void NullCheck()
        {
            if (ClothingNotData == null)
                ClothingNotData = new bool[3] { false, false, false };
        }

        public ExtensibleSaveFormat.PluginData Serialize()
        {
            var data = new ExtensibleSaveFormat.PluginData() { version = Constants.SaveVersion };
            data.data.Add(Constants.CoordinateKey, MessagePackSerializer.Serialize(this));
            return data;
        }
    }
}
