using System.Collections.Generic;
using UnityEngine;

namespace Singularis.StackVR.Scriptables.Editor {
    [CreateAssetMenu(fileName = "NodeData", menuName = "Singularis/Narrative/Node")]
    public class NodeData : ScriptableObject {
        public string id;
        public new string name;

        public enum NodeType {
            Image,
            Video,
            //Audio,
            //Text,
            //Web,
            //Node
        }
        public NodeType type;

        public bool isLock;
        public bool isStereo;

        public float posX;
        public float posY;

        public List<NodeData> inputs;
        public List<NodeData> outputs;

        public float north;
        public Texture image;
        public List<HotspotData> hotspots;
        public bool isEmpty;
    }
}
