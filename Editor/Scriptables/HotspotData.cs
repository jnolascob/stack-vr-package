using Singularis.StackVR.Scriptables.Editor;
using UnityEngine;

namespace Singularis.StackVR.Scriptables.Editor {
    [CreateAssetMenu(fileName = "HotspotData", menuName = "Singularis/Narrative/Hotspot")]
    public class HotspotData : ScriptableObject {
        public int id;
        public new string name;


        public enum HotspotType {
            //Image,
            //Video,
            //Audio,
            //Text,
            //Web,
            //Node
            location,
            custom,
            question
        }
        public HotspotType type;


        public float distance;
        public float angleX;
        public float angleY;
        public float scale = 1f;

        public Texture2D icon;
        public Color color = Color.white;

        public NodeData target;
        public int targetId;
     
    }
}
