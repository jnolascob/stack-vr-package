using UnityEngine;
using System.Collections.Generic;
using Singularis.StackVR.Editor;

namespace Singularis.StackVR.Scriptables.Editor {
    [CreateAssetMenu(fileName = "HotspotData", menuName = "Singularis/Narrative/Question")]
    public class HotspotQuestionData : HotspotData {
        public List<Answer> answers;
        public string question;
        public int kindOfQuestion;
        public Texture textureElement;
    }

}
