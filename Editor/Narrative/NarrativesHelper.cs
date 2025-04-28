using System.IO;
using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Singularis.StackVR.Editor;
using Singularis.StackVR.Scriptables.Editor;


namespace Singularis.StackVR.Narrative.Editor {
    public static class NarrativesHelper {

        public static void CreateNarrativeInGraph(List<NodeData> nodesData, string uniqueFolderName) {
            NarrativeScriptableObject newNarrative = ScriptableObject.CreateInstance<NarrativeScriptableObject>();
            newNarrative.date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            newNarrative.version = 1;
            newNarrative.name = "New Narrative";
            newNarrative.nodes = nodesData;

            string pathNarrative = Path.Combine("Assets", "ImportedFiles", uniqueFolderName, $"New Narrrative.asset");
            AssetDatabase.CreateAsset(newNarrative, pathNarrative);
            AssetDatabase.Refresh(); // Crear Nueva Narrativa                
            StackProjectConfig stackProject = StackProjectConfig.currentNarrative;
            stackProject.narrativeScriptableObject = newNarrative;
        }


        public static HotspotData CreateHostpot(NodeData node, string childName) {
            HotspotData hostpot = ScriptableObject.CreateInstance<HotspotData>();

            string assetPath = AssetDatabase.GetAssetPath(node);
            string folderPath = Path.GetDirectoryName(assetPath);
            string filePath = Path.Combine(folderPath, $"{childName}.asset");

            int i = 0;
            while (File.Exists(filePath)) {
                Debug.Log($"[NodeInspectorWindow] File Exists: {filePath}");
                i++;
                filePath = Path.Combine(folderPath, $"{childName} ({i}).asset");
            }

            Debug.Log($"[NodeInspectorWindow] Create Asset: {filePath}");
            AssetDatabase.CreateAsset(hostpot, Path.Combine(filePath));
            return hostpot;

        }

    }
}
