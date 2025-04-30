using System.Collections.Generic;
using UnityEngine;
using Singularis.StackVR.Editor;

namespace Singularis.StackVR.Scriptables.Editor {
    [CreateAssetMenu(fileName = "NarrativeScriptableObject", menuName = "Singularis/Scriptable Objects/Narrative")]
    public class NarrativeScriptableObject : ScriptableObject {

        public new string name;
        public string date;
        public int version;

        public List<NodeData> nodes;


        public NarrativeData ToNarrativeData() {
            NarrativeData narrativeData = new() {
                name = name,
                date = date,
                version = version
            };

            return narrativeData;
        }
    }
}
