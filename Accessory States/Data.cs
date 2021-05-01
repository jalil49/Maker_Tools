using System;
using System.Collections.Generic;
using System.Linq;

namespace Accessory_States
{
    public class Data
    {
        internal int Personality;
        internal string BirthDay;
        internal string FullName;
        internal CharaEvent Controller;
        internal bool processed = false;

        public Dictionary<int, int>[] ACC_Binding_Dictionary = new Dictionary<int, int>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
        public Dictionary<int, int[]>[] ACC_State_array = new Dictionary<int, int[]>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
        public Dictionary<int, string>[] ACC_Name_Dictionary = new Dictionary<int, string>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
        public Dictionary<int, bool>[] ACC_Parented_Dictionary = new Dictionary<int, bool>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];

        public Dictionary<int, string> Now_ACC_Name_Dictionary = new Dictionary<int, string>();
        public Dictionary<int, int> Now_ACC_Binding_Dictionary = new Dictionary<int, int>();
        public Dictionary<int, int[]> Now_ACC_State_array = new Dictionary<int, int[]>();
        public Dictionary<int, bool> Now_Parented_Dictionary = new Dictionary<int, bool>();
        public Dictionary<int, string> Now_Parented_Name_Dictionary = new Dictionary<int, string>();

        public Data(int _Personality, string _BirthDay, string _FullName, CharaEvent controller)
        {
            Personality = _Personality;
            BirthDay = _BirthDay;
            FullName = _FullName;
            Controller = controller;
            for (int i = 0, n = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length; i < n; i++)
            {
                ACC_Binding_Dictionary[i] = new Dictionary<int, int>();
                ACC_State_array[i] = new Dictionary<int, int[]>();
                ACC_Name_Dictionary[i] = new Dictionary<int, string>();
                ACC_Parented_Dictionary[i] = new Dictionary<int, bool>();
                //ACC_Parented_Name_Dictionary[i] = new Dictionary<int, string>();
            }
        }

        public void Update_Now_Coordinate()
        {
            int outfitnum = Controller.ChaControl.fileStatus.coordinateType;
            Settings.Logger.LogWarning((ChaFileDefine.CoordinateType)outfitnum);
            if (KKAPI.KoikatuAPI.GetCurrentGameMode() == KKAPI.GameMode.Maker)
            {
                Now_ACC_Binding_Dictionary = new Dictionary<int, int>();
                Now_ACC_Binding_Dictionary = ACC_Binding_Dictionary[outfitnum];

                Now_ACC_Name_Dictionary = new Dictionary<int, string>();
                Now_ACC_Name_Dictionary = ACC_Name_Dictionary[outfitnum];

                Now_ACC_State_array = new Dictionary<int, int[]>();
                Now_ACC_State_array = ACC_State_array[outfitnum];

                Now_Parented_Dictionary = new Dictionary<int, bool>();
                Now_Parented_Dictionary = ACC_Parented_Dictionary[outfitnum];
                //Now_Parented_Name_Dictionary = ACC_Parented_Name_Dictionary[outfitnum];
            }
            else
            {
                Now_ACC_Binding_Dictionary = new Dictionary<int, int>(ACC_Binding_Dictionary[outfitnum]);
                Now_ACC_Name_Dictionary = new Dictionary<int, string>(ACC_Name_Dictionary[outfitnum]);
                Now_ACC_State_array = new Dictionary<int, int[]>(ACC_State_array[outfitnum]);
                Now_Parented_Dictionary = new Dictionary<int, bool>(ACC_Parented_Dictionary[outfitnum]);
                //Now_Parented_Name_Dictionary = new Dictionary<int, string>(ACC_Parented_Name_Dictionary[outfitnum]);
            }
            Update_Parented_Name();
        }
        public void Update_Parented_Name()
        {
            Now_Parented_Name_Dictionary.Clear();
            var ParentedList = Now_Parented_Dictionary.Where(x => x.Value);
            foreach (var item in ParentedList)
            {
                Settings.Logger.LogWarning($"Slot: {item.Key} is parented to {Controller.Accessorys_Parts[item.Key].parentKey} ");
                Now_Parented_Name_Dictionary[item.Key] = Controller.Accessorys_Parts[item.Key].parentKey;
            }
        }
        public void Clear_Now_Coordinate()
        {
            Now_ACC_Binding_Dictionary.Clear();
            Now_ACC_Name_Dictionary.Clear();
            Now_ACC_State_array.Clear();
            Now_Parented_Dictionary.Clear();
            Now_Parented_Name_Dictionary.Clear();
        }

        public void Clear()
        {
            for (int i = 0, n = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length; i < n; i++)
            {
                ACC_Binding_Dictionary[i].Clear();
                ACC_State_array[i].Clear();
                ACC_Name_Dictionary[i].Clear();
                ACC_Parented_Dictionary[i].Clear();
            }
            Now_Parented_Name_Dictionary.Clear();
        }
    }
}
