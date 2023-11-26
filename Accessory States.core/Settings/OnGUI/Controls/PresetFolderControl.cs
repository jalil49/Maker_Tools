using System.Collections.Generic;
using System.IO;
using Accessory_States.Classes.PresetStorage;
using Extensions.GUI_Classes;
using UnityEngine;
using static Extensions.OnGUIExtensions;
using GL = UnityEngine.GUILayout;

namespace Accessory_States.OnGUI
{
    public class PresetFolderControl
    {
        private readonly List<PresetFolder> _container;
        private readonly TextAreaGUI _description;
        private readonly TextFieldGUI _fileName;
        private readonly TextFieldGUI _name;
        private readonly PresetFolder _presetFolder;
        public bool ShowContents;

        public PresetFolderControl(PresetFolder presetFolder, List<PresetFolder> container)
        {
            _presetFolder = presetFolder;
            _container = container;

            _name = new TextFieldGUI(new GUIContent(presetFolder.Name),
                (oldValue, newValue) => { _presetFolder.Name = newValue; }, GL.ExpandWidth(true));

            _fileName = new TextFieldGUI(new GUIContent(presetFolder.FileName), (oldVal, newVal) =>
                {
                    if (newVal.IsNullOrWhiteSpace())
                    {
                        newVal = _presetFolder.Name;
                    }

                    if (newVal.Length == 0)
                    {
                        newVal = _presetFolder.GetHashCode().ToString();
                    }

                    newVal = string.Concat(newVal.Split(Path.GetInvalidFileNameChars())).Trim();

                    if (_presetFolder.SavedOnDisk)
                    {
                        Presets.Rename(oldVal, newVal);
                    }

                    _presetFolder.FileName = newVal;
                    _fileName.ManuallySetNewText(_presetFolder.FileName);
                })
                ;

            _description = new TextAreaGUI(presetFolder.Description)
            {
                action = val => { _presetFolder.Description = val; }
            };
        }

        public bool Filter(string filter) => _presetFolder.Filter(filter);

        public void Draw(SlotData slotData, int slot)
        {
            GL.BeginHorizontal();
            {
                if (Button(ShowContents ? "-" : "+", "Show or Hide Folder Contents", false))
                {
                    ShowContents = !ShowContents;
                }

                _name.ActiveDraw();

                var index = _container.IndexOf(_presetFolder);

                if (Button("↑", $"Move Up: Index {index}, Hold Shift to move to top", false) && index > 0)
                {
                    _container.RemoveAt(index);
                    if (Event.current.shift)
                    {
                        _container.Insert(0, _presetFolder);
                    }
                    else
                    {
                        _container.Insert(index - 1, _presetFolder);
                    }
                }

                if (Button("↓", $"Move Down: Index {index}, Hold Shift to move to bottom", false) &&
                    index < _container.Count - 1)
                {
                    _container.RemoveAt(index);
                    if (Event.current.shift)
                    {
                        _container.Add(_presetFolder);
                    }
                    else
                    {
                        _container.Insert(index + 1, _presetFolder);
                    }
                }
            }

            GL.EndHorizontal();
            GL.Space(10);
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();

                if (Button("Add Preset", string.Empty, false))
                {
                    _presetFolder.PresetDatas.Add(PresetData.ConvertSlotData(slotData, slot));
                }

                if (_fileName.GUIContent.text.Length > 0 && Button("Save", string.Empty, false))
                {
                    _presetFolder.SaveFile();
                }

                if (Button("Remove", "Unload from Memory", false))
                {
                    _container.Remove(_presetFolder);
                }

                GL.Space(10);
                if (_presetFolder.SavedOnDisk && Button("Delete", "Delete From disk", false))
                {
                    _container.Remove(_presetFolder);
                    _presetFolder.Delete();
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
    }
}