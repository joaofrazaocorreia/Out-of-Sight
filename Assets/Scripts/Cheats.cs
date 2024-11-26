using UnityEngine;
using UnityEngine.InputSystem;

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
    private bool trespassing;

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
            speedBoost = !speedBoost;

            if(speedBoost)
                playerController.SpeedBoost = 4f;

            else
                playerController.SpeedBoost = 1f;
        }

        if(playerInput.actions["ToggleAlarm"].WasPressedThisFrame())
        {
            if(alarm.IsOn)
                alarm.AlarmTimer = 0f;
            else
                alarm.TriggerAlarm(true);
        }

        if(playerInput.actions["ToggleTrespassing"].WasPressedThisFrame())
        {
            trespassing = !trespassing;

            if(trespassing)
                player.GainStatus(Player.Status.Trespassing);
            else
                player.LoseStatus(Player.Status.Trespassing);
        }

        if(playerInput.actions["GetKeycard"].WasPressedThisFrame())
        {
            playerInventory.AddItem(keycard, 1);
        }

        if(playerInput.actions["GetCode"].WasPressedThisFrame())
        {
            playerInventory.AddItem(code, 1);
        }

        if(playerInput.actions["GetFiles"].WasPressedThisFrame())
        {
            playerInventory.AddItem(files, 1);
        }
    }
}
