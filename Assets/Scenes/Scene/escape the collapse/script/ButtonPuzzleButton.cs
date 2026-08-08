
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ButtonPuzzleButton : MonoBehaviour, IInteractable
{
    [Header("Button Settings")]
    public string buttonName = "Square";

    [Header("References")]
    public Transform player;
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.E;

    private ButtonPuzzleUI puzzleUI;

    private void Start()
    {
        puzzleUI = FindObjectOfType<ButtonPuzzleUI>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (player == null || puzzleUI == null)
            return;

        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        if (distance <= interactRange &&
            Input.GetKeyDown(interactKey))
        {
            TryPressButton();
        }
    }

    // =========================================================
    // IInteractable
    // =========================================================

    public void Interact()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(
                player.position,
                transform.position
            );

            if (distance > interactRange)
            {
                Debug.Log("[ButtonPuzzleButton] Too far away.");
                return;
            }
        }

        TryPressButton();
    }

    public string GetPrompt()
    {
        return "Press E / Say \"Pick Up\" to interact";
    }

    // =========================================================
    // BUTTON PUZZLE
    // =========================================================

    private void TryPressButton()
    {
        if (puzzleUI == null)
        {
            Debug.LogWarning(
                "[ButtonPuzzleButton] ButtonPuzzleUI not found!"
            );
            return;
        }

        if (puzzleUI.CanPressButton(buttonName))
        {
            Debug.Log(
                $"{buttonName} pressed correctly!"
            );

            puzzleUI.NextSequenceStep();
        }
        else
        {
            Debug.Log(
                $"{buttonName} was wrong. Restarting puzzle!"
            );

            puzzleUI.ResetSequence();
        }
    }
}