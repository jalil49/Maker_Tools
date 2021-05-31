using HarmonyLib;
using Hook_Space;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using MoreAccessoriesKOI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToolBox;
using UniRx;

namespace Additional_Card_Info
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        static readonly MakerToggle[] CoordinateKeepToggles = new MakerToggle[Constants.ClothingTypesLength];
        static readonly MakerToggle[] PersonalKeepToggles = new MakerToggle[Constants.ClothingTypesLength];
        static readonly MakerRadioButtons[] PersonalityToggles = new MakerRadioButtons[Constants.PersonalityLength];
        static readonly MakerRadioButtons[] TraitToggles = new MakerRadioButtons[Constants.TraitsLength];
        static readonly MakerRadioButtons[] InterestToggles = new MakerRadioButtons[Constants.InterestLength];
        static readonly MakerToggle[] HeightToggles = new MakerToggle[Constants.HeightLength];
        static readonly MakerToggle[] BreastsizeToggles = new MakerToggle[Constants.BreastsizeLength];
        static readonly MakerToggle[] ClothNotToggles = new MakerToggle[3];

        static MakerToggle Character_Cosplay_Ready;
        static MakerToggle MakeUpToggle;

        static AccessoryControlWrapper<MakerToggle, bool> AccKeepToggles;
        static AccessoryControlWrapper<MakerToggle, bool> HairKeepToggles;

        static MakerRadioButtons CoordinateTypeRadio;
        static MakerRadioButtons CoordinateSubTypeRadio;
        static MakerRadioButtons ClubTypeRadio;
        static MakerRadioButtons HStateTypeRadio;
        static MakerRadioButtons GenderRadio;
        static MakerTextbox Creator;
        static MakerTextbox Set_Name;
        static MakerTextbox Sub_Set_Name;

        public static void MakerAPI_MakerStartedLoading(object sender, RegisterCustomControlsEvent e)
        {
            AccessoriesApi.AccessoriesCopied += AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred += AccessoriesApi_AccessoryTransferred;

            MakerAPI.MakerExiting += MakerAPI_MakerExiting;

            MakerAPI.ReloadCustomInterface += (s, e2) =>
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

                Controller.StartCoroutine(Controller.UpdateSlots());
            };
            AccessoriesApi.SelectedMakerAccSlotChanged += (s, e2) => VisibiltyToggle();
            MakerAPI.MakerFinishedLoading += (s, e2) => VisibiltyToggle();
        }

        private static void MakerAPI_MakerExiting(object sender, EventArgs e)
        {
            AccessoriesApi.AccessoriesCopied -= AccessoriesApi_AccessoriesCopied;
            AccessoriesApi.AccessoryTransferred -= AccessoriesApi_AccessoryTransferred;


            MakerAPI.MakerExiting -= MakerAPI_MakerExiting;

            AccessoriesApi.SelectedMakerAccSlotChanged -= (s, e2) => VisibiltyToggle();
            MakerAPI.MakerFinishedLoading -= (s, e2) => VisibiltyToggle();
            MakerAPI.ReloadCustomInterface -= (s, e2) => { var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>(); Controller.StartCoroutine(Controller.UpdateSlots()); };
        }

        public static void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;

            #region Personal Settings
            MakerCategory category = new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings");
            e.AddSubCategory(category);

            //e.AddControl(new MakerText(null, category, owner));
            e.AddControl(new MakerText("Toggle when all Hair accessories and accessories you want to keep on this character are ready.", category, owner));
            e.AddControl(new MakerText("Example Mecha Chika who requires her arm and legs accessories", category, owner));

            e.AddControl(new MakerButton("Keep All Accessories", category, owner)).OnClick.AddListener(delegate ()
            {
                var ChaControl = MakerAPI.GetCharacterControl();
                var Controller = ChaControl.GetComponent<CharaEvent>();
                var ACCKeep = Controller.AccKeep[Controller.CoordinateNum];
                for (int i = 0, n = AccKeepToggles.Control.ControlObjects.Count(); i < n; i++)
                {
                    if (ChaControl.GetAccessoryObject(i) != null)
                    {
                        AccKeepToggles.SetValue(i, true);
                        if (!ACCKeep.Contains(i))
                        {
                            ACCKeep.Add(i);
                        }
                    }
                }
            });

            Character_Cosplay_Ready = e.AddControl(new MakerToggle(category, "Cosplay Academy Ready", false, owner));

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Select data that shouldn't be overwritten by other mods.", category, owner));
            e.AddControl(new MakerText("Example Mecha Chika who doesn't work with socks/pantyhose and requires her shoes/glove", category, owner));
            #region Keep Toggles

            for (int i = 0; i < Constants.ClothingTypesLength; i++)
            {
                PersonalClothingKeepControls(i, e);
            }

            MakeUpToggle = e.AddControl(new MakerToggle(category, "Keep Makeup", owner));

            MakeUpToggle.ValueChanged.Subscribe(x =>
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
                Controller.MakeUpKeep[Controller.CoordinateNum] = x;
            });
            #endregion


            #endregion

            #region Accessory Window Settings
            category = new MakerCategory("03_ClothesTop", "tglACCSettings", MakerConstants.Clothes.Copy.Position + 2, "Accessory Settings");
            var Keep = new MakerToggle(category, "Keep this Accessory", owner);
            AccKeepToggles = MakerAPI.AddEditableAccessoryWindowControl<MakerToggle, bool>(Keep);
            AccKeepToggles.ValueChanged += AccKeep_ValueChanged;

            var HairKeep = new MakerToggle(category, "Is this a hair piece?", owner);
            HairKeepToggles = MakerAPI.AddEditableAccessoryWindowControl<MakerToggle, bool>(HairKeep);
            HairKeepToggles.ValueChanged += AccHairKeep_ValueChanged;
            #endregion

            #region Clothing Settings

            category = new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings");
            e.AddSubCategory(category);

            e.AddControl(new MakerText("Settings to be applied to coordinates", category, owner));

            e.AddControl(new MakerSeparator(category, owner));

            Creator = new MakerTextbox(category, "Author", Settings.CreatorName.Value, owner);
            Set_Name = new MakerTextbox(category, "Set Name", "", owner);
            Sub_Set_Name = new MakerTextbox(category, "Sub Set Name", "", owner);
            e.AddControl(Creator).ValueChanged.Subscribe(x => { var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>(); Controller.CreatorNames[Controller.CoordinateNum] = x; });
            e.AddControl(Set_Name).ValueChanged.Subscribe(x => { var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>(); Controller.SetNames[Controller.CoordinateNum] = x; });
            e.AddControl(Sub_Set_Name).ValueChanged.Subscribe(x => { var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>(); Controller.SubSetNames[Controller.CoordinateNum] = x; });


            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Coordinate Type information", category, owner));

            var SimpleCoord = Enum.GetNames(typeof(Constants.SimplifiedCoordinateTypes));
            for (int i = 0; i < Constants.SimplifiedCoordinateTypesLength; i++)
            {
                SimpleCoord[i] = SimpleCoord[i].Replace('_', ' ');
            }
            CoordinateTypeRadio = new MakerRadioButtons(category, owner, "Coordinate Type", 0, SimpleCoord)
            {
                ColumnCount = 2
            };
            e.AddControl(CoordinateTypeRadio).ValueChanged.Subscribe(x => { var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>(); Controller.CoordinateType[Controller.CoordinateNum] = x; });

            List<string> buttons = new List<string> { "General" };
            buttons.AddRange(Enum.GetNames(typeof(Constants.ClothingTypes)));
            buttons.AddRange(Enum.GetNames(typeof(Constants.AdditonalClothingTypes)));
            for (int i = 0, n = buttons.Count; i < n; i++)
            {
                buttons[i] = buttons[i].Replace('_', ' ');
            }
            e.AddControl(new MakerText(null, category, owner));
            CoordinateSubTypeRadio = new MakerRadioButtons(category, owner, "Sub Type", 0, buttons.ToArray())
            {
                ColumnCount = 3
            };
            e.AddControl(CoordinateSubTypeRadio).ValueChanged.Subscribe(x =>
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
                Controller.CoordinateSubType[Controller.CoordinateNum] = x;
            });

            e.AddControl(new MakerText(null, category, owner));

            GenderRadio = new MakerRadioButtons(category, owner, "Gender", new string[] { "Female", "Both", "Male" });
            e.AddControl(GenderRadio).ValueChanged.Subscribe(x => { var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>(); Controller.GenderType[Controller.CoordinateNum] = x; });


            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("H State Restrictions", category, owner));
            string[] HstatesOptions = Enum.GetNames(typeof(Constants.HStates));
            for (int i = 0; i < HstatesOptions.Length; i++)
            {
                HstatesOptions[i] = HstatesOptions[i].Replace('_', ' ');
            }
            HStateTypeRadio = new MakerRadioButtons(category, owner, "H State", 0, HstatesOptions)
            {
                ColumnCount = 3
            };
            e.AddControl(HStateTypeRadio).ValueChanged.Subscribe(x =>
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

                Controller.HstateType_Restriction[Controller.CoordinateNum] = x;
            });

            //e.AddControl(new MakerText(null, category, owner));

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Club Restrictions", category, owner));
            string[] ClubOptions = Enum.GetNames(typeof(Constants.Club));
            for (int i = 0; i < ClubOptions.Length; i++)
            {
                ClubOptions[i] = ClubOptions[i].Replace('_', ' ');
            }
            ClubTypeRadio = new MakerRadioButtons(category, owner, "Club", 0, ClubOptions)
            {
                ColumnCount = 3
            };

            e.AddControl(ClubTypeRadio).ValueChanged.Subscribe(x =>
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

                Controller.ClubType_Restriction[Controller.CoordinateNum] = x - 1;
            });

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Non-replaceable", category, owner));
            for (int ClothingInt = 0; ClothingInt < 9; ClothingInt++)
            {
                ClothingKeepControls(ClothingInt, e);
            }
            #endregion

            e.AddControl(new MakerSeparator(category, owner));

            e.AddControl(new MakerText("Clothing Nots", category, owner));

            for (int i = 0; i < 3; i++)
            {
                ClothNotsControls(i, e);
            }
            e.AddControl(new MakerButton("Get Nots", category, owner)).OnClick.AddListener(delegate ()
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
                ClothNotToggles[0].SetValue(Controller.ChaControl.notBot);
                ClothNotToggles[1].SetValue(Controller.ChaControl.notBra);
                ClothNotToggles[2].SetValue(Controller.ChaControl.notShorts);
            });
            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Breast Size Restrictions (exclusive)", category, owner));
            for (int i = 0; i < Constants.BreastsizeLength; i++)
            {
                BreastSizeRestrictionControls(i, e);
            }
            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Height Restrictions (exclusive)", category, owner));

            for (int i = 0; i < Constants.HeightLength; i++)
            {
                HeightRestrictionControls(i, e);
            }

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Interest Restrictions: Koi_Sun", category, owner));

            for (int i = 0; i < Constants.InterestLength; i++)
            {
                InterestsRestrictionControls(i, e);
            }

            #region Personality Restrictions
            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Personality Restrictions", category, owner));
            for (int PersonNum = 0, n = Enum.GetNames(typeof(Constants.Personality)).Length; PersonNum < n; PersonNum++)
            {
                PersonalityRestrictionControls(PersonNum, e);
            }

            e.AddControl(new MakerSeparator(category, owner));
            e.AddControl(new MakerText("Trait Restrictions", category, owner));
            for (int TraitNum = 0, n = Enum.GetNames(typeof(Constants.Traits)).Length; TraitNum < n; TraitNum++)
            {
                TraitRestrictionControls(TraitNum, e);
            }
            #endregion

            var GroupingID = "Maker_Tools_" + Settings.NamingID.Value;
            AccKeepToggles.Control.GroupingID = GroupingID;
            HairKeepToggles.Control.GroupingID = GroupingID;
        }

        private static void PersonalClothingKeepControls(int ClothingInt, RegisterSubCategoriesEvent e)
        {
            PersonalKeepToggles[ClothingInt] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglSettings", MakerConstants.Clothes.Copy.Position + 3, "Settings"), $"Keep {((Constants.ClothingTypes)ClothingInt).ToString().Replace('_', ' ')}", false, Settings.Instance));
            PersonalKeepToggles[ClothingInt].ValueChanged.Subscribe(x =>
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
                Controller.PersonalClothingBools[ClothingInt] = x; /*Settings.Logger.LogWarning($"Chaging Outfitnum restriction {(Constants.ClothingTypes)ClothingInt}");*/
            });
        }

        private static void PersonalityRestrictionControls(int PersonalityNum, RegisterSubCategoriesEvent e)
        {
            PersonalityToggles[PersonalityNum] = e.AddControl(new MakerRadioButtons(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), Settings.Instance, ((Constants.Personality)PersonalityNum).ToString().Replace('_', ' '), 1, new string[] { "Exclude", "Neutral", "Include" }));
            PersonalityToggles[PersonalityNum].ValueChanged.Subscribe(x =>
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
                //Settings.Logger.LogWarning($"Changing Outfitnum restriction {(Constants.Personality)PersonalityNum}");
                if (--x == 0)
                {
                    Controller.PersonalityType_Restriction[Controller.CoordinateNum].Remove(PersonalityNum);
                    return;
                }
                Controller.PersonalityType_Restriction[Controller.CoordinateNum][PersonalityNum] = x;
            });
        }

        private static void TraitRestrictionControls(int TraitNum, RegisterSubCategoriesEvent e)
        {
            TraitToggles[TraitNum] = e.AddControl(new MakerRadioButtons(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), Settings.Instance, ((Constants.Traits)TraitNum).ToString().Replace('_', ' '), 1, new string[] { "Exclude", "Neutral", "Include" }));
            TraitToggles[TraitNum].ValueChanged.Subscribe(x =>
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
                //Settings.Logger.LogWarning($"Changing Outfitnum restriction {(Constants.Traits)TraitNum}");
                if (--x == 0)
                {
                    Controller.PersonalityType_Restriction[Controller.CoordinateNum].Remove(TraitNum);
                    return;
                }
                Controller.PersonalityType_Restriction[Controller.CoordinateNum][TraitNum] = x;
            });
        }

        private static void InterestsRestrictionControls(int IntrestNum, RegisterSubCategoriesEvent e)
        {
            InterestToggles[IntrestNum] = e.AddControl(new MakerRadioButtons(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), Settings.Instance, ((Constants.Interests)IntrestNum).ToString().Replace('_', ' '), 1, new string[] { "Exclude", "Neutral", "Include" }));
            InterestToggles[IntrestNum].ValueChanged.Subscribe(x =>
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
                //Settings.Logger.LogWarning($"Changing Outfitnum restriction {(Constants.Traits)TraitNum}");
                if (--x == 0)
                {
                    Controller.PersonalityType_Restriction[Controller.CoordinateNum].Remove(IntrestNum);
                    return;
                }
                Controller.PersonalityType_Restriction[Controller.CoordinateNum][IntrestNum] = x;
            });
        }

        private static void ClothingKeepControls(int ClothingInt, RegisterSubCategoriesEvent e)
        {
            CoordinateKeepToggles[ClothingInt] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), $"Keep {((Constants.ClothingTypes)ClothingInt).ToString().Replace('_', ' ')}", false, Settings.Instance));
            CoordinateKeepToggles[ClothingInt].ValueChanged.Subscribe(x =>
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
                Controller.CoordinateSaveBools[Controller.CoordinateNum][ClothingInt] = x; /*Settings.Logger.LogWarning($"Chaging Outfitnum restriction {(Constants.ClothingTypes)ClothingInt}");*/
            });
        }

        private static void BreastSizeRestrictionControls(int size, RegisterSubCategoriesEvent e)
        {
            BreastsizeToggles[size] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), $"{((Constants.Breastsize)size)}", false, Settings.Instance));
            BreastsizeToggles[size].ValueChanged.Subscribe(x =>
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
                Controller.Breastsize_Restriction[Controller.CoordinateNum][size] = x;
                /*Settings.Logger.LogWarning($"Chaging Outfitnum restriction {(Constants.Breastsize)size}");*/
            });
        }

        private static void HeightRestrictionControls(int size, RegisterSubCategoriesEvent e)
        {
            HeightToggles[size] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), $"{((Constants.Height)size)}", false, Settings.Instance));
            HeightToggles[size].ValueChanged.Subscribe(x =>
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
                Controller.Height_Restriction[Controller.CoordinateNum][size] = x;
                /*Settings.Logger.LogWarning($"Chaging Outfitnum restriction {(Constants.Height)size}");*/
            });
        }

        private static void ClothNotsControls(int index, RegisterSubCategoriesEvent e)
        {
            ClothNotToggles[index] = e.AddControl(new MakerToggle(new MakerCategory("03_ClothesTop", "tglClothSettings", MakerConstants.Clothes.Copy.Position + 2, "Clothing Settings"), $"{((Constants.ClothesNot)index)}", false, Settings.Instance));
            ClothNotToggles[index].ValueChanged.Subscribe(x =>
            {
                var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
                Controller.ClothNotData[Controller.CoordinateNum][index] = x;
            });
        }

        private static void AccHairKeep_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<bool> e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            if (Controller.HairAcc == null)
            {
                return;
            }
            if (e.NewValue)
            {
                Controller.HairAcc[Controller.CoordinateNum].Add(e.SlotIndex);
            }
            else
            {
                Controller.HairAcc[Controller.CoordinateNum].Remove(e.SlotIndex);
            }
        }

        private static void AccKeep_ValueChanged(object sender, AccessoryWindowControlValueChangedEventArgs<bool> e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            if (Controller.AccKeep == null)
            {
                return;
            }
            if (e.NewValue)
            {
                Controller.AccKeep[Controller.CoordinateNum].Add(e.SlotIndex);
            }
            else
            {
                Controller.AccKeep[Controller.CoordinateNum].Remove(e.SlotIndex);
            }
        }

        internal void Slot_ACC_Change(int slotNo, int type)
        {
            if (type == 120)
            {
                AccKeepToggles.SetValue(slotNo, false, false);
                HairKeepToggles.SetValue(slotNo, false, false);
                HairAcc[CoordinateNum].Remove(slotNo);
                AccKeep[CoordinateNum].Remove(slotNo);
            }
            VisibiltyToggle();
        }

        private static void VisibiltyToggle()
        {
            if (!MakerAPI.InsideMaker)
                return;

            var accessory = MakerAPI.GetCharacterControl().GetAccessoryObject(AccessoriesApi.SelectedMakerAccSlot);
            if (accessory == null)
            {

                AccKeepToggles.Control.Visible.OnNext(false);
                HairKeepToggles.Control.Visible.OnNext(false);
            }
            else
            {
                HairKeepToggles.Control.Visible.OnNext(true);
                AccKeepToggles.Control.Visible.OnNext(true);
            }
        }

        private IEnumerator UpdateSlots()
        {
            yield return null;
            var count = AccessoryCount();
            while (!MakerAPI.InsideAndLoaded || AccKeepToggles.Control.ControlObjects.Count() < count)
            {
                yield return null;
            }

            for (int i = 0; i < PersonalClothingBools.Length; i++)
            {
                PersonalKeepToggles[i].SetValue(PersonalClothingBools[i], false);
            }

            for (int i = 0, n = AccKeepToggles.Control.ControlObjects.Count(); i < n; i++)
            {
                bool keep = false;
                if (AccKeep[CoordinateNum].Contains(i))
                {
                    keep = true;
                }
                AccKeepToggles.SetValue(i, keep, false);
                if (HairAcc[CoordinateNum].Contains(i))
                {
                    keep = true;
                }
                else
                {
                    keep = false;
                }
                HairKeepToggles.SetValue(i, keep, false);
            }

            for (int i = 0; i < CoordinateKeepToggles.Length; i++)
            {
                CoordinateKeepToggles[i].SetValue(CoordinateSaveBools[CoordinateNum][i], false);
            }

            for (int i = 0; i < PersonalityToggles.Length; i++)
            {
                if (!PersonalityType_Restriction[CoordinateNum].TryGetValue(i, out int Value))
                {
                    Value = 0;
                }
                PersonalityToggles[i].SetValue(++Value, false);
            }

            for (int i = 0; i < TraitToggles.Length; i++)
            {
                if (!TraitType_Restriction[CoordinateNum].TryGetValue(i, out int Value))
                {
                    Value = 0;
                }
                TraitToggles[i].SetValue(++Value, false);
            }

            for (int i = 0; i < HeightToggles.Length; i++)
            {
                HeightToggles[i].SetValue(Height_Restriction[CoordinateNum][i], false);
            }

            for (int i = 0; i < BreastsizeToggles.Length; i++)
            {
                BreastsizeToggles[i].SetValue(Breastsize_Restriction[CoordinateNum][i], false);
            }

            for (int i = 0; i < 3; i++)
            {
                ClothNotToggles[i].SetValue(false, false);
            }

            for (int i = 0; i < Constants.InterestLength; i++)
            {
                InterestToggles[i].SetValue(Interest_Restriction[CoordinateNum][i]);
            }

            MakeUpToggle.SetValue(MakeUpKeep[CoordinateNum], false);
            CoordinateTypeRadio.SetValue(CoordinateType[CoordinateNum], false);
            CoordinateSubTypeRadio.SetValue(CoordinateSubType[CoordinateNum], false);
            ClubTypeRadio.SetValue(ClubType_Restriction[CoordinateNum], false);
            HStateTypeRadio.SetValue(HstateType_Restriction[CoordinateNum], false);
            Creator.SetValue(CreatorNames[CoordinateNum], false);
            Set_Name.SetValue(SetNames[CoordinateNum], false);
        }

        private static void AccessoriesApi_AccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();
            if (Controller.HairAcc[Controller.CoordinateNum].Contains(e.SourceSlotIndex))
            {
                Controller.HairAcc[Controller.CoordinateNum].Add(e.DestinationSlotIndex);
            }
            if (Controller.AccKeep[Controller.CoordinateNum].Contains(e.SourceSlotIndex))
            {
                Controller.AccKeep[Controller.CoordinateNum].Add(e.DestinationSlotIndex);
            }
            VisibiltyToggle();
        }

        private static void AccessoriesApi_AccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            var Controller = MakerAPI.GetCharacterControl().GetComponent<CharaEvent>();

            var CopiedSlots = e.CopiedSlotIndexes.ToArray();
            var Source = (int)e.CopySource;
            var Dest = (int)e.CopyDestination;
            //Settings.Logger.LogWarning($"Source {Source} Dest {Dest}");
            for (int i = 0; i < CopiedSlots.Length; i++)
            {
                //Settings.Logger.LogWarning($"ACCKeep");
                if (Controller.AccKeep[Source].Contains(CopiedSlots[i]) && !Controller.AccKeep[Dest].Contains(CopiedSlots[i]))
                {
                    Controller.AccKeep[Dest].Add(CopiedSlots[i]);
                }
                //Settings.Logger.LogWarning($"HairKeep");
                if (Controller.HairAcc[Source].Contains(CopiedSlots[i]) && !Controller.HairAcc[Dest].Contains(CopiedSlots[i]))
                {
                    Controller.HairAcc[Dest].Add(CopiedSlots[i]);
                }
            }
            VisibiltyToggle();
        }

        private int AccessoryCount()
        {
            WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();
            if (_accessoriesByChar.TryGetValue(ChaFileControl, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
            }
            return data.nowAccessories.Count() + 20;
        }

        internal void MovIt(List<QueueItem> queue)
        {
            var Acckeep = this.AccKeep[CoordinateNum];
            var HairAcc = this.HairAcc[CoordinateNum];
            foreach (var item in queue)
            {
                if (Acckeep.Contains(item.SrcSlot))
                {
                    Acckeep.Add(item.DstSlot);
                    Acckeep.Remove(item.SrcSlot);
                    AccKeepToggles.SetValue(item.DstSlot, true, false);
                    AccKeepToggles.SetValue(item.SrcSlot, false, false);
                }
                if (HairAcc.Contains(item.SrcSlot))
                {
                    HairAcc.Add(item.DstSlot);
                    HairAcc.Remove(item.SrcSlot);
                    HairKeepToggles.SetValue(item.DstSlot, true, false);
                    HairKeepToggles.SetValue(item.SrcSlot, false, false);
                }
            }
        }
    }
}
