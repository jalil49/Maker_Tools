using MessagePack;
using System;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject(true)]
    public class CoordinateData : IMessagePackSerializationCallbackReceiver
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

        public CoordinateData()
        {
#if KKS
            _assShowPreference = 1;//KKS Only has outdoor shoes
#else
            _assShowPreference = 0;
#endif
            ClothingNotData = new bool[3] { false, false, false };
        }

        public void Clear()
        {
            ClothingNotData = null;
            NullCheck();
#if KKS
            _assShowPreference = 1;//KKS Only has outdoor shoes
#else
            _assShowPreference = 0;
#endif
        }

        internal void NullCheck()
        {
            ClothingNotData = ClothingNotData ?? new bool[3] { false, false, false };
            if (_assShowPreference < 0 || _assShowPreference > 1)
            {
#if KKS
                _assShowPreference = 1;//KKS Only has outdoor shoes
#else
                _assShowPreference = 0;
#endif
            }
        }

        public ExtensibleSaveFormat.PluginData Serialize()
        {
            var data = new ExtensibleSaveFormat.PluginData() { version = Constants.SaveVersion };
            data.data.Add(Constants.CoordinateKey, MessagePackSerializer.Serialize(this));
            return data;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() { NullCheck(); }
    }
}
