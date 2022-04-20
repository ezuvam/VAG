using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{
    public class VAGChapter : VAGCustomGameObject
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }
        public string Title { get { return GetDataStr("Title"); } set { SetDataStr("Title", value); } }
        public string Description { get { return GetDataStr("Description"); } set { SetDataStr("Description", value); } }
        public int Level { get { return GetDataInt("Level"); } set { SetDataInt("Level", value); } }
        public VAGActionsCollection StartActions;
        public VAGActionsCollection EndActions;
        public VAGConditionCollection FinishConditions;

        public VAGChapter(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore)
        {
            StartActions = new VAGActionsCollection(GetDataObject("StartActions"), ownerStore);
            EndActions = new VAGActionsCollection(GetDataObject("EndActions"), ownerStore);
            FinishConditions = new VAGConditionCollection(GetDataObject("FinishConditions"), ownerStore);
        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            StartActions.LoadFromJSON(GetDataObject("StartActions"));
            EndActions.LoadFromJSON(GetDataObject("EndActions"));
            FinishConditions.LoadFromJSON(GetDataObject("FinishConditions"));

        }
        public override void Clear()
        {
            StartActions.Clear();
            EndActions.Clear();
            FinishConditions.Clear();
            base.Clear();
        }
        public override void BindToScene(VAGHandler Handler)
        {
            base.BindToScene(Handler);
            StartActions.BindToScene(Handler);
            EndActions.BindToScene(Handler);
            FinishConditions.BindToScene(Handler);

        }
        public override void AddToDict(Dictionary<string, VAGCustomStorable> Dict, string AttrName)
        {
            base.AddToDict(Dict, AttrName);
            StartActions.AddToDict(Dict, AttrName);
            EndActions.AddToDict(Dict, AttrName);
            FinishConditions.AddToDict(Dict, AttrName);
        }

    }

    public class VAGChapterCollection : VAGCustomStorableCollection
    {
        public VAGChapterCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGChapter(initialData, Store); }
        public VAGChapter ActiveChapter { get { return _activeChapter; } }
        private VAGChapter _activeChapter = null;
        public VAGChapter Add(string Name = "")
        {
            VAGChapter item = AddNewItem() as VAGChapter;
            item.Name = Name;
            return item;
        }

        public VAGChapter ByIndex(int index)
        {
            return childs[index] as VAGChapter;
        }
        public VAGChapter ByName(string Name)
        {
            return (VAGChapter)base.ByName(Name, typeof(VAGChapter));
        }

        public bool ChangeChapter(VAGHandler Handler, int chapterIndex, bool isRestore = false)
        {
            if ((chapterIndex > -1) & (isRestore | !chapterIndex.Equals(GameStates["currentchapter"])))
            {
                VAGChapter chapter = ByIndex(chapterIndex);
                if (Assigned(chapter))
                {
                    if (!isRestore & Assigned(ActiveChapter))
                    {
                        Handler.PlayObject(ActiveChapter.EndActions);
                    }

                    _activeChapter = chapter;

                    Handler.PlayObject(chapter.StartActions);

                    if (!isRestore)
                    {
                        GameStates["currentchapter"].AsInt = chapterIndex;
                    }
                    return true;
                }
                else
                {
                    SuperController.LogError($"Chapter with index {chapterIndex} not found!");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool MoveToNextChapter(VAGHandler Handler)
        {
            if (!Assigned(ActiveChapter))
            {
                if (ChangeChapter(Handler, 0))
                {
                    return MoveToNextChapter(Handler);
                }
                else
                {
                    return false;
                }
            }
            else
            if (ActiveChapter.FinishConditions.Evaluate())
            {
                int NextIndex = childs.IndexOf(ActiveChapter) + 1;
                if (NextIndex < Count)
                {
                    if (ChangeChapter(Handler, NextIndex))
                    {
                        return MoveToNextChapter(Handler);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }

        }

        public override void GameStateChanged(VAGHandler Handler)
        {
            base.GameStateChanged(Handler);

            if (!string.IsNullOrEmpty(GameStates["currentchapter"])) { ChangeChapter(Handler, GameStates["currentchapter"].AsInt, true); };
        }
    }
}