using System.IO;
using UnityEditor;
using UnityEngine;
using Singularis.StackVR.Scriptables.Editor;

namespace Singularis.StackVR.Editor {
    public class StackProjectConfig : ScriptableObject {
        //string configPath = Path.Combine(Application.dataPath, "Singularis/StackProjectConfig.asset");
        static private string configPath = "Assets/Singularis/StackVR/StackProjectConfig.asset";


        static private StackProjectConfig _currentNarrative = null;
        static public StackProjectConfig currentNarrative {
            get {
                Debug.Log(configPath);

                StackProjectConfig projectConfig = null;
                try {
                    projectConfig = AssetDatabase.LoadAssetAtPath(configPath, typeof(StackProjectConfig)) as StackProjectConfig;
                }
                catch (System.Exception e) {
                    Debug.LogWarning($"Unable to load ProjectConfig from {configPath}, error {e.Message}");
                }

                return projectConfig;
            }

            set {
                _currentNarrative = value;
            }
        }



        static StackProjectConfig() {
            EditorApplication.update += Update;
        }


        static private void Update() {
            GetProjectConfig();
            EditorApplication.update -= Update;
        }


        static public void GetProjectConfig() {
            if (currentNarrative == null && !BuildPipeline.isBuildingPlayer) {
                Debug.LogFormat($"Creating ProjectConfig at path {configPath}");

                if (!File.Exists(configPath)) {
                    Directory.CreateDirectory(Path.GetDirectoryName(configPath));
                }

                StackProjectConfig projectConfig = CreateInstance<StackProjectConfig>();
                projectConfig.narrativeScriptableObject = null;
                AssetDatabase.CreateAsset(projectConfig, configPath);

                currentNarrative = projectConfig;
            }
        }



        [SerializeField]
        public NarrativeScriptableObject narrativeScriptableObject;
        [SerializeField]
        public string narrativeSavePath;
    }
}
