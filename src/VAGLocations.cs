using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{

    public class VAGPlace : VAGCustomGameObject
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }
        public string Description { get { return GetDataStr("Description"); } set { SetDataStr("Description", value); } }
        public string Transition { get { return GetDataStr("Transition"); } set { SetDataStr("Transition", value); } }

        public VAGActionsCollection Actions;
        public VAGAtomSettingsCollection Atoms;
        public VAGLocation Location { get; set; }
        public VAGPlace(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore)
        {
            Actions = new VAGActionsCollection(GetDataObject("Actions"), ownerStore);
            Atoms = new VAGAtomSettingsCollection(GetDataObject("Atoms"), ownerStore);
        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            Actions.LoadFromJSON(GetDataObject("Actions"));
            Atoms.LoadFromJSON(GetDataObject("Atoms"));
        }

        public override void Start(VAGHandler Handler)
        {
            base.Start(Handler);

            SuperController.LogMessage($"Changing place to {Name}"); 

            Handler.Store.GameStates.activePlace = Name;

            VAGAtomSettingsCollection otherPlacesAtoms = new VAGAtomSettingsCollection(null, Store);

            for (int i = 0; i < Location.Places.Count; i++)
            {
                VAGPlace place = Location.Places.ByIndex(i);

                if (!System.Object.ReferenceEquals(place, this))
                {
                    for (int i2 = 0; i2 < place.Atoms.Count; i2++)
                    {
                        VAGAtomSetting atomSetting = place.Atoms.ByIndex(i2);

                        if (atomSetting.OnEnabled & atomSetting.On)
                        {
                            if (Atoms.ByAttrValue("AtomName", atomSetting.AtomName) == null)
                            {
                                VAGAtomSetting otherPlacesAtomsSetting = otherPlacesAtoms.Add(atomSetting.AtomName);
                                otherPlacesAtomsSetting.AtomName = atomSetting.AtomName;
                                otherPlacesAtomsSetting.OnEnabled = true;
                                otherPlacesAtomsSetting.On = false;                                
                            }
                        }
                    }
                }
            }

            //SuperController.LogMessage($"-> disabling atoms count {otherPlacesAtoms.Count} place atoms count {Atoms.Count}");

            VAGTransition transobj = Handler.Store.Transitions.ByName(Transition);
            VAGObject[] questObjs = { otherPlacesAtoms, Atoms, Actions };
            Handler.PlayObject(new VAGChainedQuestObjContext(questObjs, transobj));
        }

    }

    public class VAGPlacesCollection : VAGCustomStorableCollection
    {
        public VAGPlacesCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGPlace(initialData, Store) { Location = this.Location }; }
        public VAGLocation Location { get; set; }

        public VAGPlace Add(string name = "", string description = "")
        {
            VAGPlace item = AddNewItem() as VAGPlace;
            item.Name = name;
            item.Description = description;
            return item;
        }

        public VAGPlace ByIndex(int index)
        {
            return childs[index] as VAGPlace;
        }
        public new VAGPlace ByName(string Name)
        {
            return base.ByName(Name) as VAGPlace;
        }

    }
    public class VAGLocation : VAGCustomGameObject
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }
        public string Description { get { return GetDataStr("Description"); } set { SetDataStr("Description", value); } }
        public string SceneFile { get { return GetDataStr("SceneFile"); } set { SetDataStr("SceneFile", value); } }
        public bool MergeLoad { get { return GetDataBool("MergeLoad"); } set { SetDataBool("MergeLoad", value); } }
        public VAGActionsCollection BeforeLoadActions;
        public VAGActionsCollection AfterLoadActions;
        public VAGPlacesCollection Places;
        public VAGLocation(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore)
        {
            BeforeLoadActions = new VAGActionsCollection(GetDataObject("BeforeLoadActions"), ownerStore);
            AfterLoadActions = new VAGActionsCollection(GetDataObject("AfterLoadActions"), ownerStore);
            Places = new VAGPlacesCollection(GetDataObject("Places"), ownerStore) { Location = this };
        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);
            BeforeLoadActions.LoadFromJSON(GetDataObject("BeforeLoadActions"));
            AfterLoadActions.LoadFromJSON(GetDataObject("AfterLoadActions"));
            Places.LoadFromJSON(GetDataObject("Places"));
        }
        public override void Start(VAGHandler Handler)
        {
            base.Start(Handler);
            Handler.PlayObject(BeforeLoadActions);
            Handler.Store.GameStates.activeLocation = Name;
        }

        public void AfterLoad(VAGHandler Handler)
        {
            Handler.PlayObject(AfterLoadActions);
        }
    }

    public class VAGQuestLocationsCollection : VAGCustomStorableCollection
    {
        public VAGQuestLocationsCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGLocation(initialData, Store); }

        public VAGLocation Add(string name = "", string description = "")
        {
            VAGLocation item = AddNewItem() as VAGLocation;
            item.Name = name;
            item.Description = description;
            return item;
        }

        public VAGLocation ByIndex(int index)
        {
            return childs[index] as VAGLocation;
        }

        public new VAGLocation ByName(string Name)
        {
            return base.ByName(Name) as VAGLocation;
        }

        public VAGPlace PlaceByName(string Name)
        {
            VAGPlace place = null;

            for (int i = 0; i < Count; i++)
            {
                place = ByIndex(i).Places.ByName(Name);

                if (Assigned(place))
                {
                    break;
                }
            }
            return place;
        }

    }
}