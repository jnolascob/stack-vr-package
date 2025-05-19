using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Singularis.StackVR.Narrative {
    public class UIQuestionAnswer : MonoBehaviour {

        [SerializeField]
        private QuestionAnswer answer = null;

        public TextMeshProUGUI txtLabel;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            gameObject.SetActive(answer != null);
        }

        // Update is called once per frame
        void Update() {

        }


        public void FillData(QuestionAnswer answer) {
            this.answer = answer;
            txtLabel.text = answer.description;
        }

        public void Select() {
            
        }
    }
}
