using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Singularis.StackVR.Editor;
using Singularis.StackVR.Scriptables.Editor;
using UnityEditor.Build.Reporting;
using Codice.Utils;

namespace Singularis.StackVR.Narrative.Editor {
    public class SimpleConsoleWindow : EditorWindow {


        private static bool isImportGraph = false;
        private static bool isEditGraph = false;
        private static string narrativePath = string.Empty;


        [MenuItem("Singularis/Narrative/New", priority = 1)]
        public static void ShowWindow() {

            string path = EditorUtility.SaveFilePanelInProject("Save narrative", "new narrative", "narrative", "Please enter a file name to save the narrative");

            if (string.IsNullOrEmpty(path)) {
                Debug.Log("No path selected. Exiting function.");
                return;
            }


            Debug.Log($"Path: {path}");
            narrativePath = path + ".asset";
            Debug.Log($"Narrative Path: {narrativePath}");

            // Create a new instance of the NarrativeScriptableObject
            NarrativeScriptableObject narrative = ScriptableObject.CreateInstance<NarrativeScriptableObject>();
            // Set the name of the scriptable object to the file name
            narrative.name = Path.GetFileNameWithoutExtension(path);

            
            // Save the scriptable object to the specified path
            AssetDatabase.CreateAsset(narrative, narrativePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            narrative.guid = GetGuidOfObject(narrative);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string currentGuid = narrative.guid;

            string folderPath = "Assets/Narratives/" + narrative.guid;
            AssetDatabase.CreateFolder("Assets/Narratives", narrative.guid);
            string newPath = folderPath + "/" + narrative.name + ".asset";
            AssetDatabase.MoveAsset(narrativePath, newPath);
            narrative.guid = currentGuid;
            narrativePath = newPath;
            EditorUtility.SetDirty(narrative);  

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
          

            //StackProjectConfig.currentNarrative.narrativeSavePath = path;
            //StackProjectConfig.currentNarrative.narrativeDirectoryPath = Path.GetDirectoryName(path);

            //ResetNarrative();

            isImportGraph = false;
            isEditGraph = false;
            // TODO Create and show the window
            GetWindow<SimpleConsoleWindow>("Simple Console");
        }




        public static string GetGuidOfObject(UnityEngine.Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            return AssetDatabase.AssetPathToGUID(path);
        }






        public static void ShowWindow(bool importGraph = false) {
            isImportGraph = importGraph;
            GetWindow<SimpleConsoleWindow>("Simple Console");
        }

        [MenuItem("Singularis/Narrative/Open", priority = 2)]
        public static void EditWindow() {

            string path = EditorUtility.OpenFilePanel("Open narrative", "Assets/", "narrative.asset");
            narrativePath = "Assets" + path[Application.dataPath.Length..];


            isImportGraph = false;
            isEditGraph = true;

            GetWindow<SimpleConsoleWindow>("Simple Console");
        }


        [MenuItem("Singularis/Narrative/Build", priority = 3)]
        public static void BuildScene()
        {

            Debug.Log("Try To Build Scene");

            // Ruta de salida
            // Mostrar diálogo para seleccionar ruta de guardado
            string buildPath = EditorUtility.SaveFilePanel(
                "Guardar APK",
                "",
                "MiEscena.apk",
                "apk"
            );

            if (string.IsNullOrEmpty(buildPath))
            {
                Debug.Log("?? Build cancelado por el usuario.");
                return;
            }

            // Obtener la escena activa
            string scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;

            if (string.IsNullOrEmpty(scenePath))
            {
                EditorUtility.DisplayDialog(
                   "Not Active Scene",                      // Título
                    "Please save the scene before building", // Mensaje
                   "OK"                             // Botón
                  );
                return;
            }

            // Crear carpeta si no existe
            string dir = Path.GetDirectoryName(buildPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Configurar opciones de build
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { scenePath },
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            // Ejecutar build
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"? Build APK exitoso: {summary.totalSize / 1024 / 1024} MB en {buildPath}");
                EditorUtility.RevealInFinder(buildPath);

                EditorUtility.DisplayDialog(
                 "Succes",                      // Título
                  "Succes Building Scene", // Mensaje
                 "OK"                             // Botón
                );


            }
            else if (summary.result == BuildResult.Failed)
            {
                EditorUtility.DisplayDialog(
                   "Error",                      // Título
                    "Error Building Scene", // Mensaje
                   "OK"                             // Botón
                  );
            }








            //GetWindow<SimpleConsoleWindow>("Simple Console");
        }









        public static void OpenWindow(NarrativeScriptableObject narrative) {
            isImportGraph = false;
            isEditGraph = true;
            narrativePath = AssetDatabase.GetAssetPath(narrative);
            GetWindow<SimpleConsoleWindow>("Simple Console");
        }



        private void OnEnable() {
            AddGraphView();
        }

        private void AddGraphView() {
            Debug.Log("AddGraphView");
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.singularisvr.stackvr/Editor/UIBUilder/NarrativeEditorWindow.uxml");
            Debug.Log($"Elemento es nulo: {visualTree == null}");

            TemplateContainer templateContainer = visualTree.CloneTree();
            templateContainer.style.flexGrow = 1;

            // Instantiate UXML
            VisualElement labelFromUXML = templateContainer;
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.singularisvr.stackvr/Editor/UIBUilder/NarrativeEditorWindow.uss");
            rootVisualElement.styleSheets.Add(styleSheet);
            rootVisualElement.Add(labelFromUXML);
            LoadDocument(templateContainer);
        }

        private void LoadDocument(TemplateContainer templateContainer) {
            var root = templateContainer;
            //var graphViewExperience = templateContainer.Q<GraphViewExperiences>(name: "GraphViewExperiences");
            var graphContainer = root.Q<VisualElement>(name: "graphContainer");
            var graphElement = graphContainer.Q<VisualElement>(name: "GraphViewExperiences");
            var graphViewExperience = new GraphViewExperiences();
            graphViewExperience.StretchToParentSize();
            graphViewExperience.Add(graphElement);
            graphContainer.Add(graphViewExperience);

            graphViewExperience.Init(narrativePath);

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
                graphViewExperience.EditNodes(narrativePath);
            }
        }



        private void OnGUI() {
            GUILayout.Label("This is a simple console window.", EditorStyles.boldLabel);
            GUILayout.Label("Check the Unity Console for the log message.");
        }
    }
}