using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Singularis.StackVR.Narrative {
    public class UIQuestionAnswer : MonoBehaviour {

        [SerializeField]
        private QuestionAnswer answer = null;
        [SerializeField]
        private Color normalColor = Color.white;
        [SerializeField]
        private Color selectedColor = Color.white;

        public TextMeshProUGUI txtLabel;
        public Image bg;


        public bool isCorrect {
            get => answer.isCorrect;
        }

        private bool _isSelected = false;
        public bool isSelected {
            get => _isSelected;
        }

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

        public void SetBGColor(Color color) {
            if (bg != null)
                bg.color = color;
        }

        public void Select(bool value) {
            _isSelected = value;
            SetBGColor(value ? selectedColor : normalColor);
        }
    }
}
