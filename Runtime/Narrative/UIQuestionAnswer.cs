using UnityEngine;
using TMPro;

namespace Singularis.StackVR.Narrative {
    public class UIQuestionAnswer : MonoBehaviour {

        [SerializeField]
        private QuestionAnswer answer = null;

        public TextMeshProUGUI txtLabel;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            
        }

        // Update is called once per frame
        void Update() {

        }


        public void SetActive(bool value) {
            gameObject.SetActive(value);
        }

        public void FillData(QuestionAnswer answer) {
            this.answer = answer;
            txtLabel.text = answer.description;
        }

        public void Select() {
            
        }
    }
}
