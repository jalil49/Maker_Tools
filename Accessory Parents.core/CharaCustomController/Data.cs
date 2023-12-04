using System.Collections.Generic;
using KKAPI.Chara;
using UnityEngine;

namespace Accessory_Parents
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private readonly Dictionary<int, int> _child = new Dictionary<int, int>();

        private readonly Dictionary<int, string> _oldParent = new Dictionary<int, string>();
        private readonly List<CustomName> _parentGroups = new List<CustomName>();

        private readonly Dictionary<int, List<int>> _relatedNames = new Dictionary<int, List<int>>();

        private readonly Dictionary<int, Vector3[]> _relativeData = new Dictionary<int, Vector3[]>();

        #region Properties

        private ChaFileAccessory.PartsInfo[] Parts
        {
            get => ChaControl.nowCoordinate.accessory.parts;
            set => ChaControl.nowCoordinate.accessory.parts = value;
        }

        #endregion
    }
}