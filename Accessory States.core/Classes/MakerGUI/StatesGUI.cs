using Extensions.GUI_Classes;
using KKAPI.Maker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static GUIHelper.OnGuiExtensions;
using static UnityEngine.GUILayout;

namespace Accessory_States
{
    //Create New NameData groups
    internal class StatesGUI : WindowGUI
    {
        private ButtonGUI<bool> AddNewNameData;
        private ButtonGUI<NameData> AddNewState;
        private ToggleGUI ShowDelete;
        private ScrollGUI Scroll;
        private CharaEvent CharaEvent;
        private IntTextFieldGUI IntTextField;
        private LabelGUI WindowName;
        public StatesGUI()
        {
            WindowID = 2;
            Init();
        }

        public override void Init()
        {
            CharaEvent = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            ShowDelete = new ToggleGUI("Show Delete", ExpandWidth(false));
            IntTextField = new IntTextFieldGUI("0");
            Text = "NameData";
            WindowName = new LabelGUI() { Text = this.Text };
            AddNewNameData = new ButtonGUI<bool>("New Group")
            {
                action = (_) =>
                {
                    var names = CharaEvent.Names;
                    var max = CharaEvent.Names.Max(x => x.Binding) + 1;
                    names.Add(new NameData() { Binding = max });
                }
            };

            Scroll = new ScrollGUI()
            {
                action = (_) =>
                {
                    foreach (var item in CharaEvent.Names)
                    {
                        BeginVertical();
                        {
                            BeginHorizontal();
                            {

                            }
                            EndHorizontal();
                        }
                        EndVertical();
                    }
                }
            };

            AddNewState = new ButtonGUI<NameData>("Add custom State")
            {
                action = (nameData) =>
                {
                    var stateValue = IntTextField.GetValue();
                    if (nameData.StateNames.ContainsKey(stateValue)) return;
                    nameData.StateNames[stateValue] = "State " + stateValue;
                }
            };
        }

        public override void WindowDraw(int id)
        {
            BeginVertical();
            {
                WindowName.Draw();
            }
            EndVertical();
        }
    }
}
