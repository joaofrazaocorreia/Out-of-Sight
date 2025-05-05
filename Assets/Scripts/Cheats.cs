using UnityEngine;
using UnityEngine.InputSystem;
using Enums;

public class Cheats : MonoBehaviour
{
    [SerializeField] private Item keycard;
    [SerializeField] private Item code;
    [SerializeField] private Item files;
    private PlayerInput playerInput;
    private Alarm alarm;
    private Player player;
    private PlayerController playerController;
    private PlayerInventory playerInventory;

    private bool speedBoost;

    private void Start()
    {
        playerInput = FindAnyObjectByType<PlayerInput>();
        alarm = FindAnyObjectByType<Alarm>();
        player = FindAnyObjectByType<Player>();
        playerController = FindAnyObjectByType<PlayerController>();
        playerInventory = FindAnyObjectByType<PlayerInventory>();

        speedBoost = false;
    }

    private void Update()
    {
        if(playerInput.actions["ToggleSpeedBoost"].WasPressedThisFrame())
        {
            Debug.Log("Toggled Speed Boost");

            speedBoost = !speedBoost;

            if(speedBoost)
                playerController.SpeedBoost = 4f;

            else
                playerController.SpeedBoost = 1f;
        }

        if(playerInput.actions["ToggleAlarm"].WasPressedThisFrame())
        {
            Debug.Log("Toggled Alarm");
            
            if(alarm.IsOn)
                alarm.AlarmTimer = 0f;
            else
            {
                alarm.TriggerAlarm(true);
                alarm.TriggerAlarm(false);
            }
        }

        if(playerInput.actions["ToggleDetection"].WasPressedThisFrame())
        {
            Debug.Log("Toggled Detection");
            
            player.detectable = !player.detectable;
        }

        if(playerInput.actions["GetKeycard"].WasPressedThisFrame())
        {
            Debug.Log("Obtained Keycard");
            
            playerInventory.AddItem(keycard);
        }

        if(playerInput.actions["GetCode"].WasPressedThisFrame())
        {
            Debug.Log("Obtained Safe Code");
            
            playerInventory.AddItem(code);
        }

        if(playerInput.actions["GetFiles"].WasPressedThisFrame())
        {
            Debug.Log("Obtained Secret Files");
            
            playerInventory.AddItem(files);
        }

        if(playerInput.actions["DisguiseCivillian"].WasPressedThisFrame())
        {
            Debug.Log("Disguised as Civillian");
            
            player.GainDisguise(Disguise.Civillian);
        }

        if(playerInput.actions["DisguiseEmployee"].WasPressedThisFrame())
        {
            Debug.Log("Disguised as Employee");
            
            player.GainDisguise(Disguise.Employee);
        }

        if(playerInput.actions["DisguiseGuardT1"].WasPressedThisFrame())
        {
            Debug.Log("Disguised as Security Guard");
            
            player.GainDisguise(Disguise.Guard_Tier1);
        }

        if(playerInput.actions["DisguiseGuardT2"].WasPressedThisFrame())
        {
            Debug.Log("Disguised as Elite Guard");
            
            player.GainDisguise(Disguise.Guard_Tier2);
        }
    }
}
