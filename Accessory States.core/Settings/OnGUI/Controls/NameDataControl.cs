﻿using System.Collections.Generic;
using System.Linq;
using Extensions.GUI_Classes;
using UnityEngine;
using static Extensions.OnGUIExtensions;
using GL = UnityEngine.GUILayout;

namespace Accessory_States.OnGUI
{
    public class NameDataControl
    {
        private readonly IntTextFieldGUI _currentState;
        private readonly IntTextFieldGUI _defaultState;
        private readonly CharaEventControl _charaEventControl;
        private readonly Dictionary<int, TextFieldGUI> _statesRename = new Dictionary<int, TextFieldGUI>();
        public NameData NameData;
        private string _newName;

        public NameDataControl(NameData name, CharaEventControl charaEventControl)
        {
            NameData = name;
            _newName = name.Name;
            _currentState = new IntTextFieldGUI(NameData.currentState.ToString(), GL.ExpandWidth(false))
            {
                Action = val => { NameData.currentState = val; }
            };
            _defaultState = new IntTextFieldGUI(name.DefaultState.ToString(), GL.ExpandWidth(false))
            {
                Action = val =>
                {
                    if (val < 0) val = 0;

                    if (val >= NameData.StateLength) val = NameData.StateLength - 1;

                    NameData.DefaultState = val;
                }
            };
            _charaEventControl = charaEventControl;
        }

        private CharaEvent CharaEvent => _charaEventControl.CharaEvent;

        public void Save(int selectedSlot)
        {
            foreach (var item in CharaEvent.SlotBindingData)
            {
                if (!item.Value.TryGetBinding(NameData, out var binding)) continue;

                var states = binding.States;
                var sort = false;

                for (var i = 0; i < NameData.StateLength; i++)
                {
                    if (states.Any(x => x.State == i)) continue;

                    states.Add(new StateInfo { State = i, Binding = NameData.binding, Slot = selectedSlot });
                    sort = true;
                }

                states.RemoveAll(x => x.State >= NameData.StateLength);

                if (sort) binding.Sort();

                CharaEvent.SaveSlotData(item.Key);
            }
        }

        public void Delete()
        {
            foreach (var item in CharaEvent.SlotBindingData)
            {
                var result = item.Value.bindingDatas.RemoveAll(x => x.NameData == NameData);
                if (result > 0) CharaEvent.SaveSlotData(item.Key);
            }
        }

        public void DrawGroupRename(int selectedSlot)
        {
            _newName = TextField(_newName);
            if (!_newName.Equals(NameData.Name) && Button("Update Name",
                    "Updates Name for all accessories as well as saving current data", false))
            {
                NameData.Name = _newName;
                Save(selectedSlot);
            }

            if (Button("Add State", "Adds a new state to this group for related accessories", false))
            {
                for (int j = 0, n = NameData.StateLength + 1; j < n; j++)
                {
                    if (NameData.StateNames.ContainsKey(j)) continue;

                    NameData.StateNames[j] = "State " + j;
                }

                Save(selectedSlot);
            }

            ;

            if (Button("Delete", "Delete all data related to this group", false))
            {
                Delete();
                CharaEvent.NameDataList.Remove(NameData);
            }
        }

        public void DrawGroupSetting()
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                if (Button("Set Main", "Change all accessory parts in group to Main Hide Category", false))
                    CharaEvent.ChangeBindingSub(0, NameData);

                if (Button("Set Sub", "Change all accessory parts in group to Sub Hide Category", false))
                    CharaEvent.ChangeBindingSub(1, NameData);

                GL.Space(10);
                NameData.stopCollision = Toggle(NameData.stopCollision, "Unique Group",
                    "None-Unique Groups will be merged when possible by Name");
            }

            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                Label($"Default {NameData.DefaultState}: {NameData.GetStateName(NameData.DefaultState)}",
                    "State to be used on load", false);
                if (Button("<", "Decrease Default State", false))
                {
                    NameData.DefaultState--;
                    if (NameData.DefaultState < 0) NameData.DefaultState = NameData.StateLength - 1;
                }

                _defaultState.Draw(NameData.DefaultState);
                if (Button(">", "Increase Default State", false))
                {
                    NameData.DefaultState++;
                    if (NameData.DefaultState >= NameData.StateLength) NameData.DefaultState = 0;
                }
            }

            GL.EndHorizontal();
        }

        public void DrawStateRename(int selectedSlot)
        {
            for (var j = 0; j < NameData.StateLength; j++)
            {
                var newStateName = TryGetTextField(j, selectedSlot);
                GL.BeginHorizontal();
                {
                    Label(j + ": ", string.Empty, false);
                    newStateName.ConfirmDraw();

                    if (j == NameData.StateLength - 1 && Button("Remove", expandwidth: false))
                    {
                        NameData.StateNames.Remove(j);
                        _statesRename.Remove(j);
                        Save(selectedSlot);
                    }
                }
                GL.EndHorizontal();
            }
        }

        public void DrawStatePreview()
        {
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();
                Label(NameData.Name + ": " + NameData.GetStateName(NameData.currentState), string.Empty, false);
                if (Button("<", "Previous State", false))
                {
                    NameData.DecrementCurrentState();
                    if (NameData.binding < Constants.ClothingLength)
                    {
                        CharaEvent.ChaControl.SetClothesStatePrev(NameData.binding);
                        NameData.currentState = CharaEvent.ChaControl.fileStatus.clothesState[NameData.binding];
                    }
                }

                _currentState.Draw(NameData.currentState);
                if (Button(">", "Next State", false))
                {
                    NameData.IncrementCurrentState();
                    if (NameData.binding < Constants.ClothingLength)
                    {
                        CharaEvent.ChaControl.SetClothesStateNext(NameData.binding);
                        NameData.currentState = CharaEvent.ChaControl.fileStatus.clothesState[NameData.binding];
                    }
                }
            }

            GL.EndHorizontal();
        }

        private TextFieldGUI TryGetTextField(int state, int selectedSlot)
        {
            if (!_statesRename.TryGetValue(state, out var newStateName))
                _statesRename[state] = newStateName = new TextFieldGUI(
                    new GUIContent(NameData.GetStateName(state), string.Empty), null, GL.ExpandWidth(true),
                    GL.MinWidth(30));

            return newStateName;
        }
    }
}