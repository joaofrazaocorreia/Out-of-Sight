using UnityEngine;

namespace Interaction
{
    public class FuseBox : MonoBehaviour, IJammable
    {
        [SerializeField] private NPCMoveInteraction npcInteraction;
        [SerializeField] private float detectionMultiplier = 3f;
        public bool Jammed {get => _jammed;}
        private bool _jammed;
        private bool _working = true;
        private DetectableObject detectableObject;

        private void Start()
        {
            _jammed = false;
            detectableObject = GetComponent<DetectableObject>();
            detectableObject.DetectionMultiplier = detectionMultiplier;
            detectableObject.enabled = _jammed;
        }

        public void ToggleJammed()
        {
            _jammed = !_jammed;
            detectableObject.enabled = _jammed;
            
            if (Jammed && _working)
            {
                _working = false;
                npcInteraction.Interact();
            }
        }
    }
}