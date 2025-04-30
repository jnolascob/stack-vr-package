
using Singularis.StackVR.Narrative.Editor;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Singularis.StackVR.Narrative.Editor {
    public class ImageNode : BaseNode {

        int amountOfUpdates;


        public ImageNode() : base() {

        }

        public override void Draw() {
            base.Draw();

            kindOfNode = KindOfNode.image;

            string bgImage = $"Packages/com.singularisvr.stackvr/Editor/Sprites/ico_image.png";
            Texture2D nodeBGTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(bgImage);

            this.Q<VisualElement>("Icon").style.backgroundImage = new StyleBackground(nodeBGTexture);
            var defaultImage = Resources.Load<Texture2D>("PlaceHolderImage");
            UpdateImage(defaultImage);
        }

        // Metodo para obtener el path de la imagen
        public void UpdateImage(Texture2D sprite) {
            if (amountOfUpdates >= 1) {
                isFull = true;
                currentImage = sprite;
            }

            pathImage = AssetDatabase.GetAssetPath(sprite);
            Debug.Log("The Current Path is" + pathImage);

            imageNode.style.backgroundImage = new StyleBackground(sprite);
            this.MarkDirtyRepaint();
            amountOfUpdates++;
        }

        // Callback cuando se selecciona el nodo
        public override void OnSelected() {
            base.OnSelected();

        }

        // Callback CUando el Nodo se deselecciona
        public override void OnUnselected() {
            base.OnUnselected();
        }

    }
}
