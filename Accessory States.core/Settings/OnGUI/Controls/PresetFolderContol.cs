using Accessory_States.Classes.PresetStorage;
using Extensions.GUI_Classes;
using System.Collections.Generic;
using UnityEngine;
using static Extensions.OnGUIExtensions;
using GL = UnityEngine.GUILayout;

namespace Accessory_States.OnGUI
{
    public class PresetFolderContol
    {
        public readonly PresetFolder PresetFolder;
        private readonly TextFieldGUI Name;
        private readonly TextFieldGUI FileName;
        private readonly TextAreaGUI Description;
        private readonly List<PresetFolder> Container;
        public bool ShowContents;

        public PresetFolderContol(PresetFolder presetFolder, List<PresetFolder> container)
        {
            PresetFolder = presetFolder;
            Container = container;

            Name = new TextFieldGUI(new GUIContent(presetFolder.Name), GL.ExpandWidth(true))
            {
                OnValueChange = (oldValue, newValue) => { PresetFolder.Name = newValue; }
            };

            FileName = new TextFieldGUI(new GUIContent(presetFolder.FileName))
            {
                OnValueChange = (oldVal, newVal) =>
                {
                    if(newVal.IsNullOrWhiteSpace())
                        newVal = PresetFolder.Name;
                    if(newVal.Length == 0)
                        newVal = PresetFolder.GetHashCode().ToString();

                    newVal = string.Concat(newVal.Split(System.IO.Path.GetInvalidFileNameChars())).Trim();

                    if(PresetFolder.SavedOnDisk)
                        Presets.Rename(oldVal, newVal);
                    PresetFolder.FileName = newVal;
                    FileName.ManuallySetNewText(PresetFolder.FileName);
                }
            };

            Description = new TextAreaGUI(presetFolder.Description)
            {
                action = (val) => { PresetFolder.Description = val; }
            };
        }

        public bool Filter(string filter)
        {
            return PresetFolder.Filter(filter);
        }

        public void Draw(SlotData slotData, int slot)
        {
            GL.BeginHorizontal();
            {
                if(Button(ShowContents ? "-" : "+", "Show or Hide Folder Contents", false))
                {
                    ShowContents = !ShowContents;
                }

                Name.ActiveDraw();

                int index = Container.IndexOf(PresetFolder);

                if(Button("↑", $"Move Up: Index {index}, Hold Shift to move to top", false) && index > 0)
                {
                    Container.RemoveAt(index);
                    if(Event.current.shift)
                        Container.Insert(0, PresetFolder);
                    else
                        Container.Insert(index - 1, PresetFolder);
                }

                if(Button("↓", $"Move Down: Index {index}, Hold Shift to move to bottom", false) && index < Container.Count - 1)
                {
                    Container.RemoveAt(index);
                    if(Event.current.shift)
                        Container.Add(PresetFolder);
                    else
                        Container.Insert(index + 1, PresetFolder);
                }
            }

            GL.EndHorizontal();
            GL.Space(10);
            GL.BeginHorizontal();
            {
                GL.FlexibleSpace();

                if(Button("Add Preset", "", false))
                {
                    PresetFolder.PresetDatas.Add(PresetData.ConvertSlotData(slotData, slot));
                }

                if(FileName.GUIContent.text.Length > 0 && Button("Save", "", false))
                {
                    PresetFolder.SaveFile();
                }

                if(Button("Remove", "Unload from Memory", false))
                {
                    Container.Remove(PresetFolder);
                }

                GL.Space(10);
                if(PresetFolder.SavedOnDisk && Button("Delete", "Delete From disk", false))
                {
                    Container.Remove(PresetFolder);
                    PresetFolder.Delete();
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
    }
}
