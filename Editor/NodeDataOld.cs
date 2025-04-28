using System.Collections.Generic;

namespace Singularis.StackVR.Editor {
    [System.Serializable]
    public class NodeDataOld {
        public int id { get; set; }
        public string type { get; set; }
        public string name { get; set; }

        public int[] input { get; set; }
        public int[] output { get; set; }

        public float xPos { get; set; }
        public float yPos { get; set; }
        public float north;


        public Resource resource { get; set; }
        public List<HotspotDataJson> hotspots { get; set; }
        public bool isSteroscopic { get; set; }
    }
}