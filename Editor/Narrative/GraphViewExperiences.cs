using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Singularis.StackVR.Editor;
using Singularis.StackVR.Scriptables.Editor;
using Singularis.StackVR.UIBuilder.Editor;
using System.Net;

namespace Singularis.StackVR.Narrative.Editor {
    // TODO remove Linq
    // TODO research UxmlElement attribute
    public partial class GraphViewExperiences : GraphView {
        public new class UxmlFactory : UxmlFactory<GraphViewExperiences, UxmlTraits> { };

        //public VisualElement root;

        // Node Elements Lists 
        public List<BaseNode> totalNodes = new List<BaseNode>();
        private Dictionary<string, BaseNode> nodeDictionary = new Dictionary<string, BaseNode>();
        private int currentNode = 0;

        private List<NodeDataOld> nodesData = new List<NodeDataOld>();
        public BaseNode inputNode;
        public BaseNode outputNode;

        private float defaultMinScale;
        private float defaultMaxScale;
        public VisualElement addImageButton;
        public VisualElement addVideoButton;
        private TemplateContainer templateContainer;
        public BaseNode selectedNode;
        public NarrativeScriptableObject currentNarrative;
        private List<HotspotData> importedHostpots = new List<HotspotData>();
        public bool isSteroscopic;
        public bool isLockedNode;



        // Vector2 Data
        private Vector2 mousePosition;
        private Vector2 currentMousePosition;
        public Vector2 currentPos;
        private Vector2 currentMousePos;

        // Visual Elements
        public VisualElement inspectorPanel;
        public VisualElement dropDownMenu;
        public VisualElement nodeMenu;
        private VisualElement currentVisualElement;

        public Button buttonSave; // TODO remove this button
        public Button buttonBuild;
        public Button buttonSaveGraph;
        public Button buttonLoadGraph;

        // Events
        private EventCallback<ClickEvent> transitionButtonCallback;
        private EventCallback<ClickEvent> deleteButtonCallback;
        public static string placeHolderImage = "Packages/com.singularisvr.stackvr/Editor/Sprites/PlaceHolderImage.jpg";

        // Drawing Line Variables
        public LineElement currentLine;
        public bool isDrawingLine = false;

        public Toggle toggleSteroscopic;


        public GraphViewExperiences() {

        }

        public void Init(string narrativePath) {

            // CODIGO ADICIONAL
            this.AddManipulator(new ClickSelector()); // Permite clicks en áreas vacías

            //currentNarrative = StackProjectConfig.currentNarrative.narrativeScriptableObject;
            currentNarrative = AssetDatabase.LoadAssetAtPath<NarrativeScriptableObject>(narrativePath);
            if (currentNarrative == null) {
                Debug.Log("Narrative Not Found");
                return;
            }
            // Set Graph Styles

            AddManipulators(); // Add Manipulators
            AddStyles(); // Add Styles to the Grid      
            AddGridBackground();// Create Grid Background      
            this.SendToBack();

            // Events Callbacks

            this.graphViewChanged = OnGraphVieCahnge;
            RegisterCallback<ContextualMenuPopulateEvent>(_ =>
                // Cache the event's mouse position when right-clicking.
                mousePosition = Event.current.mousePosition
            );

            RegisterCallback<MouseMoveEvent>((e) => {

                currentMousePosition = e.mousePosition;
                currentVisualElement = this.panel.Pick(e.mousePosition);

            });

            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseDownEvent>((e) => { OnMouseDownEvent(e); }, TrickleDown.TrickleDown);

            RegisterCallback<ContextualMenuPopulateEvent>((e) => { e.menu.ClearItems(); });
            this.RegisterCallback<MouseUpEvent>((e) => { OnRelease(); }, TrickleDown.TrickleDown);
            FFMpegHandler.InitFMpeg();
        }

        private void OnRelease() {
            if (selectedNode != null) {
                NodeData nodeData = currentNarrative.nodes.Find(node => node.id == selectedNode.id);
                nodeData.posX = selectedNode.style.left.value.value;
                nodeData.posY = selectedNode.style.top.value.value;

                ShowInspectorPanel(selectedNode);
            }
        }


        public void RemoveLine() {
            Remove(currentLine);
            isDrawingLine = false;
            inputNode = null;
            currentLine = null;
        }


        public void SetTemplateContainer(TemplateContainer templateContainer) {
            this.templateContainer = templateContainer;
        }


        public void ShowInspectorPanel(BaseNode currentNode) {
            var newPos = new Vector2(currentNode.style.left.value.value, currentNode.style.top.value.value);

            Button configButton = inspectorPanel.Q<Button>("ConfigurationButton");
            configButton.RegisterCallback<ClickEvent>((e) => {
                selectedNode.OnEnterNode();
            });

            Toggle toggleLockNode = inspectorPanel.Q<Toggle>("LockNode");


            toggleLockNode.RegisterValueChangedCallback((e) => {
                Debug.Log("Setting is Locked Node");
                isSteroscopic = e.newValue;

                if (selectedNode != null) {
                    selectedNode.isLockedNode = e.newValue;
                }
            });



            Toggle toggleItem = inspectorPanel.Q<Toggle>("Steroscopic");
            toggleItem.RegisterValueChangedCallback((e) => {
                Debug.Log("Setting is Steroscopic");
                isSteroscopic = e.newValue;

                if (selectedNode != null) {
                    selectedNode.isSteroscopic = e.newValue;

                    NodeData nodeData = currentNarrative.nodes.Find(node => node.id == selectedNode.id);
                    if (nodeData != null) {
                        nodeData.isStereo = e.newValue;
                    }

                    Debug.Log($"[GraphViewExperiences] {selectedNode.imageNode.name}");
                    if (e.newValue) {
                        selectedNode.imageNode.AddToClassList("stereo");
                        selectedNode.imageNode.RemoveFromClassList("mono");
                    }
                    else {
                        selectedNode.imageNode.AddToClassList("mono");
                        selectedNode.imageNode.RemoveFromClassList("stereo");
                    }
                }

            });

            // TODO remove namespaces
            var iPanel = inspectorPanel.Q<UnityEditor.UIElements.ObjectField>("NodeSprite");

            if (selectedNode != null) {
                iPanel.value = selectedNode.currentImage;
            }

            //iPanel.value = null;

            if (selectedNode is VideoNode) {
                iPanel.objectType = typeof(VideoClip);
                iPanel.MarkDirtyRepaint();
                inspectorPanel.Q<Label>("NameElement").text = "Video";
            }
            else {
                iPanel.objectType = typeof(Texture2D);
                iPanel.MarkDirtyRepaint();
                inspectorPanel.Q<Label>("NameElement").text = "Image";
            }


            inspectorPanel.Q<ObjectField>("NodeSprite").RegisterValueChangedCallback(async evt => {

                if (selectedNode is VideoNode) {

                    if (evt.newValue == null)
                    {

                        var videoNode = selectedNode as VideoNode;
                        videoNode.isEmpty = true;
                        videoNode.DrawEmptyNode();
                        return;
                    }

                    if (evt.newValue is VideoClip) {
                        VideoClip selectedVideo = (VideoClip)evt.newValue;


                        if (evt.newValue == null)
                        {
                            Debug.Log("You Pressed a button");
                        }

                        if (selectedVideo != null) {
                            var videoNode = selectedNode as VideoNode;

                            NodeData nodeData = currentNarrative.nodes.Find(node => node.id == videoNode.id);
                            if (nodeData != null) {
                                string pathToVideo = AssetDatabase.GetAssetPath(selectedVideo);

                                var result = await videoNode.GetVideoImage(nodeData.id, pathToVideo);
                                string fileName = Path.GetFileName(result);
                                string filePath = $"Assets/Singularis/StackVR/ImageVideos/{fileName}";
                                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);

                                if (isSteroscopic) {
                                    Debug.Log("Cut Texture");
                                    var readeableTexture = CreateReadableTexture(texture);
                                    texture = CutTextureInHalf(readeableTexture);
                                }

                                videoNode.UpdateVideo(texture);

                                nodeData.image = texture;


                                EditorUtility.SetDirty(nodeData); // Marca el asset como modificado

                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            }

                        }

                    }

                }
                else {

                  


                    Debug.Log("Selecting Image");
                    
                    if (selectedNode is ImageNode) {


                        if (evt.newValue == null)
                        {
                            Debug.Log("Boton Nulo");
                            var imageNode = selectedNode as ImageNode;
                            imageNode.isEmpty = true;
                            imageNode.DrawEmptyNode();
                            return;
                        }




                        Texture2D selectedSprite = (Texture2D)evt.newValue;
                        if (selectedSprite != null) {
                                                     

                            var imageNode = selectedNode as ImageNode;
                            imageNode.UpdateImage(selectedSprite);

                            NodeData nodeData = currentNarrative.nodes.Find(node => node.id == imageNode.id);
                            if (nodeData != null) {
                                nodeData.image = selectedSprite;
                                EditorUtility.SetDirty(nodeData); // Marca el asset como modificado

                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();

                            }
                        }
                    }
                }
            });


            if (string.IsNullOrEmpty(selectedNode.titleNode.text)) {
                inspectorPanel.Q<TextField>("TextName").value = null;
            }
            else {
                inspectorPanel.Q<TextField>("TextName").value = selectedNode.fullTitle;
            }

            inspectorPanel.Q<TextField>("TextName").RegisterCallback<ChangeEvent<string>>((value) => {
                selectedNode.ChangeLabelText(value.newValue);

                NodeData nodeData = currentNarrative.nodes.Find(node => node.id == selectedNode.id);
                if (nodeData != null) {
                    nodeData.name = value.newValue;
                }
            });

            if (string.IsNullOrEmpty(selectedNode.titleNode.text)) {
                inspectorPanel.Q<TextField>("TextName").value = null;
            }
            else {
                inspectorPanel.Q<TextField>("TextName").value = selectedNode.fullTitle;
            }


            inspectorPanel.Q<TextField>("TextName").RegisterCallback<ChangeEvent<string>>((value) => {
                selectedNode.ChangeLabelText(value.newValue);

                NodeData nodeData = currentNarrative.nodes.Find(node => node.id == selectedNode.id);
                if (nodeData != null) {
                    nodeData.name = value.newValue;
                }
            });

        }


        public bool CheckIfObectFieldIsEmpty()
        {
           if (inspectorPanel.Q<ObjectField>("NodeSprite").value == null)
            {
                return true;
            }
            else
            {
            return false;
             }
        }

        public void HideInspectorPanel() {
            //inspectorPanel.style.visibility = Visibility.Hidden;
        }

        private Texture2D CutTextureInHalf(Texture2D original) {
            // Obtener las dimensiones de la textura original
            int width = original.width;
            int height = original.height;

            // Crear una nueva textura con la mitad de la altura
            Texture2D halfTexture = new Texture2D(width, height / 2);

            // Recorrer la mitad superior de la textura original
            for (int y = 0; y < height / 2; y++) {
                for (int x = 0; x < width; x++) {
                    // Obtener el color del píxel en la posición (x, y)
                    Color pixel = original.GetPixel(x, y);

                    // Asignar el color a la nueva textura
                    halfTexture.SetPixel(x, y, pixel);
                }
            }

            // Aplicar los cambios en la nueva textura
            halfTexture.Apply();
            return halfTexture;
        }

        private Texture2D CreateReadableTexture(Texture2D source) {
            // Crear una RenderTexture temporal con las mismas dimensiones que la textura original
            RenderTexture renderTexture = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0, // Sin profundidad
                RenderTextureFormat.Default, // Usar el formato por defecto
                RenderTextureReadWrite.Linear // Espacio de color lineal
            );

            // Guardar el estado actual de la RenderTexture activa
            RenderTexture previous = RenderTexture.active;

            // Establecer la RenderTexture temporal como activa
            RenderTexture.active = renderTexture;

            // Copiar la textura original a la RenderTexture temporal
            Graphics.Blit(source, renderTexture);

            // Crear una nueva Texture2D legible
            Texture2D readableTexture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);

            // Leer los píxeles de la RenderTexture y copiarlos a la nueva Texture2D
            readableTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            readableTexture.Apply(); // Aplicar los cambios

            // Restaurar la RenderTexture activa anterior
            RenderTexture.active = previous;

            // Liberar la RenderTexture temporal
            RenderTexture.ReleaseTemporary(renderTexture);

            return readableTexture;
        }

        private void OnMouseDownEvent(MouseDownEvent e) { // Setting CUrrent Mouse Position
            currentMousePos = e.localMousePosition;
            if (e.button == 0) {

                if (e.target is GraphViewExperiences) {
                    //e.StopPropagation();
                    Debug.Log("You Pressed Out");

                    //CODIGO ADICIONAL
                    ClearSelection(); // Método nativo de GraphView

                    selectedNode = null;

                    if (isDrawingLine) {
                        Remove(currentLine);
                        isDrawingLine = false;
                        inputNode = null;
                        currentLine = null;
                    }
                    if (dropDownMenu.style.visibility == Visibility.Visible) {
                        dropDownMenu.style.visibility = Visibility.Hidden;
                    }
                    if (nodeMenu.style.visibility == Visibility.Visible) {
                        nodeMenu.style.visibility = Visibility.Hidden;
                    }
                }

            }
            if (e.button == 1) {
                CreateDropdownMenu(e.mousePosition);
            }

        }

        public void ConnectTwoNodes(BaseNode node1, BaseNode node2) {
            CustomEdge result = node1.outPutPort.ConnectTo<CustomEdge>(node2.inputPort);
            result.currentColor = Color.gray;
            this.Add(result);
            node1.MarkDirtyRepaint();
            node2.MarkDirtyRepaint();

            node2.inputPort.SetStyles(node1.outPutPort.leftPos, node1.outPutPort.topPos, true);
            node2.outPutPort.SetStyles(node1.inputPort.leftPos, node1.inputPort.topPos, true);

            CheckNodesPositions();
        }

        public void ConnectNodes(BaseNode node)// Connect Nodes
        {
            Debug.Log($"Connecting Nodes: from: {inputNode.id} @ to: {node.id}");
            string fromId = inputNode.id;
            string toId = node.id;

            if (!node.inputPort.connected && !node.outPutPort.connected) {
                node.inputPort.SetStyles(inputNode.outPutPort.leftPos, inputNode.outPutPort.topPos, true);
                node.outPutPort.SetStyles(inputNode.inputPort.leftPos, inputNode.inputPort.topPos, true);
            }
            else if (!inputNode.inputPort.connected && !inputNode.outPutPort.connected) {
                inputNode.inputPort.SetStyles(node.outPutPort.leftPos, node.outPutPort.topPos, true);
                inputNode.outPutPort.SetStyles(node.inputPort.leftPos, node.inputPort.topPos, true);
            }

            inputNode.MarkDirtyRepaint();
            node.MarkDirtyRepaint();
            CustomEdge result = inputNode.outPutPort.ConnectTo<CustomEdge>(node.inputPort);
            result.currentColor = Color.gray;
            this.Add(result);
            inputNode.MarkDirtyRepaint();
            node.MarkDirtyRepaint();
            result.MarkDirtyRepaint();
            this.MarkDirtyRepaint();
            inputNode = null;


            Debug.Log(currentNarrative.nodes.Count);
            NodeData nodeFrom = currentNarrative.nodes.Find(n => n.id == fromId);
            NodeData nodeTo = currentNarrative.nodes.Find(n => n.id == toId);
            bool hasConnection = false;

            if (nodeFrom.hotspots.Count() > 0) {

                nodeFrom.hotspots.ForEach(h => {
                    if (h.target == null)
                    {
                        return;   
                    }
                    if (h.target.id == toId) {
                        hasConnection = true;
                        return;
                    }
                });
            }


            if (!hasConnection) {
                // TODO change path
                string iconPath = $"Assets/Singularis/StackVR/Sprites/icons/ico_hotspot_location.png";
                Texture2D hotspotTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);

                HotspotData hotspot = ScriptableObject.CreateInstance<HotspotData>();
                hotspot.id = System.Guid.NewGuid().ToString();

                if (string.IsNullOrEmpty(nodeTo.name) && string.IsNullOrEmpty(nodeTo.name))
                {
                    hotspot.name = $"hotspot {nodeFrom.id}";
                }
                else
                {
                    hotspot.name = $"{nodeTo.name} - {nodeFrom.name}";
                }

                
                hotspot.type = HotspotData.HotspotType.location;
                hotspot.target = nodeTo;
                hotspot.icon = hotspotTexture;


                string assetPath = AssetDatabase.GetAssetPath(nodeFrom);
                string folderPath = Path.GetDirectoryName(assetPath);
                string filePath = Path.Combine(folderPath, $"{hotspot.name}.asset");


                int i = 0;
                while (File.Exists(filePath)) {
                    Debug.Log($"File Exists: {filePath}");
                    i++;
                    filePath = Path.Combine(folderPath, $"{hotspot.name} ({i}).asset");
                }
                Debug.Log($"Create Asset: {filePath}");
                AssetDatabase.CreateAsset(hotspot, Path.Combine(filePath));


                nodeFrom.hotspots.Add(hotspot);
            }
        }


        private NodeData GetNodeFromHostpot(string id) {
            foreach (var node in currentNarrative.nodes) {
                if (node.id == id) {
                    return node;
                }
            }
            return null;
        }



        private void CheckNodesPositions() {
            foreach (var node in totalNodes) {
                if (node.outPutPort.connected) {
                    foreach (var edge in node.outPutPort.connections) {
                        BaseNode inputNode = edge.input.node as BaseNode;
                        inputNode.inputPort.SetStyles(inputNode.inputPort.leftPos, node.outPutPort.topPos, true);

                    }
                }

                if (node.inputPort.connected) {
                    foreach (var edge in node.inputPort.connections) {
                        BaseNode outPutNode = edge.output.node as BaseNode;
                        outPutNode.outPutPort.SetStyles(outPutNode.outPutPort.leftPos, node.inputPort.topPos, true);

                    }
                }
            }
        }


        // Connect Ports to Nodes
        public void ConnectPort(BaseNode node) {
            Remove(currentLine);
            currentLine = null;
            ConnectNodes(node);
            isDrawingLine = false;
            Debug.Log("THe escale is " + defaultMinScale + defaultMaxScale);
            this.MarkDirtyRepaint();
        }

        // Draw Line to Connect Nodes
        public void ShowLine(Vector2 pos, BaseNode node) {
            if (this.inputNode == null) {
                this.inputNode = node;
                isDrawingLine = true;
                currentPos = this.WorldToLocal(pos);

                Vector2 newPos = new Vector2(node.style.left.value.value, node.style.top.value.value);

                Debug.Log(currentPos);
                currentPos = new Vector2(currentPos.x, currentPos.y);
            }
        }

        // Draw Line
        private void OnMouseMove(MouseMoveEvent evt) {
            if (currentLine == null) {
                if (!isDrawingLine)
                    return;
                var color = new Color(3f / 255f, 196f / 255f, 255f / 255f);


                Vector2 newTargetPos = this.WorldToLocal(evt.mousePosition);
                currentLine = new LineElement(1.5f, color, currentPos, newTargetPos);

                var gridBackground = this.Q<GridBackground>();
                int gridIndex = this.IndexOf(gridBackground);
                this.Insert(gridIndex + 1, currentLine);

                return;
            }

            Vector2 targetPos = this.WorldToLocal(evt.mousePosition);

            currentLine.ChangePosition(currentPos, targetPos);
            foreach (var node in nodes) {
                node.BringToFront();
                node.MarkDirtyRepaint();
            }
            this.MarkDirtyRepaint();
        }


        // Set DropDown Menu
        public void SetVisualElements(VisualElement dropDownMenu, VisualElement inspectorPanel, VisualElement nodeMenu, Button buttonCreateNode) {
            this.dropDownMenu = dropDownMenu;
            dropDownMenu.style.visibility = Visibility.Hidden;
            //inspectorPanel.style.visibility = Visibility.Hidden;
            this.inspectorPanel = inspectorPanel;
            this.nodeMenu = nodeMenu;
            this.nodeMenu.style.visibility = Visibility.Hidden;
            addImageButton = dropDownMenu.Q<VisualElement>("AddImage");

            toggleSteroscopic = inspectorPanel.Q<Toggle>("Steroscopic");
            // TODO usar boton en lugar de dropdown menu
            dropDownMenu.Q<VisualElement>("AddVideo").RegisterCallback<MouseDownEvent>((e) => {
                var position = contentViewContainer.WorldToLocal(mousePosition);
                BaseNode node = CreateVideoNode(position);
                NodeData nodeData = node.SaveAsset("");
                currentNarrative.nodes.Add(nodeData);
                AddElement(node);
            }, TrickleDown.TrickleDown);

            addImageButton.RegisterCallback<MouseDownEvent>((e) => {
                var position = contentViewContainer.WorldToLocal(mousePosition);

                BaseNode node = CreateImageNode(position);
                NodeData nodeData = node.SaveAsset(Path.GetDirectoryName(AssetDatabase.GetAssetPath(currentNarrative)));
                currentNarrative.nodes.Add(nodeData);
                AddElement(node);

                EditorUtility.SetDirty(currentNarrative);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

            }, TrickleDown.TrickleDown);


            this.buttonBuild = buttonCreateNode;
            buttonSave = this.Q<Button>("SaveButton");
            buttonSaveGraph = this.Q<Button>("ExportButton");
            buttonLoadGraph = this.Q<Button>("LoadGraph");

            buttonSave.RegisterCallback<ClickEvent>((e) => { OnButtonSave(); });


            buttonSaveGraph.RegisterCallback<ClickEvent>((e) => {
                Debug.Log("Saving Local Nodes");
                var pathFiles = CheckAllNodes(true);
                if (pathFiles.Length > 0) {
                    BuilderHelper.ExportFileAsZip(pathFiles);
                }
            });


            buttonBuild.RegisterCallback<ClickEvent>((e) => { OnButtonBuild(); });
            buttonLoadGraph.RegisterCallback<ClickEvent>((e) => { ImportNodes(); });
        }

        public void OnButtonSave() {
            CheckAllNodes(true);
        }

        public void OnButtonBuild() {

            Debug.Log($"Build current node: {AssetDatabase.GetAssetPath(currentNarrative)}");
            
            SceneGenerator.GenerateScene(currentNarrative);
        }


        // Show Menu of Nodes
        public void ShowNodeMenu(Vector2 newPos, BaseNode currentNode) {
            if (transitionButtonCallback != null)
                nodeMenu.Q<Button>("ConnectNodes").UnregisterCallback<ClickEvent>(transitionButtonCallback);

            if (deleteButtonCallback != null)
                nodeMenu.Q<Button>("DeleteNodes").UnregisterCallback<ClickEvent>(deleteButtonCallback);


            var position = this.WorldToLocal(newPos);
            nodeMenu.style.visibility = Visibility.Visible;
            nodeMenu.style.left = position.x;
            nodeMenu.style.top = position.y;
            selectedNode = currentNode;

            nodeMenu.Q<Button>("EditNodes").RegisterCallback<ClickEvent>((e) => {
                e.StopPropagation();
                HideNodeMenu();
                HideDropDownMenu();
                selectedNode.OnEnterNode();
            });
            nodeMenu.Q<Button>("SetInitialNode").RegisterCallback<ClickEvent>((e) => {
                e.StopPropagation();

                foreach (var node in totalNodes) {
                    node.DisableInitialNode();
                }

                currentNode.SetInitialNode();
                HideNodeMenu();
                HideDropDownMenu();
            });

            transitionButtonCallback = (e) => {
                Debug.Log("Making Transition");
                e.StopPropagation();
                selectedNode.OnMouseDown(newPos);

                HideNodeMenu();
                HideDropDownMenu();
            };

            deleteButtonCallback = (e) => {
                e.StopPropagation();
                DestroyNode(currentNode);
                HideNodeMenu();
                HideDropDownMenu();
            };

            nodeMenu.Q<Button>("ConnectNodes").RegisterCallback<ClickEvent>(transitionButtonCallback);
            nodeMenu.Q<Button>("DeleteNodes").RegisterCallback<ClickEvent>(deleteButtonCallback);
        }

        private void HideNodeMenu() {
            nodeMenu.style.visibility = Visibility.Hidden;
            nodeMenu.MarkDirtyRepaint();
        }


        private void HideDropDownMenu() {
            dropDownMenu.style.visibility = Visibility.Hidden;
            dropDownMenu.MarkDirtyRepaint();
        }


        private GraphViewChange OnGraphVieCahnge(GraphViewChange change) // Callback when you change a node to clear the edges
        {
            if (change.elementsToRemove != null) {
                foreach (var element in change.elementsToRemove) {
                    if (element is Node) // Deleting Edges
                    {
                        var newNode = element as Node;
                        foreach (var edge in edges.ToList()) {
                            if (edge.input.node == newNode || edge.output.node == newNode) {
                                RemoveElement(edge);
                            }
                        }
                    }
                }

            }

            return change;
        }


        // Getting Ports 
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            List<Port> compatiblesPorts = new();

            ports.ForEach(port => {
                if (startPort == port)// Current Port
                    return;

                if (startPort.node == port.node)
                    return;

                if (startPort.direction == port.direction)
                    return;

                compatiblesPorts.Add(port);
            });

            return compatiblesPorts;
        }


        public string[] CheckAllNodes(bool isLocalSave = false) {
            if (edges.Count() <= 0) {
                Debug.Log("Not Edges in Graph");
                return Array.Empty<string>();
            }
            nodesData.Clear();
            List<BaseNode> currentNodes = new();

            BaseNode firstNode;

            if (edges.Count() == 0) {
                firstNode = nodes.First() as BaseNode;
            }
            else {
                firstNode = edges.First().output.node as BaseNode;
            }

            currentNodes.Add(firstNode);
            foreach (Node node in nodes) {
                if (node is BaseNode baseNode) {
                    currentNodes.Add(baseNode);
                }
            }
            currentNodes = currentNodes.Distinct().ToList();

            string nodeId = "";
            List<string> pathFiles = new();
            foreach (var node in currentNodes) {
                if (node.isFirstElement) {
                    nodeId = node.id;
                }
                var nodeData = node.GetNode(isLocalSave);
                pathFiles.Add(nodeData.resource.path);

                nodesData.Add(nodeData);
            }


            // TODO check duplicated code
            BuilderHelper.SaveJsonFile(ref pathFiles, nodeId, nodesData);


            Tour testExperience = new() {
                version = 1,
                date = "000",
                start = nodeId,
                nodes = nodesData,
                createAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                updateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };


            Debug.Log("The total edges are" + edges.Count());
            string resultJson = JsonConvert.SerializeObject(testExperience, Formatting.Indented);
            Debug.Log(resultJson);

            string resourcesPath = StackProjectConfig.currentNarrative.narrativeSavePath.Replace("yaml", "json");
            File.WriteAllText(resourcesPath, resultJson);
            Debug.Log($"?? JSON guardado en: {resourcesPath}");

            // ?? Actualizar el sistema de archivos en Unity para que reconozca el nuevo archivo
            AssetDatabase.Refresh();
            pathFiles.Add(resourcesPath);

            return pathFiles.ToArray();
        }

        public BaseNode CreateImageNode(Vector2 position) // Method to create An Image Node
        {

            ImageNode node = new ImageNode();
            node.Initialize(position, this, inspectorPanel);
            node.id = System.Guid.NewGuid().ToString();
            node.Draw();            
            totalNodes.Add(node);
            nodeDictionary.Add(node.id.ToString(), node);

            currentNode++;
            HideDropDownMenu();
            return node;
        }


        // Create Node
        public BaseNode CreateNode(Vector2 nodePos, bool isVideoNode = false) {

            if (!isVideoNode) {
                nodePos = contentViewContainer.WorldToLocal(nodePos);
                var node = CreateImageNode(nodePos);

                AddElement(node);
                return node;
            }
            else {
                nodePos = contentViewContainer.WorldToLocal(nodePos);
                var node = CreateVideoNode(nodePos);
                AddElement(node);
                return node;
            }


        }

        // Create Video Node
        private BaseNode CreateVideoNode(Vector2 position) // Method to Create a Video Node
        {

            HideDropDownMenu();
            VideoNode node = new VideoNode();

            node.Initialize(position, this, inspectorPanel);
            node.id = System.Guid.NewGuid().ToString(); 
            node.Draw();            
            totalNodes.Add(node);
            nodeDictionary.Add(node.id.ToString(), node);
            Debug.Log("THe Nodes are" + currentNode);
            currentNode++;
            return node;
        }


        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            base.BuildContextualMenu(evt);

        }





        // SHow Contextual Menu
        private IManipulator CreateNodeContextualMenu() // Add Menu WHen User Clicks Right Button to Create New Nodes
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => {

                }
            );
            return contextualMenuManipulator;
        }

        // Create DropDownMenu
        public void CreateDropdownMenu(Vector2 newPos) {
            Debug.Log(newPos);
            var position = this.WorldToLocal(newPos);
            dropDownMenu.style.visibility = Visibility.Visible;
            dropDownMenu.style.left = position.x;
            dropDownMenu.style.top = position.y;

        }



        private void AddManipulators() // Manipulators to allow move zoom and drag elements in the node
        {
            //SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            //defaultMinScale = ContentZoomer.DefaultMinScale;
            //defaultMaxScale = ContentZoomer.DefaultMaxScale;
            //this.AddManipulator(new ContentDragger());
            //this.AddManipulator(new SelectionDragger());

            ////this.AddManipulator(new RectangleSelector());
            //this.AddManipulator(CreateNodeContextualMenu());



            //CÓDIGO ADICIONAL
            // Valores personalizados (ejemplo: zoom out 0.1x, zoom in 3x)
            float minScale = 0.1f;  // Zoom mínimo (alejarse más)
            float maxScale = 3f;    // Zoom máximo (acercarse más)

            SetupZoom(minScale, maxScale); // ¡Aquí aplicas los nuevos límites!

            // 1. Selector rectangular
            //this.AddManipulator(new RectangleSelector());

            // 2. Manipuladores de arrastre
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new RectangleSelector());


            // 3. Menú contextual
            this.AddManipulator(CreateNodeContextualMenu());
            //---------------------------------



        }

        private void AddStyles() // Add Style sheet to graph
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.singularisvr.stackvr/Editor/UIBUilder/NarrativeEditorWindow.uss");
            styleSheets.Add(styleSheet);
        }

        public void DestroyNode(BaseNode node) {
            // Find and remove all edges connected to the node
            var connectedEdges = edges.ToList().Where(edge =>
                edge.input.node == node || edge.output.node == node).ToList();


            for (int i = 0; i < currentNarrative.nodes.Count; i++)
            {
                if (currentNarrative.nodes[i].id == node.id)
                {
                    string path = AssetDatabase.GetAssetPath(currentNarrative.nodes[i]);
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    currentNarrative.nodes.RemoveAt(i);
                }

            }
          

            foreach (var edge in connectedEdges) {
                RemoveElement(edge);
            }
            totalNodes.Remove(node);
            RemoveElement(node);
            

        }


        public void EditNodes(string narrativePath) {
            currentNarrative = AssetDatabase.LoadAssetAtPath<NarrativeScriptableObject>(narrativePath);
            if (currentNarrative == null) {
                Debug.Log("Narrative Not Found");
                return;
            }

            foreach (var node in currentNarrative.nodes) {
                Vector2 posNode = new Vector2(node.posX, node.posY);
                NodeData currentNode = new NodeData(); // TODO update code to prevent warning


                if (node.type == NodeData.NodeType.Image) {
                    ImageNode imageNode = CreateNode(posNode, false) as ImageNode;

                    if (node.image != null)
                        imageNode.UpdateImage(node.image as Texture2D);

                    imageNode.id = node.id;
                    imageNode.north = node.north;
                    imageNode.ChangeLabelText(node.name);
                }
                else {
                    VideoNode videoNode = CreateNode(posNode, true) as VideoNode;



                    Debug.Log($"[GraphViewExperiences] EditNodes - node video");
                    if (node.image != null)
                    {
                        videoNode.UpdateVideo(node.image as Texture2D);
                        videoNode.UpdateImage(node.image as Texture2D);
                    }

                    videoNode.north = node.north;
                    videoNode.id = node.id;
                    videoNode.ChangeLabelText(node.name);

                    NodeData nodeData = this.currentNarrative.nodes.Find(node => node.id == videoNode.id);
                    nodeData.image = videoNode.currentTexture;
                }

            }


            foreach (var node in currentNarrative.nodes) {
                BaseNode currentNode = totalNodes.Find(e => e.id == node.id);

                foreach (var hotspot in node.hotspots) {
                    if (hotspot.type == HotspotData.HotspotType.location) {

                        Debug.Log("Checking Nodes" + totalNodes.Count);

                        if (hotspot.target != null)
                        {
                            BaseNode newNode = totalNodes.Find(e => e.id == hotspot.target.id);

                            if (newNode != null)
                            {
                                ConnectTwoNodes(currentNode, newNode);
                            }

                        }

                        

                        
                    }
                }

            }


        }

        public async void ImportNodes() {
            if (currentNarrative == null) {
                Debug.Log("Narrative Not Found");
                return;

            }

            currentNarrative.nodes.Clear();
            importedHostpots.Clear();
            if (nodes.Count() > 0) {
                DeleteElements(nodes);
                DeleteElements(edges);
                totalNodes.Clear();
                this.MarkDirtyRepaint();
            }



            string zipPath = EditorUtility.OpenFilePanel("Select ZIP File", "", "zip");

            if (!string.IsNullOrEmpty(zipPath)) {

                string parentFolder = Path.Combine(Application.dataPath, "ImportedFiles");
                string uniqueFolderName = "Import_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string importFolder = Path.Combine(parentFolder, uniqueFolderName);

                if (Directory.Exists(importFolder))
                    Directory.Delete(importFolder, true);

                Directory.CreateDirectory(importFolder);

                using (ZipArchive archive = ZipFile.OpenRead(zipPath)) {
                    bool hasJson = archive.Entries.Any(entry => entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

                    if (!hasJson) {
                        Debug.LogError("El archivo ZIP no contiene un archivo JSON. Importación cancelada.");
                        return;
                    }


                    foreach (ZipArchiveEntry entry in archive.Entries) {
                        string fullPath = Path.Combine(importFolder, entry.FullName);

                        // Si es un directorio, lo creamos
                        if (entry.FullName.EndsWith("/")) {
                            Directory.CreateDirectory(fullPath);
                            continue;
                        }

                        // Asegurar que la carpeta del archivo existe
                        string directoryPath = Path.GetDirectoryName(fullPath);
                        if (!Directory.Exists(directoryPath)) {
                            Directory.CreateDirectory(directoryPath);
                        }

                        // Si el archivo ya existe, agregar un sufijo numérico
                        string fileName = Path.GetFileName(fullPath);
                        string fileDirectory = Path.GetDirectoryName(fullPath);
                        string destinationPath = fullPath;
                        int count = 1;

                        while (File.Exists(destinationPath)) {
                            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                            string extension = Path.GetExtension(fileName);
                            destinationPath = Path.Combine(fileDirectory, $"{fileNameWithoutExt}_{count}{extension}");
                            count++;
                        }

                        // Extraer archivo con el nuevo nombre si es necesario
                        entry.ExtractToFile(destinationPath);
                    }
                }



                //ZipFile.ExtractToDirectory(zipPath, importFolder);
                Debug.Log($"ZIP extraído en: {importFolder}");

                // Buscar el archivo JSON dentro de la carpeta extraída
                string[] jsonFiles = Directory.GetFiles(importFolder, "*.json", SearchOption.AllDirectories);

                if (jsonFiles.Length == 0) {
                    Debug.LogError("No se encontró ningún archivo JSON en el ZIP.");
                    return;
                }
                AssetDatabase.Refresh();
                string jsonPath = jsonFiles[0]; // Tomar el primer JSON encontrado
                string jsonContent = File.ReadAllText(jsonPath);
                // Read the contents of the file


                // Process the JSON content (example: log it)
                Debug.Log($"JSON Content: {jsonContent}");

                // Optionally, deserialize it into a class
                var resultJson = JsonConvert.DeserializeObject<Tour>(jsonContent);


                string pathNode = Path.Combine("Assets", "ImportedFiles", uniqueFolderName);
                AssetDatabase.CreateFolder(pathNode, "Nodes");
                AssetDatabase.Refresh();
                pathNode = Path.Combine("Assets", "ImportedFiles", uniqueFolderName, "Nodes");


                string pathHostpot = Path.Combine("Assets", "ImportedFiles", uniqueFolderName);
                AssetDatabase.CreateFolder(pathHostpot, "Hostpots");
                AssetDatabase.Refresh();
                pathHostpot = Path.Combine("Assets", "ImportedFiles", uniqueFolderName, "Hostpots");



                int index = 0;

                foreach (var node in resultJson.nodes) {

                    string fileName = Path.GetFileName(node.resource.path);
                    Debug.Log(fileName); // Esto imprimirá "7.jpg" y "167-200x302.jpg"

                    Vector2 posNode = new Vector2(node.xPos, node.yPos);
                    NodeData currentNode = new NodeData();


                    if (node.type.Equals("image")) {

                        node.resource.path = Path.Combine("Assets", "ImportedFiles", uniqueFolderName, "Images", fileName); // Nueva ruta de las imagenes

                        Debug.Log("The New path is" + node.resource.path);
                        Debug.Log("IS An Image");

                        ImageNode imageNode = CreateNode(posNode, false) as ImageNode;

                        if (!string.IsNullOrEmpty(node.resource.path)) {
                            Texture2D image = AssetDatabase.LoadAssetAtPath<Texture2D>(node.resource.path);
                            imageNode.UpdateImage(image);
                        }

                        imageNode.id = node.id;
                        imageNode.north = node.north;
                        imageNode.ChangeLabelText(node.name);

                        currentNode = imageNode.SaveAsset(pathNode);
                        currentNarrative.nodes.Add(currentNode);
                    }
                    else {
                        Debug.Log("Is A Video");

                        node.resource.path = Path.Combine("Assets", "ImportedFiles", uniqueFolderName, "Videos", fileName); // Nueva ruta del video

                        Debug.Log("The New path is" + node.resource.path);

                        VideoNode videoNode = CreateNode(posNode, true) as VideoNode;

                        if (!string.IsNullOrEmpty(node.resource.path)) {
                            VideoClip video = AssetDatabase.LoadAssetAtPath<VideoClip>(node.resource.path);

                            string pathToVideo = AssetDatabase.GetAssetPath(video);

                            Debug.Log("The path tho video is " + pathToVideo);

                            var result = await videoNode.GetVideoImage(videoNode.id, pathToVideo);
                            string newFileName = Path.GetFileName(result);
                            string filePath = $"Assets/Singularis/StackVR/ImageVideos/{newFileName}";
                            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);


                            if (node.isSteroscopic) {
                                Debug.Log("Cut Texture");
                                var readeableTexture = CreateReadableTexture(texture);
                                texture = CutTextureInHalf(readeableTexture);
                            }

                            videoNode.UpdateVideo(texture);
                        }

                        videoNode.north = node.north;
                        videoNode.id = node.id;
                        videoNode.ChangeLabelText(node.name);
                        //var resutl = await videoNode.GetVideoImage();
                        currentNode = videoNode.SaveAsset(pathNode);
                        currentNarrative.nodes.Add(currentNode);
                        NodeData nodeData = currentNarrative.nodes.Find(node => node.id == videoNode.id);

                        nodeData.image = videoNode.currentTexture;
                    }



                    if (node.hotspots.Count > 0) {
                        List<HotspotData> currentHostpots = new List<HotspotData>();

                        foreach (var hostpot in node.hotspots) {
                            var newHotspot = CreateHostpot(hostpot, pathHostpot, index);
                            currentHostpots.Add(newHotspot);
                            index++;

                        }

                        currentNode.hotspots = currentHostpots;
                    }

                }

                // Set targets to Node

                foreach (var hostpot in importedHostpots) {
                    foreach (var node in currentNarrative.nodes) {
                        if (hostpot.targetId == node.id) {
                            hostpot.target = node;
                            break;
                        }
                    }
                }

                foreach (var node in resultJson.nodes) {
                    BaseNode currentNode = totalNodes.FirstOrDefault(e => e.id == node.id);

                    if (node.output != null) {
                        foreach (var output in node.output) {
                            foreach (var newNode in totalNodes) {
                                if (newNode.id == output) {
                                    ConnectTwoNodes(currentNode, newNode);
                                }
                            }

                        }
                    }

                }


                // TODO code duplicaded NarrativesHelper
                NarrativeScriptableObject newNarrative = ScriptableObject.CreateInstance<NarrativeScriptableObject>();
                newNarrative.date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                newNarrative.version = 1;
                newNarrative.name = "New Narrative";
                newNarrative.nodes = currentNarrative.nodes;
                



                string pathNarrative = Path.Combine("Assets", "ImportedFiles", uniqueFolderName, $"New Narrrative.asset");
                AssetDatabase.CreateAsset(newNarrative, pathNarrative);
                AssetDatabase.Refresh(); // Crear Nueva Narrativa                
                StackProjectConfig stackProject = StackProjectConfig.currentNarrative;
                stackProject.narrativeScriptableObject = newNarrative;
                Debug.Log("Creating New Narrative");
            }
            else {
                Debug.LogWarning("No file selected.");
            }

        }


     



        public void RefreshToggle() {
            toggleSteroscopic.value = false;
        }



        private HotspotData CreateHostpot(HotspotDataJson data, string filePath, int index) {
            HotspotData hotspot = ScriptableObject.CreateInstance<HotspotData>();
            hotspot.id = data.id;
            hotspot.name = data.name;
            hotspot.type = HotspotData.HotspotType.location;        //hotspot.target = nodeTo;
            hotspot.targetId = data.nodeId;                                                       //  


            string fullPath = Path.Combine(filePath, $"{data.name}{index}.asset");

            AssetDatabase.CreateAsset(hotspot, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            importedHostpots.Add(hotspot);
            return hotspot;
        }


        private void AddGridBackground() // Create Grid Background
        {
            GridBackground gridBackground = new GridBackground();
            Insert(0, gridBackground);
        }


    }
}
