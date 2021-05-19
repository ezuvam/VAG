using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{
    public class VAGAction : VAGCustomGameObject
    {
        public string Name { get { return GetDataStr("Name"); } set { SetDataStr("Name", value); } }
        public string ActionType { get { return GetDataStr("ActionType"); } set { SetDataStr("ActionType", value); } }
        public string ActionParam { get { return GetDataStr("ActionParam"); } set { SetDataStr("ActionParam", value); } }
        public string ActionParamValue { get { return GetDataStr("ActionParamValue"); } set { SetDataStr("ActionParamValue", value); } }
        public string Character { get { return GetDataStr("Character"); } set { SetDataStr("Character", value); } }
        public string AtomName { get { return GetDataStr("AtomName"); } set { SetDataStr("AtomName", value); } }
        public string ReceiverName { get { return GetDataStr("ReceiverName"); } set { SetDataStr("ReceiverName", value); } }
        public string ReceiverTargetName { get { return GetDataStr("ReceiverTargetName"); } set { SetDataStr("ReceiverTargetName", value); } }
        public string Plugin { get { return GetDataStr("Plugin"); } set { SetDataStr("Plugin", value); } }
        public string Method { get { return GetDataStr("Method"); } set { SetDataStr("Method", value); } }
        public string MethodParam { get { return GetDataStr("MethodParam"); } set { SetDataStr("MethodParam", value); } }        
        public float OffsetX { get { return GetDataFloat("OffsetX"); } set { SetDataFloat("OffsetX", value); } }      
        public float OffsetY { get { return GetDataFloat("OffsetY"); } set { SetDataFloat("OffsetY", value); } }      
        public float OffsetZ { get { return GetDataFloat("OffsetZ"); } set { SetDataFloat("OffsetZ", value); } }    
        public float StartDelay { get { return GetDataFloat("StartDelay"); } set { SetDataFloat("StartDelay", value); } }        

        public VAGAction(JSONClass initialData, VAGStore ownerStore) : base(initialData, ownerStore) { }

        public void Execute(VAGHandler Handler)
        {
            //SuperController.LogMessage($"Playing action {ActionType} with param {ActionParam}"); 
            try
            {
                switch (ActionType)
                {
                    case "dialog":
                        {
                            Handler.PlayDialog(ActionParam);
                            break;
                        }

                    case "place":
                        {
                            Handler.ChangePlace(ActionParam);
                            break;
                        }

                    case "wardrobe":
                        {
                            Handler.ChangeWardrobe(Character, ActionParam);
                            break;
                        }

                    case "mood":
                        {
                            Handler.ChangeMood(Character, ActionParam);
                            break;
                        }

                    case "playercamera":
                        {
                            Camera cam = CameraTarget.centerTarget?.targetCamera;
                            Atom atom = GetTargetAtom();
                            JSONStorable js;

                            if (atom?.type == "Person")
                            {
                                js = atom?.GetStorableByID("headControl");
                            }
                            else
                            { js = atom; }

                            if (js != null)
                            {
                                //SuperController.LogMessage($"player cam {cam.name} to {js.transform.position}");

                                cam.transform.position = new Vector3(js.transform.position.x - OffsetX , js.transform.position.y - OffsetY, js.transform.position.z - OffsetZ);
                                cam.transform.LookAt(js.transform);                                

                            }
                            else
                            {
                                LogTargetAtomNullError();
                            }
                            break;
                        }

                    case "trigger":
                        {
                            Atom atom = GetTargetAtom();
                            if (atom != null)
                            {
                                JSONStorable js = FindStorableByNameFlex(atom, ReceiverName);
                                js?.CallAction(ReceiverTargetName);
                            }
                            else
                            {
                                LogTargetAtomNullError();
                            }
                            break;
                        }

                    case "pluginmethod":
                        {
                            /*
                            Atom atom = GetTargetAtom();
                            if (atom != null)
                            {
                                CallPluginMethod(atom, Plugin, Method, MethodParam);
                            }
                            else
                            {
                                LogTargetAtomNullError();
                            }
                            */
                            break;
                        }

                    case "pose":
                        {
                            Atom atom = GetTargetAtom();
                            if (atom != null)
                            {
                                JSONStorable receiver = atom.GetStorableByID("PosePresets");
                                JSONStorableUrl presetBrowsePath = receiver.GetUrlJSONParam("presetBrowsePath");
                                if (presetBrowsePath != null) { presetBrowsePath.val = SuperController.singleton.NormalizePath(ActionParam); }
                                receiver.CallAction("LoadPreset");
                            }
                            else
                            {
                                LogTargetAtomNullError();
                            }
                            break;
                        }

                    case "look":
                        {
                            Atom atom = GetTargetAtom();
                            if (atom != null)
                            {
                                atom.LoadAppearancePreset(ActionParam);
                            }
                            else
                            {
                                LogTargetAtomNullError();
                            }
                            break;
                        }

                    case "preset":
                        {
                            Atom atom = GetTargetAtom();
                            if (atom != null)
                            {
                                JSONStorable receiver = atom.GetStorableByID(ReceiverName); // eg. ClothingPresets
                                JSONStorableUrl presetBrowsePath = receiver.GetUrlJSONParam("presetBrowsePath");
                                if (presetBrowsePath != null) { presetBrowsePath.val = SuperController.singleton.NormalizePath(ActionParam); }
                                receiver.CallAction("LoadPreset");
                            }
                            else
                            {
                                LogTargetAtomNullError();
                            }
                            break;
                        }

                    case "savetostore":
                        {
                            Atom atom = GetTargetAtom();
                            if (atom != null)
                            {
                                switch (ActionParam)
                                {
                                    case "1": { atom.SaveToStore1(); break; }
                                    case "2": { atom.SaveToStore2(); break; }
                                    case "3": { atom.SaveToStore3(); break; }
                                }
                            }
                            else
                            {
                                LogTargetAtomNullError();
                            }
                            break;
                        }

                    case "restorefromstore":
                        {
                            Atom atom = GetTargetAtom();
                            if (atom != null)
                            {
                                switch (ActionParam)
                                {
                                    case "1": { atom.RestoreFromStore1(); break; }
                                    case "2": { atom.RestoreFromStore2(); break; }
                                    case "3": { atom.RestoreFromStore3(); break; }
                                }
                            }
                            else
                            {
                                LogTargetAtomNullError();
                            }
                            break;
                        }

                    case "stringparam":
                        {
                            Atom atom = GetTargetAtom();
                            if (atom != null)
                            {
                                JSONStorable js = FindStorableByNameFlex(atom, ReceiverName);
                                js.SetStringParamValue(ActionParam, ActionParamValue);

                                SuperController.LogMessage($"set stringparam on {atom.name} - {ReceiverName} - {ActionParam} to {ActionParamValue}");
                            }
                            else
                            {
                                LogTargetAtomNullError();
                            }
                            break;
                        }

                    case "boolparam":
                        {
                            Atom atom = GetTargetAtom();
                            if (atom != null)
                            {
                                JSONStorable js = FindStorableByNameFlex(atom, ReceiverName);
                                js.SetBoolParamValue(ActionParam, Boolean.Parse(ActionParamValue));

                                SuperController.LogMessage($"set boolparam on {atom.name} - {ReceiverName} - {ActionParam} to {ActionParamValue}");
                            }
                            else
                            {
                                LogTargetAtomNullError();
                            }
                            break;
                        }

                    case "floatparam":
                        {
                            Atom atom = GetTargetAtom();
                            if (atom != null)
                            {
                                JSONStorable js = FindStorableByNameFlex(atom, ReceiverName);
                                js.SetFloatParamValue(ActionParam, float.Parse(ActionParamValue));

                                SuperController.LogMessage($"set floatparam on {atom.name} - {ReceiverName} - {ActionParam} to {ActionParamValue}");
                            }
                            else
                            {
                                LogTargetAtomNullError();
                            }
                            break;
                        }                        
                }
            } catch  (Exception e)
            {
               SuperController.LogError($"{nameof(VAGPlugin)} VAGAction.Execute {ActionType}: {e}"); 
            }

        }
        public Atom GetTargetAtom()
        {
            //SuperController.LogMessage($"VAGAction {Name}: getting Atom with name {AtomName} or character with name {Character}");

            if (!string.IsNullOrEmpty(Character))
            {
                return Store.Characters.ByName(Character)?.PersonAtom;
            }
            else
            {
                return SuperController.singleton.GetAtomByUid(AtomName);
            }
        }

        public void LogTargetAtomNullError()
        {
            SuperController.LogError($"VAGAction {Name}: Atom with name {AtomName} or character with name {Character} not found!");
        }
        public override void Start(VAGHandler Handler)
        {
            base.Start(Handler);
            Execute(Handler);
            Handler.StopPlayObject(this);
        }

    }

    public class VAGActionsCollection : VAGCustomStorableCollection
    {
        public VAGActionsCollection(JSONClass initialData, VAGStore ownerStore) : base(initialData, "items", ownerStore) { }
        protected override VAGCustomGameObject CreateNewItem(JSONClass initialData) { return new VAGAction(initialData, Store); }

        public VAGAction Add(string Name = "", string actionType = "", string actionParam = "", float startDelay = 0)
        {
            VAGAction item = AddNewItem() as VAGAction;
            item.Name = Name;
            item.ActionType = actionType;
            item.ActionParam = actionParam;
            item.StartDelay = startDelay;
            return item;
        }

        public VAGAction ByIndex(int index)
        {
            return childs[index] as VAGAction;
        }

        public void Execute(VAGHandler Handler)
        {
            for (int i = 0; i < childs.Count; i++)
            {
                (childs[i] as VAGAction).Execute(Handler);
            }
        }
        public override void Start(VAGHandler Handler)
        {
            base.Start(Handler);
            try
            {
                for (int i = 0; i < childs.Count; i++)
                {
                    Handler.PlayObject(childs[i], (childs[i] as VAGAction).StartDelay);
                }
            }
            finally
            {
                Handler.StopPlayObject(this);
            }
        }
    }
}