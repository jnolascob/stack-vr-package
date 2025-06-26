using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Singularis.StackVR.Scriptables.Editor;

namespace Singularis.StackVR.Narrative.Editor {
    public class NavigationHostpotWindow : HostpotBase {

        ObjectField targetObjectField;

        public NavigationHostpotWindow(VisualElement main, VisualElement hostpotElement) : base(main, hostpotElement) {

        }

        public override void SetCallbacks() {
            targetObjectField = main.Q<ObjectField>("targetObjectField");
            targetObjectField.objectType = typeof(NodeData);
            targetObjectField.value = GetData("target") as NodeData;

            targetObjectField.RegisterValueChangedCallback(evt => {
                Debug.Log("Changing Target");
                SaveData("target", evt.newValue);
            });
        }
    }
}


