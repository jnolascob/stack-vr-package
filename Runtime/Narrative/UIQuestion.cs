using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace Singularis.StackVR.Narrative {
    public class UIQuestion : MonoBehaviour {

        [SerializeField]
        private List<int> correctAnswersIndexes;
        private int correctAnswers = 0;


        public TextMeshProUGUI txtQuestion;
        public Image imgQuestion;

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

        public void FillData(string question, QuestionAnswer[] answers, Texture2D image) {
            txtQuestion.text = question;

            if (image != null) {
                imgQuestion.gameObject.SetActive(true);
                imgQuestion.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
            else {
                imgQuestion.gameObject.SetActive(false);
            }

            for (int i = 0; i < this.answers.Length; i++)
                this.answers[i].SetActive(false);

            correctAnswersIndexes = new List<int>();
            for (int i = 0; i < answers.Length; i++) {
                if (answers[i].isCorrect) {
                    correctAnswersIndexes.Add(i);
                }

                this.answers[i].FillData(answers[i]);
                this.answers[i].SetActive(true);
            }
        }

        public void CheckAnswers() {
            correctAnswers = 0;
            for (int i = 0; i < answers.Length; i++) {
                //answers[i].Select(false);
                //answers[i].SetBGColor(Color.white);

                if (answers[i].isSelected) {
                    if (answers[i].isCorrect) {
                        correctAnswers++;
                        answers[i].SetBGColor(Color.green);
                    }
                    else {
                        answers[i].SetBGColor(Color.red);
                    }
                }
                //else {
                //    answers[i].SetBGColor(Color.white);
                //}
            }


            //correctAnswers = 0;
            //correctAnswersIndexes.ForEach(i => {
            //    if (answers[i].isCorrect) {
            //        correctAnswers++;
            //        //answers[i].SetBGColor(Color.green);
            //    }
            //    //else {
            //    //    answers[i].SetBGColor(Color.red);
            //    //}
            //});
        }

        public void Continue() {
            if (correctAnswers == correctAnswersIndexes.Count)
                onCorrectAnswer?.Invoke();
            else
                onIncorrectAnswer?.Invoke();
        }

    }
}
