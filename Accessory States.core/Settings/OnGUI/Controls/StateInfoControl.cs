using System;
using Extensions.GUI_Classes;
using static Extensions.OnGUIExtensions;
using GL = UnityEngine.GUILayout;

namespace Accessory_States.OnGUI
{
    public class StateInfoControl
    {
        public readonly BindingData BData;
        private readonly CharaEventControl _charaEventControl;
        private readonly string _nameAppend;
        private readonly IntTextFieldGUI _priorityField;
        private readonly int _selectedSlot;
        public readonly StateInfo StateInfo;

        public StateInfoControl(BindingData bindingData, StateInfo name, int selectedSlot,
            CharaEventControl charaEventControl)
        {
            BData = bindingData;
            StateInfo = name;
            _selectedSlot = selectedSlot;
            _priorityField = new IntTextFieldGUI(StateInfo.Priority.ToString(), GL.ExpandWidth(false), GL.MinWidth(10))
            {
                Action = val =>
                {
                    if (val != StateInfo.Priority)
                    {
                        StateInfo.Priority = Math.Max(0, val);
                        CharaEvent.SaveSlotData(selectedSlot);
                        CharaEvent.RefreshSlots();
                    }
                }
            };
            _nameAppend = StateInfo.ShoeType == 2 ? string.Empty : StateInfo.ShoeType == 0 ? " (Indoor)" : " (Outdoor)";
            _charaEventControl = charaEventControl;
        }

        public CharaEvent CharaEvent => _charaEventControl.CharaEvent;

        public void Draw(int shoeType)
        {
            GL.BeginHorizontal();
            {
                GL.Space(20);
                Label(StateInfo.State + ": " + BData.NameData.GetStateName(StateInfo.State) + _nameAppend);
                if (shoeType != 2)
                {
                    if (StateInfo.ShoeType == 2 && Button("Shoe Split",
                            "Make this slot distinguish between being indoors and outdoors", false))
                    {
                        BData.States.Remove(StateInfo);
                        BData.States.Add(new StateInfo
                        {
                            Binding = BData.NameData.binding, Slot = _selectedSlot, Priority = StateInfo.Priority,
                            Show = StateInfo.Show, State = StateInfo.State, ShoeType = 0
                        });
                        BData.States.Add(new StateInfo
                        {
                            Binding = BData.NameData.binding, Slot = _selectedSlot, Priority = StateInfo.Priority,
                            Show = StateInfo.Show, State = StateInfo.State, ShoeType = 1
                        });
                        BData.Sort();
                        CharaEvent.SaveSlotData(_selectedSlot);
                        CharaEvent.RefreshSlots(BData.NameData.AssociatedSlots);
                    }

                    if (StateInfo.ShoeType != 2 &&
                        Button("Shoe Merge", "Remove association between indoor and outdoors", false))
                    {
                        BData.States.RemoveAll(x => x.State == StateInfo.State);
                        BData.States.Add(new StateInfo
                        {
                            Binding = BData.NameData.binding, Slot = _selectedSlot, Priority = StateInfo.Priority,
                            Show = StateInfo.Show, State = StateInfo.State, ShoeType = 2
                        });
                        BData.Sort();
                        CharaEvent.SaveSlotData(_selectedSlot);
                        CharaEvent.RefreshSlots(BData.NameData.AssociatedSlots);
                    }
                }

                if (Button(StateInfo.Show ? "Show" : "Hide", expandwidth: false))
                {
                    StateInfo.Show = !StateInfo.Show;
                    CharaEvent.SaveSlotData(_selectedSlot);
                    CharaEvent.RefreshSlots(BData.NameData.AssociatedSlots);
                }

                if (Button("↑", "Increase the priority of this state when comparing", false))
                {
                    StateInfo.Priority++;
                    CharaEvent.SaveSlotData(_selectedSlot);
                    CharaEvent.RefreshSlots();
                }

                _priorityField.Draw(StateInfo.Priority);

                if (Button("↓", "Decrease the priority of this state when comparing", false) && StateInfo.Priority != 0)
                {
                    StateInfo.Priority = Math.Max(0, StateInfo.Priority - 1);
                    CharaEvent.SaveSlotData(_selectedSlot);
                    CharaEvent.RefreshSlots(BData.NameData.AssociatedSlots);
                }
            }

            GL.EndHorizontal();
        }
    }
}