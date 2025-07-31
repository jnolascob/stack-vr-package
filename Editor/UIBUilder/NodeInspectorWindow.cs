using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Singularis.StackVR.Editor;
using Singularis.StackVR.Narrative.Editor;
using Singularis.StackVR.Scriptables.Editor;
using Singularis.StackVR.Narrative;
using System;

namespace Singularis.StackVR.UIBuilder.Editor {
    public class NodeInspectorWindow : EditorWindow {

        static private NodeInspectorWindow window;
        static private NodeData node;
        private VisualElement root = default;
        private VisualTreeAsset visualTree = default;
        private bool editNorth = false;
        // Variables para el drag del NorthBar
        private bool isDragging = false;
        private float initialMouseX = 0;
        private float initialBarX = 0;

        VisualElement hotspotDragging = null;
        VisualElement hotspotsContainer = null;
        VisualElement componentContainer;
        VisualElement outliner;
        VisualElement imageContainer;
        VisualElement imgBackground;
        VisualElement imageBG;
        VisualElement imageBg;
        VisualElement container;
        VisualElement mainToolbar;
        ToolbarToggle toggleNorth;
        ToolbarButton btnChangeSpot;
        DropdownField dropdownNavigation;
        Toggle toolbarToggle;
        VisualElement northBar;
        FloatField degressField;
        new Label title;
        ToolbarButton btnSave;
        ToolbarButton btnDiscard;
        VisualElement degreesContainer;
        VisualElement outlinerContainer;


        static public VisualElement hotspotSelected = null;


        public static void ShowNodeInspector(string guid) {
            var sampleNode = AssetDatabase.GUIDToAssetPath(guid);
            node = AssetDatabase.LoadAssetAtPath<NodeData>(sampleNode);
            //Debug.Log($"[NodeInspectorWindow - ShowNodeInspector] {node.name}");

            window = GetWindow<NodeInspectorWindow>();
            window.titleContent = new GUIContent("NodeInspectorWindow");
        }

        public static void OpenWindow(NodeData nodeData) {
            node = nodeData;
            Debug.Log($"[NodeInspectorWindow - ShowNodeInspector] {node.name}");

            window = GetWindow<NodeInspectorWindow>();
            window.titleContent = new GUIContent("NodeInspectorWindow");
        }


        void CreateGUI() {
            //Debug.Log("[NodeInspectorWindow - CreateGUI]");
        }

        void OnEnable() {
            QueryElements();

            // get all spots
            //Debug.Log($"[NodeInspectorWindow - OnEnable] {node.hotspots.Count}");
            node.hotspots.ForEach(hotspot => {
                if (hotspot.type == HotspotData.HotspotType.location)
                    if (hotspot.target != null)
                        dropdownNavigation.choices.Add(hotspot.target.name);
            });


            //FillNode();
            // TODO: FillNode
            imageContainer.style.backgroundImage = new StyleBackground((Texture2D)node.image);
            AdjustImageSize(imageBg);

            imgBackground.style.backgroundImage = new StyleBackground((Texture2D)node.image);

            if (node.isStereo) {
                Debug.Log($"[NodeInspectorWindow - OnEnable] node stereo ({imageBg.resolvedStyle.width})");

                imageBg.schedule.Execute(() => {
                    imageBg.style.width = new StyleLength(imageBg.resolvedStyle.width * 2);
                    imageBg.style.backgroundImage = new StyleBackground((Texture2D)node.image);

                    imageBg.AddToClassList("stereo");
                });


            }

            ScaleBackground();
            degressField.schedule.Execute(() => {
                degressField.value = node.north;
            });

            title.text = node.name;

            SubscriveEvents();
        }


        private void SubscriveEvents() {
            toggleNorth.RegisterValueChangedCallback(evt => {
                if (evt.newValue) {
                    ShowNorthBar();
                }
                else {
                    HideNorthBar();
                }
            });


            btnChangeSpot.clickable = new Clickable(() => {
                OnButtonChange();
            });


            //toolbarToggle.RegisterValueChangedCallback(evt => {
            //    if (evt.newValue) {
            //        componentContainer.style.display = DisplayStyle.Flex;
            //        outliner.style.display = DisplayStyle.Flex;
            //    }
            //    else {
            //        componentContainer.style.display = DisplayStyle.None;
            //        outliner.style.display = DisplayStyle.None;
            //    }
            //});


            root.RegisterCallback<GeometryChangedEvent>(evt => {
                OnRootClicked();
            });

            northBar.RegisterCallback<MouseDownEvent>(evt => {
                isDragging = true;
                initialMouseX = evt.mousePosition.x;
                initialBarX = northBar.resolvedStyle.left;

                northBar.CaptureMouse();
            });

            northBar.RegisterCallback<MouseMoveEvent>(evt => {
                if (isDragging) {
                    float deltaX = evt.mousePosition.x - initialMouseX;
                    float degress = Mathf.Clamp01((initialBarX + deltaX) / imageBg.resolvedStyle.width);

                    //northBar.style.left = Mathf.Clamp( initialBarX + deltaX, 0, imageBg.resolvedStyle.width);
                    northBar.style.left = Length.Percent(degress * 100);
                    //degressField.value = Mathf.Lerp(-180f, 180, degress);
                    degressField.SetValueWithoutNotify(Mathf.Lerp(-180f, 180, degress));
                }
            });

            northBar.RegisterCallback<MouseUpEvent>(evt => {
                isDragging = false;
                northBar.ReleaseMouse();
            });

            degressField.RegisterValueChangedCallback(evt => {
                northBar.style.left = Length.Percent(Mathf.InverseLerp(-180f, 180, evt.newValue) * 100);
            });

            degressField.RegisterCallback<FocusOutEvent>(evt => {
                degressField.SetValueWithoutNotify(Mathf.Clamp(degressField.value, -180, 180));
            });

            //ShowNorthBar();
            ShowHotspots();

            hotspotsContainer.RegisterCallback<MouseDownEvent>(evt => {
                Debug.Log($"[hotspotsContainer] MouseDownEvent: {((VisualElement)evt.target).name}");

                hotspotSelected = null;
            });


            componentContainer.RegisterCallback<MouseDownEvent>(evt => {
                OnComponentClick(evt);
            });


            btnSave.clickable = new Clickable(() => {
                OnButtonSave(degressField.value, hotspotsContainer);
            });


            btnDiscard.clickable = new Clickable(() => {
                OnButtonDiscard(degressField, hotspotsContainer);
            });


            //var degreesButons = degreesContainer.Query<ToolbarButton>().ToList();
            //Debug.Log($"degreesButons: {degreesButons.Count}");
            degreesContainer.RegisterCallback<ClickEvent>(evt => {
                OnDegreesContainer(evt, degressField);
            });
        }


        private void QueryElements() {
            //Debug.Log("[NodeInspectorWindow - QueryElements]");

            // Obtener la ra�z del UI Toolkit
            root = rootVisualElement;
            // Cargar el UXML
            visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.singularisvr.stackvr/Editor/UIBUilder/NodeInspectorWindow.uxml");
            container = visualTree.CloneTree();
            container.style.flexGrow = 1;
            root.Add(container);

            mainToolbar = root.Q<VisualElement>("main-toolbar");
            toggleNorth = mainToolbar.Q<ToolbarToggle>("toggle-north");
            dropdownNavigation = mainToolbar.Q<DropdownField>("dropdown-navigation");
            btnChangeSpot = mainToolbar.Q<ToolbarButton>("btn-change-spot");
            componentContainer = root.Q<VisualElement>(name: "components-container");
            outliner = root.Q<VisualElement>(name: "outliner");
            toolbarToggle = mainToolbar.Q<Toggle>("show-panels");
            imageContainer = root.Q<VisualElement>("image-container");
            imageBg = root.Q<VisualElement>("image-bg");
            imgBackground = root.Q<VisualElement>("imgBackground");
            northBar = imageBg.Q<VisualElement>("north-bar");
            //var degressField = root.Q<FloatField>(className: "unity-base-field");
            degressField = root.Q<FloatField>(name: "degress-field");
            title = imageContainer.Q<Label>("title");
            hotspotsContainer = root.Q<VisualElement>(name: "hotspots-container");
            btnSave = mainToolbar.Q<ToolbarButton>("btn-save");
            btnDiscard = mainToolbar.Q<ToolbarButton>("btn-discard");
            degreesContainer = root.Q<VisualElement>(name: "degrees-container");
            outlinerContainer = root.Q<VisualElement>("outlinerContainer");
            
        }


        public void ScaleBackground()
        {
            float zoomFactor = 1f;
            float maxValue = 1f;
           
            Vector2 dragStartPos = Vector2.zero;
            Vector2 currentOffset = Vector2.zero;
            bool isDragging = false;

            imageContainer.RegisterCallback<WheelEvent>(evt =>
            {
                float delta = evt.delta.y > 0 ? -0.1f : 0.1f;
               zoomFactor = Mathf.Clamp(zoomFactor + delta, 0.2f, 5f);
                
                if (zoomFactor < maxValue)
                {
                    
                        zoomFactor = maxValue;
                    
                }

                imageBg.style.scale = new Scale(new Vector3(zoomFactor, zoomFactor, 1f));
            });



            // DRAG con middle mouse (scroll click)
            imageContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == (int)MouseButton.MiddleMouse)
                {
                    isDragging = true;
                    dragStartPos = evt.mousePosition;
                    evt.StopPropagation();
                }
            });


            imageContainer.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if (isDragging)
                {
                    Vector2 delta = evt.mousePosition - dragStartPos;
                    dragStartPos = evt.mousePosition;
                    currentOffset += delta;

                    imgBackground.style.translate = new Translate(currentOffset.x, currentOffset.y, 0);
                    evt.StopPropagation();
                }
            });

            imageContainer.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button == (int)MouseButton.MiddleMouse)
                {
                    isDragging = false;
                    evt.StopPropagation();
                }
            });

        }

        private void OnButtonChange() {
            if (dropdownNavigation.text == null) return;

            Debug.Log($"[NodeInspectorWindow] Change Spot: {dropdownNavigation.text}");

            var target = node.hotspots.Find(h => h.target.name == dropdownNavigation.text);
            if (target != null) {
                Debug.Log($"[NodeInspectorWindow] Change Spot: {target.name}");

                string filePath = AssetDatabase.GetAssetPath(target.target);
                NodeInspectorWindow.ShowNodeInspector(AssetDatabase.AssetPathToGUID(filePath));
                root.Clear();

                OnEnable();
                root.schedule.Execute(() => {
                    Debug.Log($"[NodeInspectorWindow] root schedule");
                    imageBG = root.Q<VisualElement>("image-bg");
                    AdjustImageSize(imageBG);
                });
                root.MarkDirtyRepaint();
                Debug.Log($"[NodeInspectorWindow] after root.schedule");
            }
        }

        private void OnRootClicked() {
            float width = root.resolvedStyle.width;
            float height = root.resolvedStyle.height;
            imageContainer.style.height = new StyleLength(height - 21);
            AdjustImageSize(imageBg);
            //componentContainer.style.left = new StyleLength(0f);
            //componentContainer.style.top = new StyleLength((imageContainer.resolvedStyle.height * 0.5f) - (componentContainer.resolvedStyle.height * 0.5f));
            //outliner.style.right = new StyleLength(0f);
            //outliner.style.top = new StyleLength((imageContainer.resolvedStyle.height * 0.5f) - (outliner.resolvedStyle.height * 0.5f));
        }

        private void OnComponentClick(MouseDownEvent evt) {
            var target = evt.target as VisualElement;
            bool isHotspot = target.ClassListContains("hotspot");
            bool createHotspot = false;

            if (isHotspot) {
                Debug.Log($"MouseDownEvent: {isHotspot}");
                createHotspot = true;
            }
            else {
                bool parentIsHotspot = target.parent.ClassListContains("hotspot");
                Debug.Log($"MouseDownEvent: {isHotspot} {parentIsHotspot}");

                if (parentIsHotspot) {
                    createHotspot = true;
                    target = target.parent;
                }
            }

            if (createHotspot) {
                Debug.Log($"CreateHostpot: {target.name}");

                HotspotData hotspot = ScriptableObject.CreateInstance<HotspotData>();
                
                if (target.name == "hotspot-question") 
                {
                    hotspot = ScriptableObject.CreateInstance<HotspotQuestionData>();
                    hotspot.type = HotspotData.HotspotType.question;
                }
                else {
                    hotspot.type = HotspotData.HotspotType.location;
                }

                hotspot.id = hotspotsContainer.childCount + 1;
                hotspot.name = target.name;

                Debug.Log("THe New hostpot" + hotspot.id);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                CreateHotspot(hotspot, hotspotsContainer, componentContainer, outliner);

                createHotspot = false;
                //target.CaptureMouse();
            }

        }

        private void OnDegreesContainer(ClickEvent evt, FloatField degressField) {
            var target = evt.target as ToolbarButton;

            if (target != null) {
                //Debug.Log($"[degreesContainer] ClickEvent: {target.name}");

                float value = target.name switch {
                    "btn-subtract-15" => -15,
                    "btn-subtract-10" => -10,
                    "btn-subtract-5" => -5,
                    "btn-add-5" => 5,
                    "btn-add-10" => 10,
                    "btn-add-15" => 15,
                    _ => 0
                };

                degressField.value += value;
            }
        }


        public void SaveHostpos()
        {
            OnButtonSave(degressField.value, hotspotsContainer);
        }

        private void OnButtonSave(float degrees, VisualElement hotspotsContainer) {

            if (editNorth) {
                Debug.Log("[NodeInspectorWindow] Save Node");

                node.north = degrees;
            }
            else {
                Debug.Log("[NodeInspectorWindow] Save Hotspots");

                List<HotspotData> newHotspots = new List<HotspotData>();
                var children = hotspotsContainer.Children();                                                   


                foreach (var child in children) {
                    

                    Debug.Log("The CHild Name is " + child.name);
                    Dictionary<string, object> hotspotData = child.userData as Dictionary<string, object>;
                    
                    string hostpotId = hotspotData["id"].ToString();
                    Debug.Log("THe User is" + hostpotId);
                    VisualElement hotspotElement = child;                  
                                       
                    

                    Debug.Log("The Node has " + node.hotspots.Count);

                    HotspotData hotspot = new HotspotData();
                    bool hasHostpot = false;
                    

                    if (node.hotspots.Count > 0)
                    {
                        

                        foreach (var itemHostpot in node.hotspots)
                        { 

                            Debug.Log("The hostpot id new is " + hostpotId);
                            Debug.Log("The New ids are " + itemHostpot.id);
                            if (itemHostpot.id == Int32.Parse(hostpotId))
                            {
                                
                                hotspot = itemHostpot;
                                hasHostpot = true;  
                                break;
                            }
                        }


                        if (hasHostpot)  // Chequear si el hotspot no existe
                        {
                            Debug.Log($"UPDATE Hotspot: {hotspot.id} {hotspot.name}");
                        }
                        else
                        {
                            Debug.Log("Not Find Hostpot");


                            Debug.Log($"CREATE Hotspot: {child.name}");

                            if (hotspotData["type"].ToString() == "question")
                            {
                                Debug.Log("Creating Question");
                                hotspot = NarrativesHelper.CreateHotspotQuestion(node, child.name);
                            }
                            else
                            {
                                hotspot = NarrativesHelper.CreateHostpot(node, child.name);
                            }

                            newHotspots.Add(hotspot);




                            //
                            //Creacion De hotspots

                        }
                    }

                    else
                    {

                        Debug.Log($"CREATE Hotspot: {child.name}");

                        if (hotspotData["type"].ToString() == "question")
                        {
                            Debug.Log("Creating Question");
                            hotspot = NarrativesHelper.CreateHotspotQuestion(node, child.name);
                        }
                        else
                        {
                            hotspot = NarrativesHelper.CreateHostpot(node, child.name);
                        }                        

                        newHotspots.Add(hotspot);



                    }

                    foreach (var newHostpot in node.hotspots)
                    {
                        Debug.Log("HostpotName" +  newHostpot.id);    
                    }                  


                  



                    Debug.Log($"Hotspot: {hotspotData["id"]} {hotspotData["name"]} {hotspotData["distance"]}");

                    hotspot.id = int.Parse(hotspotData["id"].ToString());

                    Debug.Log("The new hostpot Id is " + hotspot.id);
                    hotspot.name = hotspotData["name"].ToString();
                    hotspot.distance = float.Parse(hotspotData["distance"].ToString());
                    hotspot.angleX = Mathf.Lerp(-180f, 180f, Mathf.Clamp01(hotspotElement.resolvedStyle.left / hotspotsContainer.resolvedStyle.width));
                    hotspot.angleY = Mathf.Lerp(-80f, 80f, Mathf.Clamp01(hotspotElement.resolvedStyle.top / hotspotsContainer.resolvedStyle.height));

                    Debug.Log("The Angle of X is " + hotspot.angleX);

                    hotspot.scale = hotspotElement.transform.scale.x;

                    hotspot.icon = hotspotElement.style.backgroundImage.value.texture;
                    hotspot.color = hotspotElement.style.unityBackgroundImageTintColor.value;

                    hotspot.target = hotspotData["target"] as NodeData;

                    EditorUtility.SetDirty(hotspot);
                  

                    Debug.Log($"Hotspot: {hotspot.type} - {hotspotData["type"]}");
                    if (hotspotData["type"].ToString() == "question") {
                        //hotspot.type = HotspotData.HotspotType.question;

                        HotspotQuestionData questionData = hotspot as HotspotQuestionData;

                        List<Answer> answers = new List<Answer>();

                        foreach (var key in hotspotData.Keys) {
                            var value = hotspotData[key];
                            if (value is Answer) {
                                Answer result = value as Answer;
                                answers.Add(result);
                            }
                        }
                        if (questionData == null)
                        {
                            Debug.Log("Question Data es Nulo");
                        }

                        questionData.type = HotspotData.HotspotType.question;
                        questionData.answers = answers;
                        questionData.kindOfQuestion = (int)hotspotData["kindOfQuestion"];
                        questionData.question = (string)hotspotData["question"];
                        if (hotspotData.ContainsKey("TextureQuestion")) {
                            questionData.textureElement = hotspotData["TextureQuestion"] as Texture;
                        }
                        EditorUtility.SetDirty(hotspot);
                       
                    }
                    else {
                        hotspot.type = HotspotData.HotspotType.location;
                        EditorUtility.SetDirty(hotspot);
                        
                    }
                }


                node.hotspots.AddRange(newHotspots);

                EditorUtility.SetDirty(node);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                newHotspots.Clear();    

            }

        }






        





        private void OnButtonDiscard(FloatField degressField, VisualElement hotspotsContainer) {

            Debug.Log("[NodeInspectorWindow] Discard Changes");
            if (editNorth) {
                degressField.value = node.north;
            }
            else {
                hotspotSelected = null;
                HotspotInspectorWindow.RepaintWindow();

                node.hotspots.ForEach(hotspot => {
                    VisualElement hotspotElement = null;
                    var childer = hotspotsContainer.Children();
                    foreach (var child in childer) {
                        if (child.name == $"{hotspot.id}-{hotspot.name}") {
                            hotspotElement = child;
                            break;
                        }
                    }

                    Dictionary<string, object> hotspotData = new Dictionary<string, object> {
                            { "id", hotspot.id },
                            { "name", hotspot.name },
                            { "type", hotspot.type },

                            { "distance", hotspot.distance },
                            { "scale", hotspot.scale },

                            { "icon", hotspot.icon },
                            { "color", hotspot.color },

                            { "target", hotspot.target },
                            { "targetId", hotspot.targetId },

                            //{ "question", hotspot.question },
                            //{ "answerA", hotspot.answerA },
                            //{ "answerB", hotspot.answerB },
                            //{ "answerC", hotspot.answerC },
                            //{ "correctAnswer", hotspot.correctAnswer }
                        };

                    hotspotElement.userData = hotspotData;

                    hotspotElement.name = $"{hotspot.id}-{hotspot.name}"; // TODO: outliner actualizar el nombre 

                    hotspotElement.style.left = Length.Percent(Mathf.InverseLerp(-180f, 180f, hotspot.angleX) * 100);
                    hotspotElement.style.top = Length.Percent(Mathf.InverseLerp(-80f, 80f, hotspot.angleY) * 100);
                    hotspotElement.transform.scale = new Vector3(hotspot.scale, hotspot.scale);
                    
                    hotspotElement.style.backgroundImage = new StyleBackground(hotspot.icon);
                    hotspotElement.style.unityBackgroundImageTintColor = new StyleColor(hotspot.color);
                    hotspotElement.MarkDirtyRepaint();
                });
            }
        }

        void OnDisable() {
            Debug.Log("[NodeInspectorWindow - OnDisable]");
            hotspotSelected = null;
        }

        private void FillNode() {

        }

        private void CreateHotspot(HotspotData hotspot, VisualElement hotspotsContainer, VisualElement componentContainer, VisualElement outliner, bool isFirstTime = false) {
            VisualElement hotspotClone = new VisualElement();

            if (hotspot.icon != null)
                hotspotClone.style.backgroundImage = new StyleBackground(hotspot.icon);
            else {
                string assetPath = $"Packages/com.singularisvr.stackvr/Editor/Sprites/ico_hotspot_{hotspot.type}.png";
                Texture2D hotspotTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                hotspotClone.style.backgroundImage = new StyleBackground(hotspotTexture);
            }
            hotspotClone.style.unityBackgroundImageTintColor = new StyleColor(hotspot.color);

            hotspotClone.style.width = 40;
            hotspotClone.style.height = 40;
            hotspotClone.style.position = Position.Absolute;
            hotspotClone.style.borderLeftWidth = 0;
            hotspotClone.style.paddingLeft = 0;
            hotspotClone.style.marginLeft = 0;

            hotspotClone.style.left = hotspotsContainer.resolvedStyle.width * (Mathf.InverseLerp(-180f, 180f, hotspot.angleX));
            hotspotClone.style.top = hotspotsContainer.resolvedStyle.height * (Mathf.InverseLerp(-80, 80f, hotspot.angleY));


            Dictionary<string, object> hotspotData = new Dictionary<string, object> {
                { "id", hotspot.id },
                { "name", hotspot.name },
                { "type", hotspot.type },

                { "distance", hotspot.distance },
                { "scale", hotspot.scale },

                { "icon", hotspot.icon },
                { "color", hotspot.color },

                { "target", hotspot.target },
                { "targetId", hotspot.targetId }, // TODO: targetId

                //{ "question", hotspot.question },
                //{ "answerA", hotspot.answerA },
                //{ "answerB", hotspot.answerB },
                //{ "answerC", hotspot.answerC },
                //{ "correctAnswer", hotspot.correctAnswer }
            };


            Debug.Log("THe hotspot type is " + hotspot.type);
            if (hotspot.type == HotspotData.HotspotType.question) {
                HotspotQuestionData questionData = hotspot as HotspotQuestionData;
                Debug.Log($"HotspotQuestionData: {questionData == null}");
                hotspotData.Add("kindOfQuestion", questionData.kindOfQuestion);
                hotspotData.Add("question", questionData.question);
                hotspotData.Add("answers", questionData.answers);
                hotspotData.Add("TextureQuestion", questionData.textureElement);
            }

            hotspotClone.userData = hotspotData;
            hotspotClone.name = $"{hotspot.id}-{hotspot.name}";

            hotspotClone.transform.scale = new Vector3(hotspot.scale, hotspot.scale);


            hotspotClone.RegisterCallback<MouseDownEvent>(evt => {
                if (editNorth) return;

                hotspotDragging = hotspotClone;

                hotspotClone.CaptureMouse();
            });

            hotspotClone.RegisterCallback<MouseMoveEvent>(evt => {
                if (editNorth) return;
                if (hotspotDragging != hotspotClone) return;

                componentContainer.style.display = DisplayStyle.None;
                outliner.style.display = DisplayStyle.None;


                float left = Mathf.Clamp01((hotspotClone.resolvedStyle.left + evt.mouseDelta.x) / hotspotsContainer.resolvedStyle.width);
                float top = Mathf.Clamp01((hotspotClone.resolvedStyle.top + evt.mouseDelta.y) / hotspotsContainer.resolvedStyle.height);

                hotspotClone.style.left = Length.Percent(left * 100f);
                hotspotClone.style.top = Length.Percent(top * 100f);
            });

            hotspotClone.RegisterCallback<MouseUpEvent>(evt => {
                if (editNorth) return;
                if (hotspotDragging == hotspotClone) {
                    hotspotClone.ReleaseMouse();
                    hotspotDragging = null;

                    componentContainer.style.display = DisplayStyle.Flex;
                    outliner.style.display = DisplayStyle.Flex;
                }

                Debug.Log($"HotspotGhost: {evt.clickCount}");
                hotspotSelected = hotspotClone;


                if (hotspot.type == HotspotData.HotspotType.location || hotspot.type == HotspotData.HotspotType.custom) {
                    HotspotInspectorWindow.ShowNodeInspector(this);
                    //HotspotInspectorWindow.FillData(hotspot);
                    HotspotInspectorWindow.FillData(hotspotClone, hotspot);
                }
                else {
                    HotspotInspectorWindow.ShowNodeInspector(this);
                    HotspotInspectorWindow.FillData(hotspotClone, hotspot);
                }

                OnButtonSave(degressField.value, hotspotsContainer);
            });

            hotspotsContainer.Add(hotspotClone);
           //List<HotspotData> hostpots = hotspotsContainer.userData as List<HotspotData>;
           //if (hotspot == null)
           // {
           //     List<HotspotData> newHostpots = new List<HotspotData>();
           //     newHostpots.Add(hotspot);
           //     hotspotsContainer.userData = newHostpots;  

           // }
           // else
           // {
           //     hostpots.Add(hotspot);
           //     hotspotsContainer.userData = hostpots;

           // }
            

            string nameHostpot = $"{hotspot.id}-{hotspot.name}";

            hotspotClone.MarkDirtyRepaint();
            hotspotsContainer.MarkDirtyRepaint();

            CreateHostpotInUI(outlinerContainer, hotspot.type.ToString(), nameHostpot);
            if (!isFirstTime)
            {
                OnButtonSave(degressField.value, hotspotsContainer);
            }
            
        }

        private void CreateHostpotInUI(VisualElement parent, string hostpotName, string nameHotspot) {
            Debug.Log($"[NodeInspectorWindow] CreateHostpotInUI: {hostpotName} {nameHotspot}");

            VisualTreeAsset buttonTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.singularisvr.stackvr/Editor/UIBUilder/buttonHotspot.uxml");
            VisualElement buttonInstance = buttonTemplate.Instantiate();
            buttonInstance.name = nameHotspot;

            buttonInstance.Q<Button>().text = hostpotName;

            parent.Add(buttonInstance);
            parent.MarkDirtyRepaint();
            AssetDatabase.Refresh();

           
        }

        private void AdjustImageSize(VisualElement imageElement) {
            var parent = imageElement.parent;
            float width = parent.resolvedStyle.width;
            float height = parent.resolvedStyle.height;

            if (float.IsNaN(height)) height = position.height;

            // Obtener proporción de la textura
            Texture2D bgImage = parent.resolvedStyle.backgroundImage.texture;
            float aspectRatio = (float)bgImage.width / bgImage.height;


            if (height == 0) height = position.height - 21;

            if (aspectRatio > width / height) {
                height = width / aspectRatio;
            }
            else {
                width = height * aspectRatio;
            }

            imageElement.style.width = width;
            imageElement.style.height = height;

        }

        private void ShowNorthBar() {
            var toolbarToggle = root.Q<Toggle>(name: "show-panels");
            var northBar = root.Q<VisualElement>(name: "north-bar");
            var degreesContainer = root.Q<VisualElement>(name: "degrees-container");
            var componentContainer = root.Q<VisualElement>(name: "components-container");
            var hotspotsContainer = root.Q<VisualElement>(name: "hotspots-container");
            var outliner = root.Q<VisualElement>(name: "outliner");

            northBar.style.display = DisplayStyle.Flex;
            degreesContainer.style.display = DisplayStyle.Flex;

            toolbarToggle.style.display = DisplayStyle.None;
            componentContainer.style.display = DisplayStyle.None;
            //hotspotsContainer.style.display = DisplayStyle.None;
            outliner.style.display = DisplayStyle.None;

            editNorth = true;
        }

        private void HideNorthBar() {
            var toolbarToggle = root.Q<Toggle>(name: "show-panels");
            var northBar = root.Q<VisualElement>(name: "north-bar");
            var degreesContainer = root.Q<VisualElement>(name: "degrees-container");
            var componentContainer = root.Q<VisualElement>(name: "components-container");
            var hotspotsContainer = root.Q<VisualElement>(name: "hotspots-container");
            var outliner = root.Q<VisualElement>(name: "outliner");

            northBar.style.display = DisplayStyle.None;
            degreesContainer.style.display = DisplayStyle.None;

            toolbarToggle.style.display = DisplayStyle.Flex;
            componentContainer.style.display = DisplayStyle.Flex;
            //hotspotsContainer.style.display = DisplayStyle.Flex;
            outliner.style.display = DisplayStyle.Flex;

            editNorth = false;
        }

        private void ShowHotspots() {
            var toolbarToggle = root.Q<Toggle>(name: "show-panels");
            var northBar = root.Q<VisualElement>(name: "north-bar");
            var degreesContainer = root.Q<VisualElement>(name: "degrees-container");
            var componentContainer = root.Q<VisualElement>(name: "components-container");
            var hotspotsContainer = root.Q<VisualElement>(name: "hotspots-container");
            var outliner = root.Q<VisualElement>(name: "outliner");

            northBar.style.display = DisplayStyle.None;
            degreesContainer.style.display = DisplayStyle.None;

            toolbarToggle.style.display = DisplayStyle.Flex;
            componentContainer.style.display = DisplayStyle.Flex;
            hotspotsContainer.style.display = DisplayStyle.Flex;
            outliner.style.display = DisplayStyle.Flex;


            imageContainer = root.Q<VisualElement>("image-container");

            //componentContainer.schedule.Execute(() =>
            //{
            //    componentContainer.style.left = new StyleLength(0f);
            //    componentContainer.style.top = new StyleLength((imageContainer.resolvedStyle.height * 0.5f) - (componentContainer.resolvedStyle.height * 0.5f));
            //});

            //outliner.schedule.Execute(() => {
            //    outliner.style.right = new StyleLength(0f);
            //    outliner.style.top = new StyleLength((imageContainer.resolvedStyle.height * 0.5f) - (outliner.resolvedStyle.height * 0.5f));
            //});

            outlinerContainer.Clear();
            //outliner.Clear();
            hotspotsContainer.Clear();

            hotspotsContainer.schedule.Execute(() => {
                // TODO: create hotspots
                node.hotspots.ForEach(hotspot => {
                    CreateHotspot(hotspot, hotspotsContainer, componentContainer, outliner, true);
                });
            });

            HotspotInspectorWindow.SetOutlinerElement(outliner);

            editNorth = false;
        }

    }
}

