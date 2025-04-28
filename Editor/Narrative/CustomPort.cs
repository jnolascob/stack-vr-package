using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace Singularis.StackVR.Narrative.Editor {
    public class CustomPort : Port {

        public GraphViewExperiences graphViewExperience;
        public Vector2 currentMousePosition;
        public Color currentColor;
        public bool isInputNode;
        public BaseNode targetNode;
        public float leftPos;
        public float topPos;

        public CustomPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type, GraphViewExperiences graphView)
            : base(portOrientation, portDirection, portCapacity, type) {


            this.graphViewExperience = graphView;
            this.portColor = Color.gray;
            this.RegisterCallback<MouseUpEvent>(OnPortDeselected);
            this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            this.RegisterCallback<ClickEvent>((e) => {
                Debug.Log("You Clicked the node");
            });

            OnPortSelected();

            m_EdgeConnector = new EdgeConnector<CustomEdge>(new EdgeConnectorListener(graphViewExperience, this));
            this.AddManipulator(m_EdgeConnector);
        }


        public void SetStyles(float leftPos, float topPos, bool inputNode) {
            this.leftPos = leftPos;
            this.topPos = topPos;
            isInputNode = inputNode;
            var image = new Texture2D(1, 1);

            if (!inputNode) 
                image = Resources.Load<Texture2D>("output");
            else
                image = Resources.Load<Texture2D>("input");
            

            this.Q<VisualElement>("connector").style.backgroundImage = null;

            var capElement = this.Q<VisualElement>("cap");
            currentColor = capElement.style.unityBackgroundImageTintColor.value;

            this.style.position = Position.Absolute;
            this.style.left = leftPos;
            //this.style.right = -100;
            this.style.top = topPos; // Position the port statically

            // Explicitly set port size for visibility
            this.style.width = 20;
            this.style.height = 20;

            capElement.style.backgroundImage = image;
            capElement.style.borderLeftWidth = 5;
            capElement.style.borderRightWidth = 5;
            capElement.style.borderTopWidth = 5;
            capElement.style.borderBottomWidth = 5;

            capElement.style.borderBottomLeftRadius = 100;
            capElement.style.borderBottomRightRadius = 100;
            capElement.style.borderTopLeftRadius = 100;
            capElement.style.borderTopRightRadius = 100;
            this.style.visibility = Visibility.Hidden;
            this.MarkDirtyRepaint();
        }


        public static CustomPort Create(Orientation orientation, Direction direction, Capacity capacity, Type type, GraphViewExperiences graphView) {
            return new CustomPort(orientation, direction, capacity, type, graphView);
        }


        public void OnMouseMove(MouseMoveEvent mouseMove) {
            currentMousePosition = mouseMove.mousePosition;
        }


        public override void OnStartEdgeDragging() {
            //base.OnStartEdgeDragging();
            Debug.Log("Starting Dragging Edge");
        }

        public override void OnStopEdgeDragging() {
            // base.OnStopEdgeDragging();
            Debug.Log("End Dragging");
        }

        public void OnPortSelected() {
            var image = new Texture2D(1, 1);

            if (isInputNode)
                image = Resources.Load<Texture2D>("inputSelected");
            else
                image = Resources.Load<Texture2D>("outputSelected");

            var capElement = this.Q<VisualElement>("cap");
            capElement.style.backgroundImage = image;
            portColor = new Color(3f / 255f, 196f / 255f, 255f / 255f);
            MarkDirtyRepaint();

        }

        public void OnPortDeselected(MouseUpEvent evt) {
            Debug.Log("Port Deselected " + this.name);
            var image = new Texture2D(1, 1);
            if (isInputNode)
                image = Resources.Load<Texture2D>("input");
            else
                image = Resources.Load<Texture2D>("output");

            Debug.Log("Port selected: " + this.name);
            var capElement = this.Q<VisualElement>("cap");
            capElement.style.backgroundImage = image;
        }


        public override void Connect(Edge edge) {
            base.Connect(edge);
        }

        public override void Disconnect(Edge edge) {
            base.Disconnect(edge);
            Debug.Log("Port disconnected: " + this.name);
        }
    }

    
}