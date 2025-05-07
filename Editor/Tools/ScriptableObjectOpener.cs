using System;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;
using Singularis.StackVR.Scriptables.Editor;

namespace Singularis.StackVR.Tools.Editor {
    public class ScriptableObjectOpener {

        private static readonly Dictionary<Type, Action<ScriptableObject>> handlers = new() {
            { typeof(HotspotData), asset => Debug.Log($"[ScriptableObjectOpener] Abriendo HotspotData: {asset.name}") },
            { typeof(NodeData), asset => Debug.Log($"[ScriptableObjectOpener] Abriendo NodeData: {asset.name}") },
            { typeof(NarrativeScriptableObject), asset => Debug.Log($"[ScriptableObjectOpener] Abriendo NarrativeScriptableObject: {asset.name}") }
        };

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
