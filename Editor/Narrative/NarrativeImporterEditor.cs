using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Singularis.StackVR.Editor;
using Singularis.StackVR.Scriptables.Editor;

namespace Singularis.StackVR.Narrative.Editor {
    public class NarrativeImporterEditor : UnityEditor.Editor {

        static string NARRATIVE_DIRECTORY {
            get => "Assets/Singularis/Narratives/";
        }

        static string NARRATIVE_DIRECTORY_PATH {
            get => Path.Combine(Application.dataPath, "Singularis/Narratives/");
        }


        //[MenuItem("Singularis/Narrative/Import", priority = 10)]
        static private void Import() {
            FindGraphView();
        }


        public static void FindGraphView() {
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();

            foreach (var window in windows) {
                Debug.Log(window.name);
                if (window.rootVisualElement.Q<GraphViewExperiences>() != null) {
                    GraphViewExperiences graphViewExperiences = window.rootVisualElement.Q<GraphViewExperiences>();
                    graphViewExperiences.ImportNodes();
                    Debug.Log($"Se encontró una GraphView en la ventana: {window.titleContent.text}");
                    return;
                }
            }
            SimpleConsoleWindow.ShowWindow(true);

            Debug.Log("No se encontró ninguna GraphView en las ventanas abiertas.");
        }


        static private void JsonToAsset(string jsonPath) {
            string content = File.ReadAllText(jsonPath);
            Debug.Log(content);

            string assetPath = Path.Combine(NARRATIVE_DIRECTORY, Path.GetFileNameWithoutExtension(jsonPath) + ".asset");


            NarrativeData narrativeData = JsonUtility.FromJson<NarrativeData>(content);
            SaveAsset(narrativeData.ToScriptableObject(), assetPath);
        }

        static private void SaveAsset(NarrativeScriptableObject scriptableObject, string path) {

            if (!Directory.Exists(NARRATIVE_DIRECTORY_PATH))
                Directory.CreateDirectory(NARRATIVE_DIRECTORY_PATH);

            AssetDatabase.CreateAsset(scriptableObject, path);
            AssetDatabase.SaveAssets();
        }

    }
}
