using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Singularis.StackVR.Narrative.Editor {
    public abstract class HostpotBase {
        public VisualElement hotspotElement;
        public VisualElement main;


        public abstract void SetCallbacks();


        public HostpotBase(VisualElement main, VisualElement hostpotElement) {
            this.hotspotElement = hostpotElement;
            this.main = main;
        }


        public void SaveData(string key, object value) {
            Dictionary<string, object> hotspotDataStored = hotspotElement.userData as Dictionary<string, object>;

            hotspotDataStored[key] = value;
            hotspotElement.userData = hotspotDataStored;
        }

        public object GetData(string key) {
            Dictionary<string, object> hotspotsDataStore = hotspotElement.userData as Dictionary<string, object>;

            if (hotspotsDataStore.ContainsKey(key)) {
                object result = hotspotsDataStore[key];

                return result;
            }
            else {
                return null;
            }

        }


    }

}
