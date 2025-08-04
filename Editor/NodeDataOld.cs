using System.Collections.Generic;

namespace Singularis.StackVR.Editor {
    [System.Serializable]
    public class NodeDataOld {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }

        public string[] input { get; set; }
        public string[] output { get; set; }

        public float xPos { get; set; }
        public float yPos { get; set; }
        public float north;


        public Resource resource { get; set; }
        public List<HotspotDataJson> hotspots { get; set; }
        public bool isSteroscopic { get; set; }
        public bool isEmpty { get; set; }
    }
}