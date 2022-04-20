using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{

    public class VAGQuest : VAGCustomGameObject
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }

        public int CurrentState
        {
            get
            {
                if (!string.IsNullOrEmpty(GameStates["state"]))
                {
                    return GameStates["state"].AsInt;
                }
                else
                {
                    return 0;
                }
            }

            set
            {
                GameStates["state"].AsInt = value;
            }
        }
        public VAGQuestsCollection Quests;
        public VAGActionsCollection StartActions;
        public VAGActionsCollection EndActions;
        public VAGConditionCollection Conditions;
        public VAGDialogsCollection Dialogs;
        public VAGChapterCollection Chapters;

        public VAGQuest(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore)
        {
            Quests = new VAGQuestsCollection(GetDataObject("Quests"), Store);
            StartActions = new VAGActionsCollection(GetDataObject("StartActions"), ownerStore);
            EndActions = new VAGActionsCollection(GetDataObject("EndActions"), ownerStore);
            Conditions = new VAGConditionCollection(GetDataObject("Conditions"), ownerStore);
            Dialogs = new VAGDialogsCollection(GetDataObject("Dialogs"), ownerStore);
            Chapters = new VAGChapterCollection(GetDataObject("Chapters"), ownerStore);
        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            Quests.LoadFromJSON(GetDataObject("Quests"));
            StartActions.LoadFromJSON(GetDataObject("StartActions"));
            EndActions.LoadFromJSON(GetDataObject("EndActions"));
            Conditions.LoadFromJSON(GetDataObject("Conditions"));
            Dialogs.LoadFromJSON(GetDataObject("Dialogs"));
            Chapters.LoadFromJSON(GetDataObject("Chapters"));
        }
        public override void Clear()
        {
            Quests.Clear();
            StartActions.Clear();
            EndActions.Clear();
            Conditions.Clear();
            Dialogs.Clear();
            Chapters.Clear();
            base.Clear();
        }
        public override void BindToScene(VAGHandler Handler)
        {
            base.BindToScene(Handler);
            Quests.BindToScene(Handler);
            StartActions.BindToScene(Handler);
            EndActions.BindToScene(Handler);
            Conditions.BindToScene(Handler);
            Dialogs.BindToScene(Handler);
            Chapters.BindToScene(Handler);
        }
        public override void AddToDict(Dictionary<string, VAGCustomStorable> Dict, string AttrName)
        {
            base.AddToDict(Dict, AttrName);
            Quests.AddToDict(Dict, AttrName);
            StartActions.AddToDict(Dict, AttrName);
            EndActions.AddToDict(Dict, AttrName);
            Conditions.AddToDict(Dict, AttrName);
            Dialogs.AddToDict(Dict, AttrName);
            Chapters.AddToDict(Dict, AttrName);
        }
        public override void GameStateChanged(VAGHandler Handler)
        {
            base.GameStateChanged(Handler);
            Quests.GameStateChanged(Handler);
            Chapters.GameStateChanged(Handler);
        }

        protected bool StartQuest(VAGHandler Handler)
        {
            if (Conditions.Evaluate())
            {
                StartActions.Execute(Handler);
                Chapters.MoveToNextChapter(Handler);
                CurrentState = 1;
                return true;
            }
            else
            { return false; }
        }

        public void UpdateQuest(VAGHandler Handler)
        {
            switch (CurrentState)
            {
                case 0:
                    StartQuest(Handler);
                    break;

                case 1:
                    if (Chapters.MoveToNextChapter(Handler))
                    {
                        EndActions.Execute(Handler);
                        CurrentState = 2;
                    }
                    break;

                case 2:
                    Quests.UpdateQuests(Handler);
                    break;
            }
        }

    }
    public class VAGQuestsCollection : VAGCustomStorableCollection
    {
        public VAGQuestsCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGQuest(initialData, Store); }

        public VAGQuest Add(string Name = "")
        {
            VAGQuest item = AddNewItem() as VAGQuest;
            item.Name = Name;
            return item;
        }

        public VAGQuest ByName(string Name)
        {
            return (VAGQuest)base.ByName(Name, typeof(VAGQuest));
        }

        public VAGQuest ByIndex(int index)
        {
            return childs[index] as VAGQuest;
        }

        public void UpdateQuests(VAGHandler Handler)
        {
            for (int i = 0; i < childs.Count; i++)
            {
                ByIndex(i).UpdateQuest(Handler);
            }
        }

    }

}