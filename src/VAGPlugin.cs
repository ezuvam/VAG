using MeshVR;
using MVR.FileManagementSecure;
using MVR.FileManagement;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;

namespace ezuvam.VAG
{
    public class VAGPluginMenu
    {
        private readonly VAGPlugin _plugin;
        private JSONStorableStringChooser _locationChooser;
        private JSONStorableStringChooser _placeChooser;
        private JSONStorableStringChooser _atomChooser;
        private JSONStorableString _gameFileString;
        public string currentSceneGameFileName { get { return _gameFileString?.val;  } }

        const string DefaultGameFileName = "Saves\\scene\\ezuvam\\VAGDemo01\\VAGGameDemo01.vagame";

        public VAGPluginMenu(VAGPlugin plugin)
        {
            _plugin = plugin;
            CreateMenu();
        }

        public UIDynamicTextField CreateTextInput(JSONStorableString jss, bool rightSide = false, InputField.LineType inputFieldType = InputField.LineType.MultiLineNewline)
        {
            UIDynamicTextField textfield = _plugin.CreateTextField(jss, rightSide);
            textfield.height = 1f;
            textfield.backgroundColor = Color.white;
            InputField input = textfield.gameObject.AddComponent<InputField>();
            input.textComponent = textfield.UItext;
            input.lineType = inputFieldType;
            input.textComponent.resizeTextMaxSize = 30;
            input.textComponent.resizeTextForBestFit = true;
            jss.inputField = input;
            return (textfield);
        }
        private void CreateMenu()
        {
            _gameFileString = new JSONStorableString("VAGGameFile", "");
            _plugin.RegisterString(_gameFileString);
            UIDynamicTextField gameFileStringTextField = CreateTextInput(_gameFileString, false, InputField.LineType.SingleLine);

            UIDynamicButton btnSaveGameFile = _plugin.CreateButton("Save game file");
            if (btnSaveGameFile != null)
            {
                btnSaveGameFile.button.onClick.AddListener(doOnSaveGameFileClick);
            }

            UIDynamicButton btnLoadGameFile = _plugin.CreateButton("Load game file");
            if (btnLoadGameFile != null)
            {
                btnLoadGameFile.button.onClick.AddListener(doOnLoadGameFileClick);
            }

            UIDynamicButton btnLoadDefaultGameFile = _plugin.CreateButton("Load default file");
            if (btnLoadDefaultGameFile != null)
            {
                btnLoadDefaultGameFile.button.onClick.AddListener(doOnLoadDefaultGameFileClick);
            }

            UIDynamicButton btnSaveInitialGameStatsFile = _plugin.CreateButton("Save current gamestate to initial");
            if (btnSaveInitialGameStatsFile != null)
            {
                btnSaveInitialGameStatsFile.button.onClick.AddListener(() => { _plugin.QuestHandler.SaveInitialGameStatsFile(); } );
            }            

            UIDynamic spacer = _plugin.CreateSpacer(false);
            //spacer.height = 400f;

            _atomChooser = new JSONStorableStringChooser("ActiveAtom", new List<string>(), "", "Active atom", doOnAtomChanged);
            UIDynamicPopup _atomChooserPopUp = _plugin.CreatePopup(_atomChooser, false);
            //_atomChooserPopUp.labelWidth = 300f;

            _atomChooserPopUp.popup.onOpenPopupHandlers += () =>
                {
                    Atom atom = SuperController.singleton.GetSelectedAtom();

                    if (atom != null)
                    {
                        List<string> list = new List<string>();
                        list.AddRange(_atomChooser.choices);

                        int currentIndex = list.IndexOf(atom.name);

                        if (currentIndex == -1)
                        {
                            list.Insert(0, atom.name);
                            currentIndex = 0;
                        }

                        _atomChooser.choices = list;

                        if (_atomChooser.choices.Count > 0)
                        {
                            _atomChooser.SetVal(_atomChooser.choices[currentIndex]);
                            doOnAtomChanged(_atomChooser.choices[currentIndex]);
                        }
                    }

                };

            _locationChooser = new JSONStorableStringChooser("Locations", new List<string>(), "", "Choose location", doOnLocationChanged);
            UIDynamicPopup locationPopUp = _plugin.CreatePopup(_locationChooser, false);
            //locationPopUp.labelWidth = 300f;
            /*
            locationPopUp.popup.onOpenPopupHandlers += () =>
                {
                    List<string> list = new List<string>();
                    _plugin.QuestHandler.Store.Locations.FillItems(list);
                    _locationChooser.choices = list;

                };
            */

            _placeChooser = new JSONStorableStringChooser("Places", new List<string>(), "", "Choose place", doOnPlaceChanged);
            UIDynamicPopup placePopUp = _plugin.CreatePopup(_placeChooser, false);
            //placePopUp.labelWidth = 300f;

            UIDynamicButton btnAddAtomToPlace = _plugin.CreateButton("Add current atom to current place");
            if (btnAddAtomToPlace != null)
            {
                btnAddAtomToPlace.button.onClick.AddListener(doOnAddAtomToPlaceClick);
            }

            JSONStorableString pluginVersionJSON = new JSONStorableString("PluginVersion", "");
            UIDynamicTextField dtext = _plugin.CreateTextField(pluginVersionJSON, false);
            pluginVersionJSON.val = VAGPlugin.pluginName + " " + VAGPlugin.pluginVersion + "\nby " + VAGPlugin.pluginAuthor;
            dtext.height = 1;

            Init();
        }

        public void Init()
        {
            _plugin.QuestHandler.Store.OnChanged += new VAGObjEvent(doOnStoreChanged);

            if (!String.IsNullOrEmpty(_gameFileString.val))
            {
                _plugin.QuestHandler.LoadStoreFromFile(_gameFileString.val);
            }
        }
        protected void doOnStoreChanged(VAGObject sender)
        {
            //SuperController.LogMessage($"{nameof(VAGPlugin)} store changed");

            _gameFileString.SetVal(_plugin.QuestHandler.ActiveGameFileName);

            List<string> list = new List<string>();
            _plugin.QuestHandler.Store.Locations.FillItems(list);
            _locationChooser.choices = list;

            if (string.IsNullOrEmpty(_locationChooser.val) & (_locationChooser.choices.Count > 0))
            {
                _locationChooser.SetVal(_locationChooser.choices[0]);
                doOnLocationChanged(_locationChooser.choices[0]);
            }
        }

        private void doOnLocationChanged(string locationName)
        {
            VAGLocation location = _plugin.QuestHandler.Store.Locations.ByName(locationName);

            List<string> list = new List<string>();

            if (location != null)
            {
                location.Places.FillItems(list);
            }

            _placeChooser.choices = list;
        }
        private void doOnPlaceChanged(string placeName)
        {              
            _plugin.QuestHandler.ChangePlace(placeName, false);
        }

        private void doOnAtomChanged(string atomName)
        {

        }

        private void doOnSaveGameFileClick()
        {
            if (!String.IsNullOrEmpty(_gameFileString.val))
            {
                _plugin.QuestHandler.SaveStoreToFile(_gameFileString.val);
            }
        }

        private void doOnLoadGameFileClick()
        {
            if (!String.IsNullOrEmpty(_gameFileString.val) & FileManagerSecure.FileExists(_gameFileString.val))
            {
                _plugin.QuestHandler.Store.Clear();
                _plugin.QuestHandler.LoadStoreFromFile(_gameFileString.val);
            }
        }

        private void doOnLoadDefaultGameFileClick()
        {
            _gameFileString.SetVal(DefaultGameFileName);
            _plugin.QuestHandler.Store.Clear();
            _plugin.QuestHandler.LoadStoreFromFile(DefaultGameFileName);            
        }

        private void doOnAddAtomToPlaceClick()
        {
            VAGPlace activePlace = _plugin.QuestHandler.Store.Locations.PlaceByName(_plugin.QuestHandler.Store.GameStates.activePlace);
            if (activePlace != null)
            {
                if (!string.IsNullOrEmpty(_atomChooser.val))
                {
                    VAGAtomSetting atomSetting = (VAGAtomSetting)activePlace.Atoms.ByAttrValue("AtomName", _atomChooser.val);

                    if (atomSetting == null)
                    {
                        atomSetting = activePlace.Atoms.Add(_atomChooser.val);
                        atomSetting.AtomName = _atomChooser.val;
                        atomSetting.PositionEnabled = false;
                        atomSetting.OnEnabled = true;
                    }
                                      
                    atomSetting.LoadFromScene();
                }
            }
        }

    }

    public class VAGPlugin : MVRScript
    {
        public const string pluginName = "VAG";
        public const string pluginAuthor = "ezuvam";
        public const string pluginVersion = "v1.0";
        private VAGHandler _questHandler;
        public VAGHandler QuestHandler { get { return _questHandler; } }
        public VAGPluginMenu pluginMenu;


        public override void Init()
        {   // IMPORTANT - DO NOT make custom enums. The dynamic C# complier crashes Unity when it encounters these for
            // some reason

            // IMPORTANT - DO NOT OVERRIDE Awake() as it is used internally by MVRScript - instead use Init() function which
            // is called right after creation
            try
            {
                //SuperController.LogMessage("VAG Debug: name: " + containingAtom.name + " type: " + containingAtom.type);

                if (containingAtom.name == "CoreControl" && containingAtom.type == "CoreControl")
                {
                    if (_questHandler == null)
                    {
                        _questHandler = new VAGHandler(this);
                    }

                    if (pluginMenu == null)
                    {
                        pluginMenu = new VAGPluginMenu(this);
                    }
                    else
                    {
                        pluginMenu.Init();
                    }
                 
                    _questHandler.Reset();
                    _questHandler.MainMenuUI.AutoPlace();                    

                    SuperController.singleton.onSceneLoadedHandlers += doOnSceneLoaded;
                    doOnSceneLoaded();

                    SuperController.LogMessage($"{nameof(VAGPlugin)} initialized");
                }
                else
                {
                    SuperController.LogError("VAG can only be loaded as a scene plugin");
                }
            }
            catch (Exception e)
            {
                SuperController.LogError($"{nameof(VAGPlugin)}.{nameof(Init)}: {e}");
            }
        }

        public void OnEnable()
        {
            try
            {
                if (_questHandler != null)
                {
                    SuperController.singleton.onSceneLoadedHandlers += doOnSceneLoaded;
                    _questHandler.Active = true;
                }
                //SuperController.LogMessage($"{nameof(VAGPlugin)} enabled");
            }
            catch (Exception e)
            {
                SuperController.LogError($"{nameof(VAGPlugin)}.{nameof(OnEnable)}: {e}");
            }
        }

        public void OnDisable()
        {
            try
            {
                if (_questHandler != null)
                {
                    _questHandler.Active = false;
                    SuperController.singleton.onSceneLoadedHandlers -= doOnSceneLoaded;
                }
                //SuperController.LogMessage($"{nameof(VAGPlugin)} disabled");
            }
            catch (Exception e)
            {
                SuperController.LogError($"{nameof(VAGPlugin)}.{nameof(OnDisable)}: {e}");
            }
        }

        public void OnDestroy()
        {
            // OnDestroy is where you should put any cleanup
            // if you registered objects to supercontroller or atom, you should unregister them here                   
            try
            {
                if (_questHandler != null)
                {
                    _questHandler.OnDestroy();
                    _questHandler = null;
                }
                //SuperController.LogMessage($"{nameof(VAGPlugin)} destroyed");
            }
            catch (Exception e)
            {
                SuperController.LogError($"{nameof(VAGPlugin)}.{nameof(OnDestroy)}: {e}");
            }
        }

        void Start()
        { // Start is called once before Update or FixedUpdate is called and after Init()
            try
            {
                if (!(_questHandler == null))
                {
                    _questHandler.Active = true;

                    //_questHandler.PlayDialog("Intro");
                }
            }
            catch (Exception e)
            {
                SuperController.LogError($"{nameof(VAGPlugin)}.{nameof(Start)}: {e}");
            }
        }

        void Update()
        {   // Update is called with each rendered frame by Unity
            try
            {
                if ((_questHandler != null) & !SuperController.singleton.isLoading)
                {
                    _questHandler.Update();
                }

            }
            catch (Exception e)
            {
                SuperController.LogError($"{nameof(VAGPlugin)}.{nameof(Update)}: {e}");
            }
        }

        void FixedUpdate()
        {  // FixedUpdate is called with each physics simulation frame by Unity
            try
            {

            }
            catch (Exception e)
            {
                SuperController.LogError($"{nameof(VAGPlugin)}.{nameof(FixedUpdate)}: {e}");
            }
        }

        private void doOnSceneLoaded()
        {
            try
            {
                if (!string.IsNullOrEmpty(pluginMenu.currentSceneGameFileName)) {
                    _questHandler.ActiveGameFileName = pluginMenu.currentSceneGameFileName;
                }

                if (!String.IsNullOrEmpty(_questHandler.ActiveGameFileName))
                {
                    _questHandler.Store.Clear();
                    _questHandler.LoadStoreFromFile(_questHandler.ActiveGameFileName);
                }
                else
                {
                    //_questHandler.DebugTest3();
                    //_questHandler.PlayDialog("LivingRoomDialog");
                }
            }
            catch (Exception e)
            {
                SuperController.LogError($"{nameof(VAGPlugin)}.{nameof(doOnSceneLoaded)}: {e}");
            }

        }

        public bool CreatePluginDataFolder()
        {
            bool result = false;

            if (FileManagerSecure.DirectoryExists("Saves\\PluginData"))
            {
                if (!FileManagerSecure.DirectoryExists("Saves\\PluginData\\ezuvam\\VAG"))
                {
                    FileManagerSecure.CreateDirectory("Saves\\PluginData\\ezuvam\\VAG");
                }

                if (FileManagerSecure.DirectoryExists("Saves\\PluginData\\ezuvam\\VAG")) result = true;
            }
            else
            {
                SuperController.LogMessage($"{nameof(VAGPlugin)} this plugin requires the folder Saves\\PluginData to exist.");
            }

            return result;
        }
    }


}
