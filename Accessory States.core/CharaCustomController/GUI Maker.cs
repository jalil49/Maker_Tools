using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;


namespace Accessory_States
{
    partial class CharaEvent : CharaCustomFunctionController
    {
        internal readonly Dictionary<int, int[]> GUI_Custom_Dict = new Dictionary<int, int[]>();

        static readonly Dictionary<int, SlotData> GUI_int_state_copy_Dict = new Dictionary<int, SlotData>();

        internal readonly Dictionary<string, bool> GUI_Parent_Dict = new Dictionary<string, bool>();

        private static bool ShowToggleInterface = false;

        private static bool ShowCustomGui = false;

        static Rect _Custom_GroupsRect;

        private static Vector2 _accessorySlotsScrollPos = new Vector2();

        private static Vector2 NameScrolling = new Vector2();
        private static Vector2 StateScrolling = new Vector2();
        private static bool autoscale = true;
        private static int minilimit = 3;
        private Rect screenRect = new Rect((int)(Screen.width * 0.33f), (int)(Screen.height * 0.09f), (int)(Screen.width * 0.225), (int)(Screen.height * 0.273));

        static bool showdelete = false;

        static int tabvalue = 0;
        static int Selectedkind = -1;
        static readonly string[] statenameassign = new string[] { "0", "state 0" };
        static string defaultstatetext = "0";
        private static readonly string[] shoetypetext = new string[] { "Inside", "Outside", "Both" };
        private static readonly string[] TabText = new string[] { "Assign", "Settings" };

        private void OnGUI()
        {
        }
    }
}
