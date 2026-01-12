using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ButtonPuzzleButton : MonoBehaviour
{
    [Header("Button Settings")]
    public string buttonName = "Square"; // e.g., Circle, Triangle, Square

    [Header("References")]
    public Transform player;             // Assign player object here
    public float interactRange = 3f;     // Distance player must be near
    public KeyCode interactKey = KeyCode.E;

    private ButtonPuzzleUI puzzleUI;

    private void Start()
    {
        puzzleUI = FindObjectOfType<ButtonPuzzleUI>();
    }

    private void Update()
    {
        if (player == null || puzzleUI == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactRange && Input.GetKeyDown(interactKey))
        {
            TryPressButton();
        }
    }

    void TryPressButton()
    {
        if (puzzleUI.CanPressButton(buttonName))
        {
            Debug.Log($"{buttonName} pressed correctly!");
            puzzleUI.NextSequenceStep();
        }
        else
        {
            Debug.Log($"{buttonName} was wrong. Restarting puzzle!");
            puzzleUI.ResetSequence();
        }
    }
}
