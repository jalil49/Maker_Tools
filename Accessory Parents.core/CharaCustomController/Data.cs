using System.Collections.Generic;
using KKAPI.Chara;
using UnityEngine;

namespace Accessory_Parents
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private CoordinateData _currentParentData = new CoordinateData();
        private Dictionary<int, CoordinateData> _parentData = new Dictionary<int, CoordinateData>();

        #region Properties

        private List<CustomName> ParentGroups
        {
            get => _currentParentData.parentGroups;
            set => _currentParentData.parentGroups = value;
        }

        private Dictionary<int, Vector3[]> RelativeData
        {
            get => _currentParentData.RelativeData;
            set => _currentParentData.RelativeData = value;
        }

        private Dictionary<int, int> Child
        {
            get => _currentParentData.Child;
            set => _currentParentData.Child = value;
        }

        private Dictionary<int, List<int>> RelatedNames
        {
            get => _currentParentData.RelatedNames;
            set => _currentParentData.RelatedNames = value;
        }

        private Dictionary<int, string> OldParent
        {
            get => _currentParentData.OldParent;
            set => _currentParentData.OldParent = value;
        }

        #endregion
    }
}