using System.Collections.Generic;
using Interaction;
using UnityEngine;

public class MapEntrance : MonoBehaviour
{
    [SerializeField] private List<ItemType> objectiveItems;
    [SerializeField] private float playerDetectionDistance = 6f;

    public static Transform Transform;
    private UIManager uiManager;
    private Transform player;
    private PlayerInventory playerInventory;
    private bool PlayerHasAllObjectives {get
    {
        foreach(ItemType i in objectiveItems)
        {
            if(!playerInventory.HasItem(i))
                return false;
        }

         return true;   
    }}


    private void Start()
    {
        Transform = transform;
        uiManager = FindAnyObjectByType<UIManager>();

        player = FindAnyObjectByType<Player>().transform;
        playerInventory = FindAnyObjectByType<PlayerInventory>();
    }


    private void Update()
    {
        // If the player has the objective in their inventory and comes close to the exit, they complete the mission
        if((transform.position - player.position).magnitude <= playerDetectionDistance)
        {
            if(PlayerHasAllObjectives)
            {
                uiManager.Win();
            }
        }
    }
}
