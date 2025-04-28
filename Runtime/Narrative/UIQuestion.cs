using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Singularis.StackVR.Narrative {
    public class UIQuestion : MonoBehaviour {
        [SerializeField]
        private string correctAnswer;

        [Header("Texts")]
        public TextMeshProUGUI txtQuestion;
        public TextMeshProUGUI txtAnswerA;
        public TextMeshProUGUI txtAnswerB;
        public TextMeshProUGUI txtAnswerC;

        [Header("Events")]
        public UnityEvent onCorrectAnswer;
        public UnityEvent onIncorrectAnswer;
        public UnityEvent onOpen;
        public UnityEvent onClose;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            Close();
        }

        // Update is called once per frame
        void Update() {

        }


        public void Open() {
            gameObject.SetActive(true);

            onOpen?.Invoke();
        }

        public void Close() {
            gameObject.SetActive(false);

            onClose?.Invoke();
        }

        public void FillData(string question, string answerA, string answerB, string answerC, string correctAnswer) {
            txtQuestion.text = question;
            txtAnswerA.text = answerA;
            txtAnswerB.text = answerB;
            txtAnswerC.text = answerC;

            this.correctAnswer = correctAnswer;
        }

        public void SelectAnswer(string answer) {
            if (answer == correctAnswer) {
                Debug.Log($"[UIQuestion::SelectAnswer] correct");
                onCorrectAnswer?.Invoke();
            }
            else {
                Debug.Log($"[UIQuestion::SelectAnswer] incorrect");
                onIncorrectAnswer?.Invoke();
            }
        }

    }
}
