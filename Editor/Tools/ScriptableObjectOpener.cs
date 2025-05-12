using System;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;
using Singularis.StackVR.Scriptables.Editor;
using Singularis.StackVR.UIBuilder.Editor;
using Singularis.StackVR.Narrative.Editor;

namespace Singularis.StackVR.Tools.Editor {
    public class ScriptableObjectOpener {

        private static readonly Dictionary<Type, Action<ScriptableObject>> handlers = new() {
            { typeof(HotspotData), asset => Debug.Log($"[ScriptableObjectOpener] Abriendo HotspotData: {asset.name}") },
            { typeof(NodeData), OnOpenNodeData },
            { typeof(NarrativeScriptableObject), OnOpenNarrative }
        };

        private static void OnOpenNarrative(ScriptableObject asset) {
            var narrative = asset as NarrativeScriptableObject;
            SimpleConsoleWindow.OpenWindow(narrative);
        }

        private static void OnOpenNodeData(ScriptableObject asset) {
            var nodeData = asset as NodeData;
            NodeInspectorWindow.OpenWindow(nodeData);
        }


        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line) {
            UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceID);

            if (obj is not ScriptableObject scriptable) return false; // Permite comportamiento por defecto para otros assets

            var type = scriptable.GetType();

            if (handlers.TryGetValue(type, out var action)) {
                action(scriptable);
                return true; // Evita abrir el inspector por defecto
            }

            return false; // Permite comportamiento por defecto para otros assets
        }
    }
}
