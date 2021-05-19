using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG {  

    public class VAGDialog : VAGCustomStorableCollection
    {    
        public string Name { get {return GetDataStr("Name"); } set { SetDataStr("Name", value); } }        
        public string Caption { get {return GetDataStr("Caption"); } set { SetDataStr("Caption", value); } } 
        public string DialogText { get {return GetDataStr("DialogText"); } set { SetDataStr("DialogText", value); } }         
        public string SpeakerName { get {return GetDataStr("SpeakerName"); } set { SetDataStr("SpeakerName", value); } }
        public float TextSpeedFactor { get {return GetDataFloat("TextSpeedFactor", 1); } set { SetDataFloat("TextSpeedFactor", value); } }      
         
        public VAGChoicesCollection Choices;
        public VAGActionsCollection Actions;

        public VAGDialog(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) {
            Choices = new VAGChoicesCollection(GetDataObject("Choices"), ownerStore);
            Actions = new VAGActionsCollection(GetDataObject("Actions"), ownerStore);     
         }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGDialog(initialData, Store); }   

        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            Choices.LoadFromJSON(GetDataObject("Choices"));
            Actions.LoadFromJSON(GetDataObject("Actions"));             
        }
        public override void Clear()
        {
            base.Clear();
            Choices.Clear();
            Actions.Clear();   
        }      

        public VAGDialog Add(string Name = "")
        {            
            VAGDialog item = AddNewItem() as VAGDialog;
            item.Name = Name;
            return item;
        }   

        public VAGDialog ByIndex(int index) {
            return childs[index] as VAGDialog;
        }

        public new VAGDialog ByName(string Name) {
            return base.ByName(Name) as VAGDialog;
        }

        public override void Start(VAGHandler Handler) { 
            base.Start(Handler);           

            VAGCharacter Character = Handler.Store.Characters.ByName(SpeakerName);
            if (Assigned(Character)) {
                States.TimeToRun = DialogText.Length * Handler.Store.TextSpeedFactor * TextSpeedFactor;                    
                Character.SpeakSpeechBubble(Handler, DialogText, States.TimeToRun); 
            } else
            {
                SuperController.LogError($"Character with name {SpeakerName} not found!"); 
            };

            if (Choices.Count > 0) { Handler.PlayObject(Choices, States.TimeToRun / 4); }
        }

        public override void Finish(VAGHandler Handler)  {
            base.Finish(Handler);
            
            if (Actions.Count > 0) { Handler.PlayObject(Actions); }                   

            for (int i = 0; i < Count; i++) {
                Handler.PlayDialog(ByIndex(i));
            }
        }        


    }

}
