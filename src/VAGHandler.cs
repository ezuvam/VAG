using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using MVR.FileManagementSecure;

namespace ezuvam.VAG
{
    public class VAGStore : VAGCustomStorable
    {
        public int storeVersion { get { return GetDataInt("storeVersion"); } set { SetDataInt("storeVersion", value); } }
        public float TextSpeedFactor { get { return GetDataFloat("TextSpeedFactor", 0.2f); } set { SetDataFloat("TextSpeedFactor", value); } }
        public string InitialGameStatsFile { get { return GetDataStr("InitialGameStatsFile"); } set { SetDataStr("InitialGameStatsFile", value); } }
        public string InitialDialog { get { return GetDataStr("InitialDialog"); } set { SetDataStr("InitialDialog", value); } }
      

        private readonly int _currentStoreVersion = 1;
        public VAGQuestsCollection Quests;
        public VAMACharacterCollection Characters;
        public VAGDialogsCollection Dialogs;
        public VAGQuestLocationsCollection Locations;
        public VAGTransitionsCollection Transitions;
        public readonly VAGGameStates GameStates = new VAGGameStates(null);

        public override void Clear()
        {
            base.Clear();
            storeVersion = _currentStoreVersion;

            Quests.Clear();
            Characters.Clear();
            Dialogs.Clear();
            Locations.Clear();
            Transitions.Clear();

            LoadFromJSON(_data);
        }
        public VAGStore(JSONClass initialData) : base(initialData)
        {
            Quests = new VAGQuestsCollection(GetDataObject("Quests"), this);
            Characters = new VAMACharacterCollection(GetDataObject("Characters"), this);
            Dialogs = new VAGDialogsCollection(GetDataObject("Dialogs"), this);
            Locations = new VAGQuestLocationsCollection(GetDataObject("Locations"), this);
            Transitions = new VAGTransitionsCollection(GetDataObject("Transitions"), this);
        }

        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            Quests.LoadFromJSON(GetDataObject("Quests"));
            Characters.LoadFromJSON(GetDataObject("Characters"));
            Dialogs.LoadFromJSON(GetDataObject("Dialogs"));
            Locations.LoadFromJSON(GetDataObject("Locations"));
            Transitions.LoadFromJSON(GetDataObject("Transitions"));

            GameStates.ResetToNew();
        }

        public void GameStateChanged(VAGHandler Handler)
        {
            Quests.GameStateChanged(Handler);
            Characters.GameStateChanged(Handler);
            Dialogs.GameStateChanged(Handler);
            Locations.GameStateChanged(Handler);
            Transitions.GameStateChanged(Handler);
        }

    }

    public class VAGGameStates : VAGCustomStorable
    {
        public int storeVersion { get { return GetDataInt("storeVersion"); } set { SetDataInt("storeVersion", value); } }
        public string activeLocation { get { return GetDataStr("activeLocation"); } set { SetDataStr("activeLocation", value); } }
        public string activePlace { get { return GetDataStr("activePlace"); } set { SetDataStr("activePlace", value); } }

        public JSONClass GetGameState(VAGCustomStorable gameObj)
        {
            //SuperController.LogMessage($"returning gamestate for {gameObj.UID}");

            return GetDataObject(gameObj.UID);
        }

        public VAGGameStates(JSONClass initialData) : base(initialData)
        {

        }

        public void ResetToNew()
        {
            Clear();
        }

    }

    public class VAGChainedQuestObjContext : VAGObject
    {
        private int _currentPlayIndex;
        private readonly VAGTransition _transition;
        public readonly VAGObject[] QuestObjects;
        public VAGChainedQuestObjContext(VAGObject[] questObjs, VAGTransition transition = null)
        {
            QuestObjects = questObjs;
            _transition = transition;
            _currentPlayIndex = -1;
        }

        protected void PlayNext(VAGHandler Handler)
        {
            //SuperController.LogMessage($"Chained actions playnext call {_currentPlayIndex}");

            _currentPlayIndex += 1;

            if (_currentPlayIndex < QuestObjects.Length)
            {
                States.TimeToRun = 0.01f;
                QuestObjects[_currentPlayIndex].OnPlayingFinish += new VAGObjEventHandler(doOnPlayingObjectFinish);
                Handler.PlayObject(QuestObjects[_currentPlayIndex]);
            }
        }
        protected void doOnPlayingObjectFinish(VAGObject sender, VAGHandler Handler)
        {
            //SuperController.LogMessage($"Chained action object finish event");

            sender.OnPlayingFinish -= new VAGObjEventHandler(doOnPlayingObjectFinish);
            PlayNext(Handler);
        }
        public override void Start(VAGHandler Handler)
        {
            base.Start(Handler);

            if (_transition != null)
            {
                _transition.OnPlayingFinish += new VAGObjEventHandler(doOnPlayingObjectFinish);
                _transition.Active = true;
                Handler.PlayObject(_transition);
            }
            else
            {
                PlayNext(Handler);
            }
        }

        public override void Finish(VAGHandler Handler)
        {
            //SuperController.LogMessage($"Chained actions finish call");

            if (_currentPlayIndex >= QuestObjects.Length)
            {
                SuperController.LogMessage($"Chained actions finish.");

                base.Finish(Handler);

                if (_transition != null)
                {
                    SuperController.LogMessage($"Ending transistion.");

                    _transition.Active = false;
                    Handler.PlayObject(_transition, _transition.Delay);
                }
            }
            else
            {
                //States.TimeToRun = 1;
            }
        }
    }

    public class VAGHandler
    {
        public Atom containingAtom { get { return OwnerPlugin.containingAtom; } }
        public readonly VAGPlugin OwnerPlugin;
        private readonly float _updateTimerInterval = 0.1f;
        private float _updateTimerVal = 0.0f;
        public readonly VAGStore Store = new VAGStore(null);
        public bool Assigned(object o) { return !System.Object.ReferenceEquals(o, null); }
        private readonly List<VAGObject> _playingObjects = new List<VAGObject>();
        public readonly VAMMainMenuDialogUI MainMenuUI;
        private readonly List<VAMCustomUIWnd> _uilist = new List<VAMCustomUIWnd>();
        public string ActiveGameFileName { get; set; }
        public bool Active { get; set; }
        public VAGTransition ActiveTransition { get; set; }

        public VAGHandler(VAGPlugin ownerPlugin)
        {
            OwnerPlugin = ownerPlugin;

            MainMenuUI = new VAMMainMenuDialogUI(OwnerPlugin);
            _uilist.Add(MainMenuUI);

        }

        public void LoadGameStatsFromFile(string fileName)
        {
            Store.GameStates.LoadFromJSON(JSON.Parse(SuperController.singleton.ReadFileIntoString(fileName)).AsObject);
            Store.GameStateChanged(this);
        }
        public void SaveGameStatsToFile(string fileName)
        {
            try
            {
                SuperController.singleton.SaveStringIntoFile(fileName, Store.GameStates.SaveToJSON().ToString());
            }
            catch (Exception e)
            {
                SuperController.LogError($"{nameof(VAGPlugin)}.{nameof(SaveStoreToFile)}: {e}");
            }
        }

        public void SaveInitialGameStatsFile()
        {
            if (string.IsNullOrEmpty(Store.InitialGameStatsFile))
            {
                Store.InitialGameStatsFile = "Saves\\scene\\VAGInitalGameState.vamsavegame";
            }

            if (!string.IsNullOrEmpty(Store.InitialGameStatsFile))
            {
                SaveGameStatsToFile(Store.InitialGameStatsFile);
            }
        }
        public void LoadStoreFromFile(string fileName)
        {
            try
            {
                Store.LoadFromJSON(JSON.Parse(SuperController.singleton.ReadFileIntoString(fileName)).AsObject);
                ActiveGameFileName = fileName;
                Store.Changed();

                if (!string.IsNullOrEmpty(Store.InitialGameStatsFile) & FileManagerSecure.FileExists(Store.InitialGameStatsFile))
                {
                    LoadGameStatsFromFile(Store.InitialGameStatsFile);
                }
                else
                {
                    if (Store.Locations.Count > 0)
                    {
                        Store.GameStates.activeLocation = Store.Locations.ByIndex(0).Name;

                        if (Store.Locations.ByIndex(0).Places.Count > 0)
                        {
                            Store.GameStates.activePlace = Store.Locations.ByIndex(0).Places.ByIndex(0).Name;
                        }

                    }

                    Store.GameStateChanged(this);

                    if (!string.IsNullOrEmpty(Store.InitialDialog))
                    {
                        PlayDialog(Store.InitialDialog);
                    }
                }

                SuperController.LogMessage($"{nameof(VAGPlugin)} gamefile '{fileName}' loaded.");

            }
            catch (Exception e)
            {
                SuperController.LogError($"{nameof(VAGPlugin)}.{nameof(LoadStoreFromFile)}: {e}");
            }

        }

        public void SaveStoreToFile(string fileName)
        {
            try
            {
                SuperController.singleton.SaveStringIntoFile(fileName, Store.SaveToJSON().ToString());
                ActiveGameFileName = fileName;
            }
            catch (Exception e)
            {
                SuperController.LogError($"{nameof(VAGPlugin)}.{nameof(SaveStoreToFile)}: {e}");
            }
        }
        public void Reset()
        {
            _playingObjects.Clear();
        }

        public void StopPlayObject(VAGObject QuestObj)
        {
            if (Assigned(QuestObj))
            {
                if ((_playingObjects.Contains(QuestObj)))
                {
                    _playingObjects.Remove(QuestObj);
                    QuestObj.Stop(this);
                }
            }
        }

        public void PlayObject(VAGObject QuestObj, float startDelay = 0)
        {
            if (!Active)
            {
                SuperController.LogMessage($"Call to {nameof(PlayObject)} on inactive quest-handler!");
            }

            if (Assigned(QuestObj))
            {
                if (!_playingObjects.Contains(QuestObj))
                {
                    QuestObj.States.StartDelay = startDelay;

                    if (startDelay == 0)
                    {
                        QuestObj.Start(this);

                        if (QuestObj.States.TimeToRun == 0)
                        {
                            QuestObj.Finish(this);

                            if (QuestObj.States.Running)
                            {
                                _playingObjects.Add(QuestObj);
                            }
                        }
                        else
                        {
                            _playingObjects.Add(QuestObj);
                        }

                    }
                    else
                    {
                        _playingObjects.Add(QuestObj);
                    }
                }
            }
        }

        public void PlayDialog(VAGDialog Dialog)
        {
            StopPlayObject(Dialog);
            PlayObject(Dialog);
        }

        public void PlayDialog(string Name)
        {
            VAGDialog Dialog = Store.Dialogs.ByName(Name);
            if (Assigned(Dialog))
            {
                PlayDialog(Dialog);
            }
            else
            {
                SuperController.LogError($"Dialog with name {Name} not found!");
            }
        }

        public void ChangePlace(string Name, bool allowSceneChange = true)
        {
            VAGPlace Place = Store.Locations.PlaceByName(Name);
            if (Assigned(Place))
            {
                if (!Place.Name.Equals(Store.GameStates.activePlace))
                {
                    if (!Place.Location.Name.Equals(Store.GameStates.activeLocation))
                    {
                        if (allowSceneChange)
                        {
                            PlayObject(Place.Location); // TODO, scene change and then load place
                        }
                        else
                        {
                            SuperController.LogError($"Can not change to scene {Place.Location.Name} in edit mode.");
                            throw new InvalidOperationException($"Can not change to scene {Place.Location.Name} in edit mode.");
                        }
                    }
                    PlayObject(Place);
                }
                else
                {
                    //SuperController.LogMessage($"Already in place {Name}");   
                }
            }
            else
            {
                SuperController.LogError($"Place with name {Name} not found!");
            }
        }
        public void ChangeWardrobe(string personName, string wardrobeName)
        {
            VAGCharacter character = Store.Characters.ByName(personName);
            if (Assigned(character))
            {
                character.ChangeWardrobe(this, wardrobeName);
            }
            else
            {
                SuperController.LogError($"Character with name {personName} not found!");
            }
        }
        public void ChangeMood(string personName, string moodName)
        {
            VAGCharacter character = Store.Characters.ByName(personName);
            if (Assigned(character))
            {
                character.ChangeMood(this, moodName);
            }
            else
            {
                SuperController.LogError($"Character with name {personName} not found!");
            }
        }

        public void Update()
        {
            //if (!SuperController.singleton.freezeAnimation & Active)
            if (Active)
            {
                _updateTimerVal += Time.deltaTime;

                if (_updateTimerVal > _updateTimerInterval)
                {
                    VAGObject questObject;

                    //SuperController.LogMessage("_playingObjects count: " + _playingObjects.Count.ToString());

                    for (int i = _playingObjects.Count - 1; i >= 0; i--)
                    {
                        questObject = _playingObjects[i];

                        if (questObject.States.Running)
                        {
                            questObject.States.TimeToRun -= _updateTimerVal;

                            if (questObject.States.TimeToRun <= 0)
                            {
                                questObject.Finish(this);

                                if (!questObject.States.Running)
                                {
                                    _playingObjects.Remove(questObject);
                                }
                            }
                        }
                        else
                        {
                            questObject.States.StartDelay -= _updateTimerVal;

                            if (questObject.States.StartDelay <= 0)
                            {
                                questObject.Start(this);
                            }
                        }

                    }

                    _updateTimerVal = 0;
                }
            }

            for (int i = 0; i < _uilist.Count; i++)
            {
                _uilist[i].Update();
            }

        }

        public void OnDestroy()
        {
            for (int i = 0; i < _uilist.Count; i++)
            {
                _uilist[i].OnDestroy();
            }
        }

        public void DebugTest1()
        {
            SuperController.LogMessage("VAGHandler creating DebugTest1 data");

            Store.Clear();
            VAGActionsCollection actions;
            VAGAtomSetting atomSetting;

            VAGCharacter Person1 = Store.Characters.Add("Person1");
            Person1.FullName = "Alexa Questa";
            Person1.FirstName = "Questa";
            Person1.LastName = "Alexa";
            Person1.AtomName = "Person";

            VAGCharacter Person2 = Store.Characters.Add("Person2");
            Person2.FullName = "Hank Wank";
            Person2.FirstName = "Wank";
            Person2.LastName = "Hank";
            Person2.AtomName = "Person#2";

            VAGTransition transition;
            transition = Store.Transitions.Add("trans_inplace");
            transition.Delay = 0;
            atomSetting = transition.Atoms.Add("");
            atomSetting.AtomName = "3PointLightSetup/LightFrontRight";
            atomSetting.OnEnabled = true;
            atomSetting.On = false;

            atomSetting = transition.Atoms.Add("");
            atomSetting.AtomName = "3PointLightSetup/LightFrontLeft";
            atomSetting.OnEnabled = true;
            atomSetting.On = false;

            atomSetting = transition.Atoms.Add("");
            atomSetting.AtomName = "3PointLightSetup/LightBack";
            atomSetting.OnEnabled = true;
            atomSetting.On = false;

            VAGLocation Location;
            VAGPlace questPlace;

            Store.GameStates.activeLocation = "home";

            Location = Store.Locations.Add("home", "your home");
            questPlace = Location.Places.Add("livingroom", "the living room");
            questPlace.Transition = "trans_inplace";
            atomSetting = questPlace.Atoms.Add("");
            atomSetting.AtomName = "Person";

            questPlace = Location.Places.Add("kitchen", "wifes work room ;-)");
            questPlace.Transition = "trans_inplace";
            atomSetting = questPlace.Atoms.Add("");
            atomSetting.AtomName = "Person";

            questPlace = Location.Places.Add("jard", "the jard outside");
            questPlace.Transition = "trans_inplace";
            atomSetting = questPlace.Atoms.Add("");
            atomSetting.AtomName = "Person";

            Location = Store.Locations.Add("city", "city");
            Location.Places.Add("restaurant", "a nice restaurant");

            Store.Quests.Add("test quest 1").Quests.Add("Sub quest 1.1").Quests.Add("Subquest 1.1.1");
            Store.Quests.Add("test quest 2").Quests.Add("Sub quest 2.1").Quests.Add("Subquest 2.1.1");

            VAGDialog Dialog = Store.Dialogs.Add("Intro");
            Dialog.DialogText = "Hey there!";

            Dialog = Dialog.Dialogs.Add();
            Dialog.DialogText = "I'm over here";

            Dialog = Dialog.Dialogs.Add();
            Dialog.DialogText = "Yes you! Don't be shy, come to me :)";

            Dialog = Dialog.Dialogs.Add();
            Dialog.DialogText = "Nice to meet you sweety. My name is {Person1.FullName}. Whats your name?";
            Dialog.Choices.Add("", "I'm completely lost i forgot my name").Actions.Add("", "dialog", "IntroDislike");
            Dialog.Choices.Add("", "My name is secret can't tell").Actions.Add("", "dialog", "IntroDislike");
            Dialog.Choices.Add("", "My name is Rudolf the Reindeer").Actions.Add("", "dialog", "IntroFunny");
            Dialog.Choices.Add("", "I'm {Person2.FullName} nice to meet you*").Actions.Add("", "dialog", "IntroLike");

            Dialog = Store.Dialogs.Add("IntroLike");
            Dialog.DialogText = "Nice to meet you {Person2.LastName}. I saw that you have been watching me secretly. Saw anyting you like?";
            Dialog.Choices.Add("", "Well as you ask, your tits are nice*").Actions.Add("", "dialog", "LikeTits");
            Dialog.Choices.Add("", "I want to fuck your ass").Actions.Add("", "dialog", "IntroDislike");
            Dialog.Choices.Add("", "Naa nothing, you are fat and ugly").Actions.Add("", "dialog", "IntroDislike");

            Dialog = Store.Dialogs.Add("LikeTits");
            Dialog.DialogText = "hihihi you are very straightforward... i kindy like that on guys. You like to see them?";
            Dialog.Choices.Add("", "Naa keep that hanging sacks packed please").Actions.Add("", "dialog", "FinishNoShow");
            Dialog.Choices.Add("", "You going to suck my cock after?").Actions.Add("", "dialog", "FinishNoShow");
            Dialog.Choices.Add("", "uhm, yes... love to!*").Actions.Add("", "dialog", "AskToGoInside");

            Dialog = Store.Dialogs.Add("AskToGoInside");
            Dialog.DialogText = "mmm baby... we better go inside don't you think?";
            Dialog.Choices.Add("", "I don't care if the neighbour can see you tits. Strip of now baby.").Actions.Add("", "dialog", "IntroDislike");
            actions = Dialog.Choices.Add("", "Move your nice ass inside that house sweety*").Actions;
            actions.Add("", "gotoplace", "livingroom");
            actions.Add("", "dialog", "StripTits", 10);

            Dialog = Store.Dialogs.Add("StripTits");
            Dialog.DialogText = "Here we go sweety but you own me something for that ;-)";
            Dialog.Actions.Add("", "clothing", "", 5);  // TODO    
            Dialog.Actions.Add("", "dialog", "Finish", 10);

            Dialog = Store.Dialogs.Add("IntroFunny");
            Dialog.DialogText = "HAHAHA You are funny. I guess we going to have a lot of fun together.";
            Dialog.Actions.Add("", "dialog", "Finish");

            Dialog = Store.Dialogs.Add("IntroDislike");
            Dialog.DialogText = "Dude what's wrong with you. I'm out";
            Dialog.Actions.Add("", "dialog", "Finish");

            Dialog = Store.Dialogs.Add("FinishNoShow");
            Dialog.DialogText = "You rude, keep dreaming. I'm out";
            Dialog.Actions.Add("", "dialog", "Finish");

            Dialog = Store.Dialogs.Add("Finish");
            Dialog.DialogText = "Thanks for playing. You reached the end, follow the * choices for full demo";


            Store.Changed();
            //SaveToFile("./Custom/Scripts/ezuvam/VAG/quests/demo1.quest.json");
            //LoadFromFile("./Custom/Scripts/ezuvam/VAG/quests/demo1.quest.json");
            //SaveToFile("./Custom/Scripts/ezuvam/VAG/quests/demo1.quest_resave_test.json");

        }

        public void DebugTest2()
        {
            SuperController.LogMessage("VAGHandler creating DebugTest2 data");

            Store.Clear();

            VAGCharacter Person1 = Store.Characters.Add("Person1");
            Person1.FullName = "Alexa Questa";
            Person1.FirstName = "Questa";
            Person1.LastName = "Alexa";
            Person1.AtomName = "Person";

            VAGDialog Dialog = Store.Dialogs.Add("Intro");
            Dialog.DialogText = "";
            Dialog.Choices.Add("", "I'm completely lost i forgot my name").Actions.Add("", "dialog", "IntroDislike");
            Dialog.Choices.Add("", "My name is secret can't tell").Actions.Add("", "dialog", "IntroDislike");
            Dialog.Choices.Add("", "My name is Rudolf the Reindeer").Actions.Add("", "dialog", "IntroFunny");
            Dialog.Choices.Add("", "I'm {Person2.FullName} nice to meet you*").Actions.Add("", "dialog", "IntroLike");

            Dialog.Choices.Add("", "I'm completely lost i forgot my name").Actions.Add("", "dialog", "IntroDislike");
            Dialog.Choices.Add("", "My name is secret can't tell").Actions.Add("", "dialog", "IntroDislike");
            Dialog.Choices.Add("", "My name is Rudolf the Reindeer").Actions.Add("", "dialog", "IntroFunny");
            Dialog.Choices.Add("", "I'm {Person2.FullName} nice to meet you*").Actions.Add("", "dialog", "IntroLike");

            Store.Changed();
        }


        public void DebugTest3()
        {
            SuperController.LogMessage("VAGHandler creating DebugTest3 data");

            Store.Clear();

            VAGActionsCollection actions;
            VAGAtomSetting atomSetting;

            VAGCharacter Person1 = Store.Characters.Add("Person1");
            Person1.FullName = "Alexa Questa";
            Person1.FirstName = "Questa";
            Person1.LastName = "Alexa";
            Person1.AtomName = "Person";

            VAGCharacter Person2 = Store.Characters.Add("Person2");
            Person2.FullName = "Hank Wank";
            Person2.FirstName = "Wank";
            Person2.LastName = "Hank";
            Person2.AtomName = "Person#2";

            VAGTransition transition;
            transition = Store.Transitions.Add("trans_inplace");
            transition.Delay = 1f;

            atomSetting = transition.Atoms.Add("");
            atomSetting.AtomName = "LightSun";
            atomSetting.OnEnabled = true;
            atomSetting.On = false;
            /*
            atomSetting = transition.Atoms.Add("");
            atomSetting.AtomName = "3PointLightSetup/LightFrontRight";
            atomSetting.OnEnabled = true;
            atomSetting.On = false;

            atomSetting = transition.Atoms.Add("");
            atomSetting.AtomName = "3PointLightSetup/LightFrontLeft";
            atomSetting.OnEnabled = true;
            atomSetting.On = false;

            atomSetting = transition.Atoms.Add("");
            atomSetting.AtomName = "3PointLightSetup/LightBack";
            atomSetting.OnEnabled = true;
            atomSetting.On = false;    
            */

            VAGLocation Location;
            VAGPlace questPlace;

            Store.GameStates.activeLocation = "home";
            Location = Store.Locations.Add("home", "your home");

            questPlace = Location.Places.Add("livingroom_place", "the living room");
            questPlace.Transition = "trans_inplace";
            //atomSetting = questPlace.Atoms.Add();
            //atomSetting.AtomName = "Person";
            //atomSetting.PositionEnabled = true;
            //atomSetting.X = 1;

            questPlace = Location.Places.Add("bedroom_place", "the bed room");
            questPlace.Transition = "trans_inplace";
            //atomSetting = questPlace.Atoms.Add();
            //atomSetting.AtomName = "Person";
            //atomSetting.PositionEnabled = true;
            //atomSetting.X = 0;

            questPlace = Location.Places.Add("beachnear_place", "the beach near the house");
            questPlace.Transition = "trans_inplace";

            questPlace = Location.Places.Add("beachfar_place", "the beach far away  from the house");
            questPlace.Transition = "trans_inplace";


            VAGDialog Dialog = Store.Dialogs.Add("LivingRoomDialog");
            Dialog.DialogText = "The living room is nice. Where should we go?";
            Dialog.Choices.Add("", "let's stay").Actions.Add("", "dialog", "LivingRoomDialog");

            actions = Dialog.Choices.Add("", "let's go to the bed room").Actions;
            actions.Add("", "gotoplace", "bedroom_place");
            actions.Add("", "dialog", "BedRoomRoomDialog", 10);

            actions = Dialog.Choices.Add("", "Nice view, let's go to the beach").Actions;
            actions.Add("", "gotoplace", "beachnear_place");
            actions.Add("", "dialog", "BeachNearRoomDialog", 10);


            Dialog = Store.Dialogs.Add("BedRoomRoomDialog");
            Dialog.DialogText = "Looks like a comfy bed :)";
            Dialog.Choices.Add("", "well we can show you how comfy it is ;)").Actions.Add("", "dialog", "BedRoomRoomDialog");

            actions = Dialog.Choices.Add("", "let's go back to the living room").Actions;
            actions.Add("", "gotoplace", "livingroom_place");
            actions.Add("", "dialog", "LivingRoomDialog", 10);


            Dialog = Store.Dialogs.Add("BeachNearRoomDialog");
            Dialog.DialogText = "awww what a nice beach, but looks a bit crowded";
            Dialog.Choices.Add("", "lets jump into the wather").Actions.Add("", "dialog", "BeachNearRoomDialog");

            actions = Dialog.Choices.Add("", "there is a more quiet place around the clif, follow me").Actions;
            actions.Add("", "gotoplace", "beachfar_place");
            actions.Add("", "dialog", "BeachFarRoomDialog", 10);


            Dialog = Store.Dialogs.Add("BeachFarRoomDialog");
            Dialog.DialogText = "love this place, no one can see us here *giggles*";
            Dialog.Choices.Add("", "well i can see you ;)").Actions.Add("", "dialog", "BeachFarRoomDialog");

            actions = Dialog.Choices.Add("", "let's go back").Actions;
            actions.Add("", "gotoplace", "beachnear_place");
            actions.Add("", "dialog", "BeachNearRoomDialog", 10);

            ActiveGameFileName = "./Custom/Scripts/ezuvam/VAG/quests/VAGGameDemo01.json";
            Store.Changed();
        }

    }



}