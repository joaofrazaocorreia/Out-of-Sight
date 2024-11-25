using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] private List<Player.Disguise> whitelistedDisguises;

    private Player player;
    private Player.Disguise lastPlayerDisguise;
    private bool whitelisted;
    private bool playerIsHere;

    private void Start()
    {
        player = FindAnyObjectByType<Player>();
        lastPlayerDisguise = player.disguise;

        playerIsHere = false;

        foreach(BoxCollider bc in GetComponentsInChildren<BoxCollider>())
        {
            bc.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == player.gameObject)
        {   
            playerIsHere = true;
            CheckWhitelist();
            UpdatePlayerStatus();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(lastPlayerDisguise != player.disguise && playerIsHere)
        {
            CheckWhitelist();
            UpdatePlayerStatus();
            lastPlayerDisguise = player.disguise;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject == player.gameObject)
        {
            playerIsHere = false;
            CheckWhitelist();
            UpdatePlayerStatus();
        }
    }

    private void CheckWhitelist()
    {
        whitelisted = false;

        if(whitelistedDisguises.Count > 0)
        {
            foreach(Player.Disguise d in whitelistedDisguises)
            {
                if(player.disguise == d)
                {
                    whitelisted = true;
                    break;
                }
            }
        }

        else
        {
            whitelisted = true;
        }
    }

    private void UpdatePlayerStatus()
    {
        if(!whitelisted)
            player.GainStatus(Player.Status.Trespassing);
        
        else
            player.LoseStatus(Player.Status.Trespassing);
    }
}
