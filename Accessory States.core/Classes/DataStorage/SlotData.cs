using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accessory_States
{
    [Serializable]
    [MessagePackObject(true)]
    public class SlotData
    {
        public List<BindingData> bindingDatas;

        public bool Parented;

        public SlotData() => NullCheck();

        public SlotData(SlotData slotdata) => CopyData(slotdata);

        public void CopyData(SlotData slotdata) => CopyData(slotdata.bindingDatas, slotdata.Parented);

        public SlotData(List<BindingData> bindingDatas, bool Parented) => CopyData(bindingDatas, Parented);

        public void CopyData(List<BindingData> bindingDatas, bool _parented)
        {
            this.bindingDatas = bindingDatas;
            Parented = _parented;
            NullCheck();
        }

        internal void NullCheck()
        {
            if (bindingDatas == null) bindingDatas = new List<BindingData>();
        }

        public bool ShouldSave()
        {
            if (Parented) return true;
            if (bindingDatas != null && bindingDatas.Count > 0) return true;
            return false;
        }

        public bool Contains(NameData nameData)
        {
            foreach (var item in bindingDatas)
            {
                if (nameData == item.NameData) return true;
            }
            return false;
        }


        public void SetSlot(int slot)
        {
            foreach (var item in bindingDatas)
            {
                item.SetSlot(slot);
            }
        }

        public ExtensibleSaveFormat.PluginData Serialize()
        {

            if (!ShouldSave()) return null;
            var data = new ExtensibleSaveFormat.PluginData() { version = Constants.SaveVersion };
            data.data.Add(Constants.AccessoryKey, MessagePackSerializer.Serialize(this));
            return data;
        }

        public bool BindingExists(int binding, int shoe)
        {
            foreach (var item in bindingDatas)
            {
                if (item.GetBinding() == binding)
                {
                    foreach (var item2 in item.States)
                    {
                        if (item2.ShoeType == 2 || item2.ShoeType == shoe) return true;
                    }
                }
            }
            return false;
        }
    }
}
