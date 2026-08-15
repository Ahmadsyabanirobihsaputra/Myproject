
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnvironmentPuzzleButton : MonoBehaviour, IInteractable
{
    // =========================================================
    // BUTTON SETTINGS
    // =========================================================

    [Header("Button Settings")]

    [Tooltip("Name used for debugging.")]
    public string buttonName = "Environment Button";

    [Tooltip("Maximum distance at which the player can interact.")]
    public float interactRange = 3f;

    [Tooltip("Keyboard key used to interact directly.")]
    public KeyCode interactKey = KeyCode.E;


    // =========================================================
    // OBJECT ACTIVATION
    // =========================================================

    [Header("Object Activation")]

    [Tooltip("Object that will be deactivated when the button is pressed.")]
    public GameObject deactivateObject;

    [Tooltip("Object that will be activated when the button is pressed.")]
    public GameObject activateObject;


    // =========================================================
    // BUTTON BEHAVIOR
    // =========================================================

    [Header("Button Behavior")]

    [Tooltip("If false, the button can only be pressed once.")]
    public bool canPressMultipleTimes = false;


    // =========================================================
    // DEBUG
    // =========================================================

    [Header("Debug")]

    public bool buttonPressed = false;

    public Transform player;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        FindPlayer();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        if (distance <= interactRange &&
            Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }


    // =========================================================
    // FIND PLAYER
    // =========================================================

    private void FindPlayer()
    {
        if (player != null)
            return;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "[EnvironmentPuzzleButton] " +
                "Player with tag 'Player' not found!"
            );
        }
    }


    // =========================================================
    // IINTERACTABLE
    // =========================================================

    public void Interact()
    {
        // -----------------------------------------------------
        // CHECK IF ALREADY PRESSED
        // -----------------------------------------------------

        if (buttonPressed &&
            !canPressMultipleTimes)
        {
            Debug.Log(
                $"[{buttonName}] Already pressed."
            );

            return;
        }


        // -----------------------------------------------------
        // CHECK DISTANCE
        // -----------------------------------------------------

        if (player != null)
        {
            float distance = Vector3.Distance(
                player.position,
                transform.position
            );

            if (distance > interactRange)
            {
                Debug.Log(
                    $"[{buttonName}] Player is too far away."
                );

                return;
            }
        }


        // -----------------------------------------------------
        // PRESS BUTTON
        // -----------------------------------------------------

        PressButton();
    }


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetPrompt()
    {
        if (buttonPressed &&
            !canPressMultipleTimes)
        {
            return "";
        }

        return $"Press {interactKey} to activate";
    }


    // =========================================================
    // BUTTON ACTION
    // =========================================================

    private void PressButton()
    {
        buttonPressed = true;

        Debug.Log(
            $"[{buttonName}] Button pressed!"
        );


        // -----------------------------------------------------
        // DEACTIVATE OBJECT
        // -----------------------------------------------------

        if (deactivateObject != null)
        {
            deactivateObject.SetActive(false);

            Debug.Log(
                $"[{buttonName}] Deactivated: " +
                deactivateObject.name
            );
        }


        // -----------------------------------------------------
        // ACTIVATE OBJECT
        // -----------------------------------------------------

        if (activateObject != null)
        {
            activateObject.SetActive(true);

            Debug.Log(
                $"[{buttonName}] Activated: " +
                activateObject.name
            );
        }
    }


    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            interactRange
        );
    }
}