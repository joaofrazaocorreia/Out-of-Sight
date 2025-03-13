using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] private bool useWhitelist;
    [SerializeField] private List<Player.Disguise> whitelistedDisguises;
    [SerializeField] private bool isCriticalArea;

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
}
