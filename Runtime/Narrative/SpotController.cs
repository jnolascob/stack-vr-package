using UnityEngine;

namespace Singularis.StackVR.Narrative {
    public class SpotController : MonoBehaviour {


        public string id = "";

        public Vector3 position {
            get => transform.position;
        }



        public void SetImage(Texture2D texture) {

            Material material = new(Shader.Find("Unlit/Texture")) {
                mainTexture = texture
            };

            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
                meshRenderer.material = material;
        }

    }
}
