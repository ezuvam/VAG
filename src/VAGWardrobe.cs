using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{
    public class VAGWardrobe : VAGCustomGameObject
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }
        public string PresetFile { get { return GetDataStr("PresetFile"); } set { SetDataStr("PresetFile", value); } }
        public VAGActionsCollection StartActions;
        public VAGActionsCollection EndActions;
        public VAGWardrobe(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore)
        {
            StartActions = new VAGActionsCollection(GetDataObject("StartActions"), ownerStore);
            EndActions = new VAGActionsCollection(GetDataObject("EndActions"), ownerStore);
        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            StartActions.LoadFromJSON(GetDataObject("StartActions"));
            EndActions.LoadFromJSON(GetDataObject("EndActions"));
        }
        public override void Clear()
        {
            StartActions.Clear();
            EndActions.Clear();
            base.Clear();
        }
        public override void BindToScene(VAGHandler Handler)
        {
            base.BindToScene(Handler);
            StartActions.BindToScene(Handler);
            EndActions.BindToScene(Handler);
        }

        public void ApplyToPerson(Atom personAtom)
        {
            JSONStorable receiver = personAtom.GetStorableByID("ClothingPresets");
            JSONStorableUrl presetBrowsePath = receiver.GetUrlJSONParam("presetBrowsePath");
            if (presetBrowsePath != null) { presetBrowsePath.val = SuperController.singleton.NormalizePath(PresetFile); }
            receiver.CallAction("LoadPreset");
        }

    }

    public class VAGWardrobeCollection : VAGCustomStorableCollection
    {
        public VAGWardrobeCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGWardrobe(initialData, Store); }

        public VAGWardrobe Add(string Name = "")
        {
            VAGWardrobe item = AddNewItem() as VAGWardrobe;
            item.Name = Name;
            return item;
        }

        public VAGWardrobe ByIndex(int index)
        {
            return childs[index] as VAGWardrobe;
        }
        public new VAGWardrobe ByName(string Name)
        {
            return base.ByName(Name) as VAGWardrobe;
        }
    }
}