using UnityEngine;
using UnityEngine.EventSystems;

namespace Singularis.StackVR.Narrative {
    [RequireComponent(typeof(EventTrigger))]
    public class Hotspot : MonoBehaviour {

        protected EventTrigger eventTrigger;


        protected virtual void Awake() {
            eventTrigger = GetComponent<EventTrigger>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected void Start() {

        }

        // Update is called once per frame
        protected void Update() {

        }


        public void SetIcon(Texture2D texture) {
            Material material = new(Shader.Find("Unlit/Hotspot")) {
                mainTexture = texture
            };
            material.SetFloat("_Progress", 1f);

            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            meshRenderer.material = material;
        }

    }
}
