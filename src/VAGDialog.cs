using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{

    public class VAGDialog : VAGCustomStorableCollection
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }
        public string Caption { get { return GetDataStr("Caption"); } set { SetDataStr("Caption", value); } }
        public string DialogText { get { return GetDataStr("DialogText"); } set { SetDataStr("DialogText", value); } }
        public string TextMode { get { return GetDataStr("TextMode"); } set { SetDataStr("TextMode", value); } }
        public string Character { get { return GetDataStr("Character"); } set { SetDataStr("Character", value); } }
        public float TextSpeedFactor { get { return GetDataFloat("TextSpeedFactor", 1); } set { SetDataFloat("TextSpeedFactor", value); } }

        public VAGDialogChoicesCollection Choices;
        public VAGActionsCollection Actions;
        public VAGDialogsCollection Dialogs;

        public VAGDialog(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore)
        {
            Choices = new VAGDialogChoicesCollection(GetDataObject("Choices"), ownerStore);
            Actions = new VAGActionsCollection(GetDataObject("Actions"), ownerStore);
            Dialogs = new VAGDialogsCollection(GetDataObject("Dialogs"), ownerStore);
        }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGDialog(initialData, Store); }

        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            Choices.LoadFromJSON(GetDataObject("Choices"));
            Actions.LoadFromJSON(GetDataObject("Actions"));
            Dialogs.LoadFromJSON(GetDataObject("Dialogs"));
        }
        public override void Clear()
        {
            Choices.Clear();
            Actions.Clear();
            Dialogs.Clear();
            base.Clear();
        }
        public override void BindToScene(VAGHandler Handler)
        {
            base.BindToScene(Handler);
            Choices.BindToScene(Handler);
            Actions.BindToScene(Handler);
            Dialogs.BindToScene(Handler);
        }
        public override void AddToDict(Dictionary<string, VAGCustomStorable> Dict, string AttrName)
        {
            base.AddToDict(Dict, AttrName);
            Choices.AddToDict(Dict, AttrName);
            Actions.AddToDict(Dict, AttrName);
            Dialogs.AddToDict(Dict, AttrName);
        }

        public override void Start(VAGHandler Handler)
        {
            base.Start(Handler);

            VAGCharacter destChar = Handler.Store.Characters.ByName(Character);
            if (Assigned(destChar))
            {
                States.TimeToRun = DialogText.Length * Handler.Store.TextSpeedFactor * TextSpeedFactor;

                destChar.SpeakSpeechBubble(Handler, DialogText, TextMode.Contains("thought"), States.TimeToRun);
            }
            else
            {
                SuperController.LogError($"Character with name {Character} not found!");
            };

            if (Choices.Count > 0) { Handler.PlayObject(Choices, States.TimeToRun / 4); }
        }

        public override void Finish(VAGHandler Handler)
        {
            base.Finish(Handler);

            if (!States.Stopped)
            {
                if (Actions.Count > 0) { Handler.PlayObject(Actions); }
                if (Dialogs.Count > 0) { Handler.PlayObject(Dialogs); }
            }
        }

    }

    public class VAGDialogsCollection : VAGCustomStorableCollection
    {
        public VAGDialogsCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGDialog(initialData, Store); }

        public VAGDialog Add(string Name = "")
        {
            VAGDialog item = AddNewItem() as VAGDialog;
            item.Name = Name;
            return item;
        }

        public VAGDialog ByIndex(int index)
        {
            return childs[index] as VAGDialog;
        }

        public new VAGDialog ByName(string Name)
        {
            return base.ByName(Name) as VAGDialog;
        }

        public override void Start(VAGHandler Handler)
        {
            base.Start(Handler);

            for (int i = 0; i < Count; i++)
            {
                Handler.PlayDialog(ByIndex(i));
            }
        }
    }

}
