using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Singularis.StackVR.Narrative.Editor {
    public class VideoNode : BaseNode {

        public Texture2D currentTextureIcon;
        public string pathToVideoImage;
        public Texture currentTexture;


        public VideoNode() : base() {
        }

        // Metodo para generar el nodo
        public override void Draw() {
            base.Draw();
            kindOfNode = KindOfNode.video;

            nodeTypeField = new EnumField("Node Type", KindOfNode.video);
            nodeTypeField.Init(KindOfNode.video);

            Texture2D iconImage = Resources.Load<Texture2D>("video_icon");
            this.Q<VisualElement>("Icon").style.backgroundImage = new StyleBackground(iconImage);

            var defaultImage = Resources.Load<Texture2D>("PlaceHolderVideo");
            UpdateImage(defaultImage);
        }


        public async Task<string> GetVideoImage(int id, string pathToVideo) {
            string videPath = Path.GetFullPath(pathToVideo);
            FFMpegHandler.InitFMpeg();
            Debug.Log("Getting Video");

            string filePath = await FFMpegHandler.ExtractFirstFrame(videPath, $"VideoImage{id}.png", 5);
            AssetDatabase.Refresh();
            pathImage = pathToVideo;

            return filePath;
        }

        public void UpdateVideo(Texture2D texture) {
            isFull = true;
            currentImage = texture;

            Debug.Log("The Current Path is" + pathImage);
            currentTexture = texture;

            BackgroundSize backgroundSize = new BackgroundSize(256, 256);
            imageNode.style.backgroundSize = new StyleBackgroundSize(backgroundSize);
            imageNode.style.backgroundImage = new StyleBackground(texture);

            this.MarkDirtyRepaint();
        }

        // Callback cuando se selecciona el nodo
        public override void OnSelected() {
            base.OnSelected();
            Debug.Log("Graph Element Selected");
        }

        // Callback cuando se deselecciona el nodo
        public override void OnUnselected() {
            base.OnUnselected();
            //inspectorPanel.style.visibility = Visibility.Hidden;
            Debug.Log("Graph Element Unselected");
        }

        public void UpdateImage(Texture2D sprite) {
            pathImage = AssetDatabase.GetAssetPath(sprite);
            imageNode.style.backgroundImage = new StyleBackground(sprite);
            this.MarkDirtyRepaint();
        }

    }
}
