using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using UnityEngine.Serialization;

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
            PersonalityTypeRestriction = PersonalityTypeRestriction ?? new Dictionary<int, int>();
            InterestRestriction = InterestRestriction ?? new Dictionary<int, int>();
            TraitTypeRestriction = TraitTypeRestriction ?? new Dictionary<int, int>();
            heightRestriction = heightRestriction ?? new bool[3];
            breastSizeRestriction = breastSizeRestriction ?? new bool[3];
        }

        internal void CleanUp()
        {
            var clean = PersonalityTypeRestriction.Where(x => x.Value == 0).Select(x => x.Key).ToList();
            foreach (var item in clean)
            {
                PersonalityTypeRestriction.Remove(item);
            }

            clean = TraitTypeRestriction.Where(x => x.Value == 0).Select(x => x.Key).ToList();
            foreach (var item in clean)
            {
                TraitTypeRestriction.Remove(item);
            }

            clean = InterestRestriction.Where(x => x.Value == 0).Select(x => x.Key).ToList();
            foreach (var item in clean)
            {
                InterestRestriction.Remove(item);
            }
        }

        #region Fields

        [Key("_personality")]
        public Dictionary<int, int> PersonalityTypeRestriction;

        [Key("_trait")]
        public Dictionary<int, int> TraitTypeRestriction;

        [Key("_interest")]
        public Dictionary<int, int> InterestRestriction;

        [FormerlySerializedAs("Height_Restriction")] [Key("_height")]
        public bool[] heightRestriction;

        [FormerlySerializedAs("BreastSize_Restriction")] [Key("_breast")]
        public bool[] breastSizeRestriction;

        [FormerlySerializedAs("HStateType_Restriction")] [Key("_htype")]
        public int hStateTypeRestriction;

        [FormerlySerializedAs("ClubType_Restriction")] [Key("_club")]
        public int clubTypeRestriction;

        [FormerlySerializedAs("GenderType")] [Key("_gender")]
        public int genderType;

        [FormerlySerializedAs("CoordinateType")] [Key("_coordtype")]
        public int coordinateType;

        [FormerlySerializedAs("CoordinateSubType")] [Key("_coordsubtype")]
        public int coordinateSubType;

        #endregion
    }
}