using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using Oculus.Interaction;
using Newtonsoft.Json;
using Singularis.StackVR.Editor;

namespace Singularis.StackVR.Narrative.Editor {
    public class SceneGenerator {

        static public void GenerateScene(string jsonDataPath) {

            // Cargar datos del JSON
            string jsonData = System.IO.File.ReadAllText(jsonDataPath);
            Tour tour = JsonConvert.DeserializeObject<Tour>(jsonData);

            if (tour == null) {
                Debug.LogError("[Singularis - SceneGenerator::GenerateScene] Error al cargar datos del JSON: " + jsonDataPath);
                return;
            }


            // Crear una nueva escena en blanco
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            string skyboxPath = "Packages/com.meta.xr.sdk.interaction/Runtime/Sample/Materials/SkyboxGradient.mat";
            Material skyboxMaterial = AssetDatabase.LoadAssetAtPath<Material>(skyboxPath);

            if (skyboxMaterial != null) {
                RenderSettings.skybox = skyboxMaterial;

                Lightmapping.lightingDataAsset = null;
                Lightmapping.Bake();
            }
            else {
                Debug.LogError("[Singularis - SceneGenerator::GenerateScene] Skybox Material no encontrado en: " + skyboxPath);
            }


            // Crear una luz direccional
            GameObject light = new GameObject("Directional Light");
            light.transform.SetPositionAndRotation(new Vector3(0, 3, 0), Quaternion.Euler(50, -30, 0));
            Light lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;


            // Instanciar OVRCameraRig
            string prefabPath = "Packages/com.meta.xr.sdk.core/Prefabs/OVRCameraRig.prefab";
            //string prefabPath = "Packages/com.meta.xr.sdk.core/Prefabs/OVRPlayerController.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab != null) {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.AddComponent<OVRPlayerControllerHelper>().initialSpotId = tour.start;
            }
            else {
                Debug.LogError("[Singularis - SceneGenerator::GenerateScene] Prefab no encontrado en: " + prefabPath);
            }

            // Crear ExperienceManager
            GameObject experienceManagerObj = new("ExperienceManager");
            ExperienceManager experienceManager = experienceManagerObj.AddComponent<ExperienceManager>();


            // Crear PointableCanvasModule
            GameObject pointableCanvasModuleObj = new("PointableCanvasModule");
            pointableCanvasModuleObj.AddComponent<EventSystem>();
            PointableCanvasModule pointableCanvasModule = pointableCanvasModuleObj.AddComponent<PointableCanvasModule>();



            string hotspotPrefabPath = "Assets/Singularis/StackVR/Prefabs/HotspotLocation.prefab";
            HotspotLocation hotspotPrefab = AssetDatabase.LoadAssetAtPath<HotspotLocation>(hotspotPrefabPath);
            string hotspotPrefabQuestionPath = "Assets/Singularis/StackVR/Prefabs/HotspotQuestion.prefab";
            HotspotQuestion hotspotPrefabQuestion = AssetDatabase.LoadAssetAtPath<HotspotQuestion>(hotspotPrefabQuestionPath);




            foreach (NodeDataOld node in tour.nodes) {
                string spotPrefabPath = $"Packages/com.singularisvr.stackvr/Runtime/Prefabs/{(node.isSteroscopic ? "Spot-stereo" : "Spot-mono")}.prefab";
                SpotController spotPrefab = AssetDatabase.LoadAssetAtPath<SpotController>(spotPrefabPath);
                SpotController spotInstance = (SpotController)PrefabUtility.InstantiatePrefab(spotPrefab);
                spotInstance.id = node.id;
                spotInstance.SetImage(AssetDatabase.LoadAssetAtPath<Texture2D>(node.resource.path));

                spotInstance.gameObject.name += $"_{node.id}-{node.name}";
                spotInstance.transform.SetPositionAndRotation(new Vector3(node.xPos * 0.1f, 0, node.yPos * 0.1f), Quaternion.Euler(0, node.north, 0));

                experienceManager.AddNode(spotInstance);


                foreach (HotspotDataJson hotspot in node.hotspots) {

                    Debug.Log("You Have a Hotspot type of" + hotspot.hostpotType);

                    if (hotspot.hostpotType == "question") {

                        HotspotQuestion hotspotInstance = (HotspotQuestion)PrefabUtility.InstantiatePrefab(hotspotPrefabQuestion);
                        hotspotInstance.SetIcon(AssetDatabase.LoadAssetAtPath<Texture2D>(hotspot.iconPath));

                        float theta = hotspot.angleX * Mathf.Deg2Rad;
                        float phi = hotspot.angleY * Mathf.Deg2Rad;
                        float distance = hotspot.distance * 0.5f;

                        float x = distance * Mathf.Cos(phi) * Mathf.Sin(theta);
                        float y = distance * Mathf.Sin(phi);
                        float z = distance * Mathf.Cos(phi) * Mathf.Cos(theta);

                        Vector3 position = spotInstance.transform.position + new Vector3(x, y, z);

                        hotspotInstance.transform.SetPositionAndRotation(
                            position,
                            Quaternion.LookRotation(position - spotInstance.position)
                        );


                        string UIQuestionPrefabPath = "Assets/Singularis/StackVR/Prefabs/UIQuestion.prefab";
                        UIQuestion UIQuestionPrefab = AssetDatabase.LoadAssetAtPath<UIQuestion>(UIQuestionPrefabPath);
                        UIQuestion UIQuestionInstance = (UIQuestion)PrefabUtility.InstantiatePrefab(UIQuestionPrefab);
                        UIQuestionInstance.FillData(
                            hotspot.question,
                            hotspot.answerA,
                            hotspot.answerB,
                            hotspot.answerC,
                            hotspot.correctAnswer
                        );

                        position.y = 0;
                        UIQuestionInstance.transform.SetPositionAndRotation(
                            position,
                            Quaternion.LookRotation(position - spotInstance.position)
                        );

                        hotspotInstance.uiQuestion = UIQuestionInstance;
                        experienceManager.AddHotspot(hotspotInstance);
                    }
                    else {
                        HotspotLocation hotspotInstance = (HotspotLocation)PrefabUtility.InstantiatePrefab(hotspotPrefab);
                        hotspotInstance.SetIcon(AssetDatabase.LoadAssetAtPath<Texture2D>(hotspot.iconPath));
                        hotspotInstance.SetTarget(hotspot.nodeId);

                        float theta = hotspot.angleX * Mathf.Deg2Rad;
                        float phi = hotspot.angleY * Mathf.Deg2Rad;
                        float distance = hotspot.distance * 0.5f;

                        float x = distance * Mathf.Cos(phi) * Mathf.Sin(theta);
                        float y = distance * Mathf.Sin(phi);
                        float z = distance * Mathf.Cos(phi) * Mathf.Cos(theta);

                        Vector3 position = spotInstance.transform.position + new Vector3(x, y, z);

                        hotspotInstance.transform.SetPositionAndRotation(
                            position,
                            Quaternion.LookRotation(position - spotInstance.position)
                        );

                        experienceManager.AddHotspot(hotspotInstance);
                    }

                }
            }

        }

    }
}
