using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float raycastDistance;
    [SerializeField] private GameObject raycastOrigin;
    [SerializeField] private GameObject _head;

    private RaycastHit _hit;
    private InteractiveObject _activeInteractiveObject;

    private void Update()
    {
        GetInteractiveObject();   
    }

    private void GetInteractiveObject()
    {
        Physics.Raycast(raycastOrigin.transform.position, _head.transform.forward, out _hit, raycastDistance);
        if(_hit.collider != null) _activeInteractiveObject = _hit.collider.gameObject.GetComponent<InteractiveObject>();
    }
    
    public void TryInteraction()
    {
        if(CheckValidInteraction()) Interact();
    }
    

    private bool CheckValidInteraction()
    {
        return false;
    }

    private void Interact()
    {
        
    }
}