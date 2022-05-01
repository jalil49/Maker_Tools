using KKAPI.Chara;
using System.Collections.Generic;

namespace Accessory_Parents
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private readonly List<Custom_Name> Parent_Groups = new List<Custom_Name>();

        private readonly Dictionary<int, UnityEngine.Vector3[]> Relative_Data = new Dictionary<int, UnityEngine.Vector3[]>();

        private readonly Dictionary<int, int> Child = new Dictionary<int, int>();

        private readonly Dictionary<int, List<int>> RelatedNames = new Dictionary<int, List<int>>();

        private readonly Dictionary<int, string> Old_Parent = new Dictionary<int, string>();

        #region Properties
        private ChaFileAccessory.PartsInfo[] Parts
        {
            get { return ChaControl.nowCoordinate.accessory.parts; }
            set { ChaControl.nowCoordinate.accessory.parts = value; }
        }
        #endregion
    }
}
