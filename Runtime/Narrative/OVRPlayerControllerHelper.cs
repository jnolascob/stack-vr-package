using UnityEngine;

namespace Singularis.StackVR.Narrative {
    public class OVRPlayerControllerHelper : MonoBehaviour {
        
        static private OVRPlayerControllerHelper instance;

        static public void ChangeLocation(SpotController spot) {
            if (instance != null) {
                instance.ChangeLocation(spot.position);
            }
            else {
                Debug.LogError("[Singularis - OVRPlayerControllerHelper::ChangeLocation] No hay instancia de OVRPlayerControllerHelper");
            }
        }

        
        private CharacterController characterController;


        public int initialSpotId = -1;


        private void Awake() {
            characterController = GetComponent<CharacterController>();

            //if (instance == null) {
            instance = this;
            //    DontDestroyOnLoad(gameObject);
            //}
            //else {
            //    Destroy(gameObject);
            //}
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {

            SpotController spot = ExperienceManager.FindNodeById(initialSpotId);

            if (spot != null) {
                ChangeLocation(ExperienceManager.FindNodeById(initialSpotId).position);
            }
            else {
                Debug.LogError("[Singularis - OVRPlayerControllerHelper::Start] No hay id de spot inicial");
            }
        }

        // Update is called once per frame
        void Update() {

        }


        public void ChangeLocation(Vector3 targetPosition) {
            //characterController.enabled = false;

            targetPosition.y = transform.position.y;
            transform.position = targetPosition;

            //characterController.enabled = true;
        }
        

    }
}
