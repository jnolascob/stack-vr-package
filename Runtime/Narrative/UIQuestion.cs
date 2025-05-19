using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Singularis.StackVR.Narrative {
    public class UIQuestion : MonoBehaviour {

        public TextMeshProUGUI txtQuestion;

        [Header("Answers")]
        public UIQuestionAnswer[] answers;

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

        public void FillData(string question, QuestionAnswer[] answers) {
            txtQuestion.text = question;

            for (int i = 0; i < this.answers.Length; i++)
                this.answers[i].SetActive(false);

            for (int i = 0; i < answers.Length; i++) {
                this.answers[i].FillData(answers[i]);
                this.answers[i].SetActive(true);
            }
        }

        public void SelectAnswer(string answer) {

        }

    }
}
