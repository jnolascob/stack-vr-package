using NUnit.Framework;
using Singularis.StackVR.Scriptables.Editor;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Singularis.StackVR.Scriptables.Editor
{
    [CreateAssetMenu(fileName = "HotspotData", menuName = "Singularis/Narrative/Question")]
    public class HotspotQuestionData : HotspotData
    {
        public List<Answer> answers;
        public string question;
        public int kindOfQuestion;
        public Texture textureElement;

    }

}


 
