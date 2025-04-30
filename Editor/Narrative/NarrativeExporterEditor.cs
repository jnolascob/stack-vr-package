using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Singularis.StackVR.Editor;
using Singularis.StackVR.Scriptables.Editor;

namespace Singularis.StackVR.Narrative.Editor {
    public class NarrativeExporterEditor : UnityEditor.Editor {

        //[MenuItem("Singularis/Narrative/Export", priority = 20)]
        static private void Export() {
            FindGraphView();
        }


        static public void FindGraphView() {
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();

            foreach (var window in windows) {
                Debug.Log(window.name);
                if (window.rootVisualElement.Q<GraphViewExperiences>() != null) {
                    GraphViewExperiences graphViewExperiences = window.rootVisualElement.Q<GraphViewExperiences>();
                    graphViewExperiences.CheckAllNodes();
                    Debug.Log($"Se encontró una GraphView en la ventana: {window.titleContent.text}");
                    return;
                }
            }

            EditorUtility.DisplayDialog(
                "Alert", // Título
                "Not Graph Created", // Mensaje
                "Accept" // Botón
            );

            Debug.Log("No se encontró ninguna GraphView en las ventanas abiertas.");
        }


        static private void AssetToJson(NarrativeScriptableObject scriptableObject, string assetPath) {
            NarrativeData narrativeData = scriptableObject.ToNarrativeData();
            SaveJson(narrativeData, assetPath);
        }

        static private void SaveJson(NarrativeData narrativeData, string path) {
            string json = JsonUtility.ToJson(narrativeData, true);
            File.WriteAllText(path, json);
        }

    }
}
