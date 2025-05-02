using UnityEngine;
using UnityEngine.EventSystems;
using Oculus.Interaction;

namespace Singularis.StackVR.Narrative {
    public class InteractableObject : MonoBehaviour {
        [Tooltip("The interactable to monitor for state changes.")]
        /// <summary>
        /// The interactable to monitor for state changes.
        /// </summary>
        [SerializeField, Interface(typeof(IInteractableView))]
        private UnityEngine.Object _interactableView;

        [SerializeField]
        private EventTrigger _eventTrigger;

        private IInteractableView InteractableView;

        protected bool _started = false;


        protected virtual void Awake() {
            InteractableView = _interactableView as IInteractableView;
        }

        protected virtual void Start() {
            this.BeginStart(ref _started);
            UpdateVisual();
            this.EndStart(ref _started);
        }

        protected virtual void OnEnable() {
            if (_started) {
                InteractableView.WhenStateChanged += UpdateVisualState;
                UpdateVisual();
            }
        }

        protected virtual void OnDisable() {
            if (_started) {
                InteractableView.WhenStateChanged -= UpdateVisualState;
            }
        }


        private void UpdateVisual() {
            switch (InteractableView.State) {
                case InteractableState.Normal:

                    break;
                case InteractableState.Hover:

                    break;
                case InteractableState.Select:
                    _eventTrigger.triggers.ForEach(entry => {
                        if (entry.eventID == EventTriggerType.PointerClick) {
                            entry.callback.Invoke(null);
                        }
                    });
                    break;
                case InteractableState.Disabled:

                    break;
            }
        }

        private void UpdateVisualState(InteractableStateChangeArgs args) => UpdateVisual();
    }
}
