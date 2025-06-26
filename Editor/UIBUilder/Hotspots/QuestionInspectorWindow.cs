using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Singularis.StackVR.Scriptables.Editor;
using Singularis.StackVR.UIBuilder.Editor;
using Singularis.StackVR.Editor;


namespace Singularis.StackVR.Narrative.Editor {
    public class QuestionInspectorWindow : EditorWindow {

        static QuestionInspectorWindow window;

        static HotspotData hotspotSelected;
        static VisualElement hotspotElement;
        static VisualElement outlinerElement;
        public static VisualElement mainElement;
        public Button addElementButton;
        public VisualElement parentQuestions;
        public List<VisualElement> questionElements = new List<VisualElement>();
        public RadioButtonGroup correctAnswer;



        [MenuItem("Singularis/Develop/HotspotInspectorWindow")]
        public static void ShowNodeInspector() {
            if (window != null) {
                window.LoadUXML();
                return;
            }


            window = GetWindow<QuestionInspectorWindow>();
            window.titleContent = new GUIContent("HotspotInspectorWindow");

            mainElement = window.rootVisualElement.Q<VisualElement>("main");
        }


        public static void RepaintWindow() {
            window?.LoadUXML();
        }

        public static void FillData(HotspotData hotspot) {
            hotspotSelected = hotspot;
            window.FillData();
        }

        public static void FillData(VisualElement hotspot, HotspotData data) {
            hotspotElement = hotspot;
            hotspotSelected = data;


            var questions = mainElement.Q<VisualElement>("Questions");

            if (data.type == HotspotData.HotspotType.question) {
                Debug.Log("Filling Data" + data.type);
                questions.style.display = DisplayStyle.Flex;
                mainElement.style.height = 811f;

            }
            else {
                Debug.Log("Filling Data" + data.type);
                questions.style.display = DisplayStyle.None;
                mainElement.style.height = 450f;
            }
            questions.MarkDirtyRepaint();
            mainElement.MarkDirtyRepaint();

            window.FillData();
        }

        public static void SetOutlinerElement(VisualElement element) {
            outlinerElement = element;
        }


        private VisualElement root = default;
        private VisualTreeAsset visualTree = default;

        private void OnEnable() {
            Debug.Log("[HotspotInspectorWindow - OnEnable]");

            LoadUXML();
        }

        private void CreateGUI() {
            Debug.Log("[HotspotInspectorWindow - CreateGUI]");
        }


        private void LoadUXML() {
            root = rootVisualElement;
            root.Clear();

            visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.singularisvr.stackvr/Editor/UIBUilder/QuestionWindow.uxml");
            var container = visualTree.CloneTree();
            root.Add(container);

            Debug.Log($"[HotspotInspectorWindow - OnEnable] {NodeInspectorWindow.hotspotSelected == null}");


            var slider = root.Q<Slider>(name: "slider");

            slider.RegisterValueChangedCallback(evt => {
                Debug.Log($"[HotspotInspectorWindow - OnEnable] slider: {evt.newValue}");
            });
        }



        public void SaveData(string key, object value) {
            Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;

            hotspotDataStored[key] = value;
            hotspotElement.userData = hotspotDataStored;
        }


        public object GetData(string key) {
            Dictionary<string, object> hotspotsDataStore = hotspotElement.userData as Dictionary<string, object>;

            if (hotspotsDataStore.ContainsKey(key)) {
                object result = hotspotsDataStore[key];

                return result;
            }
            else {
                return null;
            }
        }


        private void FillData() {
            var main = root.Q<VisualElement>("main");

            var nameTextField = main.Q<TextField>("nameTextField");
            var distanceSlider = main.Q<Slider>("distanceSlider");
            var angleXSlider = main.Q<Slider>("angleXSlider");
            var angleYSlider = main.Q<Slider>("angleYSlider");
            var scaleSlider = main.Q<Slider>("scaleSlider");
            var iconField = main.Q<ObjectField>("iconObjectField");
            var colorField = main.Q<ColorField>("colorField");
            var targetObjectField = main.Q<ObjectField>("targetObjectField");
            addElementButton = main.Q<Button>("AddElement");
            parentQuestions = main.Q<VisualElement>("ParentAnswers");
            questionElements = main.Query<VisualElement>("Answer").ToList();
            correctAnswer = main.Q<RadioButtonGroup>("KindOfAnswer");

            var questionBg = main.Q<ObjectField>("QuestionBg");


            questionBg.RegisterValueChangedCallback(value => {
                //Dictionary<string, object> hotspotsData = hotspotElement.userData as Dictionary<string, object>;
                //hotspotsData["TextureQuestion"] = (Texture)value.newValue;
                SaveData("TextureQuestion", value.newValue);
            });



            if (GetData("TextureQuestion") != null) {
                questionBg.value = GetData("TextureQuestion") as Texture;
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

            if (GetData("kindOfQuestion") != null) {
                correctAnswer.value = (int)GetData("kindOfQuestion");
            }

            correctAnswer.RegisterValueChangedCallback(result => {
                int newValue = result.newValue;
                Debug.Log("The Value is" + newValue);
                //Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;
                //hotspotDataStored["kindOfQuestion"] = newValue;
                //hotspotElement.userData = hotspotDataStored;

                SaveData("kindOfQuestion", newValue);
            });


            //Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;
            //hotspotDataStored["kindOfQuestion"] = 0;

            SaveData("kindOfQuestion", 0);
            distanceSlider.value = float.Parse(GetData("distance").ToString());

            var hotspotsContainer = hotspotElement.parent;
            angleXSlider.value = Mathf.Lerp(-180f, 180f, Mathf.Clamp01(hotspotElement.resolvedStyle.left / hotspotsContainer.resolvedStyle.width));
            angleYSlider.value = Mathf.Lerp(-80f, 80f, Mathf.Clamp01(hotspotElement.resolvedStyle.top / hotspotsContainer.resolvedStyle.height));


            scaleSlider.value = float.Parse(GetData("scale").ToString());

            nameTextField.value = GetData("name").ToString();
            iconField.value = (Texture2D)GetData("icon");
            colorField.value = (Color)GetData("color");


            targetObjectField.objectType = typeof(NodeData);
            targetObjectField.value = GetData("target") as NodeData;


            // Transform properties
            distanceSlider.RegisterValueChangedCallback(evt => {
                SaveData("distance", evt.newValue);
                //hotspotDataStored["distance"] = evt.newValue;
            });

            angleXSlider.RegisterValueChangedCallback(evt => {
                float left = Mathf.InverseLerp(-180f, 180f, evt.newValue);
                hotspotElement.style.left = Length.Percent(left * 100);
            });

            angleYSlider.RegisterValueChangedCallback(evt => {
                float top = Mathf.InverseLerp(-80f, 80f, evt.newValue);
                hotspotElement.style.top = Length.Percent(top * 100);
            });

            scaleSlider.RegisterValueChangedCallback(evt => {
                //hotspotDataStored["scale"] = evt.newValue;
                SaveData("scale", evt.newValue);
                hotspotElement.transform.scale = new Vector3(evt.newValue, evt.newValue);
            });



            // Hotspot properties
            string originalName = hotspotElement.name;


            nameTextField.RegisterValueChangedCallback(evt => {
                Debug.Log("The Name is " + originalName);
                if (outlinerElement == null) {
                    Debug.Log("The Outliner es nulo");
                }
                //hotspotDataStored["name"] = evt.newValue;
                SaveData("name", evt.newValue);
                var hotspotOutliner = outlinerElement.Q<VisualElement>(name: originalName);
                hotspotOutliner.Q<Button>().text = evt.newValue;
            });

            iconField.RegisterValueChangedCallback(evt => {
                //hotspotDataStored["icon"] = evt.newValue;
                SaveData("icon", evt.newValue);

                StyleBackground newBackground = new(evt.newValue as Texture2D);
                hotspotElement.style.backgroundImage = newBackground;

                var hotspotOutliner = outlinerElement.Q<VisualElement>(name: originalName);
                hotspotOutliner.Q<VisualElement>("IconElement").style.backgroundImage = newBackground;
            });

            colorField.RegisterValueChangedCallback(evt => {
                //hotspotDataStored["color"] = evt.newValue;
                SaveData("color", evt.newValue);

                hotspotElement.style.unityBackgroundImageTintColor = evt.newValue;

                var hotspotOutliner = outlinerElement.Q<VisualElement>(name: originalName);
                hotspotOutliner.Q<VisualElement>("IconElement").style.unityBackgroundImageTintColor = evt.newValue;
            });


            // Navigation properties
            targetObjectField.RegisterValueChangedCallback(evt => {
                //hotspotDataStored["target"] = evt.newValue as NodeData;
                SaveData("target", evt.newValue);
            });

            //Dictionary<string, object> hotspotData = hotspotElement.userData as Dictionary<string, object>;
            //hotspotData["type"] = "question";
            //hotspotElement.userData = hotspotData;
            SaveData("type", "question");
            SetDataQuestions();


            RegisterCallbacks();
        }


        private void RegisterCallbacks() {
            addElementButton.RegisterCallback<ClickEvent>(e => { OnAddAnswer(); });
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
            });

            questionElements.Add(newAnswer);


            int numero = questionElements.Count; // por ejemplo
            char letra = (char)('A' + (numero - 1));
            Debug.Log(letra); // Salida: C

            newAnswer.Q<Label>("Position").text = letra.ToString();
            newAnswer.Q<Button>("Increase").RegisterCallback<ClickEvent>(e => {
                newAnswer.Q<IntegerField>("PointsValue").value++;
            });

            newAnswer.Q<Button>("Decrease").RegisterCallback<ClickEvent>(e => {
                newAnswer.Q<IntegerField>("PointsValue").value--;
            });


            newAnswer.Q<TextField>("InputQuestion").RegisterValueChangedCallback(value => {
                //Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;
                int totalPoints = newAnswer.Q<IntegerField>("PointsValue").value;
                Answer answer = new Answer();
                answer.name = value.newValue;
                answer.points = totalPoints;
                //hotspotDataStored[$"Answer{letra}"] = answer;
                //hotspotElement.userData = hotspotDataStored;

                SaveData($"Answer{letra}", answer);
            });

            newAnswer.Q<IntegerField>("PointsValue").RegisterValueChangedCallback(value => {
                //Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;
                Answer answer = new Answer();
                string nameQuestion = newAnswer.Q<TextField>("InputQuestion").text;
                answer.name = nameQuestion;
                answer.points = value.newValue;
                //hotspotDataStored[$"Answer{letra}"] = answer;
                //hotspotElement.userData = hotspotDataStored;

                SaveData($"Answer{letra}", answer);

            });


        }



        private void SetDataQuestions() {
            var main = root.Q<VisualElement>("main");
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


        private void CreateAnswerElement(VisualElement parent) {


        }


    }
}
