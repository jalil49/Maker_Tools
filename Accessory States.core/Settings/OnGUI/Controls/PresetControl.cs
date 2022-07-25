using Accessory_States.Classes.PresetStorage;
using Extensions.GUI_Classes;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using static Extensions.OnGUIExtensions;
using GL = UnityEngine.GUILayout;

namespace Accessory_States.OnGUI
{
    public class PresetContols
    {
        public readonly PresetData PresetData;
        private readonly TextFieldGUI Name;
        private readonly TextFieldGUI FileName;
        private readonly TextAreaGUI Description;
        private readonly List<PresetData> Container;

        public PresetContols(PresetData presetData, List<PresetData> container)
        {
            PresetData = presetData;
            Container = container;

            Name = new TextFieldGUI(new GUIContent(presetData.Name), GL.ExpandWidth(true))
            {
                OnValueChange = (oldValue, newValue) => { presetData.Name = newValue; }
            };

            FileName = new TextFieldGUI(new GUIContent(presetData.FileName))
            {
                OnValueChange = (oldVal, newVal) =>
                {
                    if(newVal.IsNullOrWhiteSpace())
                        newVal = PresetData.Name;
                    if(newVal.Length == 0)
                        newVal = PresetData.GetHashCode().ToString();

                    newVal = string.Concat(newVal.Split(System.IO.Path.GetInvalidFileNameChars())).Trim();

                    if(PresetData.SavedOnDisk)
                        Presets.Rename(oldVal, newVal);
                    PresetData.FileName = newVal;
                    FileName.ManuallySetNewText(PresetData.FileName);
                }
            };

            Description = new TextAreaGUI(presetData.Description)
            {
                action = (val) => { presetData.Description = val; }
            };
        }

        public bool Filter(string filter)
        {
            return PresetData.Filter(filter);
        }

        public void Draw(CharaEvent chara, int SelectedSlot)
        {
            GL.BeginHorizontal();
            {
                Name.ActiveDraw();

                int index = Container.IndexOf(PresetData);

                if(Button("Apply", "Apply this preset's data to slot", false))
                {
                    if(chara.SlotBindingData.TryGetValue(SelectedSlot, out SlotData undoReference))
                    {
                        foreach(BindingData item in undoReference.bindingDatas)
                        {
                            item.NameData.AssociatedSlots.Remove(SelectedSlot);
                        }
                    }

                    SlotData slotData = chara.SlotBindingData[SelectedSlot] = PresetData.Data.DeepClone();
                    List<NameData> names = chara.NameDataList;
                    foreach(BindingData item in slotData.bindingDatas)
                    {
                        NameData reference = names.FirstOrDefault(x => item.NameData.Equals(x));
                        if(reference != null)
                        {
                            item.NameData = reference;
                            item.NameData.AssociatedSlots.Add(SelectedSlot);
                            item.SetSlot(SelectedSlot);
                            continue;
                        }

                        item.NameData.Binding = names.Max(x => x.Binding) + 1;
                        item.SetBinding();
                        names.Add(item.NameData);
                        item.NameData.AssociatedSlots.Add(SelectedSlot);
                    }

                    Save(chara, slotData.bindingDatas, SelectedSlot);
                    chara.RefreshSlots();
                }

                if(Button("Override", "Apply this slots data to preset", false))
                {
                    PresetData.Data = chara.SlotBindingData[SelectedSlot].DeepClone();
                }

                if(Button("↑", $"Move Up: Index {index}, Hold Shift to move to top", false) && index > 0)
                {
                    Container.RemoveAt(index);
                    if(Event.current.shift)
                        Container.Insert(0, PresetData);
                    else
                        Container.Insert(index - 1, PresetData);
                }

                if(Button("↓", $"Move Down: Index {index}, Hold Shift to move to bottom", false) && index < Container.Count - 1)
                {
                    Container.RemoveAt(index);
                    if(Event.current.shift)
                        Container.Add(PresetData);
                    else
                        Container.Insert(index + 1, PresetData);
                }
            }

            GL.EndHorizontal();
            GL.Space(10);
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();

                if(FileName.GUIContent.text.Length > 0 && Button("Save", "Save Preset to disk", false))
                {
                    PresetData.SaveFile();
                }

                if(Button("Remove", "Unload Reference", false))
                {
                    Container.Remove(PresetData);
                }

                GL.Space(10);

                if(PresetData.SavedOnDisk && Button("Delete", "Delete from disk", false))
                {
                    Container.Remove(PresetData);
                    PresetData.Delete();
                }
            }

            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                GL.Space(10);
                Label("File Name:", "", false);
                FileName.ConfirmDraw();
            }

            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                GL.Space(10);
                Label("Description:", "", false);
                Description.ActiveDraw();
            }

            GL.EndHorizontal();
        }

        private void Save(CharaEvent chara, List<BindingData> bindingDatas, int SelectedSlot)
        {
            foreach(KeyValuePair<int, SlotData> item in chara.SlotBindingData)
            {
                bool save = false;
                foreach(BindingData item2 in item.Value.bindingDatas)
                {
                    if(!bindingDatas.Any(x => x == item2))
                        continue;
                    NameData NameData = item2.NameData;
                    List<StateInfo> states = item2.States;
                    bool sort = false;

                    for(int i = 0; i < NameData.StateLength; i++)
                    {
                        if(states.Any(x => x.State == i))
                            continue;
                        states.Add(new StateInfo() { State = i, Binding = NameData.Binding, Slot = SelectedSlot });
                        sort = true;
                        save = true;
                    }

                    states.RemoveAll(x => x.State >= NameData.StateLength);

                    if(sort)
                        item2.Sort();
                }

                if(save)
                    chara.SaveSlotData(item.Key);
            }
        }
    }
}
