using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Singularis.StackVR.Narrative.Editor {
    // Interface to read edge connections
    public class EdgeConnectorListener : IEdgeConnectorListener {
        public GraphViewExperiences graphView;
        public CustomPort custonPort;

        public EdgeConnectorListener(GraphViewExperiences graphView, CustomPort custonPort) {
            this.graphView = graphView;
            this.custonPort = custonPort;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position) {

        }

        public void OnDrop(GraphView graphView, Edge edge) {
            Debug.Log("Edge dropped");
            graphView.AddElement(edge);

            foreach (var node in graphView.nodes) {
                var newNode = node as BaseNode;
                foreach (var port in newNode.ports) {
                    port.portColor = Color.blue;
                    port.MarkDirtyRepaint();
                }
            }

            foreach (var newEdge in graphView.edges) {
                newEdge.input.portColor = Color.gray;
                newEdge.output.portColor = Color.gray;
                newEdge.edgeControl.inputColor = Color.gray;
                newEdge.edgeControl.outputColor = Color.gray;
                newEdge.MarkDirtyRepaint();
                newEdge.edgeControl.MarkDirtyRepaint();
                newEdge.edgeControl.UpdateLayout();
            }
        }
    }
}
