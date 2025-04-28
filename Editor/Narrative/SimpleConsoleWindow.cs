using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Singularis.StackVR.Editor;

namespace Singularis.StackVR.Narrative.Editor {
    public class SimpleConsoleWindow : EditorWindow {


        public static bool isImportGraph = false;
        public static bool isEditGraph = false;

        [MenuItem("Singularis/Narrative/New", priority = 1)]
        public static void ShowWindow() {

            string path = EditorUtility.SaveFilePanelInProject("Save narrative", "narrative.yaml", "yaml", "Please enter a file name to save the narrative to");

            if (string.IsNullOrEmpty(path)) {
                Debug.Log("No path selected. Exiting function.");
                return;
            }

            StackProjectConfig.currentNarrative.narrativeSavePath = path;

            ResetNarrative();

            isImportGraph = false;
            isEditGraph = false;
            // Create and show the window
            GetWindow<SimpleConsoleWindow>("Simple Console");
        }

        public static void ShowWindow(bool importGraph = false) {
            isImportGraph = importGraph;
            GetWindow<SimpleConsoleWindow>("Simple Console");
        }

        [MenuItem("Singularis/Narrative/Edit", priority = 2)]
        public static void EditWindow() {
            isImportGraph = false;
            isEditGraph = true;

            GetWindow<SimpleConsoleWindow>("Simple Console");
        }



        [MenuItem("Singularis/Develop/ResetNarrative")]
        static public void ResetNarrative() {
            var currentNarrative = StackProjectConfig.currentNarrative.narrativeScriptableObject;
            currentNarrative.nodes.Clear();

            string folderPath = $"Assets/Singularis/StackVR/Scriptables/";
            try {
                foreach (string file in Directory.GetFiles(folderPath)) {
                    File.Delete(file);
                }

                foreach (string dir in Directory.GetDirectories(folderPath)) {
                    Directory.Delete(dir, true);
                }

            }
            catch (Exception ex) {
                Debug.LogError($"Error: {ex.Message}");
            }

            AssetDatabase.Refresh();
        }



        private void OnEnable() {
            AddGraphView();
        }

        private void AddGraphView() {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Singularis/StackVR/Editor/UIBUilder/TestEditorWindow.uxml");

            if (visualTree == null) {
                Debug.Log("Elemento Nulo");
            }
            TemplateContainer templateContainer = visualTree.CloneTree();
            templateContainer.style.flexGrow = 1;

            // Instantiate UXML
            VisualElement labelFromUXML = templateContainer;
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Singularis/StackVR/Editor/UIBUilder/TestEditorWindow.uss");
            rootVisualElement.styleSheets.Add(styleSheet);
            rootVisualElement.Add(labelFromUXML);
            LoadDocument(templateContainer);
        }

        private void LoadDocument(TemplateContainer templateContainer) {
            var root = templateContainer;
            var graphViewExperience = templateContainer.Q<GraphViewExperiences>();
            var button = root.Q<Button>("TestButton");
            var boxElement = root.Q<VisualElement>("InspectorPanel");
            var dropDownMenu = root.Q<VisualElement>("DropDownMenu");
            var nodeMenu = root.Q<VisualElement>("NodeMenu");
            var buildButton = root.Q<Button>("BuildButton");
            graphViewExperience.SetVisualElements(dropDownMenu, boxElement, nodeMenu, buildButton);



            var toolbarButtons = root.Query<VisualElement>(className: "ButtonToolbar").ToList();
            foreach (var element in toolbarButtons) {
                StyleColor currentColor = new StyleColor(Color.black);
                StyleColor currentTextColor = new StyleColor(Color.black);

                var icon = element.Q<VisualElement>("IconElement");
                if (icon != null) {
                    currentColor = icon.style.unityBackgroundImageTintColor;
                }

                var textElement = element.Q<Label>("TextElement");
                if (textElement != null) {
                    currentTextColor = textElement.style.color;
                }

                element.RegisterCallback<MouseEnterEvent>((e) => {
                    icon.style.unityBackgroundImageTintColor = new StyleColor(Color.black);
                    textElement.style.color = new StyleColor(Color.black);
                }, TrickleDown.TrickleDown);

                element.RegisterCallback<MouseDownEvent>((e) => {
                    icon.style.unityBackgroundImageTintColor = new StyleColor(Color.black);
                    textElement.style.color = new StyleColor(Color.black);
                }, TrickleDown.TrickleDown);

                element.RegisterCallback<MouseUpEvent>((e) => {
                    icon.style.unityBackgroundImageTintColor = new StyleColor(Color.black);
                    textElement.style.color = new StyleColor(Color.black);
                });
                element.RegisterCallback<MouseLeaveEvent>((e) => {
                    icon.style.unityBackgroundImageTintColor = currentColor;
                    textElement.style.color = currentTextColor;
                });

            }
            if (isImportGraph) {
                graphViewExperience.ImportNodes();
            }

            if (isEditGraph) {
                graphViewExperience.EditNodes();
            }
        }



        private void OnGUI() {
            GUILayout.Label("This is a simple console window.", EditorStyles.boldLabel);
            GUILayout.Label("Check the Unity Console for the log message.");
        }
    }
}