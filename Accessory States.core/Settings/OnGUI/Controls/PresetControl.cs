using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Accessory_States.Classes.PresetStorage;
using Extensions.GUI_Classes;
using UnityEngine;
using static Extensions.OnGUIExtensions;
using GL = UnityEngine.GUILayout;

namespace Accessory_States.OnGUI
{
    public class PresetContol
    {
        private readonly List<PresetData> _container;
        private readonly TextAreaGUI _description;
        private readonly TextFieldGUI _fileName;
        private readonly TextFieldGUI _name;
        public readonly PresetData PresetData;

        public PresetContol(PresetData presetData, List<PresetData> container)
        {
            PresetData = presetData;
            _container = container;

            _name = new TextFieldGUI(new GUIContent(presetData.Name),
                (oldValue, newValue) => { presetData.Name = newValue; }, GL.ExpandWidth(true));

            _fileName = new TextFieldGUI(new GUIContent(presetData.FileName), (oldVal, newVal) =>
            {
                if (newVal.IsNullOrWhiteSpace()) newVal = PresetData.Name;

                if (newVal.Length == 0) newVal = PresetData.GetHashCode().ToString();

                newVal = string.Concat(newVal.Split(Path.GetInvalidFileNameChars())).Trim();

                if (PresetData.SavedOnDisk) Presets.Rename(oldVal, newVal);

                PresetData.FileName = newVal;
                _fileName.ManuallySetNewText(PresetData.FileName);
            });

            _description = new TextAreaGUI(presetData.Description)
            {
                Action = val => { presetData.Description = val; }
            };
        }

        public bool Filter(string filter)
        {
            return PresetData.Filter(filter);
        }

        public void Draw(CharaEvent chara, int selectedSlot)
        {
            GL.BeginHorizontal();
            {
                _name.ActiveDraw();

                var index = _container.IndexOf(PresetData);

                if (Button("Apply", "Apply this preset's data to slot, No collision check", false))
                {
                    if (chara.SlotBindingData.TryGetValue(selectedSlot, out var undoReference))
                        foreach (var item in undoReference.bindingDatas)
                            item.NameData.AssociatedSlots.Remove(selectedSlot);

                    var slotData = chara.SlotBindingData[selectedSlot] = PresetData.Data.DeepClone();
                    var names = chara.NameDataList;
                    foreach (var item in slotData.bindingDatas)
                    {
                        var reference = names.FirstOrDefault(x => item.NameData.Equals(x, false));
                        if (reference != null)
                        {
                            item.NameData = reference;
                            item.NameData.collisionString =
                                Guid.NewGuid().ToString("D")
                                    .ToUpper(); //replace Collision string from preset to prevent collisions from shared presets
                            item.NameData.AssociatedSlots.Add(selectedSlot);
                            item.SetSlot(selectedSlot);
                            continue;
                        }

                        item.NameData.binding = names.Max(x => x.binding) + 1;
                        item.SetBinding();
                        names.Add(item.NameData);
                        item.NameData.AssociatedSlots.Add(selectedSlot);
                    }

                    Save(chara, slotData.bindingDatas, selectedSlot);
                    chara.RefreshSlots();
                }

                if (Button("Override", "Apply this slots data to preset", false))
                    PresetData.Data = chara.SlotBindingData[selectedSlot].DeepClone();

                if (Button("↑", $"Move Up: Index {index}, Hold Shift to move to top", false) && index > 0)
                {
                    _container.RemoveAt(index);
                    if (Event.current.shift)
                        _container.Insert(0, PresetData);
                    else
                        _container.Insert(index - 1, PresetData);
                }

                if (Button("↓", $"Move Down: Index {index}, Hold Shift to move to bottom", false) &&
                    index < _container.Count - 1)
                {
                    _container.RemoveAt(index);
                    if (Event.current.shift)
                        _container.Add(PresetData);
                    else
                        _container.Insert(index + 1, PresetData);
                }
            }

            GL.EndHorizontal();
            GL.Space(10);
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();

                if (_fileName.GUIContent.text.Length > 0 && Button("Save", "Save Preset to disk", false))
                    PresetData.SaveFile();

                if (Button("Remove", "Unload Reference", false)) _container.Remove(PresetData);

                GL.Space(10);

                if (PresetData.SavedOnDisk && Button("Delete", "Delete from disk", false))
                {
                    _container.Remove(PresetData);
                    PresetData.Delete();
                }
            }

            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                GL.Space(10);
                Label("File Name:", string.Empty, false);
                _fileName.ConfirmDraw();
            }

            GL.EndHorizontal();

            GL.BeginHorizontal();
            {
                GL.Space(10);
                Label("Description:", string.Empty, false);
                _description.ActiveDraw();
            }

            GL.EndHorizontal();
        }

        private void Save(CharaEvent chara, List<BindingData> bindingDatas, int selectedSlot)
        {
            foreach (var item in chara.SlotBindingData)
            {
                var save = false;
                foreach (var item2 in item.Value.bindingDatas)
                {
                    if (!bindingDatas.Any(x => x == item2)) continue;

                    var nameData = item2.NameData;
                    var states = item2.States;
                    var sort = false;

                    for (var i = 0; i < nameData.StateLength; i++)
                    {
                        if (states.Any(x => x.State == i)) continue;

                        states.Add(new StateInfo { State = i, Binding = nameData.binding, Slot = selectedSlot });
                        sort = true;
                        save = true;
                    }

                    states.RemoveAll(x => x.State >= nameData.StateLength);

                    if (sort) item2.Sort();
                }

                if (save) chara.SaveSlotData(item.Key);
            }
        }
    }
}