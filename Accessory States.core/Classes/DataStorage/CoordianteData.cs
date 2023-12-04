using System;
using ExtensibleSaveFormat;
using MessagePack;
using UnityEngine.Serialization;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject(true)]
    public class CoordinateData : IMessagePackSerializationCallbackReceiver
    {
        [FormerlySerializedAs("ClothingNotData")] public bool[] clothingNotData;

        private int _assShowPreference;

        public CoordinateData()
        {
#if KKS
            _assShowPreference = 1; //KKS Only has outdoor shoes
#else
            _assShowPreference = 0;
#endif
            clothingNotData = new bool[3] { false, false, false };
        }

        public int AssShowPreference
        {
            get => _assShowPreference;
            set
            {
                if (value > 1)
                    value = 1;
                if (value < 0)
                    value = 0;
                _assShowPreference = value;
            }
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            NullCheck();
        }

        public void Clear()
        {
            clothingNotData = null;
            NullCheck();
#if KKS
            _assShowPreference = 1; //KKS Only has outdoor shoes
#else
            _assShowPreference = 0;
#endif
        }

        internal void NullCheck()
        {
            clothingNotData = clothingNotData ?? new bool[3] { false, false, false };
            if (_assShowPreference < 0 || _assShowPreference > 1)
            {
#if KKS
                _assShowPreference = 1; //KKS Only has outdoor shoes
#else
                _assShowPreference = 0;
#endif
            }
        }

        public PluginData Serialize()
        {
            var data = new PluginData { version = Constants.SaveVersion };
            data.data.Add(Constants.CoordinateKey, MessagePackSerializer.Serialize(this));
            return data;
        }
    }
}