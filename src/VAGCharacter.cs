using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{
    public class VAGCharacter : VAGCustomGameObject
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }
        public string AtomName { get { return GetDataStr("AtomName"); } set { SetDataStr("AtomName", value); } }
        public string FullName { get { return GetDataStr("FullName"); } set { SetDataStr("FullName", value); } }
        public string ShortName { get { return GetDataStr("ShortName"); } set { SetDataStr("ShortName", value); } }
        public string LastName { get { return GetDataStr("LastName"); } set { SetDataStr("LastName", value); } }
        public string FirstName { get { return GetDataStr("FirstName"); } set { SetDataStr("FirstName", value); } }
        public string Bio { get { return GetDataStr("Bio"); } set { SetDataStr("Bio", value); } }
        public string Nationality { get { return GetDataStr("Nationality"); } set { SetDataStr("Nationality", value); } }
        public int Age { get { return GetDataInt("Age"); } set { SetDataInt("Age", value); } }

        public VAGMood ActiveMood { get { return _activeMood; } }
        private VAGMood _activeMood = null;
        public VAGWardrobe ActiveWardrobe { get { return _activeWardrobe; } }
        private VAGWardrobe _activeWardrobe = null;        

        public VAGWardrobeCollection Wardrobe;
        public VAGMoodCollection Moods;

        private Atom _personAtom = null;
        public Atom PersonAtom
        {
            get
            {
                if (!Assigned(_personAtom))
                {
                    _personAtom = SuperController.singleton.GetAtomByUid(AtomName);
                }
                return _personAtom;
            }
        }

        public VAGCharacter(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore)
        {
            Wardrobe = new VAGWardrobeCollection(GetDataObject("Wardrobe"), Store);
            Moods = new VAGMoodCollection(GetDataObject("Moods"), Store);

        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            Wardrobe.LoadFromJSON(GetDataObject("Wardrobe"));
            Moods.LoadFromJSON(GetDataObject("Moods"));
        }
        public override void Clear()
        {                      
            _activeWardrobe = null;
            _activeMood = null;
            Wardrobe.Clear();
            Moods.Clear();
            base.Clear();  
        }
        public override void BindToScene(VAGHandler Handler)
        {
            base.BindToScene(Handler);
            Wardrobe.BindToScene(Handler);
            Moods.BindToScene(Handler);
        }
        public void SpeakSpeechBubble(VAGHandler Handler, string bubbletext, bool isThought, float speaktime)
        {
            if (Assigned(PersonAtom))
            {
                SpeechBubbleControl bubble;

                if (isThought) 
                {
                    bubble = PersonAtom.GetStorableByID("ThoughtBubble") as SpeechBubbleControl;
                } else
                {
                    bubble = PersonAtom.GetStorableByID("SpeechBubble") as SpeechBubbleControl;
                }

                if (Assigned(bubble))
                {
                    //SuperController.LogMessage($"Person {FullName} with AtomName {AtomName} speak {bubbletext} for time {speaktime}"); 
                    bubble.UpdateText(bubbletext, speaktime);
                }
            }
            else
            {
                SuperController.LogError($"Person with AtomName {AtomName} not found for {Name}!");
            }

        }

        public void ChangeWardrobe(VAGHandler Handler, string wardrobeName, bool isRestore = false)
        {
            if (!string.IsNullOrEmpty(wardrobeName) & (isRestore | !wardrobeName.Equals(GameStates["wardrobe"])))
            {
                VAGWardrobe wardrobe = Wardrobe.ByName(wardrobeName);
                if (Assigned(wardrobe))
                {
                    if (!isRestore & Assigned(_activeWardrobe)) {
                        Handler.PlayObject(_activeWardrobe.EndActions);
                    }

                    _activeWardrobe = wardrobe;

                    wardrobe.ApplyToPerson(PersonAtom);                    
                    Handler.PlayObject(wardrobe.StartActions);

                    GameStates["wardrobe"] = wardrobeName;
                }
                else
                {
                    SuperController.LogError($"Wardrobe with name {wardrobeName} not found!");
                }
            }
        }

        public void ChangeMood(VAGHandler Handler, string moodName, bool isRestore = false)
        {
            if (isRestore | !moodName.Equals(GameStates["mood"]))
            {
                VAGMood mood = Moods.ByName(moodName);
                if (Assigned(mood))
                {
                    if (!isRestore & Assigned(_activeMood))
                    {
                        Handler.PlayObject(_activeMood.EndActions);
                    }

                    _activeMood = mood;

                    mood.ApplyToPerson(PersonAtom);
                    Handler.PlayObject(mood.StartActions);

                    GameStates["mood"] = moodName;
                }
                else
                {
                    SuperController.LogError($"Mood with name {moodName} not found!");
                }
            }
        }

        public override void GameStateChanged(VAGHandler Handler)
        {
            base.GameStateChanged(Handler);

            if (!string.IsNullOrEmpty(GameStates["wardrobe"])) { ChangeWardrobe(Handler, GameStates["wardrobe"], true); };
            if (!string.IsNullOrEmpty(GameStates["mood"])) { ChangeMood(Handler, GameStates["mood"], true); };            
        }

    }

    public class VAMACharacterCollection : VAGCustomStorableCollection
    {
        public VAMACharacterCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGCharacter(initialData, Store); }

        public VAGCharacter Add(string Name = "")
        {
            VAGCharacter item = AddNewItem() as VAGCharacter;
            item.Name = Name;
            return item;
        }

        public VAGCharacter ByIndex(int index)
        {
            return childs[index] as VAGCharacter;
        }
        public new VAGCharacter ByName(string Name)
        {
            VAGCharacter Character = base.ByName(Name) as VAGCharacter;

            if (!Assigned(Character) & (childs.Count > 0))
            {
                Character = childs[0] as VAGCharacter;
            }

            return Character;

        }
    }
}