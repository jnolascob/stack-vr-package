using UnityEngine;
using UnityEngine.EventSystems;

namespace Singularis.StackVR.Narrative {
    public class HotspotQuestion : Hotspot {

        public UIQuestion uiQuestion = null;


        protected override void Awake() {
            base.Awake();

            EventTrigger.Entry entry = new() {
                eventID = EventTriggerType.PointerClick
            };
            entry.callback.AddListener((eventData) => { OnPointerClick((PointerEventData)eventData); });

            eventTrigger.triggers.Add(entry);

            if (uiQuestion != null) {
                uiQuestion.onClose.AddListener(() => { gameObject.SetActive(true); });
                uiQuestion.onOpen.AddListener(() => { gameObject.SetActive(false); });
            }
        }


        private void OnPointerClick(PointerEventData eventData) {
            if (uiQuestion != null)
                uiQuestion.Open();
        }



    }
}

