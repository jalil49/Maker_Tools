using System;
using System.Collections.Generic;
using ExtensibleSaveFormat;
using MessagePack;
using UnityEngine.Serialization;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject(true)]
    public class SlotData : IMessagePackSerializationCallbackReceiver
    {
        public List<BindingData> bindingDatas;

        [FormerlySerializedAs("Parented")] public bool parented;

        [FormerlySerializedAs("Format")] public GameFormat format;

        public SlotData()
        {
            bindingDatas = new List<BindingData>();
            parented = false;
            format = DefaultFormat;
        }

        internal void NullCheck()
        {
            if (bindingDatas == null)
                bindingDatas = new List<BindingData>();

            if (GameFormat.Unknown == format)
                format = DefaultFormat;
        }

        public bool ShouldSave()
        {
            bindingDatas.RemoveAll(x => x.GetBinding() < 0);

            if (parented)
                return true;
            if (bindingDatas != null && bindingDatas.Count > 0)
                return true;
            return false;
        }

        public bool TryGetBinding(NameData nameData, out BindingData binding)
        {
            foreach (var item in bindingDatas)
            {
                if (nameData != item.NameData)
                    continue;
                binding = item;
                return true;
            }

            binding = null;
            return false;
        }

        public void SetSlot(int slot)
        {
            foreach (var item in bindingDatas) item.SetSlot(slot);
        }

        public PluginData Serialize()
        {
            if (!ShouldSave())
                return null;
            var data = new PluginData { version = Constants.SaveVersion };
            data.data.Add(Constants.AccessoryKey, MessagePackSerializer.Serialize(this));
            return data;
        }

        public bool BindingExists(int binding, int shoe)
        {
            foreach (var item in bindingDatas)
                if (item.GetBinding() == binding)
                    foreach (var item2 in item.States)
                        if (item2.ShoeType == 2 || item2.ShoeType == shoe)
                            return true;

            return false;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            NullCheck();
        }

        public SlotData DeepClone()
        {
            return MessagePackSerializer.Deserialize<SlotData>(MessagePackSerializer.Serialize(this));
        }

        [IgnoreMember] public const GameFormat DefaultFormat =
#if KK
            GameFormat.KK;
#elif KKS
            GameFormat.KKS;
#elif EC
            GameFormat.EC;
#endif
        public enum GameFormat
        {
            Unknown,
            KK,
            KKS,
            EC
        }
    }
}