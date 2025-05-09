using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Singularis.StackVR.Scriptables.Editor;
using System;

namespace Singularis.StackVR.Tools.Editor {
    [InitializeOnLoad]
    public class CustomAssetPostprocessor : AssetPostprocessor {

        static private HashSet<string> processedAssets = new HashSet<string>();
        static readonly Dictionary<Type, string> typeSuffixMap = new() {
            { typeof(NarrativeScriptableObject), "narrative" },
            { typeof(NodeData), "node" },
            { typeof(HotspotData), "hotspot" },
        };


        static CustomAssetPostprocessor() {
            processedAssets.Clear();
        }


        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {

            //Debug.Log($"OnPostprocessAllAssets\nImported Assets: {string.Join(", ", importedAssets)}\nDeleted Assets: {string.Join(", ", deletedAssets)}\nMoved Assets: {string.Join(", ", movedAssets)}\nMoved From Asset Paths: {string.Join(", ", movedFromAssetPaths)}");

            foreach (string path in importedAssets) {

                if (path.EndsWith(".narrative.asset") || path.EndsWith(".node.asset") || path.EndsWith(".hotspot.asset") || processedAssets.Contains(path))
                    continue;


                foreach (var kvp in typeSuffixMap) {
                    Type type = kvp.Key;
                    string suffix = kvp.Value;

                    if (path.EndsWith($".{suffix}.asset"))
                        continue; // ya renombrado

                    var asset = AssetDatabase.LoadAssetAtPath(path, type);
                    if (asset == null)
                        continue;

                    processedAssets.Add(path);

                    string newPath = path.Replace(".asset", $".{suffix}.asset");
                    string newName = Path.GetFileNameWithoutExtension(path) + $".{suffix}";

                    EditorApplication.delayCall += () => {
                        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(newPath) != null)
                            return;

                        string result = AssetDatabase.RenameAsset(path, newName);
                        processedAssets.Remove(path);

                        if (!string.IsNullOrEmpty(result))
                            Debug.LogError("Error al renombrar asset: " + result);
                        else
                            Debug.Log($"Asset renombrado a: {newPath}");
                    };

                    break;
                }




                //var asset = AssetDatabase.LoadAssetAtPath<NarrativeScriptableObject>(path);
                //if (asset != null) {
                //    processedAssets.Add(path);

                //    string oldPath = path;
                //    string newPath = path.Replace(".asset", ".narrative.asset");
                //    string newName = Path.GetFileNameWithoutExtension(path) + ".narrative";

                //    EditorApplication.delayCall += () => {
                //        if (AssetDatabase.LoadAssetAtPath<Object>(newPath) != null)
                //            return;

                //        //string result = AssetDatabase.MoveAsset(oldPath, newPath);
                //        string result = AssetDatabase.RenameAsset(oldPath, newName);
                //        processedAssets.Remove(path);
                //        if (!string.IsNullOrEmpty(result)) {
                //            Debug.LogError("Error al renombrar asset: " + result);
                //        }
                //        else {
                //            //Debug.Log("Asset renombrado a: " + newPath);
                //        }
                //    };
                //}


            }

        }

    }
}
