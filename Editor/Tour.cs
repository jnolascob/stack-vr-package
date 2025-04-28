using Singularis.StackVR.Editor;
using System.Collections.Generic;

namespace Singularis.StackVR.Editor {
    [System.Serializable]
    public class Tour {
        public string name { get; set; }
        public string date { get; set; }
        public int version { get; set; }
        public int start { get; set; }
        public string createAt { get; set; }
        public string updateAt { get; set; }
        public List<NodeDataOld> nodes { get; set; }
    }
}