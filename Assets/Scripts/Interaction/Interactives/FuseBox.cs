using Unity.VisualScripting;
using UnityEngine;

namespace Interaction
{
    public class FuseBox : MonoBehaviour, IJammable
    {
        public bool Jammed {get => _jammed;}
        private bool _jammed;
        private bool _working = true;
        [SerializeField] private NPCInteraction npcInteraction;
        
        public void ToggleJammed()
        {
            _jammed = !_jammed;
            if (Jammed && _working)
            {
                _working = false;
                npcInteraction.Interact();
            }
        }
    }
}