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
        public VAGHandler Handler { get; set; }
        public int storeVersion { get { return GetDataInt("storeVersion"); } set { SetDataInt("storeVersion", value); } }
        public float TextSpeedFactor { get { return GetDataFloat("TextSpeedFactor", 0.2f); } set { SetDataFloat("TextSpeedFactor", value); } }
        public string InitialGameStatsFile { get { return GetDataStr("InitialGameStatsFile"); } set { SetDataStr("InitialGameStatsFile", value); } }
        public string InitialDialog { get { return GetDataStr("InitialDialog"); } set { SetDataStr("InitialDialog", value); } }
        public string InitialQuest { get { return GetDataStr("InitialQuest"); } set { SetDataStr("InitialQuest", value); } }

        private readonly int _currentStoreVersion = 1;
        public VAGQuestsCollection Quests;
        public VAMACharacterCollection Characters;
        public VAGDialogsCollection Dialogs;
        public VAGItemCollection Items;
        public VAGQuestLocationsCollection Locations;
        public VAGTransitionsCollection Transitions;
        public readonly VAGGameStates GameStates = new VAGGameStates(null);

        public override void Clear()
        {
            Quests.Clear();
            Characters.Clear();
            Dialogs.Clear();
            Items.Clear();
            Locations.Clear();
            Transitions.Clear();
            base.Clear();

            storeVersion = _currentStoreVersion;

            LoadFromJSON(_data);
        }
        public VAGStore(JSONClass initialData) : base(initialData)
        {
            Quests = new VAGQuestsCollection(GetDataObject("Quests"), this);
            Characters = new VAMACharacterCollection(GetDataObject("Characters"), this);
            Dialogs = new VAGDialogsCollection(GetDataObject("Dialogs"), this);
            Items = new VAGItemCollection(GetDataObject("Items"), this);
            Locations = new VAGQuestLocationsCollection(GetDataObject("Locations"), this);
            Transitions = new VAGTransitionsCollection(GetDataObject("Transitions"), this);
        }

        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            Quests.LoadFromJSON(GetDataObject("Quests"));
            Characters.LoadFromJSON(GetDataObject("Characters"));
            Dialogs.LoadFromJSON(GetDataObject("Dialogs"));
            Items.LoadFromJSON(GetDataObject("Items"));
            Locations.LoadFromJSON(GetDataObject("Locations"));
            Transitions.LoadFromJSON(GetDataObject("Transitions"));

            GameStates.ResetToNew();
        }

        public void GameStateChanged()
        {
            Quests.GameStateChanged(Handler);
            Characters.GameStateChanged(Handler);
            Dialogs.GameStateChanged(Handler);
            Items.GameStateChanged(Handler);
            Locations.GameStateChanged(Handler);
            Transitions.GameStateChanged(Handler);
        }

        public override void BindToScene(VAGHandler Handler)
        {
            base.BindToScene(Handler);
            Quests.BindToScene(Handler);
            Characters.BindToScene(Handler);
            Dialogs.BindToScene(Handler);
            Items.BindToScene(Handler);
            Locations.BindToScene(Handler);
            Transitions.BindToScene(Handler);
        }
        public override void AddToDict(Dictionary<string, VAGCustomStorable> Dict, string AttrName)
        {
            base.AddToDict(Dict, AttrName);
            Quests.AddToDict(Dict, AttrName);
            Characters.AddToDict(Dict, AttrName);
            Dialogs.AddToDict(Dict, AttrName);
            Items.AddToDict(Dict, AttrName);
            Locations.AddToDict(Dict, AttrName);
            Transitions.AddToDict(Dict, AttrName);
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
        private bool _active = false;
        public bool Active
        {
            get { return _active; }
            set
            {
                _active = value;

                if (_active)
                {
                    Store.BindToScene(this);
                }
            }
        }
        public VAGTransition ActiveTransition { get; set; }

        public VAGHandler(VAGPlugin ownerPlugin)
        {
            OwnerPlugin = ownerPlugin;
            Store.Handler = this;

            MainMenuUI = new VAMMainMenuDialogUI(OwnerPlugin);
            _uilist.Add(MainMenuUI);
        }

        public void RegisterUI(VAMCustomUIWnd wndUI)
        {
            _uilist.Add(wndUI);
        }
        public void UnRegisterUI(VAMCustomUIWnd wndUI)
        {
            _uilist.Remove(wndUI);
        }
        public void LoadGameStatsFromFile(string fileName)
        {
            Store.GameStates.LoadFromJSON(JSON.Parse(SuperController.singleton.ReadFileIntoString(fileName)).AsObject);
            Store.GameStateChanged();
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

                if (Active)
                {
                    Store.BindToScene(this);
                }

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

                    Store.GameStateChanged();

                    if (!string.IsNullOrEmpty(Store.InitialQuest))
                    {
                        StartQuest(Store.InitialQuest);
                    }
                    else
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

        public void StopAllDialogs()
        {
            for (int i = _playingObjects.Count - 1; i >= 0; i--)
            {
                if (_playingObjects[i] is VAGDialog)
                {
                    _playingObjects[i].Stop(this);
                    _playingObjects.Remove(_playingObjects[i]);
                }
            }
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

        public void ActivateItem(string Name)
        {
            VAGItem Item = Store.Items.ByName(Name);
            if (Assigned(Item))
            {
                PlayObject(Item);
            }
            else
            {
                SuperController.LogError($"Item with name {Name} not found!");
            }
        }
        public void PlayDialog(VAGDialog Dialog)
        {
            StopPlayObject(Dialog);

            float startDelay = 0;

            if (Assigned(ActiveTransition))
            {
                startDelay = ActiveTransition.Delay;
            }
            PlayObject(Dialog, startDelay);
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
        public void StartQuest(VAGQuest Quest)
        {
            PlayObject(Quest);
        }
        public void StartQuest(string Name)
        {
            VAGQuest Quest = Store.Quests.ByName(Name);
            if (Assigned(Quest))
            {
                StartQuest(Quest);
            }
            else
            {
                SuperController.LogError($"Quest with name {Name} not found!");
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
        public static int StrToIntDef(string s, int @default = 0)
        {
            int number;
            if (int.TryParse(s, out number))
                return number;
            return @default;
        }
        public void UpdateVariable(string VarName, string VarValue)
        {
            string[] varparts = VarName.Split('.');

            if (varparts.Length > 1)
            {
                VAGCustomStorable item = Store.ByName(varparts[0], null);
                if (Assigned(item))
                {
                    item.SetGameVariable(varparts[1], VarValue);
                }
                else
                {
                    Store.GameStates.SetDataStr(VarName, VarValue);
                }
            }
            else
            {
                Store.GameStates.SetDataStr(VarName, VarValue);
            }
        }

        public string GetVariableValue(string VarName)
        {
            string[] varparts = VarName.Split('.');

            if (varparts.Length > 1)
            {
                VAGCustomStorable item = Store.ByName(varparts[0], null);
                if (Assigned(item))
                {
                    return item.GetGameVariable(varparts[1]);
                }
                else
                {
                    return Store.GameStates.GetDataStr(VarName);
                }
            }
            else
            {
                return Store.GameStates.GetDataStr(VarName);
            }
        }
        public int GetVariableValueInt(string VarName)
        {
            return StrToIntDef(GetVariableValue(VarName), 0);
        }
        public void Update()
        {
            //if (!SuperController.singleton.freezeAnimation & Active)
            if (Active)
            {
                _updateTimerVal += Time.deltaTime;

                if (_updateTimerVal > _updateTimerInterval)
                {
                    Store.Quests.UpdateQuests(this);

                    VAGObject playingObject;

                    //SuperController.LogMessage("_playingObjects count: " + _playingObjects.Count.ToString());

                    for (int i = _playingObjects.Count - 1; i >= 0; i--)
                    {
                        playingObject = _playingObjects[i];

                        if (playingObject.States.Running)
                        {
                            playingObject.States.TimeToRun -= _updateTimerVal;

                            if (playingObject.States.TimeToRun <= 0)
                            {
                                playingObject.Finish(this);

                                if (!playingObject.States.Running)
                                {
                                    _playingObjects.Remove(playingObject);
                                }
                            }
                        }
                        else
                        {
                            playingObject.States.StartDelay -= _updateTimerVal;

                            if (playingObject.States.StartDelay <= 0)
                            {
                                playingObject.Start(this);
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

    }

}