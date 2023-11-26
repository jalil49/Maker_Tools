using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace Additional_Card_Info
{
    [Serializable]
    [MessagePackObject]
    public class RestrictionData : IMessagePackSerializationCallbackReceiver
    {
        public RestrictionData() => NullCheck();

        public void OnBeforeSerialize() => CleanUp();

        public void OnAfterDeserialize() => NullCheck();

        private void NullCheck()
        {
            PersonalityType_Restriction = PersonalityType_Restriction ?? new Dictionary<int, int>();
            Interest_Restriction = Interest_Restriction ?? new Dictionary<int, int>();
            TraitType_Restriction = TraitType_Restriction ?? new Dictionary<int, int>();
            Height_Restriction = Height_Restriction ?? new bool[3];
            BreastSize_Restriction = BreastSize_Restriction ?? new bool[3];
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
        public bool[] BreastSize_Restriction;

        [Key("_htype")]
        public int HStateType_Restriction;

        [Key("_club")]
        public int ClubType_Restriction;

        [Key("_gender")]
        public int GenderType;

        [Key("_coordtype")]
        public int CoordinateType;

        [Key("_coordsubtype")]
        public int CoordinateSubType;

        #endregion
    }
}