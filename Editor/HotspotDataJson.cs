
namespace Singularis.StackVR.Editor {
    [System.Serializable]
    public class HotspotDataJson {
        public int id { get; set; }
        public string name { get; set; }

        public string hostpotType { get; set; }
        public float distance { get; set; }
        public float angleX { get; set; }
        public float angleY { get; set; }
        public float scale { get; set; }
        public string iconPath { get; set; }
        public int[] targets { get; set; }
        public int nodeId { get; set; }

        public string question { get; set; }
        public string answerA { get; set; }
        public string answerB { get; set; }
        public string answerC { get; set; }
        public string correctAnswer { get; set; }
    }
}