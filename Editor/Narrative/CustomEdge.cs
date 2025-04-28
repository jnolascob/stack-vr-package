using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Singularis.StackVR.Narrative.Editor {
    public class CustomEdge : Edge {

        private VisualElement arrowElement;
        private VisualElement midBox;
        public bool isArrowSelected;
        public Color currentColor;


        public CustomEdge() {
            var image = new Texture2D(1, 1);
            image = Resources.Load<Texture2D>("Arrow");

            arrowElement = new VisualElement();
            arrowElement.style.width = 20;
            arrowElement.style.height = 20;
            arrowElement.generateVisualContent += OnGenerateVisualContent;
            arrowElement.style.position = Position.Absolute;
            this.Add(arrowElement);
            RegisterCallback<MouseUpEvent>(OnEdgeDisconnected);
            edgeControl.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseOutEvent>((e) => { OnMouseOutEvent(); });
        }


        void OnKeyDown(KeyDownEvent evt) {
            if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace) {
                input?.Disconnect(this);
                output?.Disconnect(this);
                var graphView = this.GetFirstAncestorOfType<GraphViewExperiences>();
                graphView.RemoveElement(this);
            }
        }

        private void OnMouseOutEvent() {
            input.portColor = Color.gray;
            output.portColor = Color.gray;

            currentColor = Color.gray;

            input.MarkDirtyRepaint();
            output.MarkDirtyRepaint();

            edgeControl.inputColor = Color.gray;
            edgeControl.outputColor = Color.gray;
            this.MarkDirtyRepaint();
            edgeControl.UpdateLayout();
        }

        private void OnMouseDown(MouseDownEvent e) {
            input.portColor = new Color(3f / 255f, 196f / 255f, 255f / 255f);
            output.portColor = new Color(3f / 255f, 196f / 255f, 255f / 255f);
            var customImputPort = input as CustomPort;
            var customOutputPort = output as CustomPort;
            currentColor = new Color(3f / 255f, 196f / 255f, 255f / 255f);
            arrowElement.MarkDirtyRepaint();

            input.MarkDirtyRepaint();
            output.MarkDirtyRepaint();
            input.ConnectTo(output);

            edgeControl.inputColor = new Color(3f / 255f, 196f / 255f, 255f / 255f);
            edgeControl.outputColor = new Color(3f / 255f, 196f / 255f, 255f / 255f);
            this.MarkDirtyRepaint();
            edgeControl.UpdateLayout();

            arrowElement.MarkDirtyRepaint();
        }

        public override void OnPortChanged(bool isInput) {
            base.OnPortChanged(isInput);
        }

        private void OnGeometryChangedEvent(GeometryChangedEvent e) {
            arrowElement.MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc) {
            var painter = mgc.painter2D;
            painter.BeginPath();


            // Get control points for the Bezier curve (P0 = start, P3 = end, P1 & P2 = control points)
            Vector2 p0 = edgeControl.controlPoints[0];
            Vector2 p1 = edgeControl.controlPoints[1];
            Vector2 p2 = edgeControl.controlPoints[2];
            Vector2 p3 = edgeControl.controlPoints[3];

            // Calculate the midpoint on the Bezier curve at t = 0.9 (adjust for arrow position)
            Vector2 midPoint = CalculateBezierPoint(0.5f, p0, p1, p2, p3); // Closer to the end of the edge

            // Get the target point (end of the edge)
            Vector2 targetPoint = p1; // The last control point is the target

            // Calculate the direction vector from the midpoint to the target
            Vector2 directionToTarget = (targetPoint - midPoint).normalized;

            // Calculate the angle in degrees from the x-axis
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

            // Draw the arrow
            float arrowLength = 10; // Length of the arrow      

            // Left side of the arrow
            Vector2 arrowPoint1 = midPoint + (Vector2)(Quaternion.Euler(0, 0, -25) * directionToTarget * arrowLength); // Left side of the arrow
            Vector2 arrowPoint2 = midPoint + (Vector2)(Quaternion.Euler(0, 0, +25) * directionToTarget * arrowLength);   // Right side of the arrow


            float arrowOffset = -3f; // Adjust this value to move the arrow above the edge
            midPoint.y += arrowOffset;
            arrowPoint1.y += arrowOffset;
            arrowPoint2.y += arrowOffset;


            // Draw the arrowhead
            // Move to midpoint and draw the arrow lines
            painter.fillColor = currentColor;

            painter.MoveTo(midPoint);
            painter.LineTo(arrowPoint1);
            painter.LineTo(arrowPoint2);
            painter.ClosePath();
            painter.Fill();  // Fill the arrow shape
        }


        private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            // Calculate the point on the Bezier curve at parameter t
            Vector2 point = uuu * p0;         // (1 - t)^3 * P0
            point += 3 * uu * t * p1;         // 3 * (1 - t)^2 * t * P1
            point += 3 * u * tt * p2;         // 3 * (1 - t) * t^2 * P2
            point += ttt * p3;                // t^3 * P3

            return point;
        }

        private Vector2 CalculateBezierTangent(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) {
            float u = 1 - t;

            // Derivative of the Bezier curve function to get the tangent vector
            Vector2 tangent = 3 * u * u * (p1 - p0) +
                              6 * u * t * (p2 - p1) +
                              3 * t * t * (p3 - p2);

            return tangent.normalized;
        }

        private void OnEdgeDisconnected(MouseUpEvent evt) {

        }

        public override void OnSelected() {
            this.focusable = true;
            this.Focus();
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        public override void OnUnselected() {
            base.OnUnselected();
            currentColor = Color.gray;
            arrowElement.MarkDirtyRepaint();
        }

    }
}
