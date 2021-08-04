using KKAPI.Chara;
using System.Collections.Generic;

namespace Accessory_Parents
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private Dictionary<int, CoordinateData> Parent_Data = new Dictionary<int, CoordinateData>();

        private CoordinateData Current_Parent_Data = new CoordinateData();

        #region Properties

        private List<Custom_Name> Parent_Groups
        {
            get { return Current_Parent_Data.Parent_Groups; }
            set { Current_Parent_Data.Parent_Groups = value; }
        }

        private Dictionary<int, UnityEngine.Vector3[]> Relative_Data
        {
            get { return Current_Parent_Data.Relative_Data; }
            set { Current_Parent_Data.Relative_Data = value; }
        }

        private Dictionary<int, int> Child
        {
            get { return Current_Parent_Data.Child; }
            set { Current_Parent_Data.Child = value; }
        }

        private Dictionary<int, List<int>> RelatedNames
        {
            get { return Current_Parent_Data.RelatedNames; }
            set { Current_Parent_Data.RelatedNames = value; }
        }

        private Dictionary<int, string> Old_Parent
        {
            get { return Current_Parent_Data.Old_Parent; }
            set { Current_Parent_Data.Old_Parent = value; }
        }

        #endregion
    }
}
