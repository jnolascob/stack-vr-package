using System.Collections.Generic;
using UnityEngine;

namespace Singularis.StackVR.Narrative {
    public class ExperienceManager : MonoBehaviour {

        static private ExperienceManager instance;

        static public SpotController FindNodeById(int id) {
            if (instance != null) {
                return instance.FindNode(id);
            }
            else {
                Debug.LogError("[Singularis - ExperienceManager::FindNode] No hay instancia de ExperienceManager");
                return null;
            }
        }


        [SerializeField]
        private List<SpotController> nodes = new();
        [SerializeField]
        private List<Hotspot> hotspots = new();


        void Awake() {
            instance = this;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected void Start() {
            //instance = this;
        }


        public void AddNode(SpotController node) {
            nodes.Add(node);
        }

        public SpotController FindNode(int id) {
            return nodes.Find(node => node.id == id);
        }


        public void AddHotspot(Hotspot hotspot) {
            hotspots.Add(hotspot);
        }

    }
}
