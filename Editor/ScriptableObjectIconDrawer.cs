using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Singularis.StackVR.Scriptables.Editor;

namespace Singularis.StackVR.Editor {
    [InitializeOnLoad]
    public class ScriptableObjectIconDrawer {
        static Dictionary<Type, Texture2D> typeIcons = new Dictionary<Type, Texture2D>();

        static ScriptableObjectIconDrawer() {
            LoadIcons();
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }


        static private void LoadIcons() {
            typeIcons[typeof(HotspotData)] = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.singularisvr.stackvr/Editor/Sprites/ico_hotspot_asset.png");
            typeIcons[typeof(NodeData)] = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.singularisvr.stackvr/Editor/Sprites/ico_node_asset.png");
            typeIcons[typeof(NarrativeScriptableObject)] = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.singularisvr.stackvr/Editor/Sprites/ico_narrative_asset.png");
        }

        static private void OnProjectWindowItemGUI(string guid, Rect rect) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadMainAssetAtPath(path);

            if (asset == null)
                return;

            Type assetType = asset.GetType();

            if (typeIcons.TryGetValue(assetType, out Texture2D icon) && icon != null) {
                EditorGUIUtility.SetIconForObject(asset, icon);
                
                //Rect iconRect = new Rect(rect.x + rect.width - 18, rect.y + 1, 16, 16);
                //UnityEngine.GUI.DrawTexture(iconRect, icon);
            }
        }
    }
}
