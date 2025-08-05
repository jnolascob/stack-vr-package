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


        public static HotspotData CreateHostpot(NodeData node, string childName, int indexHostpot) {
            HotspotData hostpot = ScriptableObject.CreateInstance<HotspotData>();
            
            hostpot.name = childName;   

            string assetPath = AssetDatabase.GetAssetPath(node);
            string folderPath = Path.GetDirectoryName(assetPath);
            string filePath = Path.Combine(folderPath, $"hotspot{indexHostpot}.asset");

            int i = 0;
            while (File.Exists(filePath)) {
                Debug.Log($"[NodeInspectorWindow] File Exists: {filePath}");
                i++;
                filePath = Path.Combine(folderPath, $"hotspot ({i}).asset");
            }
          
            Debug.Log($"[NodeInspectorWindow] Create Asset: {filePath}");
            AssetDatabase.CreateAsset(hostpot, Path.Combine(filePath));
            return hostpot;

        }

        public static HotspotQuestionData CreateHotspotQuestion(NodeData node, string childName, int indexHotspot) {

            HotspotQuestionData hotspot = ScriptableObject.CreateInstance<HotspotQuestionData>();
            hotspot.type = HotspotData.HotspotType.question;

            string assetPath = AssetDatabase.GetAssetPath(node);
            string folderPath = Path.GetDirectoryName(assetPath);
            string filePath = Path.Combine(folderPath, $"hotspot{indexHotspot}.asset");

            //if (File.Exists(filePath)) {
            //AssetDatabase.DeleteAsset(filePath);

            int i = 0;
            while (File.Exists(filePath)) {
                Debug.Log($"[NodeInspectorWindow] File Exists: {filePath}");
                i++;
                filePath = Path.Combine(folderPath, $"hotspot{indexHotspot}.asset");
            }
            //}
            Debug.Log($"[NodeInspectorWindow] Create Asset: {filePath}");
            AssetDatabase.CreateAsset(hotspot, Path.Combine(filePath));
            return hotspot;
        }

    }
}
