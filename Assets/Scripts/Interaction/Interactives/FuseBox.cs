using UnityEngine;

namespace Interaction
{
    public class FuseBox : MonoBehaviour, IJammable
    {
        [SerializeField] private NPCMoveInteraction npcInteraction;
        public bool Jammed {get => _jammed;}
        private bool _jammed;
        private bool _working = true;
        private BodyCarry tempDetectable; // Replace with the IObservable interface later

        private void Start()
        {
            _jammed = false;
            tempDetectable = GetComponentInChildren<BodyCarry>();
            tempDetectable.enabled = false;   
        }

        public void ToggleJammed()
        {
            _jammed = !_jammed;
            tempDetectable.enabled = _jammed;
            tempDetectable.HasBeenDetected = false;
            
            if (Jammed && _working)
            {
                _working = false;
                npcInteraction.Interact();
            }
        }
    }
}