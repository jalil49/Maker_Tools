using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Additional_Card_Info
{
    [Serializable]
    [MessagePackObject]
    public class RestrictionInfo : IMessagePackSerializationCallbackReceiver
    {
        #region Fields
        [Key("_personality")]
        public Dictionary<int, int> PersonalityType_Restriction;

        [Key("_trait")]
        public Dictionary<int, int> TraitType_Restriction;

        [Key("_interest")]
        public Dictionary<int, int> Interest_Restriction;

        [Key("_height")]
        public bool[] Height_Restriction;

        [Key("_breast")]
        public bool[] Breastsize_Restriction;

        [Key("_htype")]
        public int HstateType_Restriction;

        [Key("_club")]
        public int ClubType_Restriction;

        [Key("_gender")]
        public int GenderType;

        [Key("_coordtype")]
        public int CoordinateType;

        [Key("_coordsubtype")]
        public int CoordinateSubType;
        #endregion

        public RestrictionInfo() { NullCheck(); }

        private void NullCheck()
        {
            PersonalityType_Restriction = PersonalityType_Restriction ?? new Dictionary<int, int>();
            Interest_Restriction = Interest_Restriction ?? new Dictionary<int, int>();
            TraitType_Restriction = TraitType_Restriction ?? new Dictionary<int, int>();
            Height_Restriction = Height_Restriction ?? new bool[3];
            Breastsize_Restriction = Breastsize_Restriction ?? new bool[3];
        }

        public void Clear()
        {
            PersonalityType_Restriction.Clear();

            TraitType_Restriction.Clear();

            Interest_Restriction.Clear();

            CoordinateType = 0;

            CoordinateSubType = 0;

            HstateType_Restriction = 0;

            ClubType_Restriction = 0;

            GenderType = 0;

            for (var i = 0; i < 3; i++)
            {
                Height_Restriction[i] = Breastsize_Restriction[i] = false;
            }
        }

        internal void CleanUp()
        {
            var clean = PersonalityType_Restriction.Where(x => x.Value == 0).Select(x => x.Key).ToList();
            foreach (var item in clean)
            {
                PersonalityType_Restriction.Remove(item);
            }
            clean = TraitType_Restriction.Where(x => x.Value == 0).Select(x => x.Key).ToList();
            foreach (var item in clean)
            {
                TraitType_Restriction.Remove(item);
            }
            clean = Interest_Restriction.Where(x => x.Value == 0).Select(x => x.Key).ToList();
            foreach (var item in clean)
            {
                Interest_Restriction.Remove(item);
            }
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() { NullCheck(); }
    }
}
