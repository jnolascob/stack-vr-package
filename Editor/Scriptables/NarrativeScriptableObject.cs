using System.Collections.Generic;
using UnityEngine;
using Singularis.StackVR.Editor;

namespace Singularis.StackVR.Scriptables.Editor {
    [CreateAssetMenu(fileName = "NewNarrative", menuName = "Singularis/StackVR/Narrative")]
    public class NarrativeScriptableObject : ScriptableObject {

        public new string name;
        public string date;
        public int version;
        public string guid;

        public string firstNodeId = "";
        public List<NodeData> nodes = new();


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
