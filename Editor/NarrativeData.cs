using UnityEngine;
using Singularis.StackVR.Scriptables.Editor;

namespace Singularis.StackVR.Editor {
    public class NarrativeData {
        public string name;
        public string date;
        public int version;


        public NarrativeScriptableObject ToScriptableObject() {
            NarrativeScriptableObject scriptableObject = ScriptableObject.CreateInstance<NarrativeScriptableObject>();
            scriptableObject.name = name;
            scriptableObject.date = date;
            scriptableObject.version = version;

            return scriptableObject;
        }
    }
}
