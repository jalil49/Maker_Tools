using System;
using Extensions.GUI_Classes;
using static Extensions.OnGUIExtensions;
using GL = UnityEngine.GUILayout;

namespace Accessory_States.OnGUI
{
    public class StateInfoControl
    {
        public readonly BindingData bData;
        private readonly CharaEventControl CharaEventControl;
        private readonly string nameAppend;
        private readonly IntTextFieldGUI PriorityField;
        private readonly int SelectedSlot;
        public readonly StateInfo StateInfo;

        public StateInfoControl(BindingData bindingData, StateInfo name, int selectedSlot,
                                CharaEventControl charaEventControl)
        {
            bData = bindingData;
            StateInfo = name;
            SelectedSlot = selectedSlot;
            PriorityField = new IntTextFieldGUI(StateInfo.Priority.ToString(), GL.ExpandWidth(false), GL.MinWidth(10))
            {
                action = val =>
                {
                    if (val != StateInfo.Priority)
                    {
                        StateInfo.Priority = Math.Max(0, val);
                        CharaEvent.SaveSlotData(selectedSlot);
                        CharaEvent.RefreshSlots();
                    }
                }
            };
            nameAppend = StateInfo.ShoeType == 2 ? string.Empty : StateInfo.ShoeType == 0 ? " (Indoor)" : " (Outdoor)";
            CharaEventControl = charaEventControl;
        }

        public CharaEvent CharaEvent => CharaEventControl.CharaEvent;

        public void Draw(int ShoeType)
        {
            GL.BeginHorizontal();
            {
                GL.Space(20);
                Label(StateInfo.State + ": " + bData.NameData.GetStateName(StateInfo.State) + nameAppend);
                if (ShoeType != 2)
                {
                    if (StateInfo.ShoeType == 2 && Button("Shoe Split",
                        "Make this slot distinguish between being indoors and outdoors", false))
                    {
                        bData.States.Remove(StateInfo);
                        bData.States.Add(new StateInfo
                        {
                            Binding = bData.NameData.Binding, Slot = SelectedSlot, Priority = StateInfo.Priority,
                            Show = StateInfo.Show, State = StateInfo.State, ShoeType = 0
                        });
                        bData.States.Add(new StateInfo
                        {
                            Binding = bData.NameData.Binding, Slot = SelectedSlot, Priority = StateInfo.Priority,
                            Show = StateInfo.Show, State = StateInfo.State, ShoeType = 1
                        });
                        bData.Sort();
                        CharaEvent.SaveSlotData(SelectedSlot);
                        CharaEvent.RefreshSlots(bData.NameData.AssociatedSlots);
                    }

                    if (StateInfo.ShoeType != 2 &&
                        Button("Shoe Merge", "Remove association between indoor and outdoors", false))
                    {
                        bData.States.RemoveAll(x => x.State == StateInfo.State);
                        bData.States.Add(new StateInfo
                        {
                            Binding = bData.NameData.Binding, Slot = SelectedSlot, Priority = StateInfo.Priority,
                            Show = StateInfo.Show, State = StateInfo.State, ShoeType = 2
                        });
                        bData.Sort();
                        CharaEvent.SaveSlotData(SelectedSlot);
                        CharaEvent.RefreshSlots(bData.NameData.AssociatedSlots);
                    }
                }

                if (Button(StateInfo.Show ? "Show" : "Hide", expandwidth: false))
                {
                    StateInfo.Show = !StateInfo.Show;
                    CharaEvent.SaveSlotData(SelectedSlot);
                    CharaEvent.RefreshSlots(bData.NameData.AssociatedSlots);
                }

                if (Button("↑", "Increase the priority of this state when comparing", false))
                {
                    StateInfo.Priority++;
                    CharaEvent.SaveSlotData(SelectedSlot);
                    CharaEvent.RefreshSlots();
                }

                PriorityField.Draw(StateInfo.Priority);

                if (Button("↓", "Decrease the priority of this state when comparing", false) && StateInfo.Priority != 0)
                {
                    StateInfo.Priority = Math.Max(0, StateInfo.Priority - 1);
                    CharaEvent.SaveSlotData(SelectedSlot);
                    CharaEvent.RefreshSlots(bData.NameData.AssociatedSlots);
                }
            }

            GL.EndHorizontal();
        }
    }
}