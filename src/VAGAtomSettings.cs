using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{
    public class VAGAtomPhysicsRestorer : VAGObject
    {
        private readonly Atom _atom;
        public VAGAtomPhysicsRestorer(Atom atom)
        {
            _atom = atom;
        }
        public override void Start(VAGHandler Handler)
        {
            base.Start(Handler);
            _atom.collisionEnabled = true;
            //_atom.SetFreezePhysics(false);


        }
    }
    public class VAGAtomSetting : VAGCustomGameObject
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }
        public string Description { get { return GetDataStr("Description"); } set { SetDataStr("Description", value); } }
        public string AtomName { get { return GetDataStr("AtomName"); } set { SetDataStr("AtomName", value); } }
        public bool OnEnabled { get { return GetDataBool("OnEnabled"); } set { SetDataBool("OnEnabled", value); } }
        public bool On { get { return GetDataBool("On"); } set { SetDataBool("On", value); } }
        public bool PositionEnabled { get { return GetDataBool("PositionEnabled"); } set { SetDataBool("PositionEnabled", value); } }
        public float X { get { return GetDataFloat("X"); } set { SetDataFloat("X", value); } }
        public float Y { get { return GetDataFloat("Y"); } set { SetDataFloat("Y", value); } }
        public float Z { get { return GetDataFloat("Z"); } set { SetDataFloat("Z", value); } }
        public float RtX { get { return GetDataFloat("RtX"); } set { SetDataFloat("RtX", value); } }
        public float RtY { get { return GetDataFloat("RtY"); } set { SetDataFloat("RtY", value); } }
        public float RtZ { get { return GetDataFloat("RtZ"); } set { SetDataFloat("RtZ", value); } }
        public float RtW { get { return GetDataFloat("RtW"); } set { SetDataFloat("RtW", value); } }

        public VAGAtomSetting(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore)
        {

        }
        public override void LoadFromJSON(JSONClass jsonData)
        {
            base.LoadFromJSON(jsonData);

        }

        public override void Start(VAGHandler Handler)
        {
            base.Start(Handler);
            ApplyToScene(Handler);
        }

        public void ApplyToScene(VAGHandler Handler)
        {
            Atom atom = SuperController.singleton.GetAtomByUid(AtomName);

            if (atom != null)
            {
                //SuperController.LogMessage($"Atom settings to {AtomName}");

                if (PositionEnabled)
                {
                    FreeControllerV3 fc = (FreeControllerV3)atom.GetStorableByID("control");

                    if (Assigned(fc))
                    {
                        atom.collisionEnabled = false;
                        //atom.SetFreezePhysics(true); // person is not moving if physics is freezed :(
                        fc.MoveControl(new Vector3(X, Y, Z));                        
                        fc.transform.SetPositionAndRotation(new Vector3(X, Y, Z), new Quaternion(RtX, RtY, RtZ, RtW));

                        float transistionTime;
                        if (Assigned(Handler.ActiveTransition)) { transistionTime = Handler.ActiveTransition.Delay; } else { transistionTime = 4; }
                        Handler.PlayObject(new VAGAtomPhysicsRestorer(atom), transistionTime); // delay to give some time to move                     
                    }
                }

                if (OnEnabled)
                {
                    atom.SetOn(On);
                    //SuperController.LogMessage($"-> Atom {AtomName} set on {Active}");
                }                
            }
            else
            {
                SuperController.LogError($"VAGAtomSetting.ApplyToScene: Atom with name {AtomName} not found!");
            }
        }

        public void LoadFromScene(VAGAtomSetting defAtomSetting = null)
        {
            if (Assigned(defAtomSetting))
            {
                PositionEnabled = defAtomSetting.PositionEnabled;
                OnEnabled = defAtomSetting.OnEnabled;
            }

            Atom atom = SuperController.singleton.GetAtomByUid(AtomName);

            if (atom != null)
            {
                //SuperController.LogMessage($"Loading settings for atom {AtomName} from scene");

                if (PositionEnabled)
                {
                    FreeControllerV3 fc = (FreeControllerV3)atom.GetStorableByID("control");

                    if (Assigned(fc))
                    {
                        X = fc.transform.position.x;
                        Y = fc.transform.position.y;
                        Z = fc.transform.position.z;

                        RtX = fc.transform.rotation.x;
                        RtY = fc.transform.rotation.y;
                        RtZ = fc.transform.rotation.z;
                        RtW = fc.transform.rotation.w;
                    }
                    else
                    {
                        PositionEnabled = false;
                    }

                }

                if (OnEnabled)
                {
                    On = atom.on;
                    //SuperController.LogMessage($"-> Atom {AtomName} on is {Active}");
                }
            }
            else
            {
                SuperController.LogError($"Atom with name {AtomName} not found!");
            }
        }

    }

    public class VAGAtomSettingsCollection : VAGCustomStorableCollection
    {
        public VAGAtomSettingsCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGAtomSetting(initialData, Store); }

        public VAGAtomSetting Add(string name = "", string description = "")
        {
            VAGAtomSetting item = AddNewItem() as VAGAtomSetting;
            item.Name = name;
            item.Description = description;
            return item;
        }

        public VAGAtomSetting ByIndex(int index)
        {
            return childs[index] as VAGAtomSetting;
        }
        public VAGAtomSetting ByName(string Name)
        {
            return (VAGAtomSetting)base.ByName(Name, typeof(VAGAtomSetting));
        }
        public void ApplyToAtoms(VAGHandler Handler)
        {
            for (int i = 0; i < Count; i++)
            {
                Handler.PlayObject(childs[i]);
            }
        }

        public void LoadFromAtoms(VAGAtomSettingsCollection atomsDef = null)
        {
            if (!Assigned(atomsDef)) { atomsDef = this; }

            // load stats from scene atoms, where atomsDef are the relevant atoms
            for (int i = 0; i < atomsDef.Count; i++)
            {
                VAGAtomSetting def = atomsDef.ByIndex(i);
                VAGAtomSetting dest = Add(def.Name);
                dest.AtomName = def.AtomName;

                dest.LoadFromScene(def);
            }
        }

        public override void Start(VAGHandler Handler)
        {
            base.Start(Handler);
            ApplyToAtoms(Handler);
        }

    }
}