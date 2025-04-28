using UnityEngine;
using UnityEngine.UIElements;
using Singularis.StackVR.Narrative.Editor;

namespace Singularis.StackVR.UIBuilder.Editor {
    public class LineElement : VisualElement {
        private float _width;
        private Color _color;
        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private VisualElement arrowElement;

        public LineElement(float width, Color color, Vector2 startPosition, Vector2 endPosition) {
            // Asignar los valores iniciales
            _width = width;
            _color = color;
            _startPosition = startPosition;
            _endPosition = endPosition;

            // Suscribirse a generateVisualContent
            generateVisualContent += OnGenerateVisualContent;

            // Crear el elemento de flecha
            arrowElement = new VisualElement {
                style =
                {
                width = 20,
                height = 20,
                position = Position.Absolute
            }
            };

            this.Add(arrowElement);
            this.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);

            this.focusable = true;
            this.Focus();

            this.RegisterCallback<KeyDownEvent>((e) => {

                if (e.keyCode == KeyCode.Escape) {
                    var graphView = this.GetFirstAncestorOfType<GraphViewExperiences>();
                    graphView.RemoveLine();
                }


            });
        }



        private void OnGeometryChangedEvent(GeometryChangedEvent e) {
            this.focusable = true;
            this.Focus();
            PositionArrow();
            arrowElement.MarkDirtyRepaint();
        }

        private void PositionArrow() {
            // Calcular el punto medio de la línea
            Vector2 midPoint = (_startPosition + _endPosition) / 2;

            // Calcular el ángulo de la línea en radianes y convertirlo a grados
            float angle = Mathf.Atan2(_endPosition.y - _startPosition.y, _endPosition.x - _startPosition.x) * Mathf.Rad2Deg;

            // Posicionar el elemento de la flecha en el punto medio
            arrowElement.style.left = midPoint.x - arrowElement.resolvedStyle.width / 2;
            arrowElement.style.top = midPoint.y - arrowElement.resolvedStyle.height / 2;

            // Rotar la flecha para alinearla con la dirección de la línea
            arrowElement.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        void OnGenerateVisualContent(MeshGenerationContext mgc) {
            var painter = mgc.painter2D;

            // Dibujar la línea
            painter.strokeColor = _color;
            painter.lineWidth = _width;
            painter.lineCap = LineCap.Round;
            painter.BeginPath();
            painter.MoveTo(_startPosition);
            painter.LineTo(_endPosition);
            painter.Stroke();

            // Dibujar la flecha
            Vector2 p0 = _startPosition;
            Vector2 p1 = Vector2.Lerp(_startPosition, _endPosition, 0.33f);
            Vector2 p2 = Vector2.Lerp(_startPosition, _endPosition, 0.66f);
            Vector2 p3 = _endPosition;

            float t = 0.5f;
            Vector2 midPoint = CalculateBezierPoint(t, p0, p1, p2, p3);
            Vector2 directionToEnd = (p0 - midPoint).normalized;

            float arrowLength = 10f;
            float arrowWidth = 5f;

            Vector2 leftWing = midPoint + (Vector2)(Quaternion.Euler(0, 0, -25) * directionToEnd * arrowLength);
            Vector2 rightWing = midPoint + (Vector2)(Quaternion.Euler(0, 0, 25) * directionToEnd * arrowLength);

            painter.fillColor = _color;
            painter.BeginPath();
            painter.MoveTo(midPoint);
            painter.LineTo(leftWing);
            painter.LineTo(rightWing);
            painter.ClosePath();
            painter.Fill();
        }

        private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 point = (uuu * p0);
            point += (3 * uu * t * p1);
            point += (3 * u * tt * p2);
            point += (ttt * p3);

            return point;
        }

        public LineElement ChangePosition(Vector2 startPosition, Vector2 endPosition) {
            _startPosition = startPosition;
            _endPosition = endPosition;
            MarkDirtyRepaint();
            return this;
        }

        public LineElement ChangeColor(Color color) {
            _color = color;
            MarkDirtyRepaint();
            return this;
        }

        public LineElement ChangeWidth(float width) {
            _width = width;
            MarkDirtyRepaint();
            return this;
        }
    }
}
