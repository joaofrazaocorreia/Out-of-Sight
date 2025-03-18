using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MapArea : MonoBehaviour
{
    [Header("Player Status Properties")]
    [SerializeField] private bool useWhitelist;
    [SerializeField] private List<Player.Disguise> whitelistedDisguises;
    [SerializeField] private bool isCriticalArea;
 
    [Header("Events")]
    [SerializeField] private UnityEvent onEnterArea;
    [SerializeField] private UnityEvent onExitArea;

    public bool UseWhitelist {get => useWhitelist;}
    public List<Player.Disguise> WhitelistedDisguises {get => whitelistedDisguises;}
    public bool IsCriticalArea {get => isCriticalArea;}

    private void Start()
    {
        foreach(BoxCollider bc in GetComponentsInChildren<BoxCollider>())
        {
            bc.isTrigger = true;
        }
    }
 
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Player>() != null)
        {
            onEnterArea?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.GetComponent<Player>() != null)
        {
            onExitArea?.Invoke();
        }
    }
}
