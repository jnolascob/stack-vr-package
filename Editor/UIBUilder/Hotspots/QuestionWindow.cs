using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using Singularis.StackVR.Editor;

namespace Singularis.StackVR.Narrative.Editor {
    public class QuestionWindow : HostpotBase {

        public VisualElement outlinerElement;
        public static VisualElement mainElement;
        public Button addElementButton;
        public VisualElement parentQuestions;
        public List<VisualElement> questionElements = new List<VisualElement>();


        public QuestionWindow(VisualElement main, VisualElement hostpotElement) : base(main, hostpotElement) {

        }

        public override void SetCallbacks() {
            addElementButton = main.Q<Button>("AddElement");
            parentQuestions = main.Q<VisualElement>("ParentAnswers");
            questionElements = main.Query<VisualElement>("Answer").ToList();



            int? totalQuestions =  (int?)GetData("totalQuestions");


            var questionBg = main.Q<ObjectField>("QuestionBg");
            questionBg.RegisterValueChangedCallback(value => {
                //Dictionary<string, object> hotspotsData = hotspotElement.userData as Dictionary<string, object>;
                //hotspotsData["TextureQuestion"] = (Texture)value.newValue;
                SaveData("TextureQuestion", value.newValue);
            });


            if (GetData("TextureQuestion") != null) {
                questionBg.value = GetData("TextureQuestion") as Texture;
            }


            if (totalQuestions != null)
            {
               int difQuestions = (int)totalQuestions - questionElements.Count;

                for (int i = 0; i < difQuestions; i++)
                {
                    OnAddAnswer();
                }

            }



            for (int i = 0; i < questionElements.Count; i++) {
                int index = i;

                int numero = i + 1; // por ejemplo
                char letra = (char)('A' + (numero - 1));
                Debug.Log(letra); // Salida: C
                questionElements[i].Q<Label>("Position").text = letra.ToString();
                questionElements[i].Q<Button>("Increase").RegisterCallback<ClickEvent>(e => {
                    questionElements[index].Q<IntegerField>("PointsValue").value++;
                });

                questionElements[i].Q<Button>("Decrease").RegisterCallback<ClickEvent>(e => {
                    questionElements[index].Q<IntegerField>("PointsValue").value--;
                });

                //Dictionary<string, object> hotspotsData = hotspotElement.userData as Dictionary<string, object>;

                if (GetData($"Answer{letra}") != null) {
                    Answer answer = GetData($"Answer{letra}") as Answer;

                    questionElements[i].Q<TextField>("InputQuestion").value = answer.name;
                    questionElements[i].Q<IntegerField>("PointsValue").value = answer.points;
                    questionElements[i].Q<Toggle>("CorrectAnswer").value = answer.isCorrect;
                }


                questionElements[i].Q<Button>("Delete").RegisterCallback<ClickEvent>(e => {
                    questionElements[index].SetEnabled(false);
                    questionElements[index].RemoveFromHierarchy();
                    questionElements.RemoveAt(index);
                });


                questionElements[i].Q<Toggle>("CorrectAnswer").RegisterValueChangedCallback(value => {
                    //Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;
                    int totalPoints = questionElements[index].Q<IntegerField>("PointsValue").value;
                    string nameQuestion = questionElements[index].Q<TextField>("InputQuestion").text;
                    Answer answer = new Answer();

                    answer.name = nameQuestion;
                    answer.points = totalPoints;
                    answer.isCorrect = value.newValue;
                    //hotspotDataStored[$"Answer{letra}"] = answer;
                    //hotspotElement.userData = hotspotDataStored;

                    SaveData($"Answer{letra}", answer);
                });


                questionElements[i].Q<TextField>("InputQuestion").RegisterValueChangedCallback(value => {
                    //Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;
                    int totalPoints = questionElements[index].Q<IntegerField>("PointsValue").value;
                    bool result = questionElements[index].Q<Toggle>("CorrectAnswer").value;
                    Answer answer = new Answer();
                    answer.name = value.newValue;
                    answer.points = totalPoints;
                    answer.isCorrect = result;
                    //hotspotDataStored[$"Answer{letra}"] = answer;
                    //hotspotElement.userData = hotspotDataStored;

                    SaveData($"Answer{letra}", answer);
                });

                questionElements[i].Q<IntegerField>("PointsValue").RegisterValueChangedCallback(value => {
                    //Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;                    
                    Answer answer = new Answer();
                    string nameQuestion = questionElements[index].Q<TextField>("InputQuestion").text;
                    bool result = questionElements[index].Q<Toggle>("CorrectAnswer").value;
                    answer.name = nameQuestion;
                    answer.points = value.newValue;
                    answer.isCorrect = result;
                    //hotspotDataStored[$"Answer{letra}"] = answer;
                    //hotspotElement.userData = hotspotDataStored;
                    SaveData($"Answer{letra}", answer);
                });

            }

            //Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;
            //hotspotDataStored["kindOfQuestion"] = 0;
            SaveData("kindOfQuestion", 0);

            SaveData("type", "question");
            SetDataQuestions();
            addElementButton.RegisterCallback<ClickEvent>(e => { OnAddAnswer(); });
        }


        private void SetDataQuestions() {
            var question = main.Q<TextField>("NameQuestion");
            question.RegisterValueChangedCallback(value => {
                //Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;
                //hotspotDataStored["question"] = value.newValue;
                //hotspotElement.userData = hotspotDataStored;
                SaveData("question", value.newValue);
            });


            Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;

            if (hotspotDataStored.ContainsKey("question")) {
                question.value = hotspotDataStored["question"]?.ToString();
            }

        }


        private void OnAddAnswer() {
            if (questionElements.Count > 5) {
                Debug.Log("YOu get more of 5 questions");

                return;
            }

            VisualTreeAsset answerTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.singularisvr.stackvr/Editor/UIBUilder/DefaultAnswer.uxml");
            VisualElement newAnswer = answerTemplate.Instantiate();

            parentQuestions.Add(newAnswer);
            parentQuestions.MarkDirtyRepaint();

            newAnswer.Q<VisualElement>("Delete").RegisterCallback<ClickEvent>(e => {
                questionElements.Remove(newAnswer);
                newAnswer.RemoveFromHierarchy();
                SaveData("totalQuestions", questionElements.Count);
            });

            questionElements.Add(newAnswer);


            

            int numero = questionElements.Count; // por ejemplo
            SaveData("totalQuestions", numero);
            char letra = (char)('A' + (numero - 1));
            Debug.Log(letra); // Salida: C

            newAnswer.Q<Label>("Position").text = letra.ToString();
            newAnswer.Q<Button>("Increase").RegisterCallback<ClickEvent>(e => {
                newAnswer.Q<IntegerField>("PointsValue").value++;
            });

            newAnswer.Q<Button>("Decrease").RegisterCallback<ClickEvent>(e => {
                newAnswer.Q<IntegerField>("PointsValue").value--;
            });

            newAnswer.Q<Toggle>("CorrectAnswer").RegisterValueChangedCallback(value => {
                //Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;
                int totalPoints = newAnswer.Q<IntegerField>("PointsValue").value;
                string nameQuestion = newAnswer.Q<TextField>("InputQuestion").text;
                Answer answer = new Answer();

                answer.name = nameQuestion;
                answer.points = totalPoints;
                answer.isCorrect = value.newValue;
                //hotspotDataStored[$"Answer{letra}"] = answer;
                //hotspotElement.userData = hotspotDataStored;

                SaveData($"Answer{letra}", answer);
            });


            newAnswer.Q<TextField>("InputQuestion").RegisterValueChangedCallback(value => {
                //Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;
                int totalPoints = newAnswer.Q<IntegerField>("PointsValue").value;
                bool result = newAnswer.Q<Toggle>("CorrectAnswer").value;
                Answer answer = new Answer();
                answer.name = value.newValue;
                answer.points = totalPoints;
                answer.isCorrect = result;

                //hotspotDataStored[$"Answer{letra}"] = answer;
                //hotspotElement.userData = hotspotDataStored;

                SaveData($"Answer{letra}", answer);
            });

            newAnswer.Q<IntegerField>("PointsValue").RegisterValueChangedCallback(value => {
                //Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;
                Answer answer = new Answer();
                string nameQuestion = newAnswer.Q<TextField>("InputQuestion").text;
                bool result = newAnswer.Q<Toggle>("CorrectAnswer").value;
                answer.name = nameQuestion;
                answer.points = value.newValue;
                answer.isCorrect = result;
                //hotspotDataStored[$"Answer{letra}"] = answer;
                //hotspotElement.userData = hotspotDataStored;

                SaveData($"Answer{letra}", answer);
            });


        }

    }
}
