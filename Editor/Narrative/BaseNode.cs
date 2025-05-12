using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Singularis.StackVR.Editor;
using Singularis.StackVR.Scriptables.Editor;
using Singularis.StackVR.UIBuilder.Editor;

namespace Singularis.StackVR.Narrative.Editor {
    public enum KindOfNode {
        image,
        video,
    }

    public class MyScritable : ScriptableObject {
        public string testName;
    }

    // Base Class de los nodos 
    public class BaseNode : Node {
        public int id;
        protected ObjectField texturePicker;
        protected Image nodeImage;
        protected KindOfNode kindOfNode;
        protected EnumField nodeTypeField;
        protected string pathImage = "";
        public CustomPort outPutPort;
        public CustomPort inputPort;
        private GraphViewExperiences graphViewExperiences;
        public string nodeTextData;
        protected VisualElement inspectorPanel;
        public bool isFirstElement;
        private Color bgColor;
        private Color borderColor;
        public List<CustomPort> ports = new List<CustomPort>();
        public VisualElement border;
        //private bool isDrawing = false;
        public VisualElement buttonAddNode;
        public VisualElement buttonSetInitialNode;
        public VisualElement buttonRemoveNode;
        public Label titleNode;
        private Color blueColor = new Color(3f / 255f, 196f / 255f, 255f / 255f);
        private Color blackColor = new Color(45f / 255f, 45f / 255f, 45f / 255f);
        public VisualElement imageNode;
        public float north;
        public bool isSteroscopic;
        public bool isFull;
        public bool isLockedNode;
        public UnityEngine.Object currentImage;


        public BaseNode() : base("Packages/com.singularisvr.stackvr/Editor/UIBUilder/NodeElement.uxml") {

            var bg = this.Q<VisualElement>("Background");
            border = this.Q<VisualElement>("Border");
            buttonSetInitialNode = this.Q<VisualElement>("InitialNode");
            imageNode = this.Q<VisualElement>("NodeImage");
            if (imageNode == null) {
                Debug.Log("Not Image");
            }
            this.BringToFront();
            bgColor = bg.style.backgroundColor.value;
            borderColor = bg.style.backgroundColor.value;

            this.Q<VisualElement>("NodeImage").RegisterCallback<MouseUpEvent>((e) => { OnReleaseNode(); }, TrickleDown.TrickleDown);

            RegisterCallback<MouseOverEvent>((e) => {
                OnMouseHoverEvent();
            }, TrickleDown.TrickleDown);

            RegisterCallback<MouseDownEvent>((e) => {
                OnMouseDownEvent(e);

            }, TrickleDown.TrickleDown);

            buttonSetInitialNode.style.visibility = Visibility.Hidden;
            titleNode = this.Q<Label>();
            border.style.backgroundColor = blackColor;

            RegisterCallback<MouseOutEvent>((e) => { OnMouseOutEvent(); }, TrickleDown.TrickleDown);

            border.RegisterCallback<MouseDownEvent>(OnConnectNodes);

            RegisterCallback<KeyDownEvent>(OnKeyDown);
            this.RegisterCallback<MouseUpEvent>((e) => { OnReleaseNode(); }, TrickleDown.TrickleDown);
            //
            RegisterCallback<PointerDownEvent>((e) => {
                if (isLockedNode) {
                    e.StopPropagation();
                }
            }, TrickleDown.TrickleDown);
        }



        private void OnReleaseNode() {
            Debug.Log("On Release Node");
            graphViewExperiences.selectedNode = this;
        }

        void OnKeyDown(KeyDownEvent evt) {
            if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace) {
                graphViewExperiences.DestroyNode(this);
            }
        }


        private void OnMouseOutEvent() {
            var bg = this.Q<VisualElement>("Background");
            var border = this.Q<VisualElement>("Border");

            if (!graphViewExperiences.isDrawingLine) {
                bg.style.backgroundColor = blackColor;
                border.style.backgroundColor = blackColor;
                foreach (var node in graphViewExperiences.totalNodes) {
                    node.ChangeBorderColor();
                    node.inputPort.portColor = Color.gray;
                    node.outPutPort.portColor = Color.gray;
                }
            }
        }


        public void ChangeBorderColor() {
            var border = this.Q<VisualElement>("Border");
            border.style.backgroundColor = blackColor;
        }


        public void ChangeLabelText(string text) {
            if (titleNode == null) {
                Debug.Log("Title Node Null");
            }
            else {
                titleNode.text = text;
                titleNode.MarkDirtyRepaint();
            }
        }


        public void SetInitialNode() {
            Debug.Log("Set Initial Node");
            isFirstElement = true;

            buttonSetInitialNode.style.visibility = Visibility.Visible;
        }

        public void DisableInitialNode() {
            isFirstElement = false;
            buttonSetInitialNode.style.visibility = Visibility.Hidden;
        }

        // Connect Nodes
        private void OnConnectNodes(MouseDownEvent evt) {
            var bg = this.Q<VisualElement>("Background");
            if (graphViewExperiences.isDrawingLine) {
                bg.style.backgroundColor = blueColor;
                graphViewExperiences.ConnectPort(this);
            }
        }


        public void OnMouseDown(Vector2 mousePosition) // Draw Line
        {
            Debug.Log("Selecting Node");
            Vector2 newPos = this.LocalToWorld(mousePosition);
            graphViewExperiences.ShowLine(mousePosition, this);
            //isDrawing = true;
        }


        // On Hover Event
        private void OnMouseHoverEvent() {
            var bg = this.Q<VisualElement>("Background");
            border = this.Q<VisualElement>("Border");

            if (graphViewExperiences.isDrawingLine) {
                border.style.backgroundColor = blueColor;
            }
            else {
                border.style.backgroundColor = Color.gray;
            }
        }


        public void OnEnterNode() {
            Debug.Log($"[BaseNode] OnEnterNode: {id}");
            var currentNarrative = graphViewExperiences.currentNarrative;
            var node = currentNarrative.nodes.Find(n => n.id == id);
            string filePath = AssetDatabase.GetAssetPath(node);
            NodeInspectorWindow.ShowNodeInspector(AssetDatabase.AssetPathToGUID(filePath));
        }

        // Click Events
        private void OnMouseDownEvent(MouseDownEvent e) {
            if (e.clickCount == 2) {

                Debug.Log("Double Click");
                OnEnterNode();

            }

            if (e.button == 0) // Change Color
            {
                if (graphViewExperiences.isDrawingLine) {
                    return;
                }
                var bg = this.Q<VisualElement>("Background");
                var border = this.Q<VisualElement>("Border");


                border.style.backgroundColor = blueColor;
                graphViewExperiences.selectedNode = this;
            }
            else if (e.button == 1) // Show Menu Nodes
            {

                graphViewExperiences.selectedNode = this;
                graphViewExperiences.ShowNodeMenu(e.mousePosition, this);
            }

        }


        // Metodo para crear la UI del panel del inspector

        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type) {
            Debug.Log("Creating Ports");
            return Port.Create<CustomEdge>(orientation, direction, capacity, type);
        }

        // Metodo para desactivar el inspector al hacer click fuera de este


        public override void OnSelected() {
            base.OnSelected();
            Debug.Log("Selected Node");
            graphViewExperiences.selectedNode = this;

            graphViewExperiences.RefreshToggle();
            this.focusable = true;
            this.Focus();
        }

        public override void OnUnselected() {
            var bg = this.Q<VisualElement>("Background");
            var border = this.Q<VisualElement>("Border");

            graphViewExperiences.selectedNode = null;
            base.OnUnselected();
            Debug.Log("On Release");

            this.focusable = false;
        }


        // Metodo para Inicializar el Nodo
        public virtual void Initialize(Vector2 position, GraphViewExperiences graphView, VisualElement inspectorPanel) // Method to Initialize the Node and set the position
        {
            this.inspectorPanel = inspectorPanel;
            SetPosition(new Rect(position, Vector2.zero));
            graphViewExperiences = graphView;
            // Configurar el estilo del nodo
            style.width = 150;
            style.height = 150;
            this.BringToFront();
            // Registrar los eventos de mouse
        }

        public virtual void Draw() // Method to Draw the Node
        {
            // Set the node size
            style.width = 150;
            style.height = 150;
            style.borderTopWidth = 0;
            style.borderRightWidth = 0;
            style.borderBottomWidth = 0; style.borderLeftWidth = 0;


            // Make the node selectable, movable, and other capabilities
            capabilities |= Capabilities.Selectable | Capabilities.Movable | Capabilities.Deletable | Capabilities.Copiable;

            // Add the ports to the node (this might be redundant depending on your setup)
            CreatePorts();
            RegisterCallback<MouseEnterEvent>(evt => RemoveBorderOnHover());
            RegisterCallback<MouseLeaveEvent>(evt => RemoveBorderOnHover());
            RegisterCallback<FocusEvent>(evt => RemoveBorderOnHover());
            RegisterCallback<BlurEvent>(evt => RemoveBorderOnHover());
            RegisterCallback<ClickEvent>(evt => RemoveBorderOnHover());
        }



        // Create Ports
        private void CreatePorts() {
            inputPort = CustomPort.Create(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool), graphViewExperiences);
            inputPort.SetStyles(70, 80, true);
            outPutPort = CustomPort.Create(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool), graphViewExperiences);
            outPutPort.SetStyles(30, 50, true);

            inputPort.MarkDirtyRepaint();
            outPutPort.MarkDirtyRepaint();

            Add(inputPort);
            Add(outPutPort);


            MarkDirtyRepaint();
        }


        private void RemoveBorderOnHover() {
            style.borderTopWidth = 0;
            style.borderRightWidth = 0;
            style.borderBottomWidth = 0;
            style.borderLeftWidth = 0;

            style.borderTopColor = new StyleColor(Color.red);
            style.borderRightColor = new StyleColor(Color.clear);
            style.borderBottomColor = new StyleColor(Color.clear);
            style.borderLeftColor = new StyleColor(Color.clear);
            RemoveFromClassList("node-border");
            RemoveFromClassList("selection-border");
        }


        public void SetPath(string path) {
            pathImage = path;
        }


        public NodeDataOld GetNode(bool isLocalData = false) // Get Node Data To Save in the Json File
        {
            NodeDataOld nodeData = new NodeDataOld();
            nodeData.type = kindOfNode.ToString();
            nodeData.name = titleNode.text;
            nodeData.id = id;
            nodeData.north = north;
            nodeData.isSteroscopic = isSteroscopic;

            List<HotspotDataJson> hotspost = new List<HotspotDataJson>();

            foreach (var node in graphViewExperiences.currentNarrative.nodes) {
                if (node.id == id) {

                    nodeData.north = node.north;
                    foreach (var hostpot in node.hotspots) {
                        HotspotDataJson newHostpot = new HotspotDataJson();
                        newHostpot.id = hostpot.id;
                        newHostpot.iconPath = AssetDatabase.GetAssetPath(hostpot.icon);
                        newHostpot.hostpotType = hostpot.type.ToString();
                        newHostpot.angleX = hostpot.angleX;
                        newHostpot.angleY = hostpot.angleY;
                        newHostpot.distance = hostpot.distance;
                        newHostpot.scale = hostpot.scale;
                        newHostpot.name = hostpot.name;

                        switch (hostpot.type) {
                            case HotspotData.HotspotType.question:
                                //Debug.Log($"Hotspot Type Question: {hostpot.question}");
                                //newHostpot.question = hostpot.question;
                                //newHostpot.answerA = hostpot.answerA;
                                //newHostpot.answerB = hostpot.answerB;
                                //newHostpot.answerC = hostpot.answerC;
                                //newHostpot.correctAnswer = hostpot.correctAnswer;

                                break;
                            case HotspotData.HotspotType.location:

                                break;

                        }


                        if (hostpot.target != null) {
                            newHostpot.nodeId = hostpot.target.id;
                        }
                        else {
                            newHostpot.nodeId = -1;
                        }
                        hotspost.Add(newHostpot);

                    }
                    break;
                }

            }
            nodeData.hotspots = hotspost; // Setting Hostpots in Json


            if (isLocalData) {
                nodeData.xPos = this.style.left.value.value;
                nodeData.yPos = this.style.top.value.value;
            }

            var resource = new Resource();
            resource.type = kindOfNode.ToString();
            resource.path = pathImage;
            resource.id = id.ToString();
            nodeData.resource = resource;
            var outPutEdges = outPutPort.connections.ToList();

            var inputPorts = inputPort.connections.ToList();


            List<int> outputNodes = new List<int>();
            List<int> inputNodes = new List<int>();

            if (outPutEdges.Count > 0) {
                foreach (var edge in outPutEdges) {
                    if (edge.output.node is BaseNode) {
                        var testNode = edge.input.node as BaseNode;
                        outputNodes.Add(testNode.id);
                    }
                }
                nodeData.output = outputNodes.ToArray();
            }

            if (inputPorts.Count > 0) {
                foreach (var edge in inputPorts) {

                    if (edge.input.node is BaseNode) {
                        var testNode = edge.output.node as BaseNode;
                        inputNodes.Add(testNode.id);
                    }
                }
                nodeData.input = inputNodes.ToArray();

            }
            return nodeData;
        }

        public NodeData SaveAsset(string path = "", Texture2D texture = null) {
            NodeData newScriptable = ScriptableObject.CreateInstance<NodeData>();

            KindOfNode kindOfNode;
            var node = GetNode();
            newScriptable.id = node.id;
            newScriptable.name = node.name;
            newScriptable.north = node.north;

            Enum.TryParse<KindOfNode>(node.type, out kindOfNode);
            Debug.Log(kindOfNode);

            newScriptable.type = kindOfNode switch {
                KindOfNode.image => NodeData.NodeType.Image,
                KindOfNode.video => NodeData.NodeType.Video,
                _ => NodeData.NodeType.Image
            };

            if (kindOfNode == KindOfNode.video) {
                if (texture != null) {
                    newScriptable.image = texture;
                    Debug.Log("Setting Video Texture");
                    Debug.Log("The texture is " + texture.name);
                }
            }
            else {
                newScriptable.image = AssetDatabase.LoadAssetAtPath<Texture>(node.resource.path);
            }

            newScriptable.hotspots = new List<HotspotData>();
            string filePath = "";
            if (string.IsNullOrEmpty(path)) {
                //filePath = $"Assets/Singularis/StackVR/Scriptables/{newScriptable.name}_{newScriptable.id}.asset";
                filePath = Path.Combine(StackProjectConfig.currentNarrative.narrativeDirectoryPath, $"{newScriptable.name}_{newScriptable.id}.asset");
            }
            else {
                filePath = Path.Combine(path, $"{newScriptable.name}{newScriptable.id}.asset");
            }


            NodeData existingScriptable = AssetDatabase.LoadAssetAtPath<NodeData>(filePath);

            if (existingScriptable != null) {
                Debug.Log("Scriptable already exists");
                existingScriptable.id = node.id;
                existingScriptable.name = node.name;

                Enum.TryParse<KindOfNode>(node.type, out kindOfNode);
                existingScriptable.type = kindOfNode switch {
                    KindOfNode.image => NodeData.NodeType.Image,
                    KindOfNode.video => NodeData.NodeType.Video,
                    _ => NodeData.NodeType.Image
                };


                if (kindOfNode == KindOfNode.video) {
                    newScriptable.image = texture;
                    Debug.Log("Setting Video Texture");
                }
                else {
                    newScriptable.image = AssetDatabase.LoadAssetAtPath<Texture>(node.resource.path);
                }

                //existingScriptable.image = AssetDatabase.LoadAssetAtPath<Texture>(node.resource.path);
                //existingScriptable.hotspots = new List<HotspotData>();
                EditorUtility.SetDirty(existingScriptable); // Marca el asset como modificado

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return existingScriptable;
            }
            else {
                Debug.Log("Scriptable does not exist");
                AssetDatabase.CreateAsset(newScriptable, filePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return newScriptable;
            }

        }

    }
}
