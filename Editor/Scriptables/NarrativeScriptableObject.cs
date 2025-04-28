using System.Collections.Generic;
using UnityEngine;

namespace Singularis.StackVR.Scriptables.Editor {
    [CreateAssetMenu(fileName = "NarrativeScriptableObject", menuName = "Singularis/Scriptable Objects/Narrative")]
    public class NarrativeScriptableObject : ScriptableObject {
        public new string name;
        public string date;
        public int version;

        public List<NodeData> nodes;
    }
}
