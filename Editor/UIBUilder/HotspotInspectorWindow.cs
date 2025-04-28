using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Singularis.StackVR.Scriptables.Editor;

namespace Singularis.StackVR.UIBuilder.Editor {
    public class HotspotInspectorWindow : EditorWindow {

        static HotspotInspectorWindow window;

        static HotspotData hotspotSelected;
        static VisualElement hotspotElement;
        static VisualElement outlinerElement;
        public static VisualElement mainElement;


        [MenuItem("Singularis/Develop/HotspotInspectorWindow")]
        public static void ShowNodeInspector() {
            if (window != null) {
                window.LoadUXML();
                return;
            }

            window = GetWindow<HotspotInspectorWindow>();
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

            visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Singularis/StackVR/Editor/UIBUilder/HotspotInspectorWindow.uxml");
            var container = visualTree.CloneTree();
            root.Add(container);

            Debug.Log($"[HotspotInspectorWindow - OnEnable] {NodeInspectorWindow.hotspotSelected == null}");

            if (NodeInspectorWindow.hotspotSelected == null) {
                root.Q<VisualElement>("default").style.display = DisplayStyle.Flex;
                root.Q<VisualElement>("main").style.display = DisplayStyle.None;

                return;
            }

            root.Q<VisualElement>("default").style.display = DisplayStyle.None;
            root.Q<VisualElement>("main").style.display = DisplayStyle.Flex;



            var slider = root.Q<Slider>(name: "slider");

            slider.RegisterValueChangedCallback(evt => {
                Debug.Log($"[HotspotInspectorWindow - OnEnable] slider: {evt.newValue}");
            });
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


            Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;


            distanceSlider.value = float.Parse(hotspotDataStored["distance"].ToString());


            var hotspotsContainer = hotspotElement.parent;
            angleXSlider.value = Mathf.Lerp(-180f, 180f, Mathf.Clamp01(hotspotElement.resolvedStyle.left / hotspotsContainer.resolvedStyle.width));
            angleYSlider.value = Mathf.Lerp(-80f, 80f, Mathf.Clamp01(hotspotElement.resolvedStyle.top / hotspotsContainer.resolvedStyle.height));

            scaleSlider.value = float.Parse(hotspotDataStored["scale"].ToString());

            nameTextField.value = hotspotDataStored["name"].ToString();
            iconField.value = (Texture2D)hotspotDataStored["icon"];
            colorField.value = (Color)hotspotDataStored["color"];


            targetObjectField.objectType = typeof(NodeData);
            targetObjectField.value = hotspotDataStored["target"] as NodeData;




            // Transform properties
            distanceSlider.RegisterValueChangedCallback(evt => {
                hotspotDataStored["distance"] = evt.newValue;
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
                hotspotDataStored["scale"] = evt.newValue;
                hotspotElement.transform.scale = new Vector3(evt.newValue, evt.newValue);
            });



            // Hotspot properties
            string originalName = hotspotElement.name;
            nameTextField.RegisterValueChangedCallback(evt => {
                hotspotDataStored["name"] = evt.newValue;
                var hotspotOutliner = outlinerElement.Q<VisualElement>(name: originalName);
                hotspotOutliner.Q<Button>().text = evt.newValue;
            });

            iconField.RegisterValueChangedCallback(evt => {
                hotspotDataStored["icon"] = evt.newValue;

                StyleBackground newBackground = new(evt.newValue as Texture2D);
                hotspotElement.style.backgroundImage = newBackground;

                var hotspotOutliner = outlinerElement.Q<VisualElement>(name: originalName);
                hotspotOutliner.Q<VisualElement>("IconElement").style.backgroundImage = newBackground;
            });

            colorField.RegisterValueChangedCallback(evt => {
                hotspotDataStored["color"] = evt.newValue;

                hotspotElement.style.unityBackgroundImageTintColor = evt.newValue;

                var hotspotOutliner = outlinerElement.Q<VisualElement>(name: originalName);
                hotspotOutliner.Q<VisualElement>("IconElement").style.unityBackgroundImageTintColor = evt.newValue;
            });



            // Navigation properties
            targetObjectField.RegisterValueChangedCallback(evt => {
                hotspotDataStored["target"] = evt.newValue as NodeData;
            });


            if (hotspotSelected.type == HotspotData.HotspotType.question) {
                Dictionary<string, object> hotspotData = hotspotElement.userData as Dictionary<string, object>;
                hotspotData["type"] = "question";
                hotspotElement.userData = hotspotData;
                SetDataQuestions();
            }
            else {
                Dictionary<string, object> hotspotData = hotspotElement.userData as Dictionary<string, object>;
                hotspotData["type"] = "location";
                hotspotElement.userData = hotspotData;
            }


        }


        private void SetDataQuestions() {
            var main = root.Q<VisualElement>("main");
            var question = main.Q<TextField>("NameQuestion");
            var firstAnswer = main.Q<TextField>("FirstAnswer");
            var secondAnswer = main.Q<TextField>("SecondAnswer");
            var thirdAnswer = main.Q<TextField>("ThirdAnswer");
            var correctAnswer = main.Q<DropdownField>("CorrectAnswer");

            Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;

            question.value = hotspotDataStored["question"]?.ToString();
            firstAnswer.value = hotspotDataStored["answerA"]?.ToString();
            secondAnswer.value = hotspotDataStored["answerB"]?.ToString();
            thirdAnswer.value = hotspotDataStored["answerC"]?.ToString();
            correctAnswer.value = hotspotDataStored["correctAnswer"]?.ToString();



            question.RegisterValueChangedCallback(evt => {
                hotspotDataStored["question"] = evt.newValue;
            });

            firstAnswer.RegisterValueChangedCallback(evt => {
                hotspotDataStored["answerA"] = evt.newValue;
            });

            secondAnswer.RegisterValueChangedCallback(evt => {
                hotspotDataStored["answerB"] = evt.newValue;
            });

            thirdAnswer.RegisterValueChangedCallback(evt => {
                hotspotDataStored["answerC"] = evt.newValue;
            });

            correctAnswer.RegisterValueChangedCallback(evt => {
                hotspotDataStored["correctAnswer"] = evt.newValue;
            });

        }


    }
}
