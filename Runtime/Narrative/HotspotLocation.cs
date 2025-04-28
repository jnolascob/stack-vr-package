using Singularis.StackVR.Narrative;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Singularis.StackVR.Narrative {
    public class HotspotLocation : Hotspot {
        [SerializeField]
        private SpotController target = null;
        [SerializeField]
        private int targetId = -1;



        protected override void Awake() {
            base.Awake();

            EventTrigger.Entry entry = new() {
                eventID = EventTriggerType.PointerClick
            };
            entry.callback.AddListener((eventData) => { OnPointerClick((PointerEventData)eventData); });

            eventTrigger.triggers.Add(entry);
        }


        private void OnPointerClick(PointerEventData eventData) {
            if (target == null)
                target = ExperienceManager.FindNodeById(targetId);

            if (target != null)
                OVRPlayerControllerHelper.ChangeLocation(target);
        }


        public void SetTarget(SpotController target) {
            this.target = target;
        }

        public void SetTarget(int targetId) {
            this.targetId = targetId;
        }


    }
}
